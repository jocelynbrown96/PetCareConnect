using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareConnect.Data;
using PetCareConnect.Models;
using PetCareConnect.Models.ViewModels;

namespace PetCareConnect.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;

        public DashboardController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext db,
            IWebHostEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _env = env;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Clinic Staff"))
                return View("CaregiverDashboard");

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var pets = await _db.Pets
                .Where(p => p.OwnerID == user.Id)
                .ToListAsync();

            var upcomingAppointments = await _db.Appointments
                .Include(a => a.Pet)
                .Where(a => a.UserID == user.Id && a.AppointmentDateTime >= DateTime.UtcNow)
                .OrderBy(a => a.AppointmentDateTime)
                .ToListAsync();

            var prescriptions = await _db.Prescriptions
                .Include(p => p.Pet)
                .Where(p => p.Pet.OwnerID == user.Id)
                .ToListAsync();

            var vm = new PetOwnerDashboardViewModel
            {
                User = user,
                Pets = pets,
                UpcomingAppointments = upcomingAppointments,
                Prescriptions = prescriptions
            };

            return View("PetOwnerDashboard", vm);
        }

        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var vm = new EditProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email ?? string.Empty,
                Address = user.Address,
                ProfilePhotoUrl = user.ProfilePhotoUrl
            };

            return View(vm);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateProfile(EditProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            if (user.Email != model.Email)
            {
                var existing = await _userManager.FindByEmailAsync(model.Email);
                if (existing != null)
                {
                    TempData["Error"] = "That email is already in use.";
                    return RedirectToAction("EditProfile");
                }
                user.Email = model.Email;
                user.UserName = model.Email;
                user.NormalizedEmail = model.Email.ToUpper();
                user.NormalizedUserName = model.Email.ToUpper();
            }

            user.FirstName = model.FirstName;
            user.LastName = model.LastName;
            user.Address = model.Address;

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                TempData["Error"] = result.Errors.First().Description;
                return RedirectToAction("EditProfile");
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("EditProfile");
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePassword(string CurrentPassword, string NewPassword, string ConfirmPassword)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            if (NewPassword != ConfirmPassword)
            {
                TempData["Error"] = "Passwords do not match.";
                return RedirectToAction("EditProfile");
            }

            var result = await _userManager.ChangePasswordAsync(user, CurrentPassword, NewPassword);
            if (!result.Succeeded)
            {
                TempData["Error"] = result.Errors.First().Description;
                return RedirectToAction("EditProfile");
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["Success"] = "Password updated successfully.";
            return RedirectToAction("EditProfile");
        }

        [HttpPost]
        public async Task<IActionResult> UpdatePhoto(IFormFile photo)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            if (photo == null || photo.Length == 0)
            {
                TempData["Error"] = "Please select a photo.";
                return RedirectToAction("EditProfile");
            }

            if (photo.Length > 5 * 1024 * 1024)
            {
                TempData["Error"] = "Photo must be under 5MB.";
                return RedirectToAction("EditProfile");
            }

            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var ext = Path.GetExtension(photo.FileName).ToLower();
            if (!allowed.Contains(ext))
            {
                TempData["Error"] = "Only JPG, PNG or GIF files are allowed.";
                return RedirectToAction("EditProfile");
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "profiles");
            Directory.CreateDirectory(uploadsFolder);

            if (!string.IsNullOrEmpty(user.ProfilePhotoUrl))
            {
                var oldPath = Path.Combine(_env.WebRootPath, user.ProfilePhotoUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            var fileName = $"{user.Id}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await photo.CopyToAsync(stream);
            }

            user.ProfilePhotoUrl = $"/uploads/profiles/{fileName}";
            await _userManager.UpdateAsync(user);

            TempData["Success"] = "Profile photo updated.";
            return RedirectToAction("EditProfile");
        }
    }
}

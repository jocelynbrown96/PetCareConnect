using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareConnect.Data;
using PetCareConnect.Models;
using PetCareConnect.Models.ViewModels;
using PetCareConnect.Services;

namespace PetCareConnect.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ApplicationDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly NotificationService _notifService;

        public DashboardController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ApplicationDbContext db,
            IWebHostEnvironment env,
            NotificationService notifService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _db = db;
            _env = env;
            _notifService = notifService;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole("Clinic Staff"))
            {
                var staff = await _userManager.GetUserAsync(User);
                if (staff == null) return RedirectToAction("Login", "Auth");

                var today = DateTime.UtcNow.Date;

                var upcomingAppointments = await _db.Appointments
                    .Include(a => a.Pet).ThenInclude(p => p.Owner)
                    .Where(a => a.AppointmentDateTime >= DateTime.UtcNow && a.Status == "Scheduled")
                    .OrderBy(a => a.AppointmentDateTime)
                    .Take(10)
                    .ToListAsync();

                var refillRequests = await _db.Prescriptions
                    .Include(p => p.Pet).ThenInclude(p => p.Owner)
                    .Where(p => p.RefillStatus == "Refill Requested")
                    .ToListAsync();

                var pendingOrders = await _db.Orders
                    .Include(o => o.OrderItems).ThenInclude(oi => oi.Product)
                    .Include(o => o.User)
                    .Where(o => o.Status == "Processing" || o.Status == "Ready")
                    .OrderBy(o => o.OrderDate)
                    .ToListAsync();

                
                var allPets = await _db.Pets
                    .Include(p => p.Owner)
                    .OrderBy(p => p.Name)
                    .Take(20)
                    .ToListAsync();

                var totalPetOwners = await _userManager.GetUsersInRoleAsync("Pet Owner");
                var todayAppts = await _db.Appointments
                    .Where(a => a.AppointmentDateTime.Date == today)
                    .CountAsync();

                var vm = new ClinicStaffDashboardViewModel
                {
                    Staff = staff,
                    UpcomingAppointments = upcomingAppointments,
                    RefillRequests = refillRequests,
                    PendingOrders = pendingOrders,
                    AllPets = allPets,
                    TotalPetOwners = totalPetOwners.Count,
                    TotalAppointmentsToday = todayAppts
                };

                return View("CaregiverDashboard", vm);
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var pets = await _db.Pets
                .Where(p => p.OwnerID == user.Id)
                .ToListAsync();

            var ownerAppointments = await _db.Appointments
                .Include(a => a.Pet)
                .Where(a => a.UserID == user.Id && a.AppointmentDateTime >= DateTime.UtcNow)
                .OrderBy(a => a.AppointmentDateTime)
                .ToListAsync();

            var prescriptions = await _db.Prescriptions
                .Include(p => p.Pet)
                .Where(p => p.Pet.OwnerID == user.Id)
                .ToListAsync();

            var recentOrders = await _db.Orders
                .Where(o => o.UserID == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .Take(3)
                .ToListAsync();

            var ownerVm = new PetOwnerDashboardViewModel
            {
                User = user,
                Pets = pets,
                UpcomingAppointments = ownerAppointments,
                Prescriptions = prescriptions,
                RecentOrders = recentOrders
            };

            return View("PetOwnerDashboard", ownerVm);
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
        // ─── STAFF: Update Appointment Status ───
        [HttpPost]
        [Authorize(Roles = "Clinic Staff")]
        public async Task<IActionResult> UpdateAppointmentStatus(int id, string status)
        {
            var appt = await _db.Appointments
                .Include(a => a.Pet).ThenInclude(p => p.Owner)
                .FirstOrDefaultAsync(a => a.AppointmentID == id);
            if (appt == null) return NotFound();

            appt.Status = status;
            await _db.SaveChangesAsync();

            var icon = status == "Completed" ? "✅" : "❌";
            await _notifService.CreateAsync(
                appt.UserID, appt.User ?? appt.Pet.Owner,
                $"{icon} Your {appt.ServiceType} appointment for {appt.Pet.Name} has been marked as {status}."
            );

            TempData["Success"] = $"Appointment marked as {status}.";
            return RedirectToAction("Index");
        }

        // ─── STAFF: Approve Refill ───
        [HttpPost]
        [Authorize(Roles = "Clinic Staff")]
        public async Task<IActionResult> ApproveRefill(int id)
        {
            var rx = await _db.Prescriptions
                .Include(p => p.Pet).ThenInclude(p => p.Owner)
                .FirstOrDefaultAsync(p => p.PrescriptionID == id);
            if (rx == null) return NotFound();

            rx.RefillStatus = "No Refills";
            await _db.SaveChangesAsync();

            await _notifService.CreateAsync(
                rx.Pet.OwnerID, rx.Pet.Owner,
                $"✅ Your refill for {rx.MedicationName} ({rx.Pet.Name}) has been approved and is ready for pickup."
            );

            TempData["Success"] = "Refill approved and dispensed.";
            return RedirectToAction("Index");
        }

        // ─── STAFF: Deny Refill ───
        [HttpPost]
        [Authorize(Roles = "Clinic Staff")]
        public async Task<IActionResult> DenyRefill(int id)
        {
            var rx = await _db.Prescriptions
                .Include(p => p.Pet).ThenInclude(p => p.Owner)
                .FirstOrDefaultAsync(p => p.PrescriptionID == id);
            if (rx == null) return NotFound();

            rx.RefillStatus = "No Refills";
            await _db.SaveChangesAsync();

            await _notifService.CreateAsync(
                rx.Pet.OwnerID, rx.Pet.Owner,
                $"❌ Your refill request for {rx.MedicationName} ({rx.Pet.Name}) was denied. Please contact the clinic."
            );

            TempData["Success"] = "Refill request denied.";
            return RedirectToAction("Index");
        }

        // ─── STAFF: Update Order Status ───
        [HttpPost]
        [Authorize(Roles = "Clinic Staff")]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var order = await _db.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderID == id);
            if (order == null) return NotFound();

            order.Status = status;
            await _db.SaveChangesAsync();

            var msg = status == "Ready"
                ? $"🛍️ Your order #{id:D6} is ready for {(order.FulfillmentType == "Pickup" ? "pickup" : "delivery")}!"
                : $"✅ Your order #{id:D6} has been completed.";

            await _notifService.CreateAsync(order.UserID, order.User, msg);

            TempData["Success"] = $"Order #{id:D6} marked as {status}.";
            return RedirectToAction("Index");
        }

        // ─── STAFF: Add Prescription GET ───
        [HttpGet]
        [Authorize(Roles = "Clinic Staff")]
        public async Task<IActionResult> AddPrescription()
        {
            var pets = await _db.Pets.Include(p => p.Owner).OrderBy(p => p.Name).ToListAsync();
            var vm = new AddPrescriptionViewModel { AllPets = pets };
            return View(vm);
        }

        // ─── STAFF: Add Prescription POST ───
        [HttpPost]
        [Authorize(Roles = "Clinic Staff")]
        public async Task<IActionResult> AddPrescription(AddPrescriptionViewModel model)
        {
            var staff = await _userManager.GetUserAsync(User);
            if (staff == null) return RedirectToAction("Login", "Auth");

            var pet = await _db.Pets.Include(p => p.Owner).FirstOrDefaultAsync(p => p.PetID == model.PetID);
            if (pet == null) return NotFound();

            var rx = new Prescription
            {
                MedicationName = model.MedicationName,
                Dosage = model.Dosage,
                ExpirationDate = model.ExpirationDate.ToUniversalTime(),
                RefillStatus = model.RefillStatus,
                PetID = pet.PetID,
                Pet = pet,
                ApprovedByUserID = staff.Id,
                ApprovedByUser = staff
            };

            _db.Prescriptions.Add(rx);
            await _db.SaveChangesAsync();

            await _notifService.CreateAsync(
                pet.OwnerID, pet.Owner,
                $"💊 A new prescription for {rx.MedicationName} has been added for {pet.Name}."
            );

            TempData["Success"] = $"Prescription added for {pet.Name}.";
            return RedirectToAction("Index");
            
            
        }
        
        
        
    }
}

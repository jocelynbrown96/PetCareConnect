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
    public class PetController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public PetController(ApplicationDbContext db, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
        }

        // ─── CREATE GET ───
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // ─── CREATE POST ───
        [HttpPost]
        public async Task<IActionResult> Create(PetFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var pet = new Pet
            {
                Name = model.Name,
                Breed = model.Breed,
                Age = model.Age,
                Color = model.Color,
                MedicalNotes = model.MedicalNotes,
                OwnerID = user.Id,
                Owner = user
            };

            // Handle photo upload
            if (model.Photo != null && model.Photo.Length > 0)
            {
                var photoUrl = await SavePetPhoto(model.Photo, null);
                if (photoUrl != null) pet.PhotoUrl = photoUrl;
            }

            _db.Pets.Add(pet);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"{pet.Name} has been added!";
            return RedirectToAction("Index", "Dashboard");
        }

        // ─── DETAILS ───
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var pet = await _db.Pets
                .Include(p => p.Owner)
                .FirstOrDefaultAsync(p => p.PetID == id && p.OwnerID == user.Id);

            if (pet == null) return NotFound();

            var prescriptions = await _db.Prescriptions
                .Where(p => p.PetID == id)
                .ToListAsync();

            var appointments = await _db.Appointments
                .Where(a => a.PetID == id)
                .OrderByDescending(a => a.AppointmentDateTime)
                .Take(5)
                .ToListAsync();

            var vm = new PetDetailsViewModel
            {
                Pet = pet,
                Prescriptions = prescriptions,
                RecentAppointments = appointments
            };

            return View(vm);
        }

        // ─── EDIT GET ───
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var pet = await _db.Pets
                .FirstOrDefaultAsync(p => p.PetID == id && p.OwnerID == user.Id);

            if (pet == null) return NotFound();

            var model = new PetFormViewModel
            {
                PetID = pet.PetID,
                Name = pet.Name,
                Breed = pet.Breed,
                Age = pet.Age,
                Color = pet.Color,
                MedicalNotes = pet.MedicalNotes,
                ExistingPhotoUrl = pet.PhotoUrl
            };

            return View(model);
        }

        // ─── EDIT POST ───
        [HttpPost]
        public async Task<IActionResult> Edit(int id, PetFormViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var pet = await _db.Pets
                .FirstOrDefaultAsync(p => p.PetID == id && p.OwnerID == user.Id);

            if (pet == null) return NotFound();

            pet.Name = model.Name;
            pet.Breed = model.Breed;
            pet.Age = model.Age;
            pet.Color = model.Color;
            pet.MedicalNotes = model.MedicalNotes;

            if (model.Photo != null && model.Photo.Length > 0)
            {
                var photoUrl = await SavePetPhoto(model.Photo, pet.PhotoUrl);
                if (photoUrl != null) pet.PhotoUrl = photoUrl;
            }

            await _db.SaveChangesAsync();

            TempData["Success"] = $"{pet.Name}'s profile has been updated.";
            return RedirectToAction("Details", new { id = pet.PetID });
        }

        // ─── DELETE ───
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var pet = await _db.Pets
                .FirstOrDefaultAsync(p => p.PetID == id && p.OwnerID == user.Id);

            if (pet == null) return NotFound();

            // Delete photo file if exists
            if (!string.IsNullOrEmpty(pet.PhotoUrl))
            {
                var oldPath = Path.Combine(_env.WebRootPath, pet.PhotoUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            _db.Pets.Remove(pet);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"{pet.Name} has been removed.";
            return RedirectToAction("Index", "Dashboard");
        }

        // ─── HELPER: Save pet photo ───
        private async Task<string?> SavePetPhoto(IFormFile photo, string? existingUrl)
        {
            var allowed = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var ext = Path.GetExtension(photo.FileName).ToLower();
            if (!allowed.Contains(ext)) return null;
            if (photo.Length > 5 * 1024 * 1024) return null;

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "pets");
            Directory.CreateDirectory(uploadsFolder);

            // Delete old photo
            if (!string.IsNullOrEmpty(existingUrl))
            {
                var oldPath = Path.Combine(_env.WebRootPath, existingUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                    System.IO.File.Delete(oldPath);
            }

            var fileName = $"{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await photo.CopyToAsync(stream);

            return $"/uploads/pets/{fileName}";
        }
    }
}

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
    public class AppointmentController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ─── CREATE GET ───
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var pets = await _db.Pets
                .Where(p => p.OwnerID == user.Id)
                .ToListAsync();

            var vm = new AppointmentFormViewModel { Pets = pets };
            return View(vm);
        }

        // ─── CREATE POST ───
        [HttpPost]
        public async Task<IActionResult> Create(AppointmentFormViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            // Reload pets for validation failure
            model.Pets = await _db.Pets
                .Where(p => p.OwnerID == user.Id)
                .ToListAsync();

            if (!ModelState.IsValid)
                return View(model);

            var pet = await _db.Pets
                .FirstOrDefaultAsync(p => p.PetID == model.PetID && p.OwnerID == user.Id);

            if (pet == null)
            {
                ModelState.AddModelError("", "Invalid pet selected.");
                return View(model);
            }

            var appointment = new Appointment
            {
                AppointmentDateTime = model.AppointmentDateTime,
                ServiceType = model.ServiceType,
                Status = "Scheduled",
                PetID = model.PetID,
                Pet = pet,
                UserID = user.Id,
                User = user
            };

            _db.Appointments.Add(appointment);
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Appointment booked for {pet.Name} on {model.AppointmentDateTime:MMM d} at {model.AppointmentDateTime:h:mm tt}.";
            return RedirectToAction("Index", "Dashboard");
        }

        // ─── DETAILS ───
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var appointment = await _db.Appointments
                .Include(a => a.Pet)
                .FirstOrDefaultAsync(a => a.AppointmentID == id && a.UserID == user.Id);

            if (appointment == null) return NotFound();

            return View(appointment);
        }

        // ─── EDIT GET ───
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var appointment = await _db.Appointments
                .Include(a => a.Pet)
                .FirstOrDefaultAsync(a => a.AppointmentID == id && a.UserID == user.Id);

            if (appointment == null) return NotFound();

            var pets = await _db.Pets
                .Where(p => p.OwnerID == user.Id)
                .ToListAsync();

            var vm = new AppointmentFormViewModel
            {
                AppointmentID = appointment.AppointmentID,
                PetID = appointment.PetID,
                ServiceType = appointment.ServiceType,
                AppointmentDateTime = appointment.AppointmentDateTime,
                Pets = pets
            };

            return View(vm);
        }

        // ─── EDIT POST ───
        [HttpPost]
        public async Task<IActionResult> Edit(int id, AppointmentFormViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var appointment = await _db.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentID == id && a.UserID == user.Id);

            if (appointment == null) return NotFound();

            var pet = await _db.Pets
                .FirstOrDefaultAsync(p => p.PetID == model.PetID && p.OwnerID == user.Id);

            if (pet == null) return NotFound();

            appointment.PetID = model.PetID;
            appointment.Pet = pet;
            appointment.ServiceType = model.ServiceType;
            appointment.AppointmentDateTime = model.AppointmentDateTime.ToUniversalTime();

            await _db.SaveChangesAsync();

            TempData["Success"] = "Appointment updated successfully.";
            return RedirectToAction("Details", new { id = appointment.AppointmentID });
        }

        // ─── CANCEL ───
        [HttpPost]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var appointment = await _db.Appointments
                .FirstOrDefaultAsync(a => a.AppointmentID == id && a.UserID == user.Id);

            if (appointment == null) return NotFound();

            appointment.Status = "Canceled";
            await _db.SaveChangesAsync();

            TempData["Success"] = "Appointment has been canceled.";
            return RedirectToAction("Index", "Dashboard");
        }
    }
}

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
        private readonly ApplicationDbContext _db;

        public DashboardController(UserManager<ApplicationUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
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
    }
}

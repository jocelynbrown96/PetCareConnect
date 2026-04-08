using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareConnect.Data;
using PetCareConnect.Models;

namespace PetCareConnect.Controllers
{
    [Authorize]
    public class PrescriptionController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public PrescriptionController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ─── INDEX: All prescriptions for this owner ───
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var prescriptions = await _db.Prescriptions
                .Include(p => p.Pet)
                .Where(p => p.Pet.OwnerID == user.Id)
                .OrderBy(p => p.Pet.Name)
                .ThenBy(p => p.MedicationName)
                .ToListAsync();

            return View(prescriptions);
        }

        // ─── DETAILS ───
        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var prescription = await _db.Prescriptions
                .Include(p => p.Pet)
                .FirstOrDefaultAsync(p => p.PrescriptionID == id && p.Pet.OwnerID == user.Id);

            if (prescription == null) return NotFound();

            return View(prescription);
        }

        // ─── REQUEST REFILL ───
        [HttpPost]
        public async Task<IActionResult> RequestRefill(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var prescription = await _db.Prescriptions
                .Include(p => p.Pet)
                .FirstOrDefaultAsync(p => p.PrescriptionID == id && p.Pet.OwnerID == user.Id);

            if (prescription == null) return NotFound();

            if (prescription.RefillStatus != "Refills Available")
            {
                TempData["Error"] = "A refill cannot be requested at this time.";
                return RedirectToAction("Details", new { id });
            }

            prescription.RefillStatus = "Refill Requested";
            await _db.SaveChangesAsync();

            TempData["Success"] = $"Refill requested for {prescription.MedicationName}.";
            return RedirectToAction("Details", new { id });
        }
    }
}

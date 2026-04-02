using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace PetCareConnect.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            if (User.IsInRole("Caregiver"))
                return View("CaregiverDashboard");

            return View("PetOwnerDashboard");
        }
    }
}
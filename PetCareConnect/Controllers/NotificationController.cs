using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareConnect.Data;
using PetCareConnect.Models;

namespace PetCareConnect.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public NotificationController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> MarkRead(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var notif = await _db.Notifications
                .FirstOrDefaultAsync(n => n.NotificationID == id && n.UserID == user.Id);

            if (notif != null)
            {
                notif.Status = "Read";
                await _db.SaveChangesAsync();
            }

            return Redirect(Request.Headers["Referer"].ToString() ?? "/Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> MarkAllRead()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var unread = await _db.Notifications
                .Where(n => n.UserID == user.Id && n.Status == "Unread")
                .ToListAsync();

            foreach (var n in unread)
                n.Status = "Read";

            await _db.SaveChangesAsync();

            return Redirect(Request.Headers["Referer"].ToString() ?? "/Dashboard");
        }
    }
}

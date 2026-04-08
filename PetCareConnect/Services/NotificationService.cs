using PetCareConnect.Data;
using PetCareConnect.Models;

namespace PetCareConnect.Services
{
    public class NotificationService
    {
        private readonly ApplicationDbContext _db;

        public NotificationService(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task CreateAsync(string userId, ApplicationUser user, string message)
        {
            var notification = new Notification
            {
                UserID = userId,
                User = user,
                Message = message,
                Status = "Unread",
                DateCreated = DateTime.UtcNow
            };

            _db.Notifications.Add(notification);
            await _db.SaveChangesAsync();
        }
    }
}

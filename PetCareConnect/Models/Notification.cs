using System.ComponentModel.DataAnnotations.Schema;

namespace PetCareConnect.Models
{
    public class Notification
    {
        public int NotificationID 
        { 
            get; 
            set; 
        }
        public string? Message 
        { 
            get; 
            set; 
        }

        public required string Status // Read, Unread, Archived
        { 
            get; 
            set; 
        }
        public DateTime DateCreated
        { 
            get; 
            set; 
        }
        // Foreign key to ApplicationUser:
        public required string UserID 
        { 
            get; 
            set; 
        }
        [ForeignKey("UserID")]
        public required ApplicationUser User 
        { 
            get; 
            set;
        }
    }
}

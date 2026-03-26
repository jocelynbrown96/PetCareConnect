using System.ComponentModel.DataAnnotations.Schema;

namespace PetCareConnect.Models
{
    public class Appointment
    {
        public int AppointmentID 
        { 
            get; 
            set; 
        }
        public DateTime AppointmentDateTime
        { 
            get; 
            set; 
        }
        public required string ServiceType 
        { 
            get; 
            set; 
        }

        public string? Status // Scheduled, Completed, Canceled
        { 
            get; 
            set;
        }

        // Foreign key to Pet:
        public int PetID
        { 
            get; 
            set;
        }

        public required Pet Pet 
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

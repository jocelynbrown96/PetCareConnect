using System.ComponentModel.DataAnnotations.Schema;

namespace PetCareConnect.Models
{
    public class Prescription
    {
        public int PrescriptionID 
        { 
            get; 
            set; 
        }
        public required string MedicationName 
        { 
            get; 
            set; 
        }
        public required string Dosage 
        { 
            get; 
            set; 
        }
        public DateTime ExpirationDate
        { 
            get; 
            set; 
        }

        public required string RefillStatus // No Refills, Refills Available, Refill Requested
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

        // Foreign key to ApplicationUser (User Who Approved of the Refill Request):
        public required string ApprovedByUserID 
        { 
            get; 
            set;
        }

        [ForeignKey("ApprovedByUserID")]
        public required ApplicationUser ApprovedByUser 
        { 
            get; 
            set;
        }
    }
}

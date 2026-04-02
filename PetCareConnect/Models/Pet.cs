using System.ComponentModel.DataAnnotations.Schema;

namespace PetCareConnect.Models
{
    public class Pet
    {
        public int PetID 
        { 
            get; 
            set; 
        }

        public required string Name 
        { 
            get; 
            set;
        }

        public required string Breed 
        { 
            get; 
            set;
        }

        public int Age 
        { 
            get; 
            set;
        }

        public string? Color 
        { 
            get; 
            set;
        }

        public string? MedicalNotes 
        { 
            get; 
            set;
        }

        // Foreign key to ApplicationUser:
        public required string OwnerID 
        { 
            get; 
            set;
        }

        [ForeignKey("OwnerID")]
        public required ApplicationUser Owner 
        { 
            get; 
            set; 
        }
        public string? PhotoUrl
        {
            get;
            set;
        }
    }
}

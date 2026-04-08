using System.ComponentModel.DataAnnotations;
using PetCareConnect.Models;

namespace PetCareConnect.Models.ViewModels
{
    public class AppointmentFormViewModel
    {
        public int AppointmentID { get; set; }

        [Required(ErrorMessage = "Please select a pet.")]
        public int PetID { get; set; }

        [Required(ErrorMessage = "Please select a service type.")]
        public string ServiceType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select a date and time.")]
        public DateTime AppointmentDateTime { get; set; } = DateTime.Now.AddDays(1);

        public string? Notes { get; set; }

        // For populating the pet dropdown
        public List<Pet> Pets { get; set; } = new();

        public static readonly List<string> ServiceTypes = new()
        {
            "Checkup",
            "Vaccination",
            "Grooming",
            "Dental",
            "Emergency",
            "Other"
        };
    }
}

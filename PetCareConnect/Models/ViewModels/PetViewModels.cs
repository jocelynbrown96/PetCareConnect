using System.ComponentModel.DataAnnotations;
using PetCareConnect.Models;

namespace PetCareConnect.Models.ViewModels
{
    public class PetFormViewModel
    {
        public int PetID { get; set; }

        [Required(ErrorMessage = "Name is required.")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Breed is required.")]
        public string Breed { get; set; } = string.Empty;

        [Required(ErrorMessage = "Age is required.")]
        [Range(0, 50, ErrorMessage = "Age must be between 0 and 50.")]
        public int Age { get; set; }

        public string? Color { get; set; }
        public string? MedicalNotes { get; set; }
        public IFormFile? Photo { get; set; }
        public string? ExistingPhotoUrl { get; set; }
    }

    public class PetDetailsViewModel
    {
        public required Pet Pet { get; set; }
        public List<Prescription> Prescriptions { get; set; } = new();
        public List<Appointment> RecentAppointments { get; set; } = new();
    }
}

using PetCareConnect.Models;

namespace PetCareConnect.Models.ViewModels
{
    public class PetOwnerDashboardViewModel
    {
        public required ApplicationUser User { get; set; }
        public List<Pet> Pets { get; set; } = new();
        public List<Appointment> UpcomingAppointments { get; set; } = new();
        public List<Prescription> Prescriptions { get; set; } = new();
    }
}

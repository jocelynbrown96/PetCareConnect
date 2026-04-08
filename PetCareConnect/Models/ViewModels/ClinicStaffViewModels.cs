using PetCareConnect.Models;

namespace PetCareConnect.Models.ViewModels
{
    public class ClinicStaffDashboardViewModel
    {
        public required ApplicationUser Staff { get; set; }
        public List<Appointment> UpcomingAppointments { get; set; } = new();
        public List<Prescription> RefillRequests { get; set; } = new();
        public List<Order> PendingOrders { get; set; } = new();
        public List<Pet> AllPets { get; set; } = new();
        public int TotalPetOwners { get; set; }
        public int TotalAppointmentsToday { get; set; }
    }

    public class AddPrescriptionViewModel
    {
        public int PetID { get; set; }
        public string MedicationName { get; set; } = string.Empty;
        public string Dosage { get; set; } = string.Empty;
        public DateTime ExpirationDate { get; set; } = DateTime.Now.AddMonths(6);
        public string RefillStatus { get; set; } = "No Refills";
        public List<Pet> AllPets { get; set; } = new();

        public static readonly List<string> RefillOptions = new()
        {
            "No Refills",
            "Refills Available"
        };
    }
}

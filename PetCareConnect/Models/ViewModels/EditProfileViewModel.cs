namespace PetCareConnect.Models.ViewModels
{
    public class EditProfileViewModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string? ProfilePhotoUrl { get; set; }
    }
}

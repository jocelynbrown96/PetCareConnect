using System.ComponentModel.DataAnnotations.Schema;

namespace PetCareConnect.Models
{
    public class Order
    {
        public int OrderID 
        { 
            get; 
            set; 
        }

        public DateTime OrderDate
        { 
            get; 
            set; 
        }

        public decimal TotalPrice 
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

        public required ICollection<OrderItem> OrderItems 
        { 
            get; 
            set;
        }
        public string FulfillmentType { get; set; } = "Delivery"; // Delivery or Pickup

        public string? PickupLocation { get; set; } // clinic name if pickup

        public string Status { get; set; } = "Processing"; // Processing, Ready, Completed
    }
}

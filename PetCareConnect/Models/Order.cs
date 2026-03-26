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
    }
}

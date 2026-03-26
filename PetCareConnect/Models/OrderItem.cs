using System.ComponentModel.DataAnnotations.Schema;

namespace PetCareConnect.Models
{
    public class OrderItem
    {
        public int OrderItemID 
        { 
            get; 
            set; 
        }

        public int Quantity 
        { 
            get; 
            set; 
        }
        public decimal UnitPrice 
        { 
            get; 
            set;
        }
        // Foreign key to Order:
        public int OrderID
        { 
            get; 
            set; 
        }
   
        public required Order Order 
            { 
                get; 
                set;
        }

        // Foreign key to Product:
        public int ProductID
        { 
            get; 
            set;
        }

        public required Product Product 
        { 
            get; 
            set;
        }
    }
}

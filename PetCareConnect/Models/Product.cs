namespace PetCareConnect.Models
{
    public class Product
    {
        public int ProductId 
        { 
            get; 
            set; 
        }

        public required string Name
        { 
            get; 
            set;
        }

        public required string Description
        { 
            get; 
            set;
        }

        public decimal Price
        { 
            get; 
            set;
        }

        public int StockQuantity
            { 
                get; 
                set;
            }
        public string Category { get; set; } = "General"; // Food, Toys, Health, General

        public string? ImageEmoji { get; set; } // e.g. "🦴" "🐟" "💊" — no image uploads needed
    }
}

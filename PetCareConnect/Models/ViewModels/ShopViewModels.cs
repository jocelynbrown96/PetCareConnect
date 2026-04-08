using System.ComponentModel.DataAnnotations;
using PetCareConnect.Models;

namespace PetCareConnect.Models.ViewModels
{
    public class CartItem
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public string? ImageEmoji { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal => UnitPrice * Quantity;
    }

    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new();
        public decimal Total => Items.Sum(i => i.Subtotal);
        public int ItemCount => Items.Sum(i => i.Quantity);
    }

    public class CheckoutViewModel
    {
        public List<CartItem> Items { get; set; } = new();
        public decimal Total => Items.Sum(i => i.Subtotal);

        [Required]
        public string FulfillmentType { get; set; } = "Delivery";
        public string? PickupLocation { get; set; }

        public static readonly List<string> PickupLocations = new()
        {
            "PetCareConnect Clinic — Downtown Toronto (123 King St W)",
            "PetCareConnect Clinic — North York (456 Yonge St)",
            "PetCareConnect Clinic — Scarborough (789 Ellesmere Rd)",
            "PetCareConnect Clinic — Mississauga (321 Hurontario St)"
        };
    }

    public class ShopIndexViewModel
    {
        public List<Product> Products { get; set; } = new();
        public string? ActiveCategory { get; set; }
        public List<string> Categories { get; set; } = new();
        public int CartCount { get; set; }
    }
}

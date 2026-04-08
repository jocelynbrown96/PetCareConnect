using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCareConnect.Data;
using PetCareConnect.Models;
using PetCareConnect.Models.ViewModels;
using System.Text.Json;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PetCareConnect.Controllers
{
    [Authorize]
    public class ShopController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private const string CartKey = "PCC_Cart";

        public ShopController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        // ─── HELPERS ───
        private List<CartItem> GetCart()
        {
            var json = HttpContext.Session.GetString(CartKey);
            return json == null ? new List<CartItem>() : JsonSerializer.Deserialize<List<CartItem>>(json) ?? new();
        }

        private void SaveCart(List<CartItem> cart)
        {
            HttpContext.Session.SetString(CartKey, JsonSerializer.Serialize(cart));
        }

        // ─── INDEX ───
        [HttpGet]
        public async Task<IActionResult> Index(string? category)
        {
            var query = _db.Products.AsQueryable();

            if (!string.IsNullOrEmpty(category) && category != "All")
                query = query.Where(p => p.Category == category);

            var products = await query.OrderBy(p => p.Name).ToListAsync();
            var categories = await _db.Products.Select(p => p.Category).Distinct().OrderBy(c => c).ToListAsync();
            var cart = GetCart();

            var vm = new ShopIndexViewModel
            {
                Products = products,
                ActiveCategory = category ?? "All",
                Categories = categories,
                CartCount = cart.Sum(i => i.Quantity)
            };

            return View(vm);
        }

        // ─── ADD TO CART ───
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var product = await _db.Products.FindAsync(productId);
            if (product == null) return NotFound();

            var cart = GetCart();
            var existing = cart.FirstOrDefault(i => i.ProductId == productId);

            if (existing != null)
                existing.Quantity += quantity;
            else
                cart.Add(new CartItem
                {
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Category = product.Category,
                    ImageEmoji = product.ImageEmoji,
                    UnitPrice = product.Price,
                    Quantity = quantity
                });

            SaveCart(cart);
            TempData["Success"] = $"{product.Name} added to cart!";
            return RedirectToAction("Index", new { category = Request.Form["returnCategory"].ToString() });
        }

        // ─── CART ───
        [HttpGet]
        public IActionResult Cart()
        {
            var cart = GetCart();
            var vm = new CartViewModel { Items = cart };
            return View(vm);
        }

        // ─── UPDATE CART ───
        [HttpPost]
        public IActionResult UpdateCart(int productId, int quantity)
        {
            var cart = GetCart();
            var item = cart.FirstOrDefault(i => i.ProductId == productId);

            if (item != null)
            {
                if (quantity <= 0)
                    cart.Remove(item);
                else
                    item.Quantity = quantity;
            }

            SaveCart(cart);
            return RedirectToAction("Cart");
        }

        // ─── REMOVE FROM CART ───
        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            var cart = GetCart();
            cart.RemoveAll(i => i.ProductId == productId);
            SaveCart(cart);
            return RedirectToAction("Cart");
        }

        // ─── CHECKOUT ───
        [HttpGet]
        public IActionResult Checkout()
        {
            var cart = GetCart();
            if (!cart.Any()) return RedirectToAction("Cart");

            var vm = new CheckoutViewModel { Items = cart };
            return View(vm);
        }

        // ─── PLACE ORDER ───
        [HttpPost]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var cart = GetCart();
            if (!cart.Any()) return RedirectToAction("Cart");

            var order = new Order
            {
                OrderDate = DateTime.UtcNow,
                TotalPrice = cart.Sum(i => i.Subtotal),
                UserID = user.Id,
                User = user,
                FulfillmentType = model.FulfillmentType,
                PickupLocation = model.FulfillmentType == "Pickup" ? model.PickupLocation : null,
                Status = "Processing",
                OrderItems = cart.Select(i => new OrderItem
                {
                    ProductID = i.ProductId,
                    Product = _db.Products.Find(i.ProductId)!,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Order = null!
                }).ToList()
            };

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            // Clear cart
            SaveCart(new List<CartItem>());

            return RedirectToAction("Confirmation", new { id = order.OrderID });
        }

        // ─── CONFIRMATION ───
        [HttpGet]
        public async Task<IActionResult> Confirmation(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.OrderID == id && o.UserID == user.Id);

            if (order == null) return NotFound();
            return View(order);
        }

        // ─── ORDERS HISTORY ───
        [HttpGet]
        public async Task<IActionResult> Orders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var orders = await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Where(o => o.UserID == user.Id)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        // ─── RECEIPT PDF ───
        [HttpGet]
        public async Task<IActionResult> Receipt(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Auth");

            var order = await _db.Orders
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.OrderID == id && o.UserID == user.Id);

            if (order == null) return NotFound();

            QuestPDF.Settings.License = LicenseType.Community;

            var pdf = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(50);
                    page.DefaultTextStyle(x => x.FontFamily("Helvetica").FontSize(11));

                    page.Header().Column(col =>
                    {
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("PetCareConnect").FontSize(22).Bold().FontColor("#1A1A2E");
                                c.Item().Text("Your pet's one-stop shop").FontSize(10).FontColor("#6B7280");
                            });
                            row.ConstantItem(120).AlignRight().Column(c =>
                            {
                                c.Item().Text("RECEIPT").FontSize(16).Bold().FontColor("#4A7C59");
                                c.Item().Text($"#{order.OrderID:D6}").FontSize(10).FontColor("#6B7280");
                            });
                        });
                        col.Item().PaddingTop(10).LineHorizontal(1).LineColor("#E5E0D8");
                    });

                    page.Content().PaddingTop(20).Column(col =>
                    {
                        // Order info
                        col.Item().Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Bill To").FontSize(9).FontColor("#6B7280").Bold();
                                c.Item().Text($"{order.User.FirstName} {order.User.LastName}").Bold();
                                c.Item().Text(order.User.Email ?? "").FontColor("#6B7280");
                            });
                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text("Order Date").FontSize(9).FontColor("#6B7280").Bold();
                                c.Item().Text(order.OrderDate.ToLocalTime().ToString("MMMM d, yyyy")).Bold();
                                c.Item().Text(order.OrderDate.ToLocalTime().ToString("h:mm tt")).FontColor("#6B7280");
                            });
                        });

                        col.Item().PaddingVertical(16).LineHorizontal(1).LineColor("#E5E0D8");

                        // Fulfillment
                        col.Item().PaddingBottom(16).Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("Fulfillment").FontSize(9).FontColor("#6B7280").Bold();
                                c.Item().Text(order.FulfillmentType).Bold();
                                if (order.FulfillmentType == "Pickup" && !string.IsNullOrEmpty(order.PickupLocation))
                                    c.Item().Text(order.PickupLocation).FontColor("#6B7280").FontSize(10);
                            });
                            row.RelativeItem().AlignRight().Column(c =>
                            {
                                c.Item().Text("Status").FontSize(9).FontColor("#6B7280").Bold();
                                c.Item().Text(order.Status).Bold().FontColor("#4A7C59");
                            });
                        });

                        // Items header
                        col.Item().Background("#FAF8F4").Padding(10).Row(row =>
                        {
                            row.RelativeItem(4).Text("Item").Bold().FontSize(10);
                            row.RelativeItem(1).AlignCenter().Text("Qty").Bold().FontSize(10);
                            row.RelativeItem(1).AlignRight().Text("Unit Price").Bold().FontSize(10);
                            row.RelativeItem(1).AlignRight().Text("Subtotal").Bold().FontSize(10);
                        });

                        // Items
                        foreach (var item in order.OrderItems)
                        {
                            col.Item().BorderBottom(1).BorderColor("#E5E0D8").Padding(10).Row(row =>
                            {
                                row.RelativeItem(4).Column(c =>
                                {
                                    c.Item().Text(item.Product.Name).Bold();
                                    c.Item().Text(item.Product.Category).FontSize(9).FontColor("#6B7280");
                                });
                                row.RelativeItem(1).AlignCenter().Text(item.Quantity.ToString());
                                row.RelativeItem(1).AlignRight().Text($"${item.UnitPrice:F2}");
                                row.RelativeItem(size: 1).AlignRight().Text($"${item.UnitPrice * item.Quantity:F2}").Bold();
                            });
                        }

                        // Total
                        col.Item().PaddingTop(8).Row(row =>
                        {
                            row.RelativeItem();
                            row.ConstantItem(200).Column(c =>
                            {
                                c.Item().LineHorizontal(1).LineColor("#1A1A2E");
                                c.Item().PaddingTop(8).Row(r =>
                                {
                                    r.RelativeItem().Text("TOTAL").Bold().FontSize(13);
                                    r.AutoItem().Text($"${order.TotalPrice:F2}").Bold().FontSize(13).FontColor("#4A7C59");
                                });
                            });
                        });

                        col.Item().PaddingTop(40).Text("Thank you for shopping with PetCareConnect! 🐾")
                            .FontColor("#6B7280").FontSize(10).Italic();
                    });

                    page.Footer().AlignCenter()
                        .Text($"PetCareConnect · Order #{order.OrderID:D6} · Generated {DateTime.Now:MMM d, yyyy}")
                        .FontSize(8).FontColor("#6B7280");
                });
            });

            var bytes = pdf.GeneratePdf();
            return File(bytes, "application/pdf", $"PetCareConnect-Order-{order.OrderID:D6}.pdf");
        }
    }
}

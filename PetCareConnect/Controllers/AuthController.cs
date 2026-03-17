using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using PetCareConnect.Models;
using PetCareConnect.Models.DTOs;
using System.Threading.Tasks;

namespace PetCareConnect.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        public AuthController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _roleManager = roleManager;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            if (!await _roleManager.RoleExistsAsync(model.Role))
            {
                return BadRequest($"Role '{model.Role}' does not exist");
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FirstName = model.FirstName,
                LastName = model.LastName,
                Address = model.Address,
                Role = model.Role,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
                return BadRequest(ModelState);

            await _userManager.AddToRoleAsync(user, model.Role);

            return Ok(new { message = "User has been successfully registered!" });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return Unauthorized("Invalid email or password. Please try again");
            }
            var result = await _signInManager.PasswordSignInAsync(user, model.Password, isPersistent: false, lockoutOnFailure: false);
            if (!result.Succeeded)
            {
                return Unauthorized("Invalid email or password. Please try again");
            }
            
            user.LastLogin = System.DateTime.UtcNow;
            await _userManager.UpdateAsync(user);

            return Ok("Logged in successfully!");

        }

        [Authorize(Roles = "System Administrator")]
        [HttpGet("admin-test")]
        public IActionResult AdminTest()
        {
            return Ok("You are an admin!");
        }
    }
}

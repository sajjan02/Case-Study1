using Autho_micro.Repo;
using Autho_micro.Models;
using Autho_micro.Repo;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using Microsoft.EntityFrameworkCore; // Required for async methods
using BCrypt.Net;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Autho_micro.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RegisterController : ControllerBase
    {
        private readonly RetailManagement1Context _context;

        public RegisterController(RetailManagement1Context context)
        {
            _context = context;
        }
        // GET: api/<RegisterController>
        [HttpGet]
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }




        // Import the BCrypt.Net namespace

        [HttpPost("register")]
        public async Task<IActionResult> RegisterUser([FromBody] UserCredentials userDto)
        {
            // Check if the email already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == userDto.Email);

            if (existingUser != null)
            {
                return BadRequest("Email already exists.");
            }

            // Hash the password
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(userDto.Password);

            // Create a new user
            var newUser = new User
            {
                FirstName = userDto.FirstName,
                Username=userDto.Username,
                Email = userDto.Email,
                Upassword = hashedPassword
            };
            List<User> users = _context.Users.ToList();

            newUser.UserId=users.Count+1;
            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok("User registered successfully.");
        }

    }

}


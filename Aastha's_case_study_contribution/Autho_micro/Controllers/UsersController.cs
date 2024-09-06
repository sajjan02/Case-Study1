using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using Autho_micro.Models;
using Microsoft.CodeAnalysis.Scripting;

[ApiController]
[Route("[controller]")]

public class AuthController : ControllerBase
{
    private readonly RetailManagement1Context _context;
    private readonly IConfiguration _config;

    public AuthController(RetailManagement1Context context, IConfiguration config)
    {
        _context = context;
        _config = config;
    }
   
    [HttpPost]//Defines an asynchronous method that returns an IActionResult, which represents the result of the HTTP request.
    [Route("authenticate")]
    [AllowAnonymous]
    public async Task<IActionResult> Authenticate([FromBody] UserCredentials user)//Binds the incoming request body to the User object.
    {
        

        // Check for user existence and username match
        var dbUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == user.Username);

        if (dbUser == null)
        {
            return Unauthorized("Invalid username or password");
        }

        // Verify password
        bool passwordMatch = BCrypt.Net.BCrypt.Verify(user.Password, dbUser.Upassword);
        // Compares the provided password (user.Upassword) with the hashed password stored in the database (dbUser.Upassword).
        if (passwordMatch)
        {
            var key = Encoding.UTF8.GetBytes(_config["JWT:Key"]);//Retrieves and encodes the JWT secret key from the configuration settings.

            var tokenDescriptor = new SecurityTokenDescriptor
            //create  a jwt token
            {
                Subject = new ClaimsIdentity(new[]
                {new Claim(ClaimTypes.Name, dbUser.UserId.ToString()),

                    // Add other claims if needed
                }),
                Issuer = _config["JWT:Issuer"],
                Audience = _config["JWT:Audience"],
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(tokenHandler.WriteToken(token));
        }
        else
        {
            return Unauthorized("Invalid username or password");
        }
    }
}

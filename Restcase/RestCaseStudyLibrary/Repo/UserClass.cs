using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using RestCaseStudyLibrary.Data;
using RestCaseStudyLibrary.Models;
using RestCaseStudyLibrary.Repo;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestCaseStudyLibrary.Repo
{
    internal class UserClass : ControllerBase, IUser
    {
        RetailManagement1Context _context;
        IConfiguration _config;
        public UserClass(RetailManagement1Context retailManagement1Context, IConfiguration config)
        {
            _context = retailManagement1Context;
            _config = config;
        }

        public List<User> GetAllUsers()
        {
            List<User> list = _context.Users.ToList();
            return list;
        }

        public User GetUserById(int id)
        {
            User user = _context.Users.Where(u => u.UserId == id).FirstOrDefault();
            return user;
        }

        public void AddUser(User user)
        {
            _context.Add(user);
            _context.SaveChangesAsync();
        }

        public void DeleteUserById(int id)
        {
            User user = _context.Users.Find(id);
            _context.Remove(user);
            _context.SaveChangesAsync();
        }

        public void UpdateUser(User user)
        {
            User exitCust = _context.Users.Find(user.UserId);

            if (exitCust != null)
            {
                exitCust.UserId = user.UserId;
                exitCust.FirstName = user.FirstName;
                exitCust.LastName = user.LastName;
                exitCust.Email = user.Email;
                _context.SaveChanges();
                //throw new NotImplementedException();
            }
        }

        public async Task<ActionResult> Authenticate(UserLogin user)
        {
            using (var context = new RetailManagement1Context(/* DbContextOptions */))
            {
                // Validate user from the database
                var dbUser = await _context.Users
                                          .FirstOrDefaultAsync(u => u.Username == user.Username && u.Password == user.Password);

                if (dbUser != null)
                {
                    // Generate JSON Web Token with the valid details and return
                    var key = Encoding.UTF8.GetBytes(_config["JWT:Key"]);
                    var tokenDescriptor = new SecurityTokenDescriptor
                    {
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
                    return Unauthorized("Invalid username/password!!!");
                }
            }
        }

       
    }
}

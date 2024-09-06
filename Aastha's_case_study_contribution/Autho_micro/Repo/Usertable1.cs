using Autho_micro.Repo;


using Autho_micro.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Autho_micro.Repo
{ 
    public class Usertable1 : Iuser
    {
        private readonly RetailManagement1Context _context;

        public Usertable1(RetailManagement1Context context)
        {
            _context = context;
        }

        public User Userdetails1(int id)
        {
            User user = _context.Users.Where(u => u.UserId == id).FirstOrDefault();
            return user;
        }
    }
}

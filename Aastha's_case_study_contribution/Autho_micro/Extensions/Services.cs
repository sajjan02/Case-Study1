using Microsoft.EntityFrameworkCore;


using Autho_micro.Models;
using Autho_micro.Repo;


using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Autho_micro.Extensions
{
    

public static class Services
    {
        public static IServiceCollection AddCustomerLibrary(this IServiceCollection services, string Constr)
        {
            services.AddDbContext<RetailManagement1Context>(options => options.UseSqlServer(Constr));
            services.AddScoped<Iuser, Usertable1>();
            return services;
        }
    }
}

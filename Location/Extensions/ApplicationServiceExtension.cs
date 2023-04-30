using Location.IRepository;
using Location.Models;
using Location.Repository;
using Location.Repository.Generic;
using Location.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Location.Extensions
{
    public static class ApplicationServiceExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config) 
        {
            services.AddTransient<ILocationRepository, LocationRepository>();
           services.AddScoped<IUnitOfWork<DatabaseContextCla>, UnitOfWork<DatabaseContextCla>>();

            services.AddTransient(typeof(IGenericRepository<>), typeof(GenericRepository<>));

            services.AddDbContext<DatabaseContextCla>(options =>
            {
                options.UseSqlServer(config.GetConnectionString("backendAPIContext"));
            });

            return services;
        }
    }
}

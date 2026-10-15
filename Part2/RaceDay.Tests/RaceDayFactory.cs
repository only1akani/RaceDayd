using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RaceDay.Api.Data;

namespace RaceDay.Tests
{//start of namespace

    //starts the real API for the tests, but with an in-memory database instead of SQL Server
    public class RaceDayFactory : WebApplicationFactory<Program>
    {//start of class

        //every factory gets its own empty database
        private readonly string _dbName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {//start of method

            builder.ConfigureServices(services =>
            {
                //remove the SQL Server setup from Program.cs
                ServiceDescriptor? descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<RaceDayDbContext>));

                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                //use the in-memory database instead
                services.AddDbContext<RaceDayDbContext>(options =>
                    options.UseInMemoryDatabase(_dbName));
            });

        }//end of method

        protected override IHost CreateHost(IHostBuilder builder)
        {//start of method

            IHost host = base.CreateHost(builder);

            //create the tables and seed the two roles
            using (IServiceScope scope = host.Services.CreateScope())
            {
                RaceDayDbContext db = scope.ServiceProvider.GetRequiredService<RaceDayDbContext>();
                db.Database.EnsureCreated();
            }

            return host;

        }//end of method

    }//end of class
}//end of namespace
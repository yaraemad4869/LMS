using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LearningManagementSystem;
using LearningManagementSystem.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
namespace tests.LMS.E2ETesting.TestHelpers
{
    public class TestApplicationFactory : WebApplicationFactory<Program>
    {
        protected override IHost CreateHost(IHostBuilder builder)
        {
            // Configure test services if needed
            builder.ConfigureServices(services =>
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                    options.EnableSensitiveDataLogging();
                });
            });

            return base.CreateHost(builder);
        }
    }
}

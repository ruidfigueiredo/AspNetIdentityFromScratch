using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

namespace AspNetIdentityFromScratch
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {            
            services.AddMvc();

            services.AddDbContext<IdentityDbContext>(options => 
                options.UseSqlite("Data Source=users.sqlite", 
                    optionsBuilder => optionsBuilder.MigrationsAssembly("AspNetIdentityFromScratch")));            
            services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<IdentityDbContext>()
                .AddDefaultTokenProviders();

            services.AddTransient<IMessageService, FileMessageService>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IdentityDbContext dbContext)
        {
            loggerFactory.AddConsole();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                dbContext.Database.Migrate();
            }

            app.UseIdentity(); 
            app.UseStaticFiles();           
            app.UseMvcWithDefaultRoute();
        }
    }
}

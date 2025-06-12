using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyBestBlog.Core.Entities;
using MyBestBlog.Infrastructure.Data;
using MyBestBlog.Services.Implementations;
using MyBestBlog.Services.Interfaces;

namespace MyBestBlog.Web
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            builder.Services.AddIdentity<User, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<BlogDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.AddScoped<IAuthService, AuthService>();

            builder.Services.AddDbContext<BlogDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
                db.Database.Migrate();
            }

            using (var scope = app.Services.CreateScope())
            {
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

                var roles = new[] { "User", "Moderator", "Admin" };
                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                        await roleManager.CreateAsync(new IdentityRole<Guid>(role));
                }

                var adminEmail = "admin@example.com";
                var adminPassword = "Admin123!";
                if (!await authService.EmailAlreadyExists(adminEmail))
                {
                    await authService.RegisterAsync(
                        email: adminEmail,
                        password: adminPassword,
                        role: "Admin");
                }

                var testUserEmail = "testUser@example.com";
                var testUserPassword = "TestUser123!";
                if (!await authService.EmailAlreadyExists(testUserEmail))
                {
                    await authService.RegisterAsync(
                        email: testUserEmail,
                        password: testUserPassword);
                    // роль тестовому пользователю вручную создавать не будем, она по умолчанию должна стать User
                }

                var moderatorEmail = "moderator@example.com";
                var moderatorPassword = "Moder123!";
                if (!await authService.EmailAlreadyExists(moderatorEmail))
                {
                    await authService.RegisterAsync(
                        email: moderatorEmail,
                        password: moderatorPassword,
                        role: "Moderator");
                }
            }

            app.Run();
        }
    }
}

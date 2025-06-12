using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyBestBlog.Core.Entities;
using MyBestBlog.Core.Interfaces;
using MyBestBlog.Infrastructure.Data;
using MyBestBlog.Infrastructure.Repositories;
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
            builder.Services.AddScoped<IArticleRepository, ArticleRepository>();
            builder.Services.AddScoped<ITagRepository, TagRepository>();
            builder.Services.AddScoped<IUserRepository, UserRepository>();

            builder.Services.AddDbContext<BlogDbContext>(options => { 
                options.UseSqlite("Data Source=myapp.db");
                options.UseSqlite(b => b.MigrationsAssembly("MyBestBlog.Web"));
            });

            var app = builder.Build();

            var logger = app.Services.GetRequiredService<ILogger<Program>>(); // полное логгирование 
            foreach (var svc in builder.Services)
            {
                logger.LogInformation($"Service: {svc.ServiceType.FullName}");
            }

            //using (var scope = app.Services.CreateScope()) // была проблема с ArticleRepository и ArticleController, проверял
            //{
            //    try
            //    {
            //        var repo = scope.ServiceProvider.GetRequiredService<IArticleRepository>();
            //        Console.WriteLine("ArticleRepository успешно разрешён!"); 
            //    }
            //    catch (Exception ex)
            //    {
            //        Console.WriteLine($"Ошибка: {ex.Message}");
            //    }
            //}

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

            app.MapControllerRoute(
                name: "auth",
                pattern: "auth/{action=Login}",
                defaults: new { controller = "Auth" });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
                db.Database.Migrate(); // реализованы миграции на случай дополнения БД
            }

            using (var scope = app.Services.CreateScope()) // доабвление тестовых пользователей Админ, Модератор и Тестовый Пользователь
            {
                var authService = scope.ServiceProvider.GetRequiredService<IAuthService>();
                var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

                var articleRepo = scope.ServiceProvider.GetRequiredService<IArticleRepository>();
                var tagRepo = scope.ServiceProvider.GetRequiredService<ITagRepository>();
                var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                var roles = new[] { "User", "Moderator", "Admin" }; // три роли
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
                    // роль тестовому пользователю должна будет стать User умолчанию, как и всем регистрирующимся
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

                if (!await articleRepo.AnyAsync())
                {
                    var admin = await userRepo.GetUserByUserEmailAsync(adminEmail);
                    var testTag = new Tag { Name = "ASP.NET Core", Description = "Статьи по ASP.NET" };

                    await tagRepo.AddAsync(testTag);

                    var testArticle = new Article
                    {
                        Title = "Первая тестовая статья",
                        Content = "Это содержимое тестовой статьи...",
                        AuthorId = admin.Id,
                        Tags = new List<ArticleTag> { new() { Tag = testTag } }
                    };

                    await articleRepo.AddAsync(testArticle);
                }
            }

            app.Run();
        }
    }
}

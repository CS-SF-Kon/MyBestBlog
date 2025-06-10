using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyBestBlog.Core.Entities;
using MyBestBlog.Infrastructure.Data;

namespace MyBestBlog.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();
            
            builder.Services.AddDbContext<BlogDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddIdentity<User, IdentityRole<Guid>>()
                .AddEntityFrameworkStores<BlogDbContext>()
                .AddDefaultTokenProviders();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<BlogDbContext>();
                db.Database.Migrate();
            }

            app.Run();
        }
    }
}

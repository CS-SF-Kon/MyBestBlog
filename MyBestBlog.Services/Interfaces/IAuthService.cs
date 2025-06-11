using Microsoft.AspNetCore.Identity;

namespace MyBestBlog.Services.Interfaces;

public interface IAuthService
{
    Task<IdentityResult> RegisterAsync(string email, string password);
    Task<bool> LoginAsync(string email, string password);
}

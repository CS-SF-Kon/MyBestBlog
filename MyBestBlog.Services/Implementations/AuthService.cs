using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Storage.Json;
using MyBestBlog.Core.Entities;
using MyBestBlog.Services.Interfaces;

namespace MyBestBlog.Services.Implementations;

public class AuthService : IAuthService
{
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;

    public AuthService(UserManager<User> userManager, SignInManager<User> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    public async Task<IdentityResult> RegisterAsync(string email, string password, string? role = null)
    {
        var user = new User { UserName = email, Email = email };
        var result = await _userManager.CreateAsync(user, password);

        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, role ?? "User");
        }

        return result;
    }

    public async Task<bool> LoginAsync(string email, string password)
    {
        var result = await _signInManager.PasswordSignInAsync(
            email, password, isPersistent: false, lockoutOnFailure: false);

        return result.Succeeded;
    }

    public async Task<bool> EmailAlreadyExists(string email)
    {
        if (await _userManager.FindByEmailAsync(email) == null)
        {
            return false;
        }
        return true;
    }
}

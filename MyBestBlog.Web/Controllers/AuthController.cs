using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyBestBlog.Core.Entities;
using MyBestBlog.Services.Interfaces;
using System.Security.Claims;

namespace MyBestBlog.Web.Controllers;

public class AuthController : Controller
{
    private readonly IAuthService _authService;
    private readonly UserManager<User> _userManager;
    private readonly SignInManager<User> _signInManager;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _userManager = userManager;
        _signInManager = signInManager;
        _logger = logger;
    }

    /// <summary>
    /// Авторизация - представление
    /// </summary>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        _logger.LogInformation("User trying to sign in");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    /// <summary>
    /// Авторизация - отработка POST
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <param name="returnUrl"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(
        string email,
        string password,
        string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (ModelState.IsValid)
        {
            var result = await _signInManager.PasswordSignInAsync(
                email, password, isPersistent: false, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                var user = await _userManager.FindByEmailAsync(email);
                await AddUserRolesToClaims(user);

                return LocalRedirect(returnUrl ?? "/");
            }

            ModelState.AddModelError(string.Empty, "Неверный email или пароль");
        }

        _logger.LogInformation("User just signed in");

        return View();
    }

    /// <summary>
    /// Регистрация - представление
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public IActionResult Register()
    {
        _logger.LogInformation("New user trying to sign up");

        return View();
    }

    /// <summary>
    /// Регистрация - отработка POST
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <param name="confirmPassword"></param>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(
        string email,
        string password,
        string confirmPassword)
    {
        if (password != confirmPassword)
        {
            ModelState.AddModelError("confirmPassword", "Пароли не совпадают");
            return View();
        }

        var result = await _authService.RegisterAsync(email, password);

        if (result.Succeeded)
        {
            await _signInManager.PasswordSignInAsync(
                email, password, isPersistent: false, lockoutOnFailure: false);

            return RedirectToAction("Index", "Home");
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }

        _logger.LogInformation("New user just signed up");

        return View();
    }

    /// <summary>
    /// Выход
    /// </summary>
    /// <returns></returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        _logger.LogInformation("User just signed out");

        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    private async Task AddUserRolesToClaims(User user)
    {
        var claims = new List<Claim>();
        var roles = await _userManager.GetRolesAsync(user);

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        await _userManager.AddClaimsAsync(user, claims);
    }
}
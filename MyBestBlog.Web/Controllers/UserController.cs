using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyBestBlog.Core.Entities;
using MyBestBlog.Core.Interfaces;
using MyBestBlog.Web.Models;
using System.Security.Claims;

namespace MyBestBlog.Web.Controllers;

public class UserController : Controller
{
    private readonly IUserRepository _userRepo;
    private readonly IArticleRepository _articleRepo;
    private readonly UserManager<User> _userManager;

    public UserController(
        IUserRepository userRepo,
        IArticleRepository articleRepo,
        UserManager<User> userManager)
    {
        _userRepo = userRepo;
        _articleRepo = articleRepo;
        _userManager = userManager;
    }
    [HttpGet]
    public async Task<IActionResult> Profile(Guid id)
    {
        var user = await _userRepo.GetUserByUserIdAsync(id);
        if (user == null) return NotFound();

        var currentUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(currentUserIdClaim, out var currentUserId))
        {
            return Forbid(); // или другая обработка ошибки
        }
        var isAdmin = User.IsInRole("Admin");

        var articles = (await _articleRepo.GetArticlesByUserIdAsync(id))
            .Select(a => new UserArticleViewModel
            {
                Id = a.Id,
                Title = a.Title,
                Tags = a.Tags.Select(t => t.Tag.Name).ToList()
            }).ToList();

        var model = new UserProfileViewModel
        {
            UserId = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault(),
            RegistrationDate = user.RegistrationDate,
            Articles = articles,
            CanEdit = id == currentUserId || isAdmin
        };

        return View(model);
    }
}

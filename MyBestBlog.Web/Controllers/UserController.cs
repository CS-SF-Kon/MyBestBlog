using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
    private readonly ILogger<UserController> _logger;

    public UserController(
        IUserRepository userRepo,
        IArticleRepository articleRepo,
        UserManager<User> userManager,
        ILogger<UserController> logger)
    {
        _userRepo = userRepo;
        _articleRepo = articleRepo;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Просмотр профиля пользователя
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Profile(Guid id)
    {
        _logger.LogInformation("User views a profile");

        var user = await _userRepo.GetUserByUserIdAsync(id);
        if (user == null) return View("../Error/Forbidden");

        var currentUserIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(currentUserIdClaim, out var currentUserId))
        {
            return View("../Error/Forbidden");
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

    /// <summary>
    /// Редактирование профиля пользователя - представление
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(Guid id)
    {
        var user = await _userRepo.GetUserByUserIdAsync(id);
        if (user == null) return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 404 });

        var currentUserId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        if (id != currentUserId && !isAdmin)
        {
            _logger.LogInformation("Unauthorised user just tried to edit user profile");

            return View("../Error/Forbidden");
        }

        _logger.LogInformation("Authorised user trying to edit user profile");

        var model = new UserEditViewModel
        {
            Id = user.Id,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = (await _userManager.GetRolesAsync(user)).FirstOrDefault()
        };

        if (isAdmin)
        {
            model.AvailableRoles = new List<SelectListItem>
        {
            new("Пользователь", "User"),
            new("Модератор", "Moderator"),
            new("Администратор", "Admin")
        };
        }

        return View(model);
    }

    /// <summary>
    /// Редактирование профиля пользователя - отработка POST
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserEditViewModel model)
    {
        if (!ModelState.IsValid)
        {
            if (User.IsInRole("Admin"))
            {
                model.AvailableRoles = GetAvailableRoles();
            }
            return View(model);
        }

        var user = await _userRepo.GetUserByUserIdAsync(model.Id);
        if (user == null) return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 404 });

        var currentUserId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        if (model.Id != currentUserId && !isAdmin)
        {
            return View("../Error/Forbidden");
        }

        user.DisplayName = model.DisplayName;
        user.Email = model.Email;
        user.UserName = model.Email;

        if (!string.IsNullOrEmpty(model.NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return View(model);
            }
        }

        if (isAdmin && !string.IsNullOrEmpty(model.Role)) // смена роли, доступная только админу
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.Role);
        }

        _logger.LogInformation("Authorised user just edited user profile");

        await _userManager.UpdateAsync(user);
        return RedirectToAction("Profile", new { id = model.Id });
    }

    /// <summary>
    /// Достпуные для блога роли (да, захардкодено)
    /// </summary>
    /// <returns></returns>
    private List<SelectListItem> GetAvailableRoles()
    {
        return new List<SelectListItem>
        {
            new("Пользователь", "User"),
            new("Модератор", "Moderator"),
            new("Администратор", "Admin")
        };
    }

    /// <summary>
    /// Получение Id пользователя текущей сессии - для создания статей, комментариев, проверки на авторство на странице статьи (дубилруется с таким же методом в UserController)
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
    protected Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        throw new UnauthorizedAccessException("User ID is invalid or not found");
    }
}

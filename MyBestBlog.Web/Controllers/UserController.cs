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

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> Edit(Guid id)
    {
        var user = await _userRepo.GetUserByUserIdAsync(id);
        if (user == null) return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 404 });

        var currentUserId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        // Проверка прав
        if (id != currentUserId && !isAdmin)
        {
            return View("../Error/Forbidden");
        }

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

        // Проверка прав
        if (model.Id != currentUserId && !isAdmin)
        {
            return View("../Error/Forbidden");
        }

        // Обновляем основные данные
        user.DisplayName = model.DisplayName;
        user.Email = model.Email;
        user.UserName = model.Email; // Для Identity userName обычно совпадает с email

        // Обновляем пароль, если указан
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

        if (isAdmin && !string.IsNullOrEmpty(model.Role))
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, model.Role);
        }

        await _userManager.UpdateAsync(user);
        return RedirectToAction("Profile", new { id = model.Id });
    }

    private List<SelectListItem> GetAvailableRoles()
    {
        return new List<SelectListItem>
        {
            new("Пользователь", "User"),
            new("Модератор", "Moderator"),
            new("Администратор", "Admin")
        };
    }

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

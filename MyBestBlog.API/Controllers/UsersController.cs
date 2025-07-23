using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBestBlog.API.DTO;
using MyBestBlog.Core.Entities;
using MyBestBlog.Core.Interfaces;
using System.Security.Claims;

namespace MyBestBlog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepo;
    private readonly IArticleRepository _articleRepo;
    private readonly UserManager<User> _userManager;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IUserRepository userRepo,
        IArticleRepository articleRepo,
        UserManager<User> userManager,
        ILogger<UsersController> logger)
    {
        _userRepo = userRepo;
        _articleRepo = articleRepo;
        _userManager = userManager;
        _logger = logger;
    }

    // GET: api/users
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        _logger.LogInformation("Getting all users");

        var users = await _userManager.Users
            .Include(u => u.Articles)
            .ThenInclude(a => a.Tags)
            .ThenInclude(at => at.Tag)
            .ToListAsync();

        var result = new List<UserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            result.Add(new UserDto
            {
                Id = user.Id,
                UserName = user.UserName,
                DisplayName = user.DisplayName,
                Role = roles.FirstOrDefault() ?? "User",
                RegistrationDate = user.RegistrationDate,
                Articles = user.Articles?.Select(a => new UserArticleDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Tags = a.Tags?.Select(t => t.Tag.Name).ToList() ?? new List<string>()
                }).ToList() ?? new List<UserArticleDto>()
            });
        }

        return Ok(result);
    }

    // GET: api/users/{id}
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<UserDetailsDto>> GetUserByUserId(Guid id)
    {
        _logger.LogInformation("Getting user profile for {UserId}", id);

        var user = await _userManager.Users
            .Include(u => u.Articles)
            .ThenInclude(a => a.Tags)
            .ThenInclude(at => at.Tag)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null) return NotFound();

        var currentUserId = User.Identity?.IsAuthenticated == true
            ? GetCurrentUserId()
            : Guid.Empty;

        var roles = await _userManager.GetRolesAsync(user);
        var articles = await _articleRepo.GetArticlesByUserIdAsync(id);

        return Ok(new UserDetailsDto
        {
            Id = user.Id,
            UserName = user.UserName,
            DisplayName = user.DisplayName,
            Email = user.Email,
            Role = roles.FirstOrDefault() ?? "User",
            RegistrationDate = user.RegistrationDate,
            Articles = articles.Select(a => new UserArticleDto
            {
                Id = a.Id,
                Title = a.Title,
                Tags = a.Tags.Select(t => t.Tag.Name).ToList()
            }).ToList(),
            CanEdit = id == currentUserId || User.IsInRole("Admin")
        });
    }

    // PUT: api/users/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(id.ToString());
        if (user == null) return NotFound();

        var currentUserId = GetCurrentUserId();
        var isAdmin = User.IsInRole("Admin");

        if (id != currentUserId && !isAdmin)
        {
            _logger.LogWarning("Unauthorized update attempt by user {UserId}", currentUserId);
            return Forbid();
        }

        // Обновляем основные данные
        if (!string.IsNullOrEmpty(dto.DisplayName))
            user.DisplayName = dto.DisplayName;

        if (!string.IsNullOrEmpty(dto.Email))
        {
            user.Email = dto.Email;
            user.UserName = dto.Email;
        }

        // Обновление пароля
        if (!string.IsNullOrEmpty(dto.NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
                return BadRequest(ModelState);
            }
        }

        // Обновление роли (только для админа)
        if (isAdmin && !string.IsNullOrEmpty(dto.Role))
        {
            var currentRoles = await _userManager.GetRolesAsync(user);
            await _userManager.RemoveFromRolesAsync(user, currentRoles);
            await _userManager.AddToRoleAsync(user, dto.Role);
        }

        await _userManager.UpdateAsync(user);

        _logger.LogInformation("User {UserId} updated by {CurrentUserId}", id, currentUserId);

        return NoContent();
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("User ID is invalid or not found");
    }
}

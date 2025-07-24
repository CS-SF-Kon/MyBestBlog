using System.ComponentModel.DataAnnotations;

namespace MyBestBlog.API.DTO;

/// <summary>
/// Краткая информация о пользователе
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string Role { get; set; } = "User";
    public DateTime RegistrationDate { get; set; }
    public List<UserArticleDto> Articles { get; set; } = new();
}

/// <summary>
/// Полная информация о пользователе
/// </summary>
public class UserDetailsDto : UserDto
{
    public string Email { get; set; } = string.Empty;
    public bool CanEdit { get; set; }
}

/// <summary>
/// Модель для редактирования данных о пользователе
/// </summary>
public class UpdateUserDto
{
    [StringLength(50, ErrorMessage = "Не более 50 символов")]
    public string? DisplayName { get; set; }

    [EmailAddress(ErrorMessage = "Некорректный email")]
    public string? Email { get; set; }

    [StringLength(100, MinimumLength = 6, ErrorMessage = "Пароль должен быть от {2} до {1} символов")]
    public string? NewPassword { get; set; }

    [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
    public string? ConfirmPassword { get; set; }

    // По идее должно быть доступно только для админов
    public string? Role { get; set; }
}

/// <summary>
/// Краткая информация о статьях пользователя
/// </summary>
public class UserArticleDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public List<string> Tags { get; set; } = new();
}

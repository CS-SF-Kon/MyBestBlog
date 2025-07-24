using System.ComponentModel.DataAnnotations;

namespace MyBestBlog.API.DTO;

/// <summary>
/// Модель для регистрации
/// </summary>
public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string DisplayName { get; set; } = string.Empty;
}

/// <summary>
/// Модель для авторизации
/// </summary>
public class LoginDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

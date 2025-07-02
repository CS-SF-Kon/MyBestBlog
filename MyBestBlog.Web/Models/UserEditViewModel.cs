using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace MyBestBlog.Web.Models;

public class UserEditViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Имя обязательно")]
    [StringLength(50, ErrorMessage = "Не более 50 символов")]
    public string DisplayName { get; set; }

    [Required(ErrorMessage = "Email обязателен")]
    [EmailAddress(ErrorMessage = "Некорректный email")]
    public string Email { get; set; }

    [DataType(DataType.Password)]
    [StringLength(100, ErrorMessage = "Пароль должен быть от {2} до {1} символов", MinimumLength = 6)]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare("NewPassword", ErrorMessage = "Пароли не совпадают")]
    public string? ConfirmPassword { get; set; }

    // Только для админа
    public string? Role { get; set; }
    public List<SelectListItem>? AvailableRoles { get; set; }
}

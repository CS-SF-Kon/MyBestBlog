using Microsoft.AspNetCore.Identity;

namespace MyBestBlog.Core.Entities;

/// <summary>
/// класс Пользователя Блога
/// </summary>
public class User : IdentityUser<Guid>
{
    public string? DisplayName { get; set; }
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;
    public ICollection<Article> Articles { get; set; } = new List<Article>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}

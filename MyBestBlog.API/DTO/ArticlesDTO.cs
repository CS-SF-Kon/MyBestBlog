using System.ComponentModel.DataAnnotations;

namespace MyBestBlog.API.DTO;

/// <summary>
/// Краткая информация о статье
/// </summary>
public class ArticleDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ContentPreview { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public AuthorDto Author { get; set; } = null!;
    public List<TagDto> Tags { get; set; } = new();
}

/// <summary>
/// Полная информация о статье
/// </summary>
public class ArticleDetailsDto : ArticleDto
{
    public string Content { get; set; } = string.Empty;
    public List<CommentDto> Comments { get; set; } = new();
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

/// <summary>
/// Модель для создания статьи (с валидацией)
/// </summary>
public class CreateArticleDto
{
    [Required(ErrorMessage = "Заголовок обязателен")]
    [StringLength(100, ErrorMessage = "Не более 100 символов")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Содержание обязательно")]
    public string Content { get; set; } = string.Empty;

    public List<Guid>? TagIds { get; set; } // не понимаю почему, но в шаблоне json почему-то автоматически добавляется нигде не существующий Guid в лист тегов
}

/// <summary>
/// Модель для изменения статьи
/// </summary>
public class UpdateArticleDto
{
    [Required(ErrorMessage = "Заголовок обязателен")]
    [StringLength(100, ErrorMessage = "Не более 100 символов")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Содержание обязательно")]
    public string Content { get; set; } = string.Empty;

    public List<Guid>? TagIds { get; set; }
}

/// <summary>
/// Модель для отображения комментария
/// </summary>
public class CommentDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public AuthorDto Author { get; set; } = null!;
}

/// <summary>
/// Модель для создания комментария
/// </summary>
public class AddCommentDto
{
    [Required(ErrorMessage = "Текст комментария обязателен")]
    [StringLength(1000, ErrorMessage = "Не более 1000 символов")]
    public string Text { get; set; } = string.Empty;
}

/// <summary>
/// Модель для автора
/// </summary>
public class AuthorDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}
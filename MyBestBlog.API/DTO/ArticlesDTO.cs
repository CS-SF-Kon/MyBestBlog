using System.ComponentModel.DataAnnotations;

namespace MyBestBlog.API.DTO;

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

public class ArticleDetailsDto : ArticleDto
{
    public string Content { get; set; } = string.Empty;
    public List<CommentDto> Comments { get; set; } = new();
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}

public class CreateArticleDto
{
    [Required(ErrorMessage = "Заголовок обязателен")]
    [StringLength(100, ErrorMessage = "Не более 100 символов")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Содержание обязательно")]
    public string Content { get; set; } = string.Empty;

    public List<Guid>? TagIds { get; set; }
}

public class UpdateArticleDto
{
    [Required(ErrorMessage = "Заголовок обязателен")]
    [StringLength(100, ErrorMessage = "Не более 100 символов")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Содержание обязательно")]
    public string Content { get; set; } = string.Empty;

    public List<Guid>? TagIds { get; set; }
}

public class CommentDto
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public AuthorDto Author { get; set; } = null!;
}

public class AddCommentDto
{
    [Required(ErrorMessage = "Текст комментария обязателен")]
    [StringLength(1000, ErrorMessage = "Не более 1000 символов")]
    public string Text { get; set; } = string.Empty;
}

public class TagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class AuthorDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}
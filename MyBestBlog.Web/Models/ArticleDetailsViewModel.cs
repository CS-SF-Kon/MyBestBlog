using MyBestBlog.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace MyBestBlog.Web.Models;

/// <summary>
/// Модель для просмотра Статьи
/// </summary>
public class ArticleDetailsViewModel
{
    public Article Article { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public List<CommentViewModel> Comments { get; set; } = new();
    public AddCommentViewModel NewComment { get; set; } = new();
}

/// <summary>
/// Модель для загрузки Комментариев к Статье
/// </summary>
public class CommentViewModel
{
    public Guid Id { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
    public string AuthorName { get; set; }
    public Guid AuthorId { get; set; }
}

/// <summary>
/// Модель для добавления комментария
/// </summary>
public class AddCommentViewModel
{
    [Required]
    public Guid ArticleId { get; set; }

    //[Required(ErrorMessage = "Комментарий не может быть пустым")] - не работает
    //[StringLength(1000, ErrorMessage = "Комментарий не должен превышать 1000 символов")] - не работает
    public string Text { get; set; }
}
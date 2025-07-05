using MyBestBlog.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace MyBestBlog.Web.Models;

public class ArticleDetailsViewModel
{
    public Article Article { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
    public List<CommentViewModel> Comments { get; set; } = new();
    public AddCommentViewModel NewComment { get; set; } = new();
}

public class CommentViewModel
{
    public Guid Id { get; set; }
    public string Text { get; set; }
    public DateTime CreatedAt { get; set; }
    public string AuthorName { get; set; }
    public Guid AuthorId { get; set; }
}

public class AddCommentViewModel
{
    [Required]
    public Guid ArticleId { get; set; }

    [Required(ErrorMessage = "Комментарий не может быть пустым")]
    [StringLength(1000, ErrorMessage = "Комментарий не должен превышать 1000 символов")]
    public string Text { get; set; }
}
namespace MyBestBlog.Core.Entities;

/// <summary>
/// Comments for articles class
/// </summary>
public class Comment
{
    public Guid Id { get; set; }
    public string Text { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Guid ArticleId { get; set; }
    public Article Article { get; set; } = null!;

    public Guid AuthorId { get; set; }
    public User Author { get; set; } = null!;
}

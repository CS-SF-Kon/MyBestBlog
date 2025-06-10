namespace MyBestBlog.Core.Entities;

/// <summary>
/// Tags for articles class
/// </summary>
public class Tag
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public ICollection<ArticleTag> Articles { get; set; } = new List<ArticleTag>();
}

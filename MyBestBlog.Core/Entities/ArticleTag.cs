namespace MyBestBlog.Core.Entities;

/// <summary>
/// класс для связывания Статей и Тегов
/// </summary>
public class ArticleTag
{
    public Guid ArticleId { get; set; }
    public Article Article { get; set; } = null!;

    public Guid TagId { get; set; }
    public Tag Tag { get; set; } = null!;
}

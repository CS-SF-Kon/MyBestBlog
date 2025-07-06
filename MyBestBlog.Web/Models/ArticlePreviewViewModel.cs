namespace MyBestBlog.Web.Models;

/// <summary>
/// Модель для предпросмотра Статьи на главной странице или при поиске по Тегу
/// </summary>
public class ArticlePreviewViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public string PreviewContent { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<TagInfo> Tags { get; set; }
    public AuthorInfo Author { get; set; }

    public class TagInfo
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }

    public class AuthorInfo
    {
        public Guid Id { get; set; }
        public string UserName { get; set; }
    }
}

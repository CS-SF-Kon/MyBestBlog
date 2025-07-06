using MyBestBlog.Core.Entities;

namespace MyBestBlog.Web.Models;

/// <summary>
/// Модель для просмотра Тегов
/// </summary>
public class TagListViewModel
{
    public IEnumerable<Tag> Tags { get; set; }
    public bool CanCreate { get; set; }
}

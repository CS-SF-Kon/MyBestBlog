using System.ComponentModel.DataAnnotations;

namespace MyBestBlog.Web.Models;

public class ArticleCreateViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Заголовок обязателен")]
    [StringLength(100, ErrorMessage = "Не более 100 символов")]
    public string Title { get; set; }

    [Required(ErrorMessage = "Содержание обязательно")]
    public string Content { get; set; }

    public List<Guid>? SelectedTagIds { get; set; }
    public List<TagViewModel> AvailableTags { get; set; } = new();
}

public class TagViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}

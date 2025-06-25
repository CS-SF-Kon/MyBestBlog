using System.ComponentModel.DataAnnotations;

namespace MyBestBlog.Web.Models;

public class TagCreateViewModel
{
    public Guid Id { get; set; }

    [Required(ErrorMessage = "Название тега обязательно")]
    [StringLength(50, ErrorMessage = "Не более 50 символов")]
    public string Name { get; set; }

    [StringLength(200, ErrorMessage = "Не более 200 символов")]
    public string? Description { get; set; }
}

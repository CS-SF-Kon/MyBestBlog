using System.ComponentModel.DataAnnotations;

namespace MyBestBlog.API.DTO;

/// <summary>
/// Полная информация о теге
/// </summary>
public class TagDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
}

/// <summary>
/// Кратная информация о теге
/// </summary>
public class TagListDto
{
    public IEnumerable<TagDto> Tags { get; set; } = new List<TagDto>();
    public bool CanCreate { get; set; }
}

/// <summary>
/// Модель для создания и редактирования тега
/// </summary>
public class CreateUpdateTagDto
{
    [Required(ErrorMessage = "Название тега обязательно")]
    [StringLength(50, ErrorMessage = "Не более 50 символов")]
    public string Name { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "Не более 200 символов")]
    public string? Description { get; set; }
}

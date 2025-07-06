namespace MyBestBlog.Web.Models;

/// <summary>
/// Модель для просмотра перечня Пользователей
/// </summary>
public class UserListViewModel
{
    public Guid Id { get; set; }
    public string UserName { get; set; }
    public string Role { get; set; }
    public List<UserArticleViewModel> Articles { get; set; }

}
namespace MyBestBlog.Web.Models;

public class UserProfileViewModel
{
    public Guid UserId { get; set; }
    public string UserName { get; set; }
    public string DisplayName { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public DateTime RegistrationDate { get; set; }
    public List<UserArticleViewModel> Articles { get; set; }
    public bool CanEdit { get; set; }
}

public class UserArticleViewModel
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public List<string> Tags { get; set; }
}
using MyBestBlog.Core.Entities;

namespace MyBestBlog.Web.Models;

public class ArticleDetailsViewModel
{
    public Article Article { get; set; }
    public bool CanEdit { get; set; }
    public bool CanDelete { get; set; }
}
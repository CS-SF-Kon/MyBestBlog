using MyBestBlog.Core.Entities;

namespace MyBestBlog.Web.Models;

public class TagListViewModel
{
    public IEnumerable<Tag> Tags { get; set; }
    public bool CanCreate { get; set; }
}

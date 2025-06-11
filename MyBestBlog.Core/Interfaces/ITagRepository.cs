using MyBestBlog.Core.Entities;

namespace MyBestBlog.Core.Interfaces;

public interface ITagRepository: IRepository<Tag>
{
    /// <summary>
    /// get Tag by it's Id
    /// </summary>
    /// <param name="tagId"></param>
    /// <returns></returns>
    Task<Tag?> GetTagByTagIdAsync(Guid tagId);

    /// <summary>
    /// get all Tags
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<Tag?>> GetAllTagsAsync();
}

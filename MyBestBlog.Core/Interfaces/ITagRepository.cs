using MyBestBlog.Core.Entities;

namespace MyBestBlog.Core.Interfaces;

public interface ITagRepository: IRepository<Tag>
{
    /// <summary>
    /// получить Тег по его Id
    /// </summary>
    /// <param name="tagId"></param>
    /// <returns></returns>
    Task<Tag?> GetTagByTagIdAsync(Guid tagId);

    /// <summary>
    /// получить все Теги
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<Tag?>> GetAllTagsAsync();
}

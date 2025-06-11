using MyBestBlog.Core.Entities;

namespace MyBestBlog.Core.Interfaces;

public interface IArticleRepositiry : IRepository<Article>
{
    /// <summary>
    /// get all Articles
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<Article>> GetAllArticlesAsync();

    /// <summary>
    /// get list of Articles by Author's Id
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<IEnumerable<Article>> GetArticlesByUserIdAsync(Guid userId);
}

using MyBestBlog.Core.Entities;

namespace MyBestBlog.Core.Interfaces;

public interface IArticleRepository : IRepository<Article>
{
    /// <summary>
    /// получить все Статьи
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<Article>> GetAllArticlesAsync();

    /// <summary>
    /// получить статью (статьи) по Id Пользователя
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<IEnumerable<Article>> GetArticlesByUserIdAsync(Guid userId);

    /// <summary>
    /// получить Статью по её Id
    /// </summary>
    /// <param name="articleId"></param>
    /// <returns></returns>
    Task<Article?> GetArticleByArticleIdAsync(Guid articleId, bool includeAuthor = false, bool includeTags = false);

    /// <summary>
    /// проверка если статей нет (для создания тестовой статьи при первом запуске проекта)
    /// </summary>
    /// <returns></returns>
    Task<bool> AnyAsync();
}

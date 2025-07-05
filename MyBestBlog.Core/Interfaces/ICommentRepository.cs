using MyBestBlog.Core.Entities;

namespace MyBestBlog.Core.Interfaces;

public interface ICommentRepository : IRepository<Comment>
{
    /// <summary>
    /// получить все Комментарии
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<Comment>> GetAllCommentsAsync();

    /// <summary>
    /// получить Комментарий по его Id
    /// </summary>
    /// <param name="commentId"></param>
    /// <returns></returns>
    Task<Comment?> GetCommentByCommentIdAsync(Guid commentId);

    /// <summary>
    /// получить все Комментарии по Id Статьи
    /// </summary>
    /// <param name="articleId"></param>
    /// <returns></returns>
    Task<IEnumerable<Comment>> GetCommentsForArticleAsync(Guid articleId);
}

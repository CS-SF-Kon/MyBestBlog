using MyBestBlog.Core.Entities;

namespace MyBestBlog.Core.Interfaces;

public interface ICommentRepository : IRepository<Comment>
{
    /// <summary>
    /// get all Comments
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<Comment>> GetAllCommentsAsync();

    /// <summary>
    /// get Comment by it's Id
    /// </summary>
    /// <param name="commentId"></param>
    /// <returns></returns>
    Task<Comment?> GetCommentByCommentIdAsync(Guid commentId);
}

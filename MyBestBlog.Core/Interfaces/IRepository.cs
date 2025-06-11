namespace MyBestBlog.Core.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// create (register) User, Article, Comment, Tag
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task AddAsync(TEntity entity);

    /// <summary>
    /// edit User, Article, Comment, Tag
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task UpdateAsync(TEntity entity);

    /// <summary>
    /// delete User, Article, Comment, Tag
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task DeleteAsync(TEntity entity);
}

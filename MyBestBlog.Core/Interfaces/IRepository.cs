namespace MyBestBlog.Core.Interfaces;

public interface IRepository<TEntity> where TEntity : class
{
    /// <summary>
    /// создать (зарегистрировать) Пользователя, Статью, Комментарий, Тег
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task AddAsync(TEntity entity);

    /// <summary>
    /// изменить Пользователя, Статью, Комментарий, Тег
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task UpdateAsync(TEntity entity);

    /// <summary>
    /// удалить Пользователя, Статью, Комментарий, Тег
    /// </summary>
    /// <param name="entity"></param>
    /// <returns></returns>
    Task DeleteAsync(TEntity entity);
}

using MyBestBlog.Core.Entities;

namespace MyBestBlog.Core.Interfaces;

public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// получить всех Пользователей
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<User>> GetAllUsersAsync();

    /// <summary>
    /// получить Пользователя по его Id
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<User?> GetUserByUserIdAsync(Guid userId);

    /// <summary>
    /// получить пользователя по его Email
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<User?> GetUserByUserEmailAsync(string email);
}

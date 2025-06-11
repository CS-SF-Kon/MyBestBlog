using MyBestBlog.Core.Entities;

namespace MyBestBlog.Core.Interfaces;

public interface IUserRepository : IRepository<User>
{
    /// <summary>
    /// get all Users
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<User>> GetAllUsersAsync();

    /// <summary>
    /// get User by his Id
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<User?> GetUserByUserIdAsync(Guid userId);
}

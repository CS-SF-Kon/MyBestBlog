using Microsoft.AspNetCore.Identity;

namespace MyBestBlog.Services.Interfaces;

public interface IAuthService
{
    /// <summary>
    /// регистрация нового Пользователя
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <param name="role"></param>
    /// <returns></returns>
    Task<IdentityResult> RegisterAsync(string email, string password, string? role = null);

    /// <summary>
    /// авторизация Пользователя
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    Task<bool> LoginAsync(string email, string password);

    /// <summary>
    /// проверка на существование по Email
    /// </summary>
    /// <param name="email"></param>
    /// <returns></returns>
    Task<bool> EmailAlreadyExists(string email);
}

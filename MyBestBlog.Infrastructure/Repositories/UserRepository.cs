using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyBestBlog.Core.Entities;
using MyBestBlog.Core.Interfaces;
using MyBestBlog.Infrastructure.Data;

namespace MyBestBlog.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    private readonly UserManager<User> _userManager;
    public UserRepository(BlogDbContext context, UserManager<User> userManager) : base(context)
    {
        _userManager = userManager;
    }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _context.Set<User>().ToListAsync();
    }

    public async Task<User?> GetUserByUserEmailAsync(string email)
    {
        return await _userManager.FindByEmailAsync(email);
    }

    public async Task<User?> GetUserByUserIdAsync(Guid userId)
    {
        return await _context.Set<User>().FindAsync(userId);
    }
}

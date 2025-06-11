using Microsoft.EntityFrameworkCore;
using MyBestBlog.Core.Entities;
using MyBestBlog.Core.Interfaces;
using MyBestBlog.Infrastructure.Data;

namespace MyBestBlog.Infrastructure.Repositories;

public class UserRepository : BaseRepository<User>, IUserRepository
{
    public UserRepository(BlogDbContext context) : base(context) { }

    public async Task<IEnumerable<User>> GetAllUsersAsync()
    {
        return await _context.Set<User>().ToListAsync();
    }

    public async Task<User?> GetUserByUserIdAsync(Guid userId)
    {
        return await _context.Set<User>().FindAsync(userId);
    }
}

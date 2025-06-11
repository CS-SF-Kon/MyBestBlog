using Microsoft.EntityFrameworkCore;
using MyBestBlog.Core.Entities;
using MyBestBlog.Core.Interfaces;
using MyBestBlog.Infrastructure.Data;

namespace MyBestBlog.Infrastructure.Repositories;

public class CommentRepository : BaseRepository<Comment>, ICommentRepository
{
    public CommentRepository(BlogDbContext context) : base(context) { }

    public async Task<IEnumerable<Comment>> GetAllCommentsAsync()
    {
        return await _context.Set<Comment>().ToListAsync();
    }

    public async Task<Comment?> GetCommentByCommentIdAsync(Guid commentId)
    {
        return await _context.Set<Comment>().FindAsync(commentId);
    }
}

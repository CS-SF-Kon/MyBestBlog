using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using MyBestBlog.Core.Entities;
using MyBestBlog.Core.Interfaces;
using MyBestBlog.Infrastructure.Data;

namespace MyBestBlog.Infrastructure.Repositories;

public class CommentRepository : BaseRepository<Comment>, ICommentRepository
{
    private readonly ILogger<CommentRepository> _logger;

    public CommentRepository(BlogDbContext context, ILogger<CommentRepository> logger) : base(context)
    {
        _logger = logger;
    }

    public async Task<IEnumerable<Comment>> GetAllCommentsAsync()
    {
        return await _context.Set<Comment>().ToListAsync();
    }

    public async Task<Comment?> GetCommentByCommentIdAsync(Guid commentId)
    {
        return await _context.Set<Comment>().FindAsync(commentId);
    }

    public async Task<IEnumerable<Comment>> GetCommentsForArticleAsync(Guid articleId)
    {
        return await _context.Comments
            .Where(c => c.ArticleId == articleId)
            .Include(c => c.Author)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public override async Task AddAsync(Comment entity)
    {
        try
        {
            _logger.LogDebug("Добавление комментария в БД: {@Comment}", entity);
            await _context.Set<Comment>().AddAsync(entity);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Комментарий сохранен в БД. ID: {Id}", entity.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка сохранения комментария");
            throw;
        }
    }
}

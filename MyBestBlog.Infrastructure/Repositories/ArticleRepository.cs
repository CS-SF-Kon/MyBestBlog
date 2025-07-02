using Microsoft.EntityFrameworkCore;
using MyBestBlog.Core.Entities;
using MyBestBlog.Core.Interfaces;
using MyBestBlog.Infrastructure.Data;

namespace MyBestBlog.Infrastructure.Repositories;

public class ArticleRepository : BaseRepository<Article>, IArticleRepository
{
    public ArticleRepository(BlogDbContext context) : base(context) { }

    public async Task<bool> AnyAsync()
    {
        return await _context.Articles.AnyAsync();
    }

    public async Task<IEnumerable<Article>> GetAllArticlesAsync()
    {
        return await _context.Set<Article>().ToListAsync();
    }

    public async Task<Article?> GetArticleByArticleIdAsync(Guid articleId, bool includeAuthor = false, bool includeTags = false)
    {
        var query = _context.Articles.AsQueryable();

        if (includeAuthor)
        {
            query = query.Include(a => a.Author);
        }

        if (includeTags)
        {
            query = query.Include(a => a.Tags)
                        .ThenInclude(at => at.Tag);
        }

        return await query.FirstOrDefaultAsync(a => a.Id == articleId);
    }

    public async Task<IEnumerable<Article>> GetArticlesByUserIdAsync(Guid userId)
    {
        return await _context.Articles
            .Where(article => article.AuthorId == userId)
            .OrderByDescending(article => article.CreatedAt)
            .ToListAsync();
    }
}

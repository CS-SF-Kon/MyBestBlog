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

    public async Task<Article?> GetArticleByArticleIdAsync(Guid articleId)
    {
        return await _context.Set<Article>().FindAsync(articleId);
    }

    public async Task<IEnumerable<Article>> GetArticlesByUserIdAsync(Guid userId)
    {
        return await _context.Articles
            .Where(article => article.AuthorId == userId)
            .OrderByDescending(article => article.CreatedAt)
            .ToListAsync();
    }
}

using Microsoft.EntityFrameworkCore;
using MyBestBlog.Core.Entities;
using MyBestBlog.Core.Interfaces;
using MyBestBlog.Infrastructure.Data;

namespace MyBestBlog.Infrastructure.Repositories;

public class ArticleRepostory : BaseRepository<Article>, IArticleRepositiry
{
    public ArticleRepostory(BlogDbContext context) : base(context) { }

    public async Task<IEnumerable<Article>> GetAllArticlesAsync()
    {
        return await _context.Set<Article>().ToListAsync();
    }

    public async Task<IEnumerable<Article>> GetArticlesByUserIdAsync(Guid userId)
    {
        return await _context.Articles
            .Where(article => article.AuthorId == userId)
            .OrderByDescending(article => article.CreatedAt)
            .ToListAsync();
    }
}

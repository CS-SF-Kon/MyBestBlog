using Microsoft.EntityFrameworkCore;
using MyBestBlog.Core.Entities;
using MyBestBlog.Core.Interfaces;
using MyBestBlog.Infrastructure.Data;

namespace MyBestBlog.Infrastructure.Repositories;

public class TagRepository : BaseRepository<Tag>, ITagRepository
{
    public TagRepository(BlogDbContext context) : base(context) { }
    public async Task<IEnumerable<Tag?>> GetAllTagsAsync()
    {
        return await _context.Tags
            .Include(t => t.Articles)
            .ThenInclude(at => at.Article)
            .ToListAsync();
    }

    public async Task<Tag?> GetTagByTagIdAsync(Guid tagId)
    {
        return await _context.Set<Tag>().FindAsync(tagId);
    }
}

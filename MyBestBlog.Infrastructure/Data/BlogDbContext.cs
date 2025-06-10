using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyBestBlog.Core.Entities;

namespace MyBestBlog.Infrastructure.Data;

public class BlogDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
{
    public BlogDbContext(DbContextOptions<BlogDbContext> options) : base(options) { }

    public DbSet<Article> Articles => Set<Article>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<ArticleTag> ArticleTags => Set<ArticleTag>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<User>()
        .HasMany(u => u.Articles)
        .WithOne(a => a.Author)
        .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<User>()
            .HasMany(u => u.Comments)
            .WithOne(c => c.Author)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Article>()
            .HasMany(a => a.Comments)
            .WithOne(c => c.Article)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Article>()
            .HasMany(a => a.Tags)
            .WithOne(t => t.Article)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Comment>()
            .HasOne(c => c.Article)
            .WithMany(a => a.Comments)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<ArticleTag>().HasKey(at => new { at.ArticleId, at.TagId });
    }
}

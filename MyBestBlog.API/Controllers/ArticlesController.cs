using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBestBlog.API.DTO;
using MyBestBlog.Core.Entities;
using MyBestBlog.Core.Interfaces;
using System.Security.Claims;

namespace MyBestBlog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ArticlesController : ControllerBase
{
    private readonly IArticleRepository _articleRepo;
    private readonly ITagRepository _tagRepo;
    private readonly ICommentRepository _commentRepo;
    private readonly ILogger<ArticlesController> _logger;

    public ArticlesController(
        IArticleRepository articleRepo,
        ITagRepository tagRepo,
        ICommentRepository commentRepo,
        ILogger<ArticlesController> logger)
    {
        _articleRepo = articleRepo;
        _tagRepo = tagRepo;
        _commentRepo = commentRepo;
        _logger = logger;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }
        throw new UnauthorizedAccessException("User ID is invalid or not found");
    }

    // GET: api/articles
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ArticleDto>>> GetAllArticles()
    {
        var articles = await _articleRepo.GetAllArticlesAsync();

        return Ok(articles.Select(a => new ArticleDto
        {
            Id = a.Id,
            Title = a.Title,
            ContentPreview = a.Content.Length > 100 ? a.Content[..100] + "..." : a.Content,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt,
            Author = a.Author != null ? new AuthorDto // Добавляем проверку
            {
                Id = a.Author.Id,
                UserName = a.Author.UserName,
                DisplayName = a.Author.DisplayName
            } : null,
            Tags = a.Tags?.Select(t => new TagDto
            {
                Id = t.Tag.Id,
                Name = t.Tag.Name
            }).ToList() ?? new List<TagDto>()
        }));
    }

    // GET: api/articles/{id}
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<ArticleDetailsDto>> GetArticleByArticleId(Guid id)
    {
        var article = await _articleRepo.GetArticleByArticleIdAsync(id, includeAuthor: true, includeTags: true);
        if (article == null) return NotFound();

        var comments = await _commentRepo.GetCommentsForArticleAsync(id);
        var currentUserId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : Guid.Empty;

        return Ok(new ArticleDetailsDto
        {
            Id = article.Id,
            Title = article.Title,
            Content = article.Content,
            ContentPreview = article.Content.Length > 100 ? article.Content[..100] + "..." : article.Content,
            CreatedAt = article.CreatedAt,
            UpdatedAt = article.UpdatedAt,
            Author = new AuthorDto
            {
                Id = article.Author.Id,
                UserName = article.Author.UserName,
                DisplayName = article.Author.DisplayName
            },
            Tags = article.Tags.Select(t => new TagDto
            {
                Id = t.Tag.Id,
                Name = t.Tag.Name
            }).ToList(),
            Comments = comments.Select(c => new CommentDto
            {
                Id = c.Id,
                Text = c.Text,
                CreatedAt = c.CreatedAt,
                Author = new AuthorDto
                {
                    Id = c.Author.Id,
                    UserName = c.Author.UserName,
                    DisplayName = c.Author.DisplayName
                }
            }).ToList(),
            CanEdit = User.IsInRole("Admin") || User.IsInRole("Moderator") || article.AuthorId == currentUserId,
            CanDelete = User.IsInRole("Admin") || User.IsInRole("Moderator") || article.AuthorId == currentUserId
        });
    }

    // POST: api/articles
    [HttpPost]
    public async Task<ActionResult<ArticleDto>> CreateArticle([FromBody] CreateArticleDto dto)
    {
        var article = new Article
        {
            Title = dto.Title,
            Content = dto.Content,
            AuthorId = GetCurrentUserId(),
            Tags = dto.TagIds?.Select(tagId => new ArticleTag { TagId = tagId }).ToList()
        };

        await _articleRepo.AddAsync(article);

        _logger.LogInformation("Article created with ID: {ArticleId}", article.Id);

        return CreatedAtAction(nameof(GetArticleByArticleId), new { id = article.Id }, new ArticleDto
        {
            Id = article.Id,
            Title = article.Title,
            ContentPreview = article.Content.Length > 100 ? article.Content[..100] + "..." : article.Content,
            CreatedAt = article.CreatedAt,
            Author = new AuthorDto
            {
                Id = article.AuthorId,
                UserName = User.Identity?.Name ?? "Unknown"
            },
            Tags = article.Tags?.Select(t => new TagDto
            {
                Id = t.TagId,
                Name = string.Empty // Будет заполнено при последующем запросе
            }).ToList() ?? new List<TagDto>()
        });
    }

    // PUT: api/articles/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateArticle(Guid id, [FromBody] UpdateArticleDto dto)
    {
        var article = await _articleRepo.GetArticleByArticleIdAsync(id, includeTags: true);
        if (article == null) return NotFound();

        var isAdmin = User.IsInRole("Admin");
        var isModerator = User.IsInRole("Moderator");
        var isAuthor = article.AuthorId == GetCurrentUserId();

        if (!isAdmin && !isModerator && !isAuthor)
        {
            _logger.LogWarning("Unauthorized update attempt by user {UserId}", GetCurrentUserId());
            return Forbid();
        }

        article.Title = dto.Title;
        article.Content = dto.Content;
        article.UpdatedAt = DateTime.UtcNow;

        if (dto.TagIds != null)
        {
            article.Tags.Clear();
            article.Tags = dto.TagIds.Select(tagId => new ArticleTag { TagId = tagId }).ToList();
        }

        await _articleRepo.UpdateAsync(article);

        _logger.LogInformation("Article {ArticleId} updated by user {UserId}", id, GetCurrentUserId());

        return NoContent();
    }

    // DELETE: api/articles/{id}
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> DeleteArticle(Guid id)
    {
        var article = await _articleRepo.GetArticleByArticleIdAsync(id);
        if (article == null) return NotFound();

        var isAdmin = User.IsInRole("Admin");
        var isAuthor = article.AuthorId == GetCurrentUserId();

        if (!isAdmin && !isAuthor)
        {
            _logger.LogWarning("Unauthorized delete attempt by user {UserId}", GetCurrentUserId());
            return Forbid();
        }

        await _articleRepo.DeleteAsync(article);

        _logger.LogInformation("Article {ArticleId} deleted by user {UserId}", id, GetCurrentUserId());

        return NoContent();
    }

    // POST: api/articles/{id}/comments
    [HttpPost("{id}/comments")]
    public async Task<ActionResult<CommentDto>> AddComment(Guid id, [FromBody] AddCommentDto dto)
    {
        var article = await _articleRepo.GetArticleByArticleIdAsync(id);
        if (article == null) return NotFound();

        var comment = new Comment
        {
            Text = dto.Text,
            ArticleId = id,
            AuthorId = GetCurrentUserId(),
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepo.AddAsync(comment);

        _logger.LogInformation("Comment added to article {ArticleId} by user {UserId}", id, GetCurrentUserId());

        return CreatedAtAction(nameof(GetArticleByArticleId), new { id }, new CommentDto
        {
            Id = comment.Id,
            Text = comment.Text,
            CreatedAt = comment.CreatedAt,
            Author = new AuthorDto
            {
                Id = comment.AuthorId,
                UserName = User.Identity?.Name ?? "Unknown",
                DisplayName = User.FindFirstValue("DisplayName")
            }
        });
    }
}

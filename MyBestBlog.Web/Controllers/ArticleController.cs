using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBestBlog.Core.Entities;
using MyBestBlog.Core.Interfaces;
using MyBestBlog.Infrastructure.Repositories;
using MyBestBlog.Services.Interfaces;
using System.Security.Claims;
using MyBestBlog.Web.Models;
using System.Xml.Linq;

namespace MyBestBlog.Web.Controllers;

[Authorize]
public class ArticleController : Controller
{
    private readonly IArticleRepository _articleRepo;
    private readonly ITagRepository _tagRepo;
    private readonly ICommentRepository _commentRepo;

    public ArticleController(IArticleRepository articleRepo, ITagRepository tagRepo, ICommentRepository commentRepo)
    {
        _articleRepo = articleRepo;
        _tagRepo = tagRepo;
        _commentRepo = commentRepo;
    }

    protected Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        throw new UnauthorizedAccessException("User ID is invalid or not found");
    }

    /// <summary>
    /// удаление Статьи
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var article = await _articleRepo.GetArticleByArticleIdAsync(id);
        if (article == null) return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 404 });

        var isAdmin = User.IsInRole("Admin");
        var isAuthor = article.AuthorId.ToString() == User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!isAdmin && !isAuthor)
        {
            return View("../Error/Forbidden");
        }

        await _articleRepo.DeleteAsync(article);
        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var tags = await _tagRepo.GetAllTagsAsync();
        var model = new ArticleCreateViewModel
        {
            AvailableTags = tags.Select(t => new TagViewModel
            {
                Id = t.Id,
                Name = t.Name
            }).ToList()
        };
        return View(model);
    }

    public async Task<IActionResult> Create(ArticleCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableTags = (await _tagRepo.GetAllTagsAsync())
                .Select(t => new TagViewModel { Id = t.Id, Name = t.Name })
                .ToList();
            return View(model);
        }

        var article = new Article
        {
            Title = model.Title,
            Content = model.Content,
            AuthorId = GetCurrentUserId(),
            Tags = model.SelectedTagIds?.Select(tagId => new ArticleTag
            {
                TagId = tagId
            }).ToList()
        };

        await _articleRepo.AddAsync(article);
        return RedirectToAction("Details", new { id = article.Id });
    }

    [Authorize]
    public async Task<IActionResult> Edit(Guid id)
    {
        var article = await _articleRepo.GetArticleByArticleIdAsync(id, includeTags: true);
        if (article == null) return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 404 });

        if (!User.IsInRole("Admin") && !User.IsInRole("Moderator") && article.AuthorId != GetCurrentUserId())
        {
            return View("../Error/Forbidden");
        }

        var allTags = await _tagRepo.GetAllTagsAsync();

        var model = new ArticleCreateViewModel
        {
            Id = article.Id,
            Title = article.Title,
            Content = article.Content,
            SelectedTagIds = article.Tags?.Select(t => t.TagId).ToList(),
            AvailableTags = allTags.Select(t => new TagViewModel
            {
                Id = t.Id,
                Name = t.Name
            }).ToList()
        };

        return View(model);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Edit(ArticleCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableTags = (await _tagRepo.GetAllTagsAsync())
                .Select(t => new TagViewModel { Id = t.Id, Name = t.Name })
                .ToList();
            return View(model);
        }

        var article = await _articleRepo.GetArticleByArticleIdAsync(model.Id, includeTags: true);
        if (article == null) return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 404 });

        article.Title = model.Title;
        article.Content = model.Content;

        if (model.SelectedTagIds != null)
        {
            article.Tags.Clear();

            foreach (var tagId in model.SelectedTagIds)
            {
                article.Tags.Add(new ArticleTag { TagId = tagId });
            }
        }

        await _articleRepo.UpdateAsync(article);
        return RedirectToAction("Details", new { id = model.Id });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(Guid id)
    {
        var article = await _articleRepo.GetArticleByArticleIdAsync(id, includeAuthor: true);
        if (article == null) return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 404 });

        var comments = await _commentRepo.GetCommentsForArticleAsync(id);

        var model = new ArticleDetailsViewModel
        {
            Article = article,
            Comments = comments.Select(c => new CommentViewModel
            {
                Id = c.Id,
                Text = c.Text,
                CreatedAt = c.CreatedAt,
                AuthorName = c.Author.UserName,
                AuthorId = c.Author.Id
            }).ToList(),
            NewComment = new AddCommentViewModel { ArticleId = id },
            CanEdit = false,
            CanDelete = false
        };

        if (User.Identity?.IsAuthenticated == true)
        {
            var isAuthorized = User.IsInRole("Admin") ||
                             User.IsInRole("Moderator") ||
                             article.AuthorId == GetCurrentUserId();
            model.CanEdit = isAuthorized;
            model.CanDelete = isAuthorized;
        }

        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(
    [FromForm] Guid ArticleId,
    [FromForm] string Text)
    {
        if (!ModelState.IsValid)
        {
            return RedirectToAction("Details", new { id = ArticleId });
        }

        var comment = new Comment
        {
            Text = Text,
            ArticleId = ArticleId,
            AuthorId = GetCurrentUserId(),
            CreatedAt = DateTime.UtcNow
        };

        await _commentRepo.AddAsync(comment);
        return RedirectToAction("Details", new { id = ArticleId });
    }
}
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
    private readonly ILogger<ArticleController> _logger;

    public ArticleController(IArticleRepository articleRepo, ITagRepository tagRepo, ICommentRepository commentRepo, ILogger<ArticleController> logger)
    {
        _articleRepo = articleRepo;
        _tagRepo = tagRepo;
        _commentRepo = commentRepo;
        _logger = logger;
    }

    /// <summary>
    /// Получение Id пользователя текущей сессии - для создания статей, комментариев, проверки на авторство на странице статьи (дубилруется с таким же методом в ArticleController)
    /// </summary>
    /// <returns></returns>
    /// <exception cref="UnauthorizedAccessException"></exception>
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
        _logger.LogInformation("User just deleted an article");

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

    /// <summary>
    /// Создание статьи - представление
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        _logger.LogInformation("User trying to wrote an article");

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

    /// <summary>
    /// Создание статьи - отработка POST
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    public async Task<IActionResult> Create(ArticleCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.AvailableTags = (await _tagRepo.GetAllTagsAsync())
                .Select(t => new TagViewModel { Id = t.Id, Name = t.Name })
                .ToList();
            return View(model);
        }

        _logger.LogInformation("User just wrote an article");

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

    /// <summary>
    /// Редактирование статьи - представление
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize]
    public async Task<IActionResult> Edit(Guid id)
    {
        var article = await _articleRepo.GetArticleByArticleIdAsync(id, includeTags: true);
        if (article == null) return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 404 });

        if (!User.IsInRole("Admin") && !User.IsInRole("Moderator") && article.AuthorId != GetCurrentUserId())
        {
            _logger.LogInformation("Unautorised user tried to edit an article");

            return View("../Error/Forbidden");
        }

        _logger.LogInformation("User trying to edit an article");


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

    /// <summary>
    /// Редактирование статьи - отработка POST
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
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

        _logger.LogInformation("User just edited an article");

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

    /// <summary>
    /// Просмотр статьи
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Details(Guid id)
    {
        _logger.LogInformation("User reads an article");

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

    /// <summary>
    /// Добавление комментария
    /// </summary>
    /// <param name="ArticleId"></param>
    /// <param name="Text"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddComment(
    [FromForm] Guid ArticleId,
    [FromForm] string Text)
    {
        // Ручная валидация, потому что по-нормальному не работает, всё испробовал
        if (string.IsNullOrWhiteSpace(Text))
        {
            ModelState.AddModelError("Text", "Комментарий не может быть пустым");
        }
        else if (Text.Length > 1000)
        {
            ModelState.AddModelError("Text", "Комментарий не должен превышать 1000 символов");
        }

        if (!ModelState.IsValid)
        {
            TempData["Error"] = ModelState["Text"]?.Errors.FirstOrDefault()?.ErrorMessage;
            return RedirectToAction("Details", new { id = ArticleId });
        }

        _logger.LogInformation("User just wrote an comment");

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
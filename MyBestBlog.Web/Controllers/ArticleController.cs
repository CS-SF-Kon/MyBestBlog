using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBestBlog.Core.Entities;
using MyBestBlog.Core.Interfaces;
using MyBestBlog.Infrastructure.Repositories;
using MyBestBlog.Services.Interfaces;
using System.Security.Claims;
using MyBestBlog.Web.Models;

namespace MyBestBlog.Web.Controllers;

[Authorize]
public class ArticleController : Controller
{
    private readonly IArticleRepository _articleRepo;
    private readonly ITagRepository _tagRepo;

    public ArticleController(IArticleRepository articleRepo, ITagRepository tagRepo)
    {
        _articleRepo = articleRepo;
        _tagRepo = tagRepo;
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
        if (article == null) return NotFound();

        var isAdmin = User.IsInRole("Admin");
        var isAuthor = article.AuthorId.ToString() == User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!isAdmin && !isAuthor)
        {
            return Forbid();
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
            model.AvailableTags = (List<TagViewModel>)await _tagRepo.GetAllTagsAsync();
            return View(model);
        }

        var article = new Article
        {
            Title = model.Title,
            Content = model.Content,
            AuthorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)),
            Tags = model.SelectedTagIds?.Select(tagId => new ArticleTag
            {
                TagId = tagId
            }).ToList()
        };

        await _articleRepo.AddAsync(article);
        return RedirectToAction("Details", new { id = article.Id });
    }
}
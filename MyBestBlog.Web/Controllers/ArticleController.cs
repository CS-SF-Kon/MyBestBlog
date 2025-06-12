using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBestBlog.Core.Entities;
using MyBestBlog.Core.Interfaces;
using MyBestBlog.Infrastructure.Repositories;
using MyBestBlog.Services.Interfaces;
using System.Security.Claims;

[Authorize]
public class ArticleController : Controller
{
    private readonly IArticleRepository _articleRepo;

    public ArticleController(IArticleRepository articleRepo)
    {
        _articleRepo = articleRepo;
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
}
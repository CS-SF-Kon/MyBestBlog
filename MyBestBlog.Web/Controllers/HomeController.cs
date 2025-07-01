using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBestBlog.Core.Entities;
using MyBestBlog.Core.Interfaces;
using MyBestBlog.Infrastructure.Data;
using MyBestBlog.Web.Models;
using System.Diagnostics;

namespace MyBestBlog.Web.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IArticleRepository _articleRepo;
    private readonly BlogDbContext _context;
    private readonly IUserRepository _userRepo;

    public HomeController(ILogger<HomeController> logger, IArticleRepository articleRepo, BlogDbContext context, IUserRepository userRepo)
    {
        _logger = logger;
        _articleRepo = articleRepo;
        _context = context;
        _userRepo = userRepo;
    }

    public async Task<IActionResult> Index(Guid? tagId)
    {
        List<Article> articles;

        if (tagId.HasValue)
        {
            // Получаем статьи по тегу
            articles = await _context.Articles
                .Where(a => a.Tags.Any(t => t.TagId == tagId.Value))
                .ToListAsync();

            ViewData["CurrentTag"] = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == tagId.Value);
        }
        else
        {
            // Получаем все статьи (оригинальная логика)
            articles = (List<Article>)await _articleRepo.GetAllArticlesAsync();
        }

        return View(articles);
    }

    public async Task<IActionResult> Users()
    {
        var users = await _userRepo.GetAllUsersAsync();
        return View(users);
    }

    //public IActionResult Privacy() - можно будет удалить, наверное
    //{
    //    return View();
    //}

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

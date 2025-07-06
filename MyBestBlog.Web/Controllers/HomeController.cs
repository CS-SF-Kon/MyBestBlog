using Microsoft.AspNetCore.Identity;
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
    private readonly UserManager<User> _userManager;

    public HomeController(ILogger<HomeController> logger, IArticleRepository articleRepo, BlogDbContext context, IUserRepository userRepo, UserManager<User> userManager)
    {
        _logger = logger;
        _articleRepo = articleRepo;
        _context = context;
        _userRepo = userRepo;
        _userManager = userManager;
    }

    /// <summary>
    /// Главная страница с перечнем статей
    /// </summary>
    /// <param name="tagId"></param>
    /// <returns></returns>
    public async Task<IActionResult> Index(Guid? tagId)
    {
        IQueryable<Article> query = _context.Articles
            .Include(a => a.Tags)
            .ThenInclude(t => t.Tag)
            .Include(a => a.Author);

        if (tagId.HasValue)
        {
            query = query.Where(a => a.Tags.Any(t => t.TagId == tagId.Value));
            ViewData["CurrentTag"] = await _context.Tags
                .FirstOrDefaultAsync(t => t.Id == tagId.Value);
        }

        var articles = await query.ToListAsync();

        var viewModels = articles.Select(a => new ArticlePreviewViewModel
        {
            Id = a.Id,
            Title = a.Title,
            PreviewContent = a.Content.Length > 100
                ? a.Content.Substring(0, 100) + "..."
                : a.Content,
            CreatedAt = a.CreatedAt,
            Tags = a.Tags?.Select(t => new ArticlePreviewViewModel.TagInfo
            {
                Id = t.Tag.Id,
                Name = t.Tag.Name
            }).ToList() ?? new List<ArticlePreviewViewModel.TagInfo>(),
            Author = new ArticlePreviewViewModel.AuthorInfo
            {
                Id = a.Author.Id,
                UserName = a.Author.UserName
            }
        }).ToList();

        return View(viewModels);
    }

    /// <summary>
    /// Перечень всех пользователей блога
    /// </summary>
    /// <returns></returns>
    public async Task<IActionResult> Users()
    {
        var users = await _context.Users
            .Include(u => u.Articles)
            .ThenInclude(a => a.Tags)
            .ThenInclude(at => at.Tag)
            .ToListAsync();

        var viewModels = new List<UserListViewModel>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var viewModel = new UserListViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Role = roles.FirstOrDefault() ?? "User",
                Articles = user.Articles?.Select(a => new UserArticleViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    Tags = a.Tags?.Select(t => t.Tag.Name).ToList() ?? new List<string>()
                }).ToList() ?? new List<UserArticleViewModel>()
            };

            viewModels.Add(viewModel);
        }

        return View(viewModels);
    }

    /// <summary>
    /// Отработка ошибок
    /// </summary>
    /// <returns></returns>
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}

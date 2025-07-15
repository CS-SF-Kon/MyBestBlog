using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBestBlog.Core.Entities;
using MyBestBlog.Core.Interfaces;
using MyBestBlog.Web.Models;

namespace MyBestBlog.Web.Controllers;

public class TagController : Controller
{
    private readonly ITagRepository _tagRepo;
    private readonly ILogger<TagController> _logger;

    public TagController(ITagRepository tagRepo, ILogger<TagController> logger)
    {
        _tagRepo = tagRepo;
        _logger = logger;
    }

    /// <summary>
    /// Вывод всех тегов блога
    /// </summary>
    /// <returns></returns>
    [AllowAnonymous]
    public async Task<IActionResult> All()
    {
        _logger.LogInformation("User browsing all tags of blog");

        var tags = await _tagRepo.GetAllTagsAsync();
        var model = new TagListViewModel
        {
            Tags = tags,
            CanCreate = User.IsInRole("Admin") || User.IsInRole("Moderator")
        };
        return View(model);
    }

    /// <summary>
    /// Создание тега - представление. Лишнее - создание тега реализовано в Edit при условии отсутствия параметров
    /// </summary>
    /// <returns></returns>
    //[Authorize(Roles = "Admin,Moderator")]
    //public IActionResult Create()
    //{
    //    return View("Edit", new TagCreateViewModel());
    //}

    /// <summary>
    /// Создание тега - отработка POST. Лишнее - создание тега реализовано в Edit при условии отсутствия параметров
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    //[HttpPost]
    //[Authorize(Roles = "Admin,Moderator")]
    //[ValidateAntiForgeryToken]
    //public async Task<IActionResult> Create(TagCreateViewModel model)
    //{
    //    if (!ModelState.IsValid)
    //    {
    //        return View(model);
    //    }

    //    var tag = new Tag
    //    {
    //        Name = model.Name,
    //        Description = model.Description
    //    };

    //    await _tagRepo.AddAsync(tag);
    //    return RedirectToAction("All");
    //}

    /// <summary>
    /// Создание и редактирование тега - представление
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            _logger.LogInformation("User trying to create a tag");

            return View(new TagCreateViewModel());
        }

        var tag = await _tagRepo.GetTagByTagIdAsync(id.Value);
        if (tag == null) return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 404 });

        _logger.LogInformation("User trying to edit a tag");

        return View(new TagCreateViewModel
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description
        });
    }

    /// <summary>
    /// Создание и редактирование тега - отработка POST
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Moderator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TagCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        if (model.Id == Guid.Empty) // Создание
        {
            _logger.LogInformation("User just created a tag");

            var tag = new Tag { Name = model.Name, Description = model.Description };
            await _tagRepo.AddAsync(tag);
        }
        else // Редактирование
        {
            _logger.LogInformation("User just edited a tag");

            var tag = await _tagRepo.GetTagByTagIdAsync(model.Id);
            if (tag == null) return NotFound();

            tag.Name = model.Name;
            tag.Description = model.Description;
            await _tagRepo.UpdateAsync(tag);
        }

        return RedirectToAction("All");
    }
}

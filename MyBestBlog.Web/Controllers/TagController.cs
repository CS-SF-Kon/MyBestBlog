using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBestBlog.Core.Entities;
using MyBestBlog.Core.Interfaces;
using MyBestBlog.Web.Models;

namespace MyBestBlog.Web.Controllers;

public class TagController : Controller
{
    private readonly ITagRepository _tagRepo;

    public TagController(ITagRepository tagRepo)
    {
        _tagRepo = tagRepo;
    }

    [AllowAnonymous]
    public async Task<IActionResult> All()
    {
        var tags = await _tagRepo.GetAllTagsAsync();
        var model = new TagListViewModel
        {
            Tags = tags,
            CanCreate = User.IsInRole("Admin") || User.IsInRole("Moderator")
        };
        return View(model);
    }

    [Authorize(Roles = "Admin,Moderator")]
    public IActionResult Create()
    {
        return View("Edit", new TagCreateViewModel());
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Moderator")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TagCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var tag = new Tag
        {
            Name = model.Name,
            Description = model.Description
        };

        await _tagRepo.AddAsync(tag);
        return RedirectToAction("All");
    }

    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> Edit(Guid? id)
    {
        if (id == null)
        {
            return View(new TagCreateViewModel());
        }

        var tag = await _tagRepo.GetTagByTagIdAsync(id.Value);
        if (tag == null) return NotFound();

        return View(new TagCreateViewModel
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description
        });
    }

    [HttpPost]
    public async Task<IActionResult> Edit(TagCreateViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        if (model.Id == Guid.Empty) // Создание
        {
            await _tagRepo.AddAsync(new Tag { Name = model.Name, Description = model.Description });
        }
        else // Редактирование
        {
            var tag = await _tagRepo.GetTagByTagIdAsync(model.Id);
            tag.Name = model.Name;
            tag.Description = model.Description;
            await _tagRepo.UpdateAsync(tag);
        }

        return RedirectToAction("All");
    }
}

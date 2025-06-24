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
        return View();
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
}

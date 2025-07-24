using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBestBlog.API.DTO;
using MyBestBlog.Core.Entities;
using MyBestBlog.Core.Interfaces;

namespace MyBestBlog.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly ITagRepository _tagRepo;
    private readonly ILogger<TagsController> _logger;

    public TagsController(ITagRepository tagRepo, ILogger<TagsController> logger)
    {
        _tagRepo = tagRepo;
        _logger = logger;
    }

    /// <summary>
    /// Получение всех тегов блога
    /// </summary>
    /// <returns></returns>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<TagListDto>> GetAll()
    {
        _logger.LogInformation("Getting all tags");

        var tags = await _tagRepo.GetAllTagsAsync();
        var canCreate = User.IsInRole("Admin") || User.IsInRole("Moderator");

        return Ok(new TagListDto
        {
            Tags = tags.Select(t => new TagDto
            {
                Id = t.Id,
                Name = t.Name,
                Description = t.Description
            }),
            CanCreate = canCreate
        });
    }

    /// <summary>
    /// Создание нового тега
    /// </summary>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<ActionResult<TagDto>> Create([FromBody] CreateUpdateTagDto dto)
    {
        _logger.LogInformation("Creating new tag");

        var tag = new Tag
        {
            Name = dto.Name,
            Description = dto.Description
        };

        await _tagRepo.AddAsync(tag);

        return CreatedAtAction(nameof(GetAll), new { id = tag.Id }, new TagDto
        {
            Id = tag.Id,
            Name = tag.Name,
            Description = tag.Description
        });
    }

    /// <summary>
    /// Редактирование тега
    /// </summary>
    /// <param name="id"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateUpdateTagDto dto)
    {
        _logger.LogInformation("Updating tag {TagId}", id);

        var tag = await _tagRepo.GetTagByTagIdAsync(id);
        if (tag == null) return NotFound();

        tag.Name = dto.Name;
        tag.Description = dto.Description;

        await _tagRepo.UpdateAsync(tag);

        return NoContent();
    }

    /// <summary>
    /// Удаление тега
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Moderator")]
    public async Task<IActionResult> Delete(Guid id)
    {
        _logger.LogInformation("Deleting tag {TagId}", id);

        var tag = await _tagRepo.GetTagByTagIdAsync(id);
        if (tag == null) return NotFound();

        await _tagRepo.DeleteAsync(tag);

        return NoContent();
    }
}

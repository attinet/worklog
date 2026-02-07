using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkLog.Domain.Entities;
using WorkLog.Domain.Interfaces;
using WorkLog.Shared.DTOs;
using WorkLog.Shared.DTOs.Todos;

namespace WorkLog.Api.Controllers;

/// <summary>
/// 待辦事項分類管理 API（僅管理人員可存取）
/// </summary>
[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class TodoCategoriesController : ControllerBase
{
    private readonly ITodoCategoryRepository _categoryRepository;

    public TodoCategoriesController(ITodoCategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    /// <summary>
    /// 取得所有分類
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<TodoCategoryResponse>>> GetAll()
    {
        var categories = await _categoryRepository.GetAllAsync();
        return Ok(categories.Select(c => new TodoCategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            ColorCode = c.ColorCode,
            Icon = c.Icon
        }));
    }

    /// <summary>
    /// 取得單一分類
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<TodoCategoryResponse>> GetById(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) return NotFound();

        return Ok(new TodoCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            ColorCode = category.ColorCode,
            Icon = category.Icon
        });
    }

    /// <summary>
    /// 新增分類
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<TodoCategoryResponse>> Create([FromBody] TodoCategoryRequest request)
    {
        var category = new TodoCategory
        {
            Name = request.Name,
            ColorCode = request.ColorCode,
            Icon = request.Icon ?? string.Empty,
            IsActive = true,
            SortOrder = request.SortOrder
        };

        await _categoryRepository.CreateAsync(category);

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, new TodoCategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            ColorCode = category.ColorCode,
            Icon = category.Icon
        });
    }

    /// <summary>
    /// 更新分類
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult> Update(int id, [FromBody] TodoCategoryRequest request)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) return NotFound();

        category.Name = request.Name;
        category.ColorCode = request.ColorCode;
        category.Icon = request.Icon ?? string.Empty;
        category.IsActive = request.IsActive;
        category.SortOrder = request.SortOrder;

        await _categoryRepository.UpdateAsync(category);

        return NoContent();
    }

    /// <summary>
    /// 刪除分類
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var category = await _categoryRepository.GetByIdAsync(id);
        if (category == null) return NotFound();

        await _categoryRepository.DeleteAsync(id);

        return NoContent();
    }
}

/// <summary>
/// 分類請求 DTO（內部使用）
/// </summary>
public class TodoCategoryRequest
{
    public string Name { get; set; } = string.Empty;
    public string ColorCode { get; set; } = "#808080";
    public string? Icon { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}

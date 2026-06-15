using System.Security.Claims;
using Large_Scale_CommunityPlatform.Data;
using Large_Scale_CommunityPlatform.Models.Dtos.Category;
using Large_Scale_CommunityPlatform.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Large_Scale_CommunityPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoryController:ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    
    public CategoryController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [Authorize]
    [HttpPost("request")]
    public async Task<IActionResult> RequestCategory(CategoryRequestDto dto)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)? .Value;

        if (userId == null)
        {
            return Unauthorized();
        }

        var CategoryName = dto.CategoryName.Trim();

        if (string.IsNullOrWhiteSpace(CategoryName))
        {
            return BadRequest(new
            {
                message = "Category name is required"
            });
        }

        var exists = await _dbContext.Categories
            .AnyAsync(c => c.CategoryName.ToLower() == CategoryName.ToLower());

        var category = new Category
        {
            CategoryName = CategoryName,
            Status = CategoryStatus.Pending,
            RequestedById = userId,
            CreatedAt = DateTime.UtcNow,
        };

        _dbContext.Categories.Add(category);
        
        await _dbContext.SaveChangesAsync();
        return Ok(category);
    }
    
    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> GetCategories()
    {
        var categories = await _dbContext.Categories
            .OrderBy(c => c.CategoryName)
            .Select(c => new CategoryResponseDto
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Status = c.Status.ToString(),
                RequestedById = c.RequestedById,
                ApprovedById = c.ApprovedById,
                CreatedAt = c.CreatedAt,
                ApprovedAt = c.ApprovedAt,
                RejectedAt = c.RejectedAt
            })
            .ToListAsync();

        return Ok(categories);
    }

    [AllowAnonymous]
    [HttpGet("{categoryId:long}")]
    public async Task<IActionResult> GetCategory(long categoryId)
    {
        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

        if (category == null)
        {
            return NotFound(new
            {
                message = "Category not found"
            });
        }

        return Ok(ToResponse(category));
    }

    [Authorize(Roles = "Admin,CategoryManager")]
    [HttpPost("{categoryId:long}/approve")]
    public async Task<IActionResult> ApproveCategory(long categoryId)
    {
        var managerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (managerId == null)
        {
            return Unauthorized();
        }

        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

        if (category == null)
        {
            return NotFound(new
            {
                message = "Category not found"
            });
        }

        if (category.Status == CategoryStatus.Approved)
        {
            return BadRequest(new
            {
                message = "Category is already approved"
            });
        }

        category.Status = CategoryStatus.Approved;
        category.ApprovedById = managerId;
        category.ApprovedAt = DateTime.UtcNow;
        category.RejectedAt = null;

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(category));
    }

    [Authorize(Roles = "Admin,CategoryManager")]
    [HttpPost("{categoryId:long}/reject")]
    public async Task<IActionResult> RejectCategory(long categoryId)
    {
        var managerId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (managerId == null)
        {
            return Unauthorized();
        }

        var category = await _dbContext.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

        if (category == null)
        {
            return NotFound(new
            {
                message = "Category not found"
            });
        }

        if (category.Status == CategoryStatus.Rejected)
        {
            return BadRequest(new
            {
                message = "Category is already rejected"
            });
        }

        category.Status = CategoryStatus.Rejected;
        category.ApprovedById = managerId;
        category.ApprovedAt = null;
        category.RejectedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return Ok(ToResponse(category));
    }

    [Authorize(Roles = "Admin,CategoryManager")]
    [HttpDelete("{categoryId:long}")]
    public async Task<IActionResult> DeleteCategory(long categoryId)
    {
        var category = await _dbContext.Categories
            .Include(c => c.Posts)
            .FirstOrDefaultAsync(c => c.CategoryId == categoryId);

        if (category == null)
        {
            return NotFound(new
            {
                message = "Category not found"
            });
        }

        if (category.Posts.Any())
        {
            return BadRequest(new
            {
                message = "Cannot delete category because it has posts"
            });
        }

        _dbContext.Categories.Remove(category);
        await _dbContext.SaveChangesAsync();

        return Ok(new
        {
            message = "Category deleted successfully"
        });
    }

    private static CategoryResponseDto ToResponse(Category category)
    {
        return new CategoryResponseDto
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            Status = category.Status.ToString(),
            RequestedById = category.RequestedById,
            ApprovedById = category.ApprovedById,
            CreatedAt = category.CreatedAt,
            ApprovedAt = category.ApprovedAt,
            RejectedAt = category.RejectedAt
        };
    }
    
}
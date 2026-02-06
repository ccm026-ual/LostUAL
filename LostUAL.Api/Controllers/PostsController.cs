using LostUAL.Contracts.Posts;
using LostUAL.Data.Entities;
using LostUAL.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace LostUAL.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly LostUALDbContext _db;

    public PostsController(LostUALDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<List<PostListItemDto>>> Get()
    {
        var items = await _db.Posts
            .Include(p => p.Category)
            .Include(p => p.Location)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Select(p => new PostListItemDto(
                p.Id,
                p.Type,
                p.Title,
                p.Category!.Name,
                p.Location!.Name,
                p.DateApprox,
                p.Status,
                p.CreatedAtUtc
            ))
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PostDetailDto>> GetById(int id)
    {
        var item = await _db.Posts
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Include(p => p.Category)
            .Include(p => p.Location)
            .Select(p => new PostDetailDto(
                p.Id,
                p.Type,
                p.Title,
                p.Description,
                p.CategoryId,
                p.Category!.Name,
                p.LocationId,
                p.Location!.Name,
                p.DateApprox,
                p.Status,
                p.CreatedAtUtc
            ))
            .SingleOrDefaultAsync();

        if (item is null) return NotFound();

        return Ok(item);
    }


    [HttpPost]
    [Authorize]
    public async Task<ActionResult<PostListItemDto>> Create(CreatePostRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();
        // 1) Validación mínima (evita FK inválidas)
        var categoryExists = await _db.Categories.AnyAsync(c => c.Id == request.CategoryId);
        var locationExists = await _db.Locations.AnyAsync(l => l.Id == request.LocationId);

        if (!categoryExists) return BadRequest("CategoryId no válido.");
        if (!locationExists) return BadRequest("LocationId no válido.");

        // 2) Crear entidad
        var post = new ItemPost
        {
            CreatedByUserId = userId,
            Type = request.Type,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            CategoryId = request.CategoryId,
            LocationId = request.LocationId,
            DateApprox = request.DateApprox,
            Status = PostStatus.Open,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.Posts.Add(post);
        await _db.SaveChangesAsync();

        // 3) Cargar nombres para devolver DTO completo
        var result = await _db.Posts
            .Where(p => p.Id == post.Id)
            .Include(p => p.Category)
            .Include(p => p.Location)
            .Select(p => new PostListItemDto(
                p.Id,
                p.Type,
                p.Title,
                p.Category!.Name,
                p.Location!.Name,
                p.DateApprox,
                p.Status,
                p.CreatedAtUtc
            ))
            .SingleAsync();

        // 4) Devuelve 201 + Location header
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);

    }
}

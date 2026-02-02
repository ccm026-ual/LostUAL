using LostUAL.Contracts.Lookups;
using LostUAL.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LostUAL.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LookupsController : ControllerBase
{
    private readonly LostUALDbContext _db;
    public LookupsController(LostUALDbContext db) => _db = db;

    [HttpGet("categories")]
    public async Task<ActionResult<List<LookupItemDto>>> GetCategories()
        => Ok(await _db.Categories
            .OrderBy(c => c.Name)
            .Select(c => new LookupItemDto(c.Id, c.Name))
            .ToListAsync());

    [HttpGet("locations")]
    public async Task<ActionResult<List<LookupItemDto>>> GetLocations()
        => Ok(await _db.Locations
            .OrderBy(l => l.Name)
            .Select(l => new LookupItemDto(l.Id, l.Name))
            .ToListAsync());
}

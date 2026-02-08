using LostUAL.Contracts.Claims;
using LostUAL.Contracts.Posts;
using LostUAL.Contracts.Chat;
using LostUAL.Data.Entities;
using LostUAL.Data.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;


namespace LostUAL.Api.Controllers;

[Authorize]
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

    [AllowAnonymous]
    [HttpGet("preview")]
    public async Task<IActionResult> GetPreview([FromQuery] int take = 20, CancellationToken ct = default)
    {
        take = Math.Clamp(take, 1, 50);

        var items = await _db.Posts
            .AsNoTracking()
            .Where(p => p.Status == PostStatus.Open)
            .OrderByDescending(p => p.CreatedAtUtc)
            .Take(take)
            .Select(p => new PostListItemDto(
                p.Id, p.Type, p.Title, p.Category!.Name, p.Location!.Name, p.DateApprox, p.Status, p.CreatedAtUtc
            ))
            .ToListAsync(ct);

        return Ok(items);
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var isAdminOrModerator = User.IsInRole("Admin") || User.IsInRole("Moderator");

        var info = await _db.Posts
            .AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new { p.CreatedByUserId, p.Status })
            .FirstOrDefaultAsync(ct);

        if (info is null)
            return NotFound();

        var isOwner = info.CreatedByUserId == userId;

      
        if ((info.Status == PostStatus.Closed || info.Status == PostStatus.Resolved) && !(isOwner || isAdminOrModerator))
            return NotFound();

        var dto = await _db.Posts
            .AsNoTracking()
            .Where(p => p.Id == id)
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
                p.CreatedAtUtc,
                p.CreatedByUserId
            ))
            .FirstOrDefaultAsync(ct);

        if (dto is null)
            return NotFound();

        return Ok(dto);
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
    [Authorize]
    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        /*var total = await _db.Posts.CountAsync(ct);
        var mine = await _db.Posts.CountAsync(p => p.CreatedByUserId == userId, ct);
        var dbPath = _db.Database.GetDbConnection().DataSource;

        return Ok(new { userId, total, mine, dbPath });*/


        var myPosts = await _db.Posts
            .AsNoTracking()
            .Where(p => p.CreatedByUserId == userId)
            .OrderByDescending(p => p.CreatedAtUtc) 
            .Select(p => new MyPostListItemDto(
                p.Id,
                p.Title,
                p.Type,
                p.Status,
                p.CreatedAtUtc,
                _db.Claims.Count(c =>
                c.PostId == p.Id &&
                c.IsActive &&
                (c.Status == ClaimStatus.Pending || c.Status == ClaimStatus.Standby)
            ),
                _db.Claims
                .Where(c => c.PostId == p.Id && c.IsActive && c.Status == ClaimStatus.Accepted)
                .Select(c => (int?)c.Conversation!.Id)
                .FirstOrDefault()
            ))
            .ToListAsync(ct);

        return Ok(myPosts);
    }

    [Authorize]
    [HttpPost("{id:int}/close")]
    public async Task<IActionResult> Close(int id, [FromBody] ClosePostRequest? body, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (post is null)
            return NotFound();

        var isAdminOrModerator = User.IsInRole("Admin") || User.IsInRole("Moderator");
        var isOwner = post.CreatedByUserId == userId;

        // Reglas:
        // - Open -> Closed: owner o admin/mod
        // - OnClaim -> Closed: solo admin/mod
        // - Resolved: no se cierra (400)

        if (post.Status == PostStatus.Resolved)
            return BadRequest("Post ya resuelto.");

        // Si hay algún pending el creador no puede cerrar el post
        if (post.Status == PostStatus.Open && isOwner && !isAdminOrModerator)
        {
            var hasPending = await _db.Claims
                .AsNoTracking()
                .AnyAsync(c => c.PostId == post.Id && c.Status == ClaimStatus.Pending, ct);

            if (hasPending)
                return BadRequest("No puedes cerrar el post mientras existan reclamaciones pendientes. Recházalas o resuélvelas primero.");
        }


        if (post.Status == PostStatus.InClaim && !isAdminOrModerator)
            return Forbid();

        if (!isOwner && !isAdminOrModerator)
            return Forbid();

        if (post.Status == PostStatus.Closed)
            return NoContent();

        post.Status = PostStatus.Closed;
        post.ClosedAtUtc = DateTime.UtcNow;
        post.ClosedByUserId = userId;
        post.ClosedReason = body?.Reason;

        var now = DateTime.UtcNow;

        var closeClaimsStatus = isAdminOrModerator && !isOwner
            ? ClaimStatus.Rejected
            : ClaimStatus.Expired;

        var activeClaims = await _db.Claims
            .Include(c => c.Conversation)
            .Where(c => c.PostId == post.Id && c.IsActive)
            .ToListAsync(ct);

        foreach (var c in activeClaims)
        {
            c.Status = closeClaimsStatus;
            c.IsActive = false;
            c.ResolvedAtUtc = now;

            if (c.Conversation is not null)
                c.Conversation.Status = ConversationStatus.ReadOnly;
        }

        await _db.SaveChangesAsync(ct);
        return NoContent();
    }
    /*
    [Authorize]
    [HttpPost("{id:int}/claims")]
    public async Task<IActionResult> CreateClaim(int id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId))
            return Unauthorized();

        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (post is null)
            return NotFound();

        if (post.CreatedByUserId == userId)
            return BadRequest("No puedes reclamar tu propio post.");

        if (post.Status != PostStatus.Open)
            return BadRequest("Solo se puede reclamar un post en estado Open.");

        var already = await _db.Claims.AnyAsync(c =>
            c.PostId == id &&
            c.ClaimantUserId == userId &&
            c.Status != ClaimStatus.Withdrawn &&
            c.Status != ClaimStatus.Rejected &&
            c.Status != ClaimStatus.Expired, ct);

        if (already)
            return Conflict("Ya has reclamado este post.");

        var claim = new Data.Entities.Claim
        {
            PostId = id,
            ClaimantUserId = userId,
            Status = ClaimStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow,

            ExpiresAtUtc = null,
            ResolvedAtUtc = null,
            Conversation = null
        };

        _db.Claims.Add(claim);

        await _db.SaveChangesAsync(ct);

        return Created($"/api/claims/{claim.Id}", new
        {
            claim.Id,
            claim.PostId,
            claim.Status,
            claim.CreatedAtUtc
        });
    }*/

    [Authorize]
    [HttpPost("{id:int}/claims")]
    public async Task<IActionResult> CreateClaim(int id, [FromBody] CreateClaimRequest body, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var text = (body?.Message ?? "").Trim();
        if (text.Length < 2) return BadRequest("El mensaje es obligatorio.");
        if (text.Length > 1000) return BadRequest("Mensaje demasiado largo (máx 1000).");

        var post = await _db.Posts.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (post is null) return NotFound();

        if (post.CreatedByUserId == userId) return BadRequest("No puedes reclamar tu propio post.");
        if (post.Status == PostStatus.Closed)
            return BadRequest("Este post está cerrado. No se pueden crear reclamaciones nuevas.");
        if (post.Status == PostStatus.Resolved)
            return BadRequest("Este post ya está resuelto.");
        if (post.Status == PostStatus.InClaim)
            return BadRequest("Este post ya está en proceso de reclamación.");
        /*
        var already = await _db.Claims.AnyAsync(c =>
            c.PostId == id &&
            c.ClaimantUserId == userId &&
            c.Status != ClaimStatus.Withdrawn &&
            c.Status != ClaimStatus.Rejected &&
            c.Status != ClaimStatus.Expired, ct);*/

        var already = await _db.Claims.AnyAsync(c =>
            c.PostId == id &&
            c.ClaimantUserId == userId &&
            c.IsActive, ct);

        if (already) return Conflict("Ya has reclamado este post.");

        var claim = new Data.Entities.Claim
        {
            PostId = id,
            ClaimantUserId = userId,
            Status = ClaimStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };

        var conversation = new Conversation
        {
            Claim = claim,
            Status = ConversationStatus.Active,
            CreatedAtUtc = DateTime.UtcNow,
            Messages = new List<Message>
        {
            new Message
            {
                SenderUserId = userId,
                Body = text,
                CreatedAtUtc = DateTime.UtcNow
            }
        }
        };

        _db.Conversations.Add(conversation);

        var now = DateTime.UtcNow;

        conversation.LastMessageAtUtc = now;
        conversation.LastMessageByUserId = userId;

        conversation.ClaimantLastReadAtUtc = now;
        conversation.OwnerLastReadAtUtc = null;


        await _db.SaveChangesAsync(ct);

        return Created($"/api/claims/{claim.Id}", new { claimId = claim.Id, conversationId = conversation.Id });
    }




}

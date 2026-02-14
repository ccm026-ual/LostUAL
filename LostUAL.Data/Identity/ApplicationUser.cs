using Microsoft.AspNetCore.Identity;

namespace LostUAL.Data.Identity;

public class ApplicationUser : IdentityUser
{
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}

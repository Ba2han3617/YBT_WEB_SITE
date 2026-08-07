using Microsoft.AspNetCore.Identity;

namespace Ybt.Core.Entities;

public class AppUser : IdentityUser<int>
{
    public string? FullName { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Faculty { get; set; }
    public string? TcNo { get; set; }
    public string? StudentNumber { get; set; }
    public string? Address { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

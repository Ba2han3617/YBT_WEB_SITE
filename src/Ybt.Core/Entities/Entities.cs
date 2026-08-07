namespace Ybt.Core.Entities;

public class Event : BaseEntity
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime EventDate { get; set; }
    public string Location { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public string Slug { get; set; } = null!;
    
    public ICollection<EventApplication> Applications { get; set; } = new List<EventApplication>();
}

public class Blog : BaseEntity
{
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public string Slug { get; set; } = null!;
    public int AuthorId { get; set; }
    public AppUser Author { get; set; } = null!;
}

public class Project : BaseEntity
{
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string? GitHubUrl { get; set; }
    public string? TechTags { get; set; } // Comma separated tags
    public string? ImageUrl { get; set; }
}

public class EventApplication : BaseEntity
{
    public int EventId { get; set; }
    public Event Event { get; set; } = null!;
    public int UserId { get; set; }
    public AppUser User { get; set; } = null!;
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
}

public class TeamMember : BaseEntity
{
    public string FullName { get; set; } = null!;
    public string Role { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public int Order { get; set; } = 0;
}

using Ybt.Core.Entities;

namespace Ybt.Web.Models;

public class HomeViewModel
{
    // --- Metrik sayıları ---
    public int MemberCount { get; set; } = 500;      // Sabit
    public int EventCount { get; set; } = 0;          // DB'den
    public int ActiveProjectCount { get; set; } = 0; // DB'den
    public int BlogCount { get; set; } = 0;           // DB'den

    // --- Listeler ---
    public IEnumerable<Event> UpcomingEvents { get; set; } = Enumerable.Empty<Event>();
    public IEnumerable<Project> FeaturedProjects { get; set; } = Enumerable.Empty<Project>();
    public IEnumerable<Blog> RecentBlogs { get; set; } = Enumerable.Empty<Blog>();
    public IEnumerable<TeamMember> TeamMembers { get; set; } = Enumerable.Empty<TeamMember>();
}

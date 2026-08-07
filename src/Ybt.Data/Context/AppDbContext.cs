using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Ybt.Core.Entities;

namespace Ybt.Data.Context;

public class AppDbContext : IdentityDbContext<AppUser, AppRole, int>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Event> Events { get; set; }
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<EventApplication> EventApplications { get; set; }
    public DbSet<TeamMember> TeamMembers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Configuration for entities if needed
        builder.Entity<Event>().HasIndex(e => e.Slug).IsUnique();
        builder.Entity<Blog>().HasIndex(b => b.Slug).IsUnique();
        
        builder.Entity<EventApplication>()
            .HasOne(ea => ea.Event)
            .WithMany(e => e.Applications)
            .HasForeignKey(ea => ea.EventId);

        builder.Entity<EventApplication>()
            .HasOne(ea => ea.User)
            .WithMany()
            .HasForeignKey(ea => ea.UserId);
    }
}

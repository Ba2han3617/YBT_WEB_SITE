using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Ybt.Core.Entities;
using Ybt.Data.Context;

namespace Ybt.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
    {
        // Roles
        if (!await roleManager.RoleExistsAsync("Admin"))
        {
            await roleManager.CreateAsync(new AppRole { Name = "Admin" });
        }
        if (!await roleManager.RoleExistsAsync("User"))
        {
            await roleManager.CreateAsync(new AppRole { Name = "User" });
        }

        // Admin User
        var adminUserName = "admin";
        var adminEmail = "admin@ybt.com";
        var admin = await userManager.FindByNameAsync(adminUserName) ?? await userManager.FindByEmailAsync(adminEmail);

        if (admin == null)
        {
            var initialPassword = Environment.GetEnvironmentVariable("ADMIN_INITIAL_PASSWORD") ?? "Admin123*";
            admin = new AppUser
            {
                UserName = adminUserName,
                Email = adminEmail,
                FullName = "System Admin",
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, initialPassword);
        }
        else
        {
            if (admin.UserName != adminUserName)
            {
                admin.UserName = adminUserName;
                await userManager.UpdateAsync(admin);
            }
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Sample Events
        if (!await context.Events.AnyAsync())
        {
            context.Events.AddRange(new List<Event>
            {
                new Event { Title = "Hackathon 2026", Description = "Büyük yazılım maratonu başlıyor!", EventDate = DateTime.UtcNow.AddDays(30), Location = "İstanbul", Slug = "hackathon-2026", IsActive = true },
                new Event { Title = "AI Workshop", Description = "Yapay zeka dünyasına giriş.", EventDate = DateTime.UtcNow.AddDays(15), Location = "Ankara", Slug = "ai-workshop", IsActive = true }
            });
        }

        // Sample Blogs
        if (!await context.Blogs.AnyAsync())
        {
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser != null)
            {
                context.Blogs.AddRange(new List<Blog>
                {
                    new Blog { Title = "Yazılıma Nereden Başlamalı?", Content = "Yeni başlayanlar için yol haritası...", Slug = "yazilima-nereden-baslamali", AuthorId = adminUser.Id, IsActive = true },
                    new Blog { Title = "Modern Web Teknolojileri", Content = "2026'da öne çıkan teknolojiler.", Slug = "modern-web-teknolojileri", AuthorId = adminUser.Id, IsActive = true }
                });
            }
        }

        // Sample Team Members
        if (!await context.TeamMembers.AnyAsync())
        {
            context.TeamMembers.AddRange(new List<TeamMember>
            {
                new TeamMember { FullName = "Batuhan Y.", Role = "Başkan", ImageUrl = "https://i.pravatar.cc/300?img=1", Order = 1, IsActive = true },
                new TeamMember { FullName = "Merve K.", Role = "Başkan Yardımcısı", ImageUrl = "https://i.pravatar.cc/300?img=2", Order = 2, IsActive = true },
                new TeamMember { FullName = "Can E.", Role = "Teknik Koordinatör", ImageUrl = "https://i.pravatar.cc/300?img=3", Order = 3, IsActive = true },
                new TeamMember { FullName = "Elif S.", Role = "Organizasyon", ImageUrl = "https://i.pravatar.cc/300?img=4", Order = 4, IsActive = true }
            });
        }

        await context.SaveChangesAsync();
    }
}

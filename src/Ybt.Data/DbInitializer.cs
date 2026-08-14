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
                new Event
                {
                    Title = "YBT Hackathon 2026: Yapay Zeka ve Gelecek",
                    Description = "48 saatlik kesintisiz kodlama maratonunda yapay zeka, sürdürülebilirlik ve akıllı şehirler alanlarında yenilikçi projeler geliştiriyoruz. Mentör desteği, sürpriz ödüller ve sektör liderleriyle tanışma fırsatı seni bekliyor!",
                    EventDate = DateTime.UtcNow.AddDays(25).AddHours(10),
                    Location = "Düzce Üniversitesi Mühendislik Fakültesi Konferans Salonu",
                    Slug = "ybt-hackathon-2026",
                    ImageUrl = "https://images.unsplash.com/photo-1504384308090-c894fdcc538d?ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&q=80",
                    IsActive = true
                },
                new Event
                {
                    Title = "Modern Web ve Bulut Mimarileri Atölyesi",
                    Description = ".NET 9, Microservices, Docker ve modern frontend mimarilerinin ele alınacağı uygulamalı atölye çalışması. Gerçek bir senaryo üzerinden uçtan uca mimari geliştirme deneyimi yaşayacağız.",
                    EventDate = DateTime.UtcNow.AddDays(14).AddHours(14),
                    Location = "Merkezi Derslikler B Blok - Amfi 2",
                    Slug = "modern-web-bulut-mimarileri",
                    ImageUrl = "https://images.unsplash.com/photo-1517245386807-bb43f82c33c4?ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&q=80",
                    IsActive = true
                },
                new Event
                {
                    Title = "Siber Güvenlik & CTF Maratonu 2026",
                    Description = "Web güvenliği, tersine mühendislik ve kriptografi alanlarında bilgi ve yeteneklerini test edebileceğin heyecan dolu bayrak yakalama (CTF) yarışması.",
                    EventDate = DateTime.UtcNow.AddDays(40).AddHours(11),
                    Location = "YBT İnovasyon ve Yazılım Laboratuvarı",
                    Slug = "siber-guvenlik-ctf-2026",
                    ImageUrl = "https://images.unsplash.com/photo-1550751827-4bd374c3f58b?ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&q=80",
                    IsActive = true
                }
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
                    new Blog
                    {
                        Title = "2026'da Yazılım Dünyası: Trendler ve Kariyer Yol Haritası",
                        Content = "Teknoloji ekosistemi her geçen gün daha dinamik bir yapıya evriliyor. Yapay zeka asistanlarının geliştirme süreçlerine entegrasyonu, bulut yerel (cloud-native) mimarilerin standartlaşması ve siber güvenlik odaklı yazılım geliştirme yaklaşımları bu yılın en kritik başlıkları arasında yer alıyor.\n\nÖğrencilik yıllarında açık kaynak projelere katkı sağlamak, takım çalışması pratiği kazanmak ve topluluk etkinliklerinde aktif rol almak kariyer yolculuğunuzun en sağlam temellerini oluşturur. YBT olarak her dönem güncel teknolojileri birlikte deneyimliyor ve projeler üretiyoruz.",
                        Slug = "2026da-yazilim-dunyasi-trendler",
                        AuthorId = adminUser.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1498050108023-c5249f4df085?ixlib=rb-1.2.1&auto=format&fit=crop&w=1200&q=80",
                        IsActive = true
                    },
                    new Blog
                    {
                        Title = "Açık Kaynak Projelere Katkı Sağlama Rehberi",
                        Content = "Açık kaynak dünyası yalnızca kod yazmaktan ibaret değildir; dokümantasyon hazırlamak, hata raporlamak, topluluk tartışmalarına katılmak da bu kültürün vazgeçilmez bir parçasıdır.\n\nGitHub üzerinde ilk pull request'inizi (PR) oluştururken dikkat etmeniz gereken kurallar, temiz commit mesajları yazma alışkanlığı ve kod standartlarına uyum sağlamak sizi bir adım öne taşır. Topluluğumuz bünyesinde geliştirdiğimiz projelere katkıda bulunarak ilk adımı atabilirsiniz.",
                        Slug = "acik-kaynak-projelere-katki-rehberi",
                        AuthorId = adminUser.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1556075798-4825dfaaf498?ixlib=rb-1.2.1&auto=format&fit=crop&w=1200&q=80",
                        IsActive = true
                    },
                    new Blog
                    {
                        Title = "Yapay Zeka Destekli Modern Web Uygulamaları",
                        Content = "Büyük Dil Modelleri (LLM) ve üretken yapay zeka araçları web uygulamalarının çehresini tamamen değiştiriyor. Kullanıcı deneyimini kişiselleştiren akıllı arama sistemleri, anlık içerik analizi ve otomatik öneri mekanizmaları artık modern web projelerinin merkezinde yer alıyor.\n\nBu yazımızda modern web çatılarında yapay zeka servislerinin nasıl entegre edildiğini ve yüksek performanslı mimarilerin nasıl kurgulandığını inceliyoruz.",
                        Slug = "yapay-zeka-destekli-web-uygulamalari",
                        AuthorId = adminUser.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?ixlib=rb-1.2.1&auto=format&fit=crop&w=1200&q=80",
                        IsActive = true
                    }
                });
            }
        }

        // Sample Projects
        if (!await context.Projects.AnyAsync())
        {
            context.Projects.AddRange(new List<Project>
            {
                new Project
                {
                    Name = "YBT Web Portalı & Üyelik Sistemi",
                    Description = "Topluluk üyelerinin etkinliklere başvurduğu, blog yazılarını takip ettiği ve açık kaynak ekosistemine katıldığı modern kurumsal web platformu.",
                    TechTags = ".NET 9, PostgreSQL, Entity Framework Core, Bootstrap 5, Razor Views",
                    GitHubUrl = "https://github.com/du-yazilim/ybt-web-portal",
                    ImageUrl = "https://images.unsplash.com/photo-1460925895917-afdab827c52f?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=80",
                    IsActive = true
                },
                new Project
                {
                    Name = "DÜ Kampüs Rehberi & Etkinlik Asistanı",
                    Description = "Üniversite yerleşkesindeki akademik birimleri, kütüphane doluluk durumunu ve kulüp etkinliklerini harita üzerinde anlık gösteren mobil/web rehber uygulaması.",
                    TechTags = "Flutter, REST API, Leaflet, ASP.NET Core, Docker",
                    GitHubUrl = "https://github.com/du-yazilim/du-campus-guide",
                    ImageUrl = "https://images.unsplash.com/photo-1526778548025-fa2f459cd5c1?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=80",
                    IsActive = true
                },
                new Project
                {
                    Name = "Açık Kaynak DevPath Yol Haritası",
                    Description = "Yazılıma yeni başlayan üniversite öğrencileri için Web, Mobil, Veri Bilimi ve Siber Güvenlik alanlarında hazırlanmış interaktif Türkçe kaynak ve eğitim rehberi.",
                    TechTags = "Markdown, Docsify, Open Source, Git, CI/CD",
                    GitHubUrl = "https://github.com/du-yazilim/devpath-roadmap",
                    ImageUrl = "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=80",
                    IsActive = true
                },
                new Project
                {
                    Name = "YBT Discord Topluluk Otomasyon Botu",
                    Description = "Topluluğun 1000'den fazla üyeye sahip Discord sunucusunda rol yönetimi, teknik soru-cevap arşivi ve etkinlik duyurularını otomatikleştiren bot projesi.",
                    TechTags = "Node.js, Discord.js, TypeScript, Redis, Docker",
                    GitHubUrl = "https://github.com/du-yazilim/ybt-discord-bot",
                    ImageUrl = "https://images.unsplash.com/photo-1614680376593-902f749f7ffc?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=80",
                    IsActive = true
                }
            });
        }

        // Sample Team Members
        if (!await context.TeamMembers.AnyAsync())
        {
            context.TeamMembers.AddRange(new List<TeamMember>
            {
                new TeamMember { FullName = "Batuhan Yılmaz", Role = "Yönetim Kurulu Başkanı", ImageUrl = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=300&q=80", Order = 1, IsActive = true },
                new TeamMember { FullName = "Merve Kaya", Role = "Başkan Yardımcısı", ImageUrl = "https://images.unsplash.com/photo-1517841905240-472988babdf9?auto=format&fit=crop&w=300&q=80", Order = 2, IsActive = true },
                new TeamMember { FullName = "Can Eren", Role = "Yazılım ve Teknoloji Koordinatörü", ImageUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=300&q=80", Order = 3, IsActive = true },
                new TeamMember { FullName = "Elif Şahin", Role = "Organizasyon ve Etkinlik Koordinatörü", ImageUrl = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=300&q=80", Order = 4, IsActive = true }
            });
        }

        await context.SaveChangesAsync();
    }
}

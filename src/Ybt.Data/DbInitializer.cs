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
                SecurityStamp = Guid.NewGuid().ToString(),
                EmailConfirmed = true
            };
            await userManager.CreateAsync(admin, initialPassword);
        }
        else
        {
            if (string.IsNullOrEmpty(admin.SecurityStamp))
            {
                admin.SecurityStamp = Guid.NewGuid().ToString();
            }
            if (admin.UserName != adminUserName)
            {
                admin.UserName = adminUserName;
            }
            await userManager.UpdateAsync(admin);
        }

        if (!await userManager.IsInRoleAsync(admin, "Admin"))
        {
            await userManager.AddToRoleAsync(admin, "Admin");
        }

        // Seed / Update Events with Capacity, Category, Speaker
        var existingEvents = await context.Events.ToListAsync();
        if (!existingEvents.Any())
        {
            var event1 = new Event
            {
                Title = "YBT Hackathon 2026: Yapay Zeka ve Gelecek",
                Description = "48 saatlik kesintisiz kodlama maratonunda yapay zeka, sürdürülebilirlik ve akıllı şehirler alanlarında yenilikçi projeler geliştiriyoruz. Mentör desteği, sürpriz ödüller ve sektör liderleriyle tanışma fırsatı seni bekliyor!",
                EventDate = DateTime.UtcNow.AddDays(25).AddHours(10),
                Location = "Düzce Üniversitesi Mühendislik Fakültesi Konferans Salonu",
                Slug = "ybt-hackathon-2026",
                ImageUrl = "https://images.unsplash.com/photo-1504384308090-c894fdcc538d?ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&q=80",
                Capacity = 100,
                Category = "Hackathon",
                Speaker = "YBT Yönetim & Sektör Mentörleri",
                IsActive = true
            };
            var event2 = new Event
            {
                Title = "Modern Web ve Bulut Mimarileri Atölyesi",
                Description = ".NET 9, Microservices, Docker ve modern frontend mimarilerinin ele alınacağı uygulamalı atölye çalışması. Gerçek bir senaryo üzerinden uçtan uca mimari geliştirme deneyimi yaşayacağız.",
                EventDate = DateTime.UtcNow.AddDays(14).AddHours(14),
                Location = "Merkezi Derslikler B Blok - Amfi 2",
                Slug = "modern-web-bulut-mimarileri",
                ImageUrl = "https://images.unsplash.com/photo-1517245386807-bb43f82c33c4?ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&q=80",
                Capacity = 60,
                Category = "Workshop & Atölye",
                Speaker = "Kıdemli Bulut Mimarı Burak Erdem",
                IsActive = true
            };
            var event3 = new Event
            {
                Title = "Siber Güvenlik & CTF Maratonu 2026",
                Description = "Web güvenliği, tersine mühendislik ve kriptografi alanlarında bilgi ve yeteneklerini test edebileceğin heyecan dolu bayrak yakalama (CTF) yarışması.",
                EventDate = DateTime.UtcNow.AddDays(40).AddHours(11),
                Location = "YBT İnovasyon ve Yazılım Laboratuvarı",
                Slug = "siber-guvenlik-ctf-2026",
                ImageUrl = "https://images.unsplash.com/photo-1550751827-4bd374c3f58b?ixlib=rb-1.2.1&auto=format&fit=crop&w=1000&q=80",
                Capacity = 50,
                Category = "CTF & Siber Güvenlik",
                Speaker = "Siber Güvenlik Uzmanı Deniz Yılmaz",
                IsActive = true
            };
            context.Events.AddRange(event1, event2, event3);
            await context.SaveChangesAsync();
            existingEvents = new List<Event> { event1, event2, event3 };
        }
        else
        {
            foreach (var ev in existingEvents)
            {
                if (string.IsNullOrEmpty(ev.Category))
                {
                    ev.Category = ev.Title.Contains("Hackathon") ? "Hackathon" : (ev.Title.Contains("CTF") ? "CTF & Siber Güvenlik" : "Workshop & Atölye");
                    ev.Capacity = ev.Capacity ?? 75;
                    ev.Speaker = ev.Speaker ?? "YBT Teknik Ekip & Davetli Konuşmacı";
                }
            }
            await context.SaveChangesAsync();
        }

        // Demo Regular User (for testing applications flow)
        var demoUserEmail = "uye@ybt.com";
        var demoUser = await userManager.FindByEmailAsync(demoUserEmail);
        if (demoUser == null)
        {
            var newDemoUser = new AppUser
            {
                UserName = demoUserEmail,
                Email = demoUserEmail,
                FirstName = "Zeynep",
                LastName = "Kaya",
                FullName = "Zeynep Kaya",
                Faculty = "Mühendislik Fakültesi",
                Department = "Bilgisayar Mühendisliği",
                Grade = "3. Sınıf",
                StudentNumber = "210501042",
                TcNo = "11111111112",
                PhoneNumber = "05321234567",
                Address = "Düzce Üniversitesi Konuralp Yerleşkesi",
                Interests = "Web Geliştirme, Bulut Bilişim, Yapay Zeka",
                GitHubUrl = "https://github.com/zeynepkaya",
                LinkedInUrl = "https://linkedin.com/in/zeynepkaya",
                SecurityStamp = Guid.NewGuid().ToString(),
                EmailConfirmed = true
            };
            var createResult = await userManager.CreateAsync(newDemoUser, "Uye123*");
            if (createResult.Succeeded)
            {
                demoUser = await userManager.FindByEmailAsync(demoUserEmail);
                if (demoUser != null)
                {
                    await userManager.AddToRoleAsync(demoUser, "User");
                }
            }
        }

        // Sample Event Applications
        if (demoUser != null && !await context.EventApplications.AnyAsync() && existingEvents.Any())
        {
            var firstEvent = existingEvents[0];
            var secondEvent = existingEvents.Count > 1 ? existingEvents[1] : firstEvent;

            context.EventApplications.AddRange(new List<EventApplication>
            {
                new EventApplication
                {
                    EventId = firstEvent.Id,
                    UserId = demoUser.Id,
                    Status = "Onaylandı",
                    Notes = "Yapay zeka ve web geliştirme alanında projeler üretmek istiyorum. Takımımla birlikte katılacağım.",
                    AdminNotes = "Ön yeterlilik sağlandı. Onaylandı.",
                    CreatedAt = DateTime.UtcNow.AddDays(-3)
                },
                new EventApplication
                {
                    EventId = secondEvent.Id,
                    UserId = demoUser.Id,
                    Status = "Değerlendiriliyor",
                    Notes = ".NET 9 ve Docker mimarilerini öğrenmek istiyorum.",
                    AdminNotes = "Kontenjan durumu kontrol ediliyor.",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                }
            });
            await context.SaveChangesAsync();
        }

        // Seed / Update Blogs
        var existingBlogs = await context.Blogs.ToListAsync();
        if (!existingBlogs.Any())
        {
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser != null)
            {
                context.Blogs.AddRange(new List<Blog>
                {
                    new Blog
                    {
                        Title = "2026'da Yazılım Dünyası: Trendler ve Kariyer Yol Haritası",
                        Summary = "Yapay zeka asistanları, bulut yerel mimariler ve modern yazılım ekosisteminde öne çıkan kariyer fırsatları.",
                        Content = "Teknoloji ekosistemi her geçen gün daha dinamik bir yapıya evriliyor. Yapay zeka asistanlarının geliştirme süreçlerine entegrasyonu, bulut yerel (cloud-native) mimarilerin standartlaşması ve siber güvenlik odaklı yazılım geliştirme yaklaşımları bu yılın en kritik başlıkları arasında yer alıyor.\n\nÖğrencilik yıllarında açık kaynak projelere katkı sağlamak, takım çalışması pratiği kazanmak ve topluluk etkinliklerinde aktif rol almak kariyer yolculuğunuzun en sağlam temellerini oluşturur. YBT olarak her dönem güncel teknolojileri birlikte deneyimliyor ve projeler üretiyoruz.",
                        Category = "Kariyer & Teknoloji",
                        Tags = "Yapay Zeka, Bulut, Kariyer, .NET",
                        Slug = "2026da-yazilim-dunyasi-trendler",
                        AuthorId = adminUser.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1498050108023-c5249f4df085?ixlib=rb-1.2.1&auto=format&fit=crop&w=1200&q=80",
                        IsActive = true
                    },
                    new Blog
                    {
                        Title = "Açık Kaynak Projelere Katkı Sağlama Rehberi",
                        Summary = "GitHub üzerinde ilk pull request'inizi açmaktan temiz kod prensiplerine açık kaynak kültürü.",
                        Content = "Açık kaynak dünyası yalnızca kod yazmaktan ibaret değildir; dokümantasyon hazırlamak, hata raporlamak, topluluk tartışmalarına katılmak da bu kültürün vazgeçilmez bir parçasıdır.\n\nGitHub üzerinde ilk pull request'inizi (PR) oluştururken dikkat etmeniz gereken kurallar, temiz commit mesajları yazma alışkanlığı ve kod standartlarına uyum sağlamak sizi bir adım öne taşır. Topluluğumuz bünyesinde geliştirdiğimiz projelere katkıda bulunarak ilk adımı atabilirsiniz.",
                        Category = "Açık Kaynak",
                        Tags = "GitHub, Git, Open Source, Topluluk",
                        Slug = "acik-kaynak-projelere-katki-rehberi",
                        AuthorId = adminUser.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1556075798-4825dfaaf498?ixlib=rb-1.2.1&auto=format&fit=crop&w=1200&q=80",
                        IsActive = true
                    },
                    new Blog
                    {
                        Title = "Yapay Zeka Destekli Modern Web Uygulamaları",
                        Summary = "LLM modelleri ve vektör veritabanları ile akıllı web mimarilerinin inşa edilmesi.",
                        Content = "Büyük Dil Modelleri (LLM) ve üretken yapay zeka araçları web uygulamalarının çehresini tamamen değiştiriyor. Kullanıcı deneyimini kişiselleştiren akıllı arama sistemleri, anlık içerik analizi ve otomatik öneri mekanizmaları artık modern web projelerinin merkezinde yer alıyor.\n\nBu yazımızda modern web çatılarında yapay zeka servislerinin nasıl entegre edildiğini ve yüksek performanslı mimarilerin nasıl kurgulandığını inceliyoruz.",
                        Category = "Yazılım Geliştirme",
                        Tags = "Web, Yapay Zeka, Mimari, API",
                        Slug = "yapay-zeka-destekli-web-uygulamalari",
                        AuthorId = adminUser.Id,
                        ImageUrl = "https://images.unsplash.com/photo-1618005182384-a83a8bd57fbe?ixlib=rb-1.2.1&auto=format&fit=crop&w=1200&q=80",
                        IsActive = true
                    }
                });
            }
        }
        else
        {
            foreach (var b in existingBlogs)
            {
                if (string.IsNullOrEmpty(b.Category)) b.Category = "Yazılım & Teknoloji";
                if (string.IsNullOrEmpty(b.Summary)) b.Summary = b.Content.Length > 150 ? b.Content.Substring(0, 150) + "..." : b.Content;
                if (string.IsNullOrEmpty(b.Tags)) b.Tags = "Yazılım, Topluluk, Teknoloji";
            }
        }

        // Seed / Update Projects
        var existingProjects = await context.Projects.ToListAsync();
        if (!existingProjects.Any())
        {
            context.Projects.AddRange(new List<Project>
            {
                new Project
                {
                    Name = "YBT Web Portalı & Üyelik Sistemi",
                    Description = "Topluluk üyelerinin etkinliklere başvurduğu, blog yazılarını takip ettiği ve açık kaynak ekosistemine katıldığı modern kurumsal web platformu.",
                    TechTags = ".NET 9, PostgreSQL, Entity Framework Core, Bootstrap 5, Razor Views",
                    TeamMembers = "Batuhan Yılmaz, Can Eren, Zeynep Kaya",
                    GitHubUrl = "https://github.com/du-yazilim/ybt-web-portal",
                    DemoUrl = "http://localhost:5261",
                    ImageUrl = "https://images.unsplash.com/photo-1460925895917-afdab827c52f?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=80",
                    IsActive = true
                },
                new Project
                {
                    Name = "DÜ Kampüs Rehberi & Etkinlik Asistanı",
                    Description = "Üniversite yerleşkesindeki akademik birimleri, kütüphane doluluk durumunu ve kulüp etkinliklerini harita üzerinde anlık gösteren mobil/web rehber uygulaması.",
                    TechTags = "Flutter, REST API, Leaflet, ASP.NET Core, Docker",
                    TeamMembers = "Batuhan Yılmaz, Merve Kaya, Emre Demir",
                    GitHubUrl = "https://github.com/du-yazilim/du-campus-guide",
                    DemoUrl = "https://kampus.duzce.edu.tr",
                    ImageUrl = "https://images.unsplash.com/photo-1526778548025-fa2f459cd5c1?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=80",
                    IsActive = true
                },
                new Project
                {
                    Name = "Açık Kaynak DevPath Yol Haritası",
                    Description = "Yazılıma yeni başlayan üniversite öğrencileri için Web, Mobil, Veri Bilimi ve Siber Güvenlik alanlarında hazırlanmış interaktif Türkçe kaynak ve eğitim rehberi.",
                    TechTags = "Markdown, Docsify, Open Source, Git, CI/CD",
                    TeamMembers = "YBT Eğitim ve Yazılım Ekibi",
                    GitHubUrl = "https://github.com/du-yazilim/devpath-roadmap",
                    DemoUrl = "https://devpath.ybt.org",
                    ImageUrl = "https://images.unsplash.com/photo-1516321318423-f06f85e504b3?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=80",
                    IsActive = true
                },
                new Project
                {
                    Name = "YBT Discord Topluluk Otomasyon Botu",
                    Description = "Topluluğun 1000'den fazla üyeye sahip Discord sunucusunda rol yönetimi, teknik soru-cevap arşivi ve etkinlik duyurularını otomatikleştiren bot projesi.",
                    TechTags = "Node.js, Discord.js, TypeScript, Redis, Docker",
                    TeamMembers = "Can Eren, Selin Aksoy",
                    GitHubUrl = "https://github.com/du-yazilim/ybt-discord-bot",
                    DemoUrl = "https://discord.gg/ybt",
                    ImageUrl = "https://images.unsplash.com/photo-1614680376593-902f749f7ffc?ixlib=rb-1.2.1&auto=format&fit=crop&w=800&q=80",
                    IsActive = true
                }
            });
        }
        else
        {
            foreach (var p in existingProjects)
            {
                if (string.IsNullOrEmpty(p.DemoUrl)) p.DemoUrl = p.GitHubUrl;
                if (string.IsNullOrEmpty(p.TeamMembers)) p.TeamMembers = "YBT Proje Geliştirme Ekibi";
            }
        }

        // Seed / Update Team Members
        var existingTeam = await context.TeamMembers.ToListAsync();
        if (!existingTeam.Any())
        {
            context.TeamMembers.AddRange(new List<TeamMember>
            {
                new TeamMember
                {
                    FullName = "Batuhan Yılmaz",
                    Role = "Yönetim Kurulu Başkanı",
                    Email = "batu@ybt.com",
                    LinkedInUrl = "https://linkedin.com/in/batuhanyilmaz",
                    GitHubUrl = "https://github.com/batuhanyilmaz",
                    Bio = "Bilgisayar Mühendisliği öğrencisi. Açık kaynak ve bulut mimarileri üzerine çalışıyor.",
                    ImageUrl = "https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&w=300&q=80",
                    Order = 1,
                    IsActive = true
                },
                new TeamMember
                {
                    FullName = "Merve Kaya",
                    Role = "Başkan Yardımcısı",
                    Email = "merve@ybt.com",
                    LinkedInUrl = "https://linkedin.com/in/mervekaya",
                    GitHubUrl = "https://github.com/mervekaya",
                    Bio = "Yazılım geliştirme ve topluluk yönetimi odaklı çalışıyor.",
                    ImageUrl = "https://images.unsplash.com/photo-1517841905240-472988babdf9?auto=format&fit=crop&w=300&q=80",
                    Order = 2,
                    IsActive = true
                },
                new TeamMember
                {
                    FullName = "Can Eren",
                    Role = "Yazılım ve Teknoloji Koordinatörü",
                    Email = "can@ybt.com",
                    LinkedInUrl = "https://linkedin.com/in/caneren",
                    GitHubUrl = "https://github.com/caneren",
                    Bio = ".NET, Python ve yapay zeka alanlarında projeler geliştiriyor.",
                    ImageUrl = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?auto=format&fit=crop&w=300&q=80",
                    Order = 3,
                    IsActive = true
                },
                new TeamMember
                {
                    FullName = "Elif Şahin",
                    Role = "Organizasyon ve Etkinlik Koordinatörü",
                    Email = "elif@ybt.com",
                    LinkedInUrl = "https://linkedin.com/in/elifsahin",
                    GitHubUrl = "https://github.com/elifsahin",
                    Bio = "Hackathon, atölye ve seminer organizasyonlarını koordine ediyor.",
                    ImageUrl = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?auto=format&fit=crop&w=300&q=80",
                    Order = 4,
                    IsActive = true
                }
            });
        }
        else
        {
            foreach (var t in existingTeam)
            {
                if (string.IsNullOrEmpty(t.Email)) t.Email = "iletisim@ybt.com";
                if (string.IsNullOrEmpty(t.LinkedInUrl)) t.LinkedInUrl = "https://linkedin.com";
                if (string.IsNullOrEmpty(t.GitHubUrl)) t.GitHubUrl = "https://github.com";
                if (string.IsNullOrEmpty(t.Bio)) t.Bio = "Yazılım ve Bilişim Topluluğu yönetim ekibi üyesi.";
            }
        }

        // Seed About Features (Neler Yapıyoruz)
        var existingFeatures = await context.AboutFeatures.ToListAsync();
        if (!existingFeatures.Any())
        {
            context.AboutFeatures.AddRange(new List<AboutFeature>
            {
                new AboutFeature
                {
                    Title = "Yazılım Eğitimleri",
                    Description = "Sıfırdan ileri seviyeye Web geliştirme, Python, C#, Mobil uygulama ve Yapay zeka eğitim serileri düzenliyoruz.",
                    Icon = "bi-code-slash",
                    Tags = "Web Dev, Python, .NET",
                    AccentType = "cyan",
                    Order = 1,
                    IsActive = true
                },
                new AboutFeature
                {
                    Title = "Hackathon & Yarışmalar",
                    Description = "Teknofest, bölgesel maratonlar ve küresel kodlama yarışmalarında takımlarımızla fikirlerimizi prototipe dönüştürüyoruz.",
                    Icon = "bi-trophy",
                    Tags = "Teknofest, Hackathon, Yarışma",
                    AccentType = "purple",
                    Order = 2,
                    IsActive = true
                },
                new AboutFeature
                {
                    Title = "Açık Kaynak Projeler",
                    Description = "GitHub üzerinde topluluk projeleri üretiyor, açık kaynak projelere katkı vererek gerçek dünya deneyimi kazanıyoruz.",
                    Icon = "bi-box-seam",
                    Tags = "GitHub, Open Source, DevOps",
                    AccentType = "teal",
                    Order = 3,
                    IsActive = true
                },
                new AboutFeature
                {
                    Title = "Teknik Atölyeler",
                    Description = "Docker, Git/GitHub, Cloud architecture ve CI/CD süreçleri gibi sektörün talep ettiği araçları pratik olarak çalışıyoruz.",
                    Icon = "bi-cpu",
                    Tags = "Docker, Git, CI/CD",
                    AccentType = "teal",
                    Order = 4,
                    IsActive = true
                },
                new AboutFeature
                {
                    Title = "Mentörlük & Kariyer",
                    Description = "Sektördeki mezunlarımız ve kıdemli mühendislerle birebir mentörlük oturumları, CV incelemeleri ve staj rehberliği sunuyoruz.",
                    Icon = "bi-mortarboard",
                    Tags = "Mentörlük, Staj, Kariyer",
                    AccentType = "cyan",
                    Order = 5,
                    IsActive = true
                },
                new AboutFeature
                {
                    Title = "Blog & Teknik Yayın",
                    Description = "Öğrencilerimizin öğrendiği teknolojileri ve ürettiği çözümleri teknik makaleler ile topluluğa aktarmasını sağlıyoruz.",
                    Icon = "bi-journal-code",
                    Tags = "Teknik Makale, Medium, Blog",
                    AccentType = "purple",
                    Order = 6,
                    IsActive = true
                }
            });
        }

        await context.SaveChangesAsync();
    }
}

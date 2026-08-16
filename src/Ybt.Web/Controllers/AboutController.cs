using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ybt.Core.Entities;
using Ybt.Data.Context;
using Ybt.Web.Models;

namespace Ybt.Web.Controllers;

public class AboutController : Controller
{
    private readonly AppDbContext _context;

    public AboutController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var model = new AboutViewModel();

        try
        {
            model.TeamMembers = await _context.TeamMembers
                .Where(m => m.IsActive)
                .OrderBy(m => m.Order)
                .ToListAsync();
        }
        catch
        {
            model.TeamMembers = new();
        }

        try
        {
            var hasAnyFeatures = await _context.AboutFeatures.AnyAsync();
            if (!hasAnyFeatures)
            {
                var defaultFeatures = new List<AboutFeature>
                {
                    new AboutFeature { Title = "Yazılım Eğitimleri", Description = "Sıfırdan ileri seviyeye Web geliştirme, Python, C#, Mobil uygulama ve Yapay zeka eğitim serileri düzenliyoruz.", Icon = "bi-code-slash", Tags = "Web Dev, Python, .NET", AccentType = "cyan", Order = 1, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new AboutFeature { Title = "Hackathon & Yarışmalar", Description = "Teknofest, bölgesel maratonlar ve küresel kodlama yarışmalarında takımlarımızla fikirlerimizi prototipe dönüştürüyoruz.", Icon = "bi-trophy", Tags = "Teknofest, Hackathon, Yarışma", AccentType = "purple", Order = 2, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new AboutFeature { Title = "Açık Kaynak Projeler", Description = "GitHub üzerinde topluluk projeleri üretiyor, açık kaynak projelere katkı vererek gerçek dünya deneyimi kazanıyoruz.", Icon = "bi-box-seam", Tags = "GitHub, Open Source, DevOps", AccentType = "teal", Order = 3, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new AboutFeature { Title = "Teknik Atölyeler", Description = "Docker, Git/GitHub, Cloud architecture ve CI/CD süreçleri gibi sektörün talep ettiği araçları pratik olarak çalışıyoruz.", Icon = "bi-cpu", Tags = "Docker, Git, CI/CD", AccentType = "teal", Order = 4, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new AboutFeature { Title = "Mentörlük & Kariyer", Description = "Sektördeki mezunlarımız ve kıdemli mühendislerle birebir mentörlük oturumları, CV incelemeleri ve staj rehberliği sunuyoruz.", Icon = "bi-mortarboard", Tags = "Mentörlük, Staj, Kariyer", AccentType = "cyan", Order = 5, IsActive = true, CreatedAt = DateTime.UtcNow },
                    new AboutFeature { Title = "Blog & Teknik Yayın", Description = "Öğrencilerimizin öğrendiği teknolojileri ve ürettiği çözümleri teknik makaleler ile topluluğa aktarmasını sağlıyoruz.", Icon = "bi-journal-code", Tags = "Teknik Makale, Medium, Blog", AccentType = "purple", Order = 6, IsActive = true, CreatedAt = DateTime.UtcNow }
                };

                await _context.AboutFeatures.AddRangeAsync(defaultFeatures);
                await _context.SaveChangesAsync();
            }

            model.AboutFeatures = await _context.AboutFeatures
                .Where(f => f.IsActive)
                .OrderBy(f => f.Order)
                .ToListAsync();
        }
        catch
        {
            model.AboutFeatures = new();
        }

        return View(model);
    }
}

# YBT Web Sitesi

YBT Web Sitesi, Yazılım & Bilişim Topluluğu için geliştirilen modern bir topluluk yönetim platformudur. Site; etkinlikler, projeler, blog içerikleri, kullanıcı başvuruları, profil yönetimi ve içerik yönetimi özelliklerini tek çatı altında toplar.

## Özellikler

- Modern ve responsive kullanıcı arayüzü
- Kullanıcı kayıt/giriş sistemi
- Profil ve başvuru yönetimi
- Etkinlik listeleme ve etkinlik başvurusu
- Proje vitrini
- Blog ve teknik içerik sayfaları
- İletişim formu
- İçerik yönetimi
- Etkinlik, proje, blog, ekip ve hakkımızda içerik yönetimi
- Başvuru inceleme ve durum güncelleme
- Rol tabanlı yetkilendirme
- PostgreSQL veritabanı desteği
- Docker ile çalıştırılabilir yapı

## Kullanılan Teknolojiler

- ASP.NET Core MVC
- Entity Framework Core
- PostgreSQL
- ASP.NET Core Identity
- Bootstrap
- HTML, CSS, JavaScript
- Docker
- GitHub Actions / CodeQL

## Proje Yapısı

```text
src/
  Ybt.Web/       Web uygulaması, controller, view ve statik dosyalar
  Ybt.Core/      Entity/model sınıfları
  Ybt.Data/      DbContext, migrations ve seed işlemleri
```

## Veritabanı

Projede PostgreSQL kullanılmaktadır. Veritabanı işlemleri Entity Framework Core üzerinden yürütülür.

DbContext dosyası:

```text
src/Ybt.Data/Context/AppDbContext.cs
```

Migration dosyaları:

```text
src/Ybt.Data/Migrations/
```

Uygulama başlangıcında migration ve seed işlemleri çalıştırılır:

```csharp
context.Database.Migrate();
await DbInitializer.SeedAsync(context, userManager, roleManager);
```

## Kurulum

Önce projeyi klonlayın:

```bash
git clone <repo-url>
cd "YBT WEB SİTESİ"
```

Gerekli paketleri yükleyin:

```bash
dotnet restore
```

Veritabanı bağlantısını ayarlayın.

Development ortamı için `appsettings.Development.json` içinde connection string tanımlanabilir:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=ybt_db;Username=postgres;Password=your_password"
  }
}
```

Uygulamayı çalıştırın:

```bash
dotnet run --project src/Ybt.Web
```

Varsayılan local adres:

```text
http://localhost:5261
```

## Environment Variables

Production ortamında hassas bilgiler dosyada tutulmamalıdır. Connection string environment variable olarak verilmelidir:

```bash
ConnectionStrings__DefaultConnection="Host=...;Port=5432;Database=...;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true"
```

## Yetkilendirme

Uygulamada ASP.NET Core Identity tabanlı rol yönetimi kullanılmaktadır. Yönetim işlemlerine yalnızca yetkili kullanıcılar erişebilir.

## Docker ile Çalıştırma

Docker image oluşturmak için:

```bash
docker build -t ybt-web-sitesi .
```

Local test için:

```bash
docker run --rm -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="DB_CONNECTION_STRING" \
  ybt-web-sitesi
```

Tarayıcıdan açın:

```text
http://localhost:8080
```

## Docker Hub’a Gönderme

Image’ı etiketleyin:

```bash
docker tag ybt-web-sitesi ba2han3617/ybt-web-sitesi:latest
```

Docker Hub’a gönderin:

```bash
docker push ba2han3617/ybt-web-sitesi:latest
```

Sunucuda çalıştırmak için:

```bash
docker run -d \
  --name ybt-web-sitesi \
  --restart unless-stopped \
  -p 80:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="DB_CONNECTION_STRING" \
  ba2han3617/ybt-web-sitesi:latest
```

## Güvenlik Notları

- Production ortamında veritabanı şifresi koda yazılmamalıdır.
- Yönetim işlemleri sadece yetkili kullanıcıya açık olmalıdır.
- Varsayılan yetkili kullanıcı şifresi production ortamında kullanılmamalıdır.
- CodeQL uyarıları düzenli olarak kontrol edilmelidir.
- Üçüncü parti vendor dosyaları doğrudan düzenlenmemelidir.

## Geliştirme Notları

Yeni özellik eklerken:

```bash
git checkout -b feature/ozellik-adi
```

Değişiklikleri commit’leyin:

```bash
git add .
git commit -m "feat: açıklayıcı commit mesajı"
```

Remote’a gönderin:

```bash
git push
```

## Lisans

Bu proje Yazılım & Bilişim Topluluğu için geliştirilmiştir.

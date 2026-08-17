# YBT Web Canlı Yayın ve Yayına Alma (Deployment) Rehberi

Bu doküman, YBT ASP.NET Core 9 web uygulamasını ve PostgreSQL veritabanını canlı ortama (Production) taşımak için hazırlanmış eksiksiz canlı yayın rehberidir.

---

## 🔑 1. Gereken Çevre Değişkenleri (Environment Variables)

Canlı ortamda güvenlik nedeniyle hassas bilgiler koda veya `appsettings.json` dosyasına **yazılmamalıdır**. Bunun yerine aşağıdaki çevre değişkenleri sunucuda/cloud panelinde tanımlanmalıdır:

| Değişken Adı | Açıklama | Örnek Değer |
| :--- | :--- | :--- |
| `ASPNETCORE_ENVIRONMENT` | Çalışma ortamı türü | `Production` |
| `ConnectionStrings__DefaultConnection` | Standart Npgsql PostgreSQL bağlantı cümlesi | `Host=prod-db.example.com;Port=5432;Database=ybt_prod;Username=ybt_user;Password=GuvliSifre123!` |
| `DATABASE_URL` *(Alternatif Cloud)* | Render, Railway, Supabase, Neon platformlarının sağladığı URL formatı | `postgres://ybt_user:GuvliSifre123!@ep-cool-db.eu-central-1.aws.neon.tech:5432/ybt_prod` |
| `ADMIN_INITIAL_PASSWORD` | Canlı veritabanı boşsa oluşturulacak ilk Admin şifresi | `GüvenliAdminŞifresi2026!*` |
| `ASPNETCORE_URLS` | Uygulamanın dinleyeceği port (Docker/Container için) | `http://+:8080` |

---

## 🗄️ 2. PostgreSQL Connection String Örnekleri

### A) Standart Npgsql Formatı (`ConnectionStrings__DefaultConnection`)
```env
ConnectionStrings__DefaultConnection="Host=127.0.0.1;Port=5432;Database=ybt_prod_db;Username=ybt_admin;Password=UltraSecurePass2026!;SSL Mode=Prefer;"
```

### B) Single-Line Cloud URL (`DATABASE_URL`)
*(Supabase, Render PostgreSQL, Railway, Neon DB vb. platformlarda doğrudan sunulur)*
```env
DATABASE_URL="postgresql://ybt_user:SecurePassword123@db.supabase.co:5432/postgres"
```
> **Not:** Uygulama açılışında `DATABASE_URL` tespit edildiğinde otomatik olarak Npgsql uyumlu connection string'e dönüştürülür.

---

## 🛠️ 3. Manuel Derleme ve Yayına Alma (Publish)

Yerel makinenizde veya CI/CD boru hattınızda canlı yayın paketini oluşturmak için:

```bash
# Projeyi Release modunda derleyip publish klasörüne çıkarın
dotnet publish "src/Ybt.Web/Ybt.Web.csproj" -c Release -o ./publish
```

Üretilen `./publish` klasörü içeriğini Linux VPS / Windows IIS sunucunuza yükleyebilirsiniz.

Sunucuda çalıştırma komutu:
```bash
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Host=localhost;Database=ybt_db;Username=postgres;Password=CANLI_SIFRE"
dotnet Ybt.Web.dll
```

---

## 🐳 4. Docker Container İle Canlıya Alma

Proje kökünde hazır multi-stage `.NET 9` `Dockerfile` bulunmaktadır.

### Docker Image Oluşturma:
```bash
docker build -t ybt-web-app:latest .
```

### Docker Container Çalıştırma:
```bash
docker run -d \
  --name ybt_web_container \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Host=your-db-host;Database=ybt_prod;Username=ybt_user;Password=YourPassword123!" \
  -e ADMIN_INITIAL_PASSWORD="SecureAdminPass2026!*" \
  ybt-web-app:latest
```

---

## ⚡ 5. Veritabanı Migration ve Seed Davranışı

Uygulama canlı ortamda ayağa kalktığında (`Program.cs`):
1. **Migration Otomasyonu:** `context.Database.Migrate()` çalışarak veritabanında henüz uygulanmamış EF Core migration'larını (tablo ekleme, sütun güncelleme vb.) otomatik uygular.
2. **Idempotent Seed (Tekrarsız Veri Ekleme):** `DbInitializer.SeedAsync(...)` çalışır:
   - Eğer Admin kullanıcısı yoksa, `ADMIN_INITIAL_PASSWORD` çevre değişkeninde belirtilen şifre ile ilk Admin kullanıcısını (`admin@ybt.com` / `admin`) oluşturur.
   - Etkinlikler, projeler, bloglar veya ekip üyeleri tablosunda kayıt **varsa**, yeni veri eklemez (Duplicate kayıt oluşmaz).
   - Veritabanı sıfır/boş ise varsayılan başlangıç içeriklerini yükler.

---

## 🌐 6. Nginx Reverse Proxy ve SSL / HTTPS Ayarları (Linux VPS)

Uygulamanız Kestrel üzerinde `localhost:5261` veya Docker'da `8080` portunda çalışırken Nginx ters proxy ile dış dünyaya açılabilir.

### Nginx Konfigürasyonu (`/etc/nginx/sites-available/ybt.conf`):
```nginx
server {
    listen 80;
    server_name ybt.org.tr www.ybt.org.tr;

    location / {
        proxy_pass         http://127.0.0.1:8080;
        proxy_http_version 1.1;
        proxy_set_header   Upgrade $http_upgrade;
        proxy_set_header   Connection keep-alive;
        proxy_set_header   Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header   X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header   X-Forwarded-Proto $scheme;
    }
}
```

> **Forwarded Headers:** `Program.cs` içerisinde `app.UseForwardedHeaders()` middleware'i yapılandırıldığı için `X-Forwarded-For` ve `X-Forwarded-Proto` başlıkları üzerinden HTTPS yönlendirmeleri ve cookie güvenlik politikaları sorunsuz çalışacaktır.

### Ücretsiz SSL (Let's Encrypt / Certbot):
```bash
sudo apt update
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d ybt.org.tr -d www.ybt.org.tr
```

---

## 🛡️ 7. Production Güvenlik Kontrol Listesi

- [x] **Developer Exception Page:** Production'da kapalı. Hatalar `/Home/Error` üzerinden kullanıcı dostu gösterilir.
- [x] **Hassas Veri Gizliliği:** Canlı veritabanı şifreleri `appsettings.json` içinde tutulmaz, Environment Variable ile beslenir.
- [x] **Çerez Güvenliği:** Production'da `CookieSecurePolicy.Always` ve `SameSiteMode.Lax` aktif.
- [x] **Güvenlik Başlıkları:** `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, `Referrer-Policy` aktif.
- [x] **Rate Limiting:** `/adminstrator` giriş ekranı ve kritik endpoint'lerde `strict-limit` aktif.

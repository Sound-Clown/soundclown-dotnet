# SoundClown

Music streaming app — Blazor Server + PostgreSQL + Cloudinary.

## Tech Stack

- **ASP.NET Core 8** (Blazor Server)
- **PostgreSQL** via EF Core
- **Cloudinary** — upload audio & cover images
- **Cookie Authentication**
- **Tailwind CSS** (CDN)

---

## Start / Stop

### 1. PostgreSQL

```bash
# Start
docker compose up -d

# Stop
docker compose down

# Stop + xóa data
docker compose down -v
```

### 2. App

```bash
# Chạy app (HTTP, port 5000)
dotnet run --urls "http://localhost:5000"
```

Truy cập: **http://localhost:5000**

### Stop App

```bash
# Ctrl+C trong terminal đang chạy
```

### Watch mode (tự rebuild khi sửa code)

```bash
dotnet watch run --urls "http://localhost:5000"
```

---

## Config

Mở `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=soundclown;Username=postgres;Password=postgres"
  },
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  }
}
```

### Cloudinary Setup (miễn phí)

1. Tạo tài khoản tại [cloudinary.com](https://cloudinary.com)
2. Tạo 2 folder: `music-app/audio` và `music-app/covers`
3. Copy credentials vào `appsettings.json`

---

## Default Accounts

| Role     | Email                | Password       |
| -------- | -------------------- | -------------- |
| Admin    | `admin@music.com`    | `Admin123456!` |
| Listener | `listener@music.com` | `Listener123!` |
| Artist   | `artist@music.com`   | `Artist123!`   |

Đăng nhập tại `/login`.

---

## Workflow

1. **Listener** — nghe nhạc, like, share, đổi password
2. **Artist** — upload bài → chờ admin duyệt → hiển thị công khai
3. **Admin** — duyệt/reject bài, lock user

Bài hát sau khi upload có trạng thái `Pending` → Admin duyệt → `Approved`. Artist sửa bài → reset về `Pending`.

---

## Database

App tự tạo bảng khi khởi động (`EnsureCreated`). Để xóa và tạo lại:

```bash
# Xóa DB
docker compose exec musicdb psql -U postgres -c "DROP DATABASE soundclown;"

# App sẽ tự tạo lại khi restart
dotnet run --urls "http://localhost:5000"
```

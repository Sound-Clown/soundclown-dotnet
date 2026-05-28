# SoundClown

Ứng dụng nghe nhạc trực tuyến — Blazor Server + PostgreSQL + Cloudinary.

---

## Công nghệ sử dụng

| Lớp           | Công nghệ                                                       |
| ------------- | --------------------------------------------------------------- |
| Framework     | ASP.NET Core 8 (Blazor Server, interactive SSR)                 |
| Cơ sở dữ liệu | PostgreSQL 16 qua EF Core + Npgsql                              |
| Xác thực      | Cookie-based, BCrypt (cost 12)                                  |
| Media         | Cloudinary .NET SDK (upload audio + ảnh bìa)                    |
| Email         | MailKit SMTP (luồng reset mật khẩu)                             |
| CSS           | Tailwind CDN + dark theme tự thiết kế (`app.css`, accent `#F5A623`) |
| Audio         | `HTMLAudioElement` qua JS Interop (`wwwroot/js/player.js`)       |

---

## Cấu trúc thư mục

```
soundclown-mvp/
├── Program.cs                            # DI, middleware pipeline, cấu hình auth
├── appsettings.json / .Development.json  # Cấu hình local (DB, Cloudinary, Mail, Auth)
│
├── Components/
│   ├── App.razor                        # Root Blazor component
│   ├── Routes.razor                     # Entry point cho routing
│   ├── _Imports.razor                   # @using / @inject directives toàn cục
│   │
│   ├── Layout/
│   │   ├── MainLayout.razor             # Shell Sidebar + PlayerBar (trang đã đăng nhập)
│   │   └── AuthLayout.razor             # Layout giữa màn hình (login/register)
│   │
│   ├── Admin/
│   │   ├── AdminSongs.razor             # Hàng đợi duyệt bài + panel approve/reject
│   │   └── AdminUsers.razor             # Danh sách user + toggle khóa/mở khóa
│   │
│   ├── Artist/
│   │   ├── ArtistAlbums.razor           # CRUD album, quản lý bài trong album
│   │   ├── ArtistSongs.razor            # Danh sách bài của Artist + sửa/xóa
│   │   ├── ArtistStats.razor            # Thống kê lượt nghe/like (biểu đồ cột)
│   │   └── ArtistUpload.razor           # Form upload audio + ảnh bìa
│   │
│   ├── Auth/
│   │   ├── Login.razor                  # Form đăng nhập + tab đăng ký
│   │   ├── Register.razor               # Đăng ký + chọn role + thanh độ mạnh mật khẩu
│   │   ├── ForgotPassword.razor         # Gửi email reset mật khẩu
│   │   └── ResetPassword.razor          # Đặt lại mật khẩu bằng token từ email
│   │
│   ├── Main/
│   │   ├── Home.razor                   # Lưới bài hát đã duyệt, có ô tìm kiếm
│   │   ├── Search.razor                 # Tìm kiếm full-text có debounce
│   │   ├── Settings.razor               # Đổi mật khẩu (≥8 ký tự, thanh độ mạnh)
│   │   ├── SongDetail.razor             # Chi tiết bài: play, like, share
│   │   └── AlbumDetail.razor            # Album: ảnh bìa + danh sách bài, phát theo queue
│   │
│   └── Shared/
│       ├── SongCard.razor               # Card lưới (ảnh bìa, overlay play, like)
│       ├── SongRow.razor                # Dòng list (cho search / album)
│       ├── SongStatusBadge.razor        # Badge Pending / Approved / Rejected
│       ├── RoleBadge.razor              # Badge Listener / Artist / Admin
│       ├── PlayerBar.razor              # Player cố định phía dưới (SSR-safe, JS Interop)
│       ├── ConfirmDialog.razor          # Modal xác nhận (xóa)
│       ├── EmptyState.razor             # Icon + thông báo cho list rỗng
│       └── LoadingSpinner.razor         # Loading indicator
│
├── Controllers/
│   ├── AuthController.cs                # MVC fallback: POST /auth/login, /auth/logout
│   └── TestApiController.cs             # API endpoints phục vụ kiểm thử bằng Postman
│
├── Data/
│   ├── AppDbContext.cs                  # EF Core DbContext + cấu hình entity
│   ├── DbSeeder.cs                      # Seed tài khoản admin/listener/artist khi khởi động
│   └── SongSeeder.cs                    # Seed 1000 bài hát giả + 80 album cho demo
│
├── DTOs/
│   ├── AlbumDto.cs                      # AlbumDetailDto, AlbumListDto
│   ├── ArtistSearchDto.cs
│   ├── AuthDto.cs                       # RegisterDto, LoginDto, ChangePasswordDto, ResetPasswordDto
│   ├── PagedResult.cs                   # Wrapper phân trang (Items, Total, Page, PageSize)
│   ├── ServiceResult.cs                 # Wrapper kết quả (IsSuccess, Data, Error, FieldErrors)
│   ├── SongDto.cs
│   ├── StatsDto.cs
│   ├── UploadResult.cs                  # Url + PublicId từ Cloudinary
│   └── UserDto.cs
│
├── Entities/
│   ├── User.cs                          # Id, Username, Email, PasswordHash, Role, IsActive, CreatedAt
│   ├── Song.cs                          # Id, Title, AudioFile, CoverImage, ArtistId, AlbumId?, Status, RejectReason, PlayCount, LikeCount, CreatedAt
│   ├── Album.cs                         # Id, Name, CoverImage, ArtistId, CreatedAt
│   ├── Like.cs                          # Composite PK (UserId+SongId), cascade delete
│   └── PasswordResetToken.cs            # UserId(unique), Token(unique), ExpiresAt(30 phút)
│
├── Enums/
│   ├── Role.cs                          # Listener, Artist, Admin
│   └── SongStatus.cs                    # Pending, Approved, Rejected
│
├── Services/                            # Tất cả Scoped (Blazor Server DI)
│   ├── IAuthService.cs / AuthService.cs           # Đăng ký, login, logout, quên/đổi mật khẩu
│   ├── ICurrentUserService.cs / CurrentUserService.cs  # Wrapper ClaimsPrincipal (UserId, Role, IsAdmin, IsArtist)
│   ├── ISongService.cs / SongService.cs           # CRUD bài hát, phân trang, search, toggleLike
│   ├── IAlbumService.cs / AlbumService.cs         # CRUD album + addSong / removeSong
│   ├── IAdminService.cs / AdminService.cs         # Duyệt bài (approve/reject), khóa user
│   ├── IUploadService.cs / UploadService.cs       # Cloudinary: UploadAudio/Image/DeleteFile
│   ├── IPlayerService.cs / PlayerService.cs       # Queue, current song, sự kiện phát nhạc
│   ├── IToastService.cs / ToastService.cs         # Sự kiện toast (Success/Error/Info/Warning)
│   └── IEmailService.cs / EmailService.cs         # MailKit: SendResetPasswordEmailAsync
│
├── wwwroot/
│   ├── app.css                          # Bootstrap import + CSS tự thiết kế
│   ├── bootstrap/bootstrap.min.css      # Bootstrap 5.3 base
│   ├── css/app.css                      # Biến CSS dark theme, utilities, components
│   └── js/
│       ├── player.js                    # globalThis.musicPlayer + schedulePlayCount (timer 30s)
│       └── helpers.js                   # copyToClipboard, scrollToTop, readDropFile, showToast
│
├── docs/
│   ├── Đảm Bảo Chất Lượng Phần Mềm.docx # Báo cáo đề tài (bản chính)
│   ├── diagrams/usecase.puml            # Source PlantUML lược đồ Use Case
│   └── scripts/                         # Script Python chỉnh sửa báo cáo .docx
│
└── tests/                               # Tổng 41 testcase (20 manual + 16 unit + 5 E2E)
    ├── README.md                        # ⭐ Hướng dẫn chạy test — bắt đầu ở đây
    ├── MANUAL_TESTS.md                  # Chi tiết 20 testcase thủ công (browser + Postman)
    ├── postman/                         # Postman collection cho API test
    ├── fixtures/                        # Script sinh file MP3 test
    ├── SoundClown.UnitTests/            # xUnit + EF Core InMemory + Coverlet
    └── SoundClown.E2ETests/             # Playwright + Chromium headless
```

---

## Tính năng

### Xác thực & phân quyền

- Đăng nhập bằng Cookie (hết hạn sau 7 ngày)
- 3 vai trò: **Listener** (người nghe), **Artist** (nghệ sĩ), **Admin** (kiểm duyệt)
- Thanh độ mạnh mật khẩu (≥8 ký tự, kiểm tra chữ hoa/thường, số, ký tự đặc biệt)
- Reset mật khẩu qua email token (hết hạn sau 30 phút)
- Sidebar điều hướng theo role (Listener chỉ thấy Trang chủ; Artist thấy thêm menu upload/album/stats)

### Vòng đời bài hát

1. **Artist** upload audio + ảnh bìa (tùy chọn) → bài hát ở trạng thái `Pending`
2. **Admin** duyệt → chuyển `Approved` (công khai) hoặc `Rejected` (kèm lý do tùy chọn)
3. Artist sửa bài → status reset về `Pending` chờ duyệt lại

### Phát nhạc

- HTML5 `<audio>` qua JS Interop (không reload trang)
- Hệ thống queue: phát từng bài, phát tất cả, phát từ album/kết quả tìm kiếm
- Lượt nghe được tính sau khi nghe đủ 30 giây (timer JS → callback Blazor `OnPlayThreshold`)
- Toggle Like cập nhật số lượt thích real-time

### Tìm kiếm

- Tìm kiếm có debounce 300ms trên trang chủ
- Full-text search theo tên bài hát + tên nghệ sĩ

### Admin Panel

- Duyệt bài Pending: approve / reject kèm lý do
- Quản lý user: khóa / mở khóa tài khoản

### Artist Dashboard

- Upload bài hát (audio MP3 ≤10MB, ảnh bìa JPG/PNG/WebP ≤2MB)
- Quản lý bài của mình: sửa title/ảnh bìa/album, xóa
- Quản lý album: tạo, sửa, thêm/bỏ bài
- Thống kê: tổng lượt nghe, tổng lượt like

---

## Cấu hình

```bash
cp .env.example .env   # điền credentials vào file .env
```

Sau đó cập nhật `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=soundclown;Username=postgres;Password=postgres"
  },
  "Cloudinary": {
    "CloudName": "your-cloud-name",
    "ApiKey": "your-api-key",
    "ApiSecret": "your-api-secret"
  },
  "Mail": {
    "Host": "smtp.example.com",
    "Port": 587,
    "Username": "user@example.com",
    "Password": "your-smtp-password"
  }
}
```

### Đăng ký Cloudinary (miễn phí)

1. Đăng ký tại [cloudinary.com](https://cloudinary.com)
2. Tạo 2 folder: `soundclown/audio` và `soundclown/covers`
3. Copy Cloud Name, API Key, API Secret vào `appsettings.json`

---

## Khởi chạy / Dừng

### 1. PostgreSQL

```bash
# Khởi động
docker compose up -d

# Dừng (giữ dữ liệu)
docker compose down

# Dừng + xóa dữ liệu
docker compose down -v
```

### 2. App

```bash
# Chạy bình thường (HTTP, port 5000)
dotnet run --urls "http://localhost:5000"

# Chế độ watch (tự build lại khi file thay đổi)
dotnet watch run --urls "http://localhost:5000"
```

Truy cập: **http://localhost:5000**

### Reset cơ sở dữ liệu

```bash
docker exec -it soundclown-db psql -U postgres -c "DROP DATABASE soundclown;"
dotnet run --urls "http://localhost:5000"   # app tự tạo lại schema khi khởi động
```

---

## Tài khoản mặc định

| Vai trò  | Email                | Mật khẩu       |
| -------- | -------------------- | -------------- |
| Admin    | `admin@music.com`    | `Admin123456!` |
| Listener | `listener@demo.com`  | `Listener123!` |
| Artist   | `artist@demo.com`    | `Artist123!`   |

Trang đăng nhập: **http://localhost:5000/login**

---

## Quy trình vận hành

```
Listener  → Duyệt trang chủ, tìm kiếm, phát, like, chia sẻ, đổi mật khẩu
Artist    → Upload bài (Pending) → Chờ admin duyệt → Công khai
Admin     → Duyệt / Từ chối bài Pending, khóa/mở khóa user
```

Trạng thái bài hát sau khi upload: `Pending` → Admin duyệt → `Approved` (hiển thị công khai). Artist sửa bài → reset về `Pending` để chờ duyệt lại.

---

## Kiểm thử

Đề tài có **41 testcase** tổng cộng:

| Loại                       | Số lượng | Công cụ                             |
| -------------------------- | -------: | ----------------------------------- |
| Manual (browser + Postman) |       20 | Trình duyệt + Postman               |
| Unit test (tự động)        |       16 | xUnit + EF Core InMemory + Coverlet |
| E2E test (tự động)         |        5 | Playwright .NET + Chromium headless |

📖 **Hướng dẫn chạy test đầy đủ** → [tests/README.md](tests/README.md)

Chạy nhanh các test tự động:

```bash
# Unit test (cô lập, không cần app chạy)
dotnet test tests/SoundClown.UnitTests

# E2E (cần app + Postgres chạy ở localhost:5000)
docker compose up -d
dotnet run --urls "http://localhost:5000" &
dotnet test tests/SoundClown.E2ETests
```

Chi tiết 20 manual TC kèm ảnh minh hoạ → [tests/MANUAL_TESTS.md](tests/MANUAL_TESTS.md).

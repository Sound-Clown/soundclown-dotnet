# SoundClown

Music streaming app — Blazor Server + PostgreSQL + Cloudinary.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Framework | ASP.NET Core 8 (Blazor Server, interactive SSR) |
| Database | PostgreSQL 16 via EF Core + Npgsql |
| Auth | Cookie-based, BCrypt (cost 12) |
| Media | Cloudinary .NET SDK (audio + image upload) |
| Email | MailKit SMTP (reset-password flow) |
| CSS | Tailwind CDN + custom dark theme (`app.css`, accent `#F5A623`) |
| Audio | `HTMLAudioElement` via JS Interop (`wwwroot/js/player.js`) |

---

## Project Structure

```
soundclown-mvp/
├── Program.cs                           # DI, middleware pipeline, auth config
├── appsettings.json / .Development.json  # Local config (DB, Cloudinary, Mail, Auth)
│
├── Components/
│   ├── App.razor                        # Root Blazor component
│   ├── Routes.razor                     # Routing entry
│   ├── _Imports.razor                   # Global @using / @inject directives
│   │
│   ├── Layout/
│   │   ├── MainLayout.razor             # Sidebar + PlayerBar shell (auth pages)
│   │   └── AuthLayout.razor             # Full-screen centered layout (login/register)
│   │
│   ├── Admin/
│   │   ├── AdminSongs.razor             # Song review queue + approve/reject panel
│   │   └── AdminUsers.razor             # User list + lock/unlock toggle
│   │
│   ├── Artist/
│   │   ├── ArtistAlbums.razor           # Album CRUD, manage songs in album
│   │   ├── ArtistSongs.razor            # Own songs list + inline edit + delete
│   │   ├── ArtistStats.razor            # Play/like totals with bar chart
│   │   └── ArtistUpload.razor           # Audio + cover upload form
│   │
│   ├── Auth/
│   │   ├── Login.razor                  # Login form + inline register tab
│   │   ├── Register.razor               # Registration with role selection + password strength meter
│   │   ├── ForgotPassword.razor         # Request password reset email
│   │   └── ResetPassword.razor          # Reset with token from email link
│   │
│   ├── Main/
│   │   ├── Home.razor                  # Approved songs grid with search
│   │   ├── Search.razor                 # Debounced full-text search + results
│   │   ├── Settings.razor               # Change password form (≥8 chars, strength meter)
│   │   ├── SongDetail.razor             # Single song: play, like, share, lyric-less
│   │   └── AlbumDetail.razor            # Album cover + song list, queue playback
│   │
│   └── Shared/
│       ├── SongCard.razor              # Grid card with cover, play overlay, like
│       ├── SongRow.razor                # List row for search / album results
│       ├── SongStatusBadge.razor       # Pending / Approved / Rejected badge
│       ├── RoleBadge.razor             # Listener / Artist / Admin badge
│       ├── PlayerBar.razor             # Persistent bottom player (SSR-safe, JS Interop)
│       ├── ConfirmDialog.razor         # Modal confirmation (delete confirm)
│       ├── EmptyState.razor            # Icon + message for empty lists
│       └── LoadingSpinner.razor        # Loading indicator
│
├── Controllers/
│   └── AuthController.cs               # MVC fallback: POST /auth/login, /auth/logout
│
├── Data/
│   ├── AppDbContext.cs                 # EF Core DbContext + entity configuration
│   └── DbSeeder.cs                     # Seed admin/listener/artist accounts at startup
│
├── DTOs/
│   ├── AlbumDto.cs                    # AlbumDetailDto, AlbumListDto
│   ├── ArtistSearchDto.cs
│   ├── AuthDto.cs                      # RegisterDto, LoginDto, ChangePasswordDto, ResetPasswordDto
│   ├── PagedResult.cs                  # Pagination wrapper (Items, Total, Page, PageSize)
│   ├── ServiceResult.cs                # Generic/non-generic result wrapper (IsSuccess, Data, Error, FieldErrors)
│   ├── SongDto.cs
│   ├── StatsDto.cs
│   ├── UploadResult.cs                 # Url + PublicId from Cloudinary
│   └── UserDto.cs
│
├── Entities/
│   ├── User.cs                         # Id, Username, Email, PasswordHash, Role, IsActive, CreatedAt
│   ├── Song.cs                         # Id, Title, AudioFile, CoverImage, ArtistId(FK), AlbumId(FK,nullable), Status, RejectReason, PlayCount, LikeCount, CreatedAt
│   ├── Album.cs                        # Id, Name, CoverImage, ArtistId(FK), CreatedAt
│   ├── Like.cs                         # Composite PK (UserId+SongId), cascade delete
│   └── PasswordResetToken.cs           # UserId(unique), Token(unique), ExpiresAt(30min)
│
├── Enums/
│   ├── Role.cs                         # Listener, Artist, Admin
│   └── SongStatus.cs                   # Pending, Approved, Rejected
│
├── Models/
│   └── LoginRequest.cs
│
├── Services/                           # All Scoped (Blazor Server DI)
│   ├── IAuthService.cs / AuthService.cs          # Register, Login, Logout, ForgotPassword, ResetPassword, ChangePassword
│   ├── ICurrentUserService.cs / CurrentUserService.cs  # ClaimsPrincipal wrapper (UserId, Role, IsAdmin, IsArtist)
│   ├── ISongService.cs / SongService.cs            # CRUD, pagination, search, toggleLike, incrementPlay
│   ├── IAlbumService.cs / AlbumService.cs          # Album CRUD + addSong / removeSong
│   ├── IAdminService.cs / AdminService.cs          # Review (approve/reject), manage users (lock/unlock)
│   ├── IUploadService.cs / UploadService.cs        # Cloudinary: UploadAudioAsync, UploadImageAsync, DeleteFileAsync
│   ├── IPlayerService.cs / PlayerService.cs        # Queue, current song, playback events (Blazor Notification)
│   ├── IToastService.cs / ToastService.cs          # Toast events (Success/Error/Info/Warning → JS showToast)
│   └── IEmailService.cs / EmailService.cs         # MailKit: SendResetPasswordEmailAsync
│
└── wwwroot/
    ├── app.css                          # Bootstrap import + custom CSS
    ├── bootstrap/bootstrap.min.css      # Bootstrap 5.3 base
    ├── css/app.css                      # Dark theme variables, utilities, components
    └── js/
        ├── player.js                    # globalThis.musicPlayer + schedulePlayCount (30s JS timer)
        └── helpers.js                   # copyToClipboard, scrollToTop, readDropFile, showToast
```

---

## Features

### Authentication & Authorization
- Cookie-based login (7-day expiry)
- 3 roles: **Listener**, **Artist**, **Admin**
- Password strength meter (≥8 chars: checks length, mixed case, digits, special chars)
- Password reset via email token (30-min expiry)
- Role-based sidebar nav (Listeners see Home; Artists see extra menu items)

### Song Lifecycle
1. **Artist** uploads audio + optional cover → song status `Pending`
2. **Admin** reviews → `Approved` (public) or `Rejected` (with optional reason)
3. Artist edits song → status resets to `Pending`

### Playback
- HTML5 `<audio>` via JS Interop (no page reload)
- Queue system: play individual, play all, play from album/search
- Play count increments after 30s of audio play (JS timer → Blazor `OnPlayThreshold`)
- Like toggle with real-time count update

### Search
- Debounced search on home page (300ms)
- Full-text search by song title, artist name

### Admin Panel
- Review pending songs: approve / reject with reason
- Manage users: lock / unlock account

### Artist Dashboard
- Upload songs (audio MP3 ≤10MB, cover JPG/PNG/WebP ≤2MB)
- Manage own songs: edit title/cover/album, delete
- Album management: create, edit, add/remove songs
- Stats: total plays, total likes

---

## Config

```bash
cp .env.example .env   # fill in credentials
```

Then update `appsettings.json`:

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

### Cloudinary Setup (free)

1. Sign up at [cloudinary.com](https://cloudinary.com)
2. Create two folders: `soundclown/audio` and `soundclown/covers`
3. Copy Cloud Name, API Key, API Secret into `appsettings.json`

---

## Start / Stop

### 1. PostgreSQL

```bash
# Start
docker compose up -d

# Stop (keep data)
docker compose down

# Stop + delete data
docker compose down -v
```

### 2. App

```bash
# Normal run (HTTP, port 5000)
dotnet run --urls "http://localhost:5000"

# Watch mode (auto-rebuild on file change)
dotnet watch run --urls "http://localhost:5000"
```

Access: **http://localhost:5000**

### Reset database

```bash
docker compose exec musicdb psql -U postgres -c "DROP DATABASE soundclown;"
dotnet run --urls "http://localhost:5000"   # app re-creates tables on startup
```

---

## Default Accounts

| Role     | Email                | Password       |
|----------|----------------------|----------------|
| Admin    | `admin@music.com`    | `Admin123456!`  |
| Listener | `listener@music.com` | `Listener123!` |
| Artist   | `artist@music.com`   | `Artist123!`   |

Login at: **http://localhost:5000/login**

---

## Workflow

```
Listener        → Browse home, search, play, like, share, change password
Artist          → Upload song (Pending) → Wait for admin → Public
Admin           → Approve / Reject pending songs, lock/unlock users
```

Song status after upload: `Pending` → Admin approves → `Approved` (visible to all). Artist edits song → resets to `Pending`.
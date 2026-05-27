# SoundClown — Test Suite

Toàn bộ kiểm thử của đề tài Đảm Bảo Chất Lượng Phần Mềm: **41 testcase** chia làm 3 loại.

| Loại                       | Số lượng | Công cụ                             | TC                             |
| -------------------------- | -------: | ----------------------------------- | ------------------------------ |
| Manual (Browser + Postman) |       20 | Trình duyệt + Postman               | TC-01 → TC-20                  |
| Unit test (tự động)        |       16 | xUnit + EF Core InMemory + Coverlet | 9 SongService + 7 AdminService |
| E2E test (tự động)         |        5 | Playwright + Chromium headless      | TC-E2E-01 → TC-E2E-05          |

---

## 🚀 Quick Start

### 1. Unit Tests

```bash
cd tests/SoundClown.UnitTests
dotnet test                                          # chạy 16 test
dotnet test --collect:"XPlat Code Coverage"          # + coverage XML
```

Sinh HTML coverage report:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool   # 1 lần
reportgenerator \
  -reports:"tests/SoundClown.UnitTests/TestResults/**/coverage.cobertura.xml" \
  -targetdir:"docs/coverage-report" \
  -reporttypes:"Html;TextSummary" \
  -classfilters:"+MusicApp.Services.SongService;+MusicApp.Services.AdminService"
# → mở docs/coverage-report/index.html
```

### 2. E2E Tests

**Tiền điều kiện**:

```bash
docker start soundclown-db                                       # PostgreSQL
ASPNETCORE_URLS="http://localhost:5000" dotnet run --project .   # app ở :5000
```

**Cài Playwright browsers** (1 lần):

```bash
dotnet tool install -g Microsoft.Playwright.CLI
playwright install chromium
```

**Chạy E2E**:

```bash
cd tests/SoundClown.E2ETests
E2E_BASE_URL=http://localhost:5000 dotnet test
# → ảnh chụp tự động lưu vào tests/SoundClown.E2ETests/Screenshots/
```

### 3. Manual Tests (Postman + Browser)

Chi tiết 20 testcase với ảnh minh hoạ → xem [MANUAL_TESTS.md](MANUAL_TESTS.md).

```bash
bash tests/fixtures/create_test_files.sh    # tạo file MP3 test (1 lần)
# Import tests/postman/SoundClown_Test_Collection.json vào Postman
```

---

## 📁 Layout

```
tests/
├── README.md                         ← file này
├── MANUAL_TESTS.md                   ← 20 manual TC chi tiết
├── postman/
│   └── SoundClown_Test_Collection.json
├── fixtures/
│   ├── create_test_files.sh
│   └── files/                        ← MP3 test (sinh bằng script, .gitignored)
├── SoundClown.UnitTests/
│   ├── Helpers/
│   │   ├── TestDbFactory.cs          ← EF Core InMemory factory + seed
│   │   └── FakeCurrentUser.cs        ← stub ICurrentUserService
│   └── Services/
│       ├── SongServiceTests.cs       ← 9 test (ToggleLike, Update, Create, Search)
│       └── AdminServiceTests.cs      ← 7 test (ReviewSong, ToggleLockUser)
└── SoundClown.E2ETests/
    ├── PlaywrightFixture.cs          ← xUnit fixture share browser
    ├── Screenshots/                  ← output (.gitignored)
    └── Tests/
        ├── AuthFlowTests.cs          ← TC-E2E-01..03 (login flow)
        ├── HomeAndSearchTests.cs     ← TC-E2E-04..05 (home + search)
        └── ReportScreenshotter.cs    ← utility: screenshot coverage HTML
```

---

## 📊 Kết quả mong đợi

| Test suite                          |  Pass | Tiêu chí                           |
| ----------------------------------- | ----: | ---------------------------------- |
| Unit (xUnit)                        | 16/16 | Tất cả nhánh nghiệp vụ chính cover |
| E2E (Playwright)                    |   5/5 | Tất cả flow critical pass          |
| Coverage SongService + AdminService |     — | Line 49.1% / Branch 59.2%          |
| Manual                              | 20/20 | Đã chụp ảnh evidence (xem báo cáo) |

---

## ⚙️ Cấu hình & môi trường

- .NET SDK 8.0.x
- PostgreSQL 16 qua Docker (`docker-compose.yml` tại root)
- Cloudinary credentials trong `.env` (xem `.env.example`)
- Seed credentials (tạo bởi `Data/DbSeeder.cs`):
  - `listener@demo.com` / `Listener123!`
  - `artist@demo.com` / `Artist123!`
  - `admin@music.com` / `Admin123456!`
  - 8 fake artists (qua `Data/SongSeeder.cs`): `<username>@seed.local` / `Seed12345!`

---

## 🔄 Re-generate evidence cho báo cáo

Tất cả screenshot trong `tests/SoundClown.E2ETests/Screenshots/` được Playwright tự sinh khi chạy test. Để chụp lại coverage HTML report:

```bash
SCREENSHOT_COVERAGE=1 dotnet test \
  --filter "ScreenshotCoverageReport" \
  --project tests/SoundClown.E2ETests
# → tests/SoundClown.E2ETests/Screenshots/06-coverage-report.png
```

Sau khi có ảnh mới, swap vào docx bằng các script ở [docs/scripts/](../docs/scripts/).

---

## 📚 Tham khảo

- Báo cáo đầy đủ: `docs/Đảm Bảo Chất Lượng Phần Mềm.docx`
- Hướng dẫn viết báo cáo (thầy Hào): `docs/Viet_bao_cao_xay_dung_ung_dung.md`
- UC diagram source (PlantUML): `docs/diagrams/usecase.puml`

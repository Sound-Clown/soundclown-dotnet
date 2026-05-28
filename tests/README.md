# SoundClown — Test Suite

Toàn bộ kiểm thử của đề tài Đảm Bảo Chất Lượng Phần Mềm: **41 testcase** chia làm 3 loại.

| Loại                       | Số lượng | Công cụ                             | TC                             |
| -------------------------- | -------: | ----------------------------------- | ------------------------------ |
| Manual (Browser + Postman) |       20 | Trình duyệt + Postman               | TC-01 → TC-20                  |
| Unit test (tự động)        |       16 | xUnit + EF Core InMemory + Coverlet | 9 SongService + 7 AdminService |
| E2E test (tự động)         |        5 | Playwright + Chromium headless      | TC-E2E-01 → TC-E2E-05          |

## Tiền điều kiện

App đã chạy được ở `http://localhost:5000` (xem [README gốc](../README.md) — `.env`, Docker, dotnet run).

## 1. Unit test (xUnit)

Chạy trong cô lập, không cần app running:

```bash
dotnet test tests/SoundClown.UnitTests
```

Sinh kèm coverage report HTML:

```bash
dotnet tool install -g dotnet-reportgenerator-globaltool         # 1 lần
dotnet test tests/SoundClown.UnitTests --collect:"XPlat Code Coverage"
reportgenerator \
  -reports:"tests/SoundClown.UnitTests/TestResults/**/coverage.cobertura.xml" \
  -targetdir:"docs/coverage-report" \
  -reporttypes:"Html;TextSummary" \
  -classfilters:"+MusicApp.Services.SongService;+MusicApp.Services.AdminService"
# → mở docs/coverage-report/index.html
```

## 2. E2E test (Playwright)

Cài Chromium headless (1 lần duy nhất):

```bash
dotnet tool install -g Microsoft.Playwright.CLI
playwright install chromium
```

Chạy E2E (cần app + PostgreSQL đang chạy):

```bash
dotnet test tests/SoundClown.E2ETests
# ảnh chụp tự động lưu vào tests/SoundClown.E2ETests/Screenshots/
```

## 3. Manual test (Postman + Browser)

Chi tiết 20 testcase với ảnh minh hoạ → [MANUAL_TESTS.md](MANUAL_TESTS.md).

```bash
bash tests/fixtures/create_test_files.sh    # tạo file MP3 test (1 lần)
# Import tests/postman/SoundClown_Test_Collection.json vào Postman
```

## Cấu trúc

```
tests/
├── README.md                       file này
├── MANUAL_TESTS.md                 20 manual TC chi tiết
├── postman/                        Postman collection
├── fixtures/                       Script + folder file MP3 test
├── SoundClown.UnitTests/           xUnit + EF Core InMemory + Coverlet
└── SoundClown.E2ETests/            Playwright + Chromium headless
```

## Kết quả mong đợi

| Test suite                          |  Pass | Tiêu chí                           |
| ----------------------------------- | ----: | ---------------------------------- |
| Unit (xUnit)                        | 16/16 | Tất cả nhánh nghiệp vụ chính cover |
| E2E (Playwright)                    |   5/5 | Tất cả flow critical pass          |
| Coverage SongService + AdminService |     — | Line 49.1% / Branch 59.2%          |
| Manual                              | 20/20 | Đã chụp ảnh evidence (xem báo cáo) |

## Tài khoản dùng cho test

Seed sẵn bởi `Data/DbSeeder.cs`:

- `listener@demo.com` / `Listener123!`
- `artist@demo.com` / `Artist123!`
- `admin@music.com` / `Admin123456!`

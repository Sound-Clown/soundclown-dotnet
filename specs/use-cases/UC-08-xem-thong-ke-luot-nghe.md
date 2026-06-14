# UC-08: Xem thống kê lượt nghe

## Thông tin

| Thuộc tính | Giá trị |
|------------|---------|
| Mã UC | UC-08 |
| Tên | Xem thống kê lượt nghe |
| Actor chính | Artist |
| Trigger | Artist truy cập trang /artist/stats |
| Mức độ ưu tiên | Low |

## Mô tả

Artist theo dõi độ phổ biến tác phẩm thông qua trang thống kê. Trang hiển thị tổng lượt phát, tổng lượt thích, biểu đồ top bài hát theo lượt phát, và bảng chi tiết từng bài hát (PlayCount, LikeCount). Chỉ bài hát đã Approved mới được tính vào thống kê.

## Điều kiện trước

- Artist đã đăng nhập vào hệ thống
- Artist có vai trò Role = Artist hoặc Role = Admin

## Luồng chính — Xem thống kê

1. Artist truy cập trang /artist/stats
2. Hệ thống truy vấn tất cả bài hát Approved của Artist: `WHERE ArtistId == userId AND Status = Approved`
3. Hệ thống tính toán:
   - TotalPlays: tổng PlayCount của tất cả bài
   - TotalLikes: tổng LikeCount của tất cả bài
   - Tracks: danh sách bài hát sắp xếp theo PlayCount giảm dần
4. UI hiển thị:
   - 2 card tổng: "Tổng lượt phát" (TotalPlays), "Tổng lượt thích" (TotalLikes)
   - Biểu đồ bar chart: top 10 bài hát theo PlayCount (width % = PlayCount / maxPlayCount * 100)
   - Bảng chi tiết: #, Bài hát (cover + title), Phát, Thích
5. Số lớn được format: >= 1M hiển thị "X.XM", >= 1K hiển thị "X.XK"

## Luồng thay thế — Chưa có dữ liệu

1. Artist chưa có bài hát Approved nào
2. Hệ thống trả về StatsDto rỗng (TotalPlays = 0, TotalLikes = 0, Tracks = [])
3. UI hiển thị EmptyState: "Chưa có dữ liệu" / "Hãy upload và được duyệt ít nhất một bài hát."

## Điều kiện sau

- Artist thấy tổng quan lượt phát/thích và chi tiết từng bài hát

## Quy tắc nghiệp vụ

| Mã | Quy tắc |
|----|---------|
| BR-01 | Chỉ bài hát Approved mới được tính vào thống kê |
| BR-02 | Tracks sắp xếp theo PlayCount giảm dần |
| BR-03 | Biểu đồ hiển thị tối đa 10 bài hát |
| BR-04 | Số format: >= 1M -> "X.XM", >= 1K -> "X.XK", còn lại nguyên |
| BR-05 | Chỉ Artist (hoặc Admin) mới truy cập được trang /artist/stats |

## Acceptance Criteria

| AC | Mô tả | TC liên kết |
|----|-------|-------------|
| AC-01 | Artist có bài Approved -> trang /artist/stats hiển thị TotalPlays, TotalLikes, biểu đồ + bảng | - |
| AC-02 | Artist chưa có bài Approved -> trang /artist/stats hiển thị EmptyState | - |

## Ánh xạ code

| Thành phần | Đường dẫn |
|------------|-----------|
| Song Service (GetArtistStats) | `Services/SongService.cs` -> `GetArtistStatsAsync()` |
| ArtistStats Component | `Components/Artist/ArtistStats.razor` |
| StatsDto | `DTOs/StatsDto.cs` |
| TrackStatDto | `DTOs/StatsDto.cs` |

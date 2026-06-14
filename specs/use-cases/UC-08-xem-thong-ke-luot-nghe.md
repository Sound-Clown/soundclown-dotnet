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

## Tiêu chí chất lượng

### Mức ý niệm (Essential / Conceptual) — *UC này có cần thiết không?*

- **Mục tiêu nghiệp vụ**: cho Artist theo dõi độ phổ biến tác phẩm — vòng phản hồi quan trọng giúp Artist hiểu thị hiếu khán giả.
- **Tiêu thụ dữ liệu được tạo bởi UC khác**: PlayCount (từ UC-01), LikeCount (từ UC-03), Status Approved (từ UC-09) — UC-08 là tầng phân tích, không sinh dữ liệu mới.
- **Chính sách thống kê đúng**: chỉ tính bài Approved → loại bỏ noise từ bài Pending/Rejected.
- **Actor đúng vai**: chỉ Artist (chủ sở hữu) và Admin xem được thống kê của mình → bảo vệ riêng tư.
- **Truy vết tới BR**: UC-08 ↔ BR-01 (Quản lý bài hát) — phản ánh quyền của chủ nội dung.
- **Trung lập công nghệ**: chưa nói tới SQL aggregation, biểu đồ cụ thể, hay DTO.

### Mức thiết kế (Design-level) — *Thiết kế xử lý UC này có hợp lý không?*

- **Luồng chính + luồng "chưa có dữ liệu"**: bao phủ trải nghiệm cho Artist mới (chưa upload) và Artist đã có nội dung.
- **Bố cục thông tin có cấu trúc**: 2 card tổng (overview) + biểu đồ top (visualization) + bảng chi tiết (detail) — đúng pattern dashboard.
- **Quy tắc định lượng**: top 10 bài hát, sort theo PlayCount giảm dần — không mơ hồ.
- **Format số có ngưỡng cụ thể**: ≥ 1M → "X.XM", ≥ 1K → "X.XK" — dễ đọc, đồng bộ chuẩn ngành.
- **EmptyState có thông điệp hữu ích**: hướng dẫn Artist mới "Hãy upload và được duyệt ít nhất một bài hát" thay vì chỉ "Chưa có dữ liệu".
- **Phân quyền tách bạch**: chỉ Artist/Admin truy cập → phối hợp với UC-07 chứ không tự xử lý.
- **Quy tắc nghiệp vụ đầy đủ để suy ra Requirement**: có thể sinh TC bao phủ Artist có ≥1 bài Approved (hiển thị đầy đủ), Artist chưa có bài (EmptyState).
- **Tính module**: chỉ đọc/aggregate, không nhúng chức năng phát hay sửa.

### Mức hiện thực (Concrete / Implementation-level) — *Bản code đã chạy đúng chưa?*

- Khi Artist có ≥ 1 bài Approved, trang /artist/stats có hiển thị đúng TotalPlays, TotalLikes, biểu đồ và bảng chi tiết không?
- TotalPlays có là **tổng PlayCount của tất cả bài Approved** của Artist, không tính bài Pending/Rejected, không?
- TotalLikes có là **tổng LikeCount của tất cả bài Approved** của Artist không?
- Biểu đồ có hiển thị tối đa 10 bài hát, sort theo PlayCount giảm dần không?
- Width % của mỗi bar có tính đúng theo `PlayCount / maxPlayCount * 100` không?
- Format số có đúng: 1,500,000 → "1.5M", 12,000 → "12.0K", 500 → "500" không?
- Khi Artist chưa có bài Approved nào, UI có hiển thị EmptyState "Chưa có dữ liệu" / "Hãy upload và được duyệt ít nhất một bài hát." không?
- Khi user không phải Artist/Admin truy cập /artist/stats, có bị chặn không?
- Stats có **chỉ trả về bài của Artist hiện tại** (`ArtistId == userId`), không lộ stats của Artist khác không?
- AC-01, AC-02 chạy thực tế cho kết quả khớp expected không?

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

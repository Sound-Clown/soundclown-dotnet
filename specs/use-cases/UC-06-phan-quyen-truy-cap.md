# UC-06: Phân quyền truy cập

## Thông tin

| Thuộc tính | Giá trị |
|------------|---------|
| Mã UC | UC-06 |
| Tên | Phân quyền truy cập |
| Actor chính | Hệ thống (middleware/authorization layer) |
| Trigger | User gửi request đến tài nguyên/biến động không thuộc quyền của mình |
| Mức độ ưu tiên | High |

## Mô tả

Hệ thống kiểm soát quyền truy cập dựa trên vai trò (Role-based) và sở hữu (Ownership). User chỉ có thể thao tác trên tài nguyên thuộc quyền của mình. Các API endpoint được bảo vệ ở hai tầng: route-level (Authorize với Role) và service-level (kiểm tra ownership).

## Điều kiện trước

- User đã đăng nhập vào hệ thống
- User có vai trò xác định (Listener, Artist, hoặc Admin)

## Luồng chính — Kiểm tra quyền thành công

### K1: Admin truy cập tài nguyên Admin

1. Admin gọi API endpoint có `[Authorize(Roles = "Admin")]`
2. Middleware kiểm tra Role = Admin -> cho phép truy cập
3. Request được xử lý bình thường

### K2: Owner thao tác trên tài nguyên của mình

1. Artist gọi API update bài hát thuộc về mình (ArtistId == userId)
2. Service kiểm tra ownership -> cho phép thao tác
3. Request được xử lý bình thường

### K3: Artist thêm bài hát vào album của mình

1. Artist gọi API thêm bài hát vào album
2. Service kiểm tra: album thuộc Artist (album.ArtistId == artistId) VÀ bài hát thuộc Artist (song.ArtistId == artistId)
3. Cho phép thêm bài hát vào album

## Luồng ngoại lệ — Truy cập bị từ chối

### R1: Listener/Artist gọi API Admin -> 403 Forbidden

- User có Role = Listener hoặc Artist gọi API endpoint có `[Authorize(Roles = "Admin")]`
- Middleware kiểm tra Role != Admin -> trả về HTTP 403
- Response body rỗng (route-level rejection)

### R2: Artist A sửa bài hát của Artist B -> 403 Forbidden

- Artist gọi API update bài hát mà ArtistId != userId của mình
- Service truy vấn: `WHERE Id = songId AND ArtistId = userId` -> không tìm thấy
- Trả về HTTP 403: `{ error: "Không tìm thấy bài hát hoặc bạn không có quyền." }`

### R3: Artist thêm bài hát người khác vào album -> 400 Bad Request

- Artist gọi API thêm bài hát vào album của mình
- Service kiểm tra: bài hát có ArtistId != artistId hiện tại
- Trả về HTTP 400: `{ error: "Bài hát không hợp lệ." }`

## Điều kiện sau

- User không có quyền không thể truy cập/tương tác tài nguyên của người khác
- Mỗi request đều được xác thực và phân quyền đúng

## Quy tắc nghiệp vụ

| Mã | Quy tắc |
|----|---------|
| BR-01 | API Admin chỉ accessible bởi Role = Admin (route-level `[Authorize(Roles = "Admin")]`) |
| BR-02 | Artist chỉ có thể sửa/xóa bài hát thuộc về mình (service-level ownership check) |
| BR-03 | Artist chỉ có thể thêm bài hát thuộc về mình vào album thuộc về mình |
| BR-04 | Bài hát Pending/Rejected chỉ hiển thị cho chính Artist và Admin |
| BR-05 | Lỗi quyền: route-level trả 403, service-level trả 403/400 kèm message |

## Tiêu chí chất lượng

### Mức ý niệm (Essential / Conceptual) — *UC này để làm gì, tại sao tồn tại?*

- **Mục tiêu nghiệp vụ**: bảo vệ tài nguyên — đảm bảo mọi user chỉ tác động được lên thứ thuộc quyền của mình hoặc đúng vai trò.
- **Hai trục phân quyền song hành**: Role-based (Admin/Artist/Listener) và Ownership-based (chủ sở hữu của tài nguyên).
- **Đây là UC xuyên suốt (cross-cutting)**: không gắn 1 màn hình cụ thể, mà chi phối toàn bộ luồng các UC khác.
- **Truy vết tới BR**: UC-06 ↔ BR-05 (Phân quyền truy cập).
- **Trung lập công nghệ**: chưa nói tới `[Authorize]`, JWT, hay middleware.

### Mức thiết kế (Design-level) — *Thiết kế xử lý UC này có hợp lý không?*

- **Phòng vệ chiều sâu (defense in depth)**: thiết kế 2 tầng kiểm tra — route-level (Role) + service-level (Ownership) → không phụ thuộc 1 lớp duy nhất, ngăn được cả tấn công bypass UI.
- **Mô tả tường minh các kịch bản thành công và từ chối**: bao phủ ma trận role × hành động, không bỏ sót cặp role/tài nguyên nào.
- **Phân biệt mã lỗi có chủ đích**: 403 cho sai role / sai owner, 400 cho dữ liệu nghiệp vụ sai — giúp client phản ứng đúng và logging phân biệt được.
- **Quy tắc nghiệp vụ đầy đủ để suy ra Requirement**: từ các quy tắc có thể sinh TC bao phủ các nhánh từ chối (non-admin → API admin, artist sửa bài người khác, artist thêm bài người khác vào album).
- **Tính module (cross-cutting)**: tách "phân quyền" thành UC riêng → các UC nghiệp vụ khác giữ luồng ngắn gọn, không lặp lại logic check role/ownership.
- **Hậu điều kiện đo được**: HTTP status + message → dễ kiểm chứng.

### Mức hiện thực (Concrete / Implementation-level) — *Bản code đã chạy đúng chưa?*

- Khi Listener hoặc Artist gọi API yêu cầu Role = Admin, server có trả HTTP 403 không (chặn ở tầng route-level)?
- Khi Artist A cố sửa bài hát của Artist B, server có trả HTTP 403 kèm message "Không tìm thấy bài hát hoặc bạn không có quyền." không?
- Khi Artist cố thêm bài hát của người khác vào album của mình, server có trả HTTP 400 kèm "Bài hát không hợp lệ." không?
- Owner thao tác trên tài nguyên của mình (Artist sửa bài của mình, Admin truy cập trang admin) có hoạt động bình thường, không bị từ chối oan không?
- Bài hát ở Status Pending/Rejected có bị **ẩn** khỏi user khác (không phải owner và không phải Admin) trên trang chủ / Search không?
- Phân quyền có được thực hiện ở **cả hai tầng** (route-level qua attribute và service-level qua query filter ownership), không phụ thuộc chỉ vào một tầng?
- Mã lỗi HTTP có được dùng đúng nghĩa: 403 cho sai role / sai owner, 400 cho dữ liệu nghiệp vụ sai không?
- Message lỗi có **không lộ thông tin nhạy cảm** (vd: không trả "bạn không phải admin" mà chỉ trả status 403 trống) ở tầng route-level không?
- Tất cả AC-01, AC-02, AC-03 chạy thực tế cho kết quả khớp với expected không?

## Acceptance Criteria

| AC | Mô tả | TC liên kết |
|----|-------|-------------|
| AC-01 | Listener/Artist gọi API Admin -> HTTP 403 | TC-05 |
| AC-02 | Artist A sửa bài hát của Artist B -> HTTP 403, "Không tìm thấy bài hát hoặc bạn không có quyền." | TC-16 |
| AC-03 | Artist thêm bài hát người khác vào album -> HTTP 400, "Bài hát không hợp lệ." | TC-20 |

## Ánh xạ code

| Thành phần | Đường dẫn |
|------------|-----------|
| Authorization Middleware | `Program.cs` (JWT + Cookie auth config) |
| TestApiController | `Controllers/TestApiController.cs` (`[Authorize(Roles = "Admin")]`) |
| Song Service (Update) | `Services/SongService.cs` -> `UpdateAsync()` (ownership check) |
| Album Service (AddSong) | `Services/AlbumService.cs` -> `AddSongAsync()` (ownership check) |
| Admin Service | `Services/AdminService.cs` (IsAdmin check) |

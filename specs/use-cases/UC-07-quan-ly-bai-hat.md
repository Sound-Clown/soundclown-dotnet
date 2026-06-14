# UC-07: Quản lý bài hát

## Thông tin

| Thuộc tính | Giá trị |
|------------|---------|
| Mã UC | UC-07 |
| Tên | Quản lý bài hát |
| Actor chính | Artist |
| Trigger | Artist truy cập trang /artist/songs và thao tác xem, sửa, xóa bài hát |
| Mức độ ưu tiên | Medium |

## Mô tả

Artist xem, sửa, và xóa bài hát thuộc về mình. Khi sửa nội dung (title, coverImage), trạng thái bài hát tự động reset về Pending để Admin duyệt lại. Hệ thống kiểm soát quyền sở hữu: Artist chỉ thao tác được trên bài hát của mình. Phân quyền được thực hiện ở hai tầng: route-level (Role) và service-level (ownership).

## Điều kiện trước

- Artist đã đăng nhập vào hệ thống
- Bài hát thuộc về Artist hiện tại (ArtistId == userId)

## Luồng chính — Sửa bài hát Approved

1. Artist vào trang /artist/songs
2. Artist nhấn nút Edit (biểu tượng bút) trên bài hát của mình
3. Artist thay đổi Title và/hoặc CoverImage
4. Artist nhấn nút Lưu
5. Hệ thống kiểm tra ownership: bài hát có ArtistId == userId hiện tại
6. Hệ thống cập nhật:
   - Title/CoverImage theo giá trị mới (trim khoảng trắng)
   - **Status = Pending** (reset, bất kể nội dung có thay đổi thực sự hay không)
7. Hệ thống lưu thay đổi vào DB
8. Badge chuyển từ Approved (xanh) -> Pending (vàng)

## Luồng thay thế — Sửa AlbumId

1. Artist thay đổi album gắn với bài hát
2. AlbumId được cập nhật (có thể set null để bỏ bài khỏi album)
3. **Status KHÔNG reset** (chỉ thay đổi gán album, không thay đổi nội dung)

## Luồng thay thế — Xóa bài hát

1. Artist nhấn nút Delete trên bài hát của mình
2. Hệ thống kiểm tra ownership
3. Hệ thống xóa bản ghi Song
4. Bài hát biến mất khỏi danh sách

## Luồng ngoại lệ — Phân quyền truy cập

### R1: Listener/Artist gọi API Admin -> 403 Forbidden

- User có Role = Listener hoặc Artist gọi API endpoint có `[Authorize(Roles = "Admin")]`
- Middleware kiểm tra Role != Admin -> trả về HTTP 403
- Response body rỗng (route-level rejection)

### R2: Artist A sửa bài hát của Artist B -> 403 Forbidden

- Artist gọi API update bài hát mà ArtistId != userId của mình
- Service truy vấn: `WHERE Id = songId AND ArtistId = userId` -> không tìm thấy
- Trả về HTTP 403: `{ error: "Không tìm thấy bài hát hoặc bạn không có quyền." }`

### R3: Bài hát không thuộc Artist

- Service truy vấn: `WHERE Id = songId AND ArtistId = userId` -> không tìm thấy
- Trả về lỗi: "Không tìm thấy bài hát hoặc bạn không có quyền."

### R4: Artist thêm bài hát người khác vào album -> 400 Bad Request

- Artist gọi API thêm bài hát vào album của mình
- Service kiểm tra: bài hát có ArtistId != artistId hiện tại
- Trả về HTTP 400: `{ error: "Bài hát không hợp lệ." }`

## Điều kiện sau

- Bài hát có Title/CoverImage mới
- Status = Pending nếu Title hoặc CoverImage thay đổi
- Status không đổi nếu chỉ thay đổi AlbumId
- Bài hát không còn công khai trên trang chủ cho đến khi Admin approve lại
- User không có quyền không thể thao tác trên tài nguyên của người khác

## Quy tắc nghiệp vụ

| Mã | Quy tắc |
|----|---------|
| BR-01 | Chỉ Artist sở hữu bài hát mới được sửa/xóa (ArtistId == userId) |
| BR-02 | Khi Title hoặc CoverImage thay đổi, Status tự động reset về Pending |
| BR-03 | Chỉ thay đổi AlbumId không trigger reset Status |
| BR-04 | Title được trim khoảng trắng trước khi lưu |
| BR-05 | Bài hát Pending không hiển thị trên trang chủ cho user khác (chỉ chủ bài và Admin xem được) |
| BR-06 | API Admin chỉ accessible bởi Role = Admin (route-level `[Authorize(Roles = "Admin")]`) |
| BR-07 | Lỗi quyền: route-level trả 403, service-level trả 403/400 kèm message |

## Tiêu chí chất lượng

### Mức ý niệm (Essential / Conceptual) — *UC này có cần thiết không?*

- **Mục tiêu nghiệp vụ**: cho Artist tự bảo trì nội dung bài hát (sửa, xóa) thay vì phải xóa-rồi-upload lại — giảm ma sát vận hành.
- **Phân quyền là trục xương sống**: UC này đồng thời chi phối ai được làm gì với tài nguyên — Role-based + Ownership-based.
- **Nguyên tắc bảo toàn chất lượng**: thay đổi nội dung công khai (Title/CoverImage) phải qua kiểm duyệt lại → bảo vệ chuẩn nội dung đã được duyệt trước đó.
- **Phân biệt "nội dung" và "tổ chức"**: đổi AlbumId chỉ là tổ chức/sắp xếp → không cần duyệt lại.
- **Actor đúng vai**: chính chủ Artist + Admin có quyền can thiệp.
- **Truy vết tới BR**: UC-07 ↔ BR-01 (Quản lý bài hát) + BR-05 (Phân quyền truy cập).
- **Trung lập công nghệ**: chưa nói tới `[Authorize]`, JWT, hay middleware.

### Mức thiết kế (Design-level) — *Thiết kế xử lý UC này có hợp lý không?*

- **Phân biệt rõ 2 loại thay đổi**: nội dung (Title/CoverImage → reset Pending) vs tổ chức (AlbumId → giữ Status) — tránh hiểu sai "sửa gì cũng reset" hoặc ngược lại.
- **Phòng vệ chiều sâu (defense in depth)**: 2 tầng kiểm tra — route-level (Role) + service-level (Ownership) → không phụ thuộc 1 lớp duy nhất, ngăn được bypass UI.
- **4 nhánh ngoại lệ R1–R4 rõ ràng**: sai role (403 route-level), sai owner sửa bài (403 service-level), bài không thuộc Artist, thêm bài người khác vào album (400).
- **Phân biệt mã lỗi có chủ đích**: 403 cho sai role/owner, 400 cho dữ liệu nghiệp vụ sai.
- **Quy tắc nghiệp vụ đầy đủ để suy ra Requirement**: có thể sinh TC bao phủ sửa Title (reset), sửa AlbumId (không reset), Artist A sửa bài Artist B (403), non-admin gọi API admin (403).
- **Tính module (cross-cutting)**: phân quyền được tích hợp tự nhiên vào CRUD bài hát thay vì rải rác khắp các UC khác.

### Mức hiện thực (Concrete / Implementation-level) — *Bản code đã chạy đúng chưa?*

- Khi Artist sửa Title hoặc CoverImage của bài Approved, Status có **reset về Pending** và badge UI chuyển vàng không?
- Khi Artist chỉ đổi AlbumId (không đổi Title/CoverImage), Status có **giữ nguyên** không?
- Khi Artist cố sửa bài hát của Artist khác, server có trả HTTP 403 kèm "Không tìm thấy bài hát hoặc bạn không có quyền." không?
- Khi Listener hoặc Artist gọi API yêu cầu Role = Admin, server có trả HTTP 403 (chặn ở route-level) không?
- Khi Artist cố thêm bài hát người khác vào album của mình, server có trả HTTP 400 kèm "Bài hát không hợp lệ." không?
- Title có được trim khoảng trắng trước khi lưu không?
- Khi Artist xóa bài hát của mình, bản ghi Song có thực sự bị xóa không?
- Bài Pending có **ẩn** khỏi trang chủ với user khác (không phải owner và Admin) không?
- Phân quyền có được thực hiện ở **cả hai tầng** (route-level + service-level), không phụ thuộc chỉ vào UI không?
- Tất cả AC-01 → AC-04 chạy thực tế cho kết quả khớp expected không?

## Acceptance Criteria

| AC | Mô tả | TC liên kết |
|----|-------|-------------|
| AC-01 | Artist sửa Title bài hát Approved -> Status reset về Pending, badge chuyển vàng | TC-17 |
| AC-02 | Artist A sửa bài hát của Artist B -> HTTP 403, "Không tìm thấy bài hát hoặc bạn không có quyền." | TC-16 |
| AC-03 | Listener/Artist gọi API Admin -> HTTP 403 | TC-05 |
| AC-04 | Artist thêm bài hát người khác vào album -> HTTP 400, "Bài hát không hợp lệ." | TC-20 |

## Ánh xạ code

| Thành phần | Đường dẫn |
|------------|-----------|
| Song Service (Update) | `Services/SongService.cs` -> `UpdateAsync()` |
| Song Service (Delete) | `Services/SongService.cs` -> `DeleteAsync()` |
| API Endpoint (Update) | `Controllers/TestApiController.cs` -> `PUT /api/songs/{id}` |
| Album Service (AddSong) | `Services/AlbumService.cs` -> `AddSongAsync()` |
| Authorization Middleware | `Program.cs` (JWT + Cookie auth config) |
| TestApiController | `Controllers/TestApiController.cs` (`[Authorize(Roles = "Admin")]`) |

# UC-08: Artist sửa bài hát

## Thông tin

| Thuộc tính | Giá trị |
|------------|---------|
| Mã UC | UC-08 |
| Tên | Artist sửa bài hát |
| Actor chính | Artist |
| Trigger | Artist nhấn nút Edit trên bài hát của mình |
| Mức độ ưu tiên | Medium |

## Mô tả

Artist có thể sửa tiêu đề, ảnh bìa, và album của bài hát thuộc về mình. Khi sửa bất kỳ nội dung nào (title, coverImage), trạng thái bài hát tự động reset về Pending để Admin duyệt lại. Điều này đảm bảo nội dung đã chỉnh sửa được kiểm duyệt trước khi công khai lại.

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

## Luồng ngoại lệ

### R1: Bài hát không thuộc Artist

- Service truy vấn: `WHERE Id = songId AND ArtistId = userId` -> không tìm thấy
- Trả về lỗi: "Không tìm thấy bài hát hoặc bạn không có quyền."

## Điều kiện sau

- Bài hát có Title/CoverImage mới
- Status = Pending nếu Title hoặc CoverImage thay đổi
- Status không đổi nếu chỉ thay đổi AlbumId
- Bài hát không còn công khai trên trang chủ cho đến khi Admin approve lại

## Quy tắc nghiệp vụ

| Mã | Quy tắc |
|----|---------|
| BR-01 | Chỉ Artist sở hữu bài hát mới được sửa (ArtistId == userId) |
| BR-02 | Khi Title hoặc CoverImage thay đổi, Status tự động reset về Pending |
| BR-03 | Chỉ thay đổi AlbumId không trigger reset Status |
| BR-04 | Title được trim khoảng trắng trước khi lưu |
| BR-05 | Bài hát Pending không hiển thị trên trang chủ cho user khác (chỉ chủ bài và Admin xem được) |

## Tiêu chí chất lượng

### Mức ý niệm (Essential / Conceptual) — *UC này để làm gì, tại sao tồn tại?*

- **Mục tiêu nghiệp vụ**: cho Artist tự bảo trì nội dung bài hát (đổi tên, đổi ảnh bìa, gán album) thay vì phải xóa-rồi-upload lại.
- **Nguyên tắc bảo toàn chất lượng**: thay đổi nội dung công khai (Title/CoverImage) phải qua kiểm duyệt lại → bảo vệ standard nội dung đã được duyệt trước đó.
- **Phân biệt "nội dung" và "tổ chức"**: đổi AlbumId chỉ là tổ chức/sắp xếp → không cần duyệt lại.
- **Actor**: chính chủ Artist của bài hát.
- **Truy vết tới BR**: UC-08 ↔ BR-01 (Quản lý bài hát).
- **Trung lập công nghệ**: chưa nói tới SongStatus enum, JS, hay endpoint.

### Mức thiết kế (Design-level) — *Thiết kế xử lý UC này có hợp lý không?*

- **Phân biệt rõ 2 loại thay đổi**: thay đổi nội dung (Title/CoverImage → reset Pending) vs thay đổi tổ chức (AlbumId → giữ Status) — mô tả tường minh ở luồng chính và luồng thay thế, tránh hiểu sai kiểu "sửa gì cũng reset" hoặc "sửa gì cũng không reset".
- **Quy tắc reset Status có chủ đích**: phản ánh đúng nguyên tắc nghiệp vụ — nội dung công khai phải qua kiểm duyệt lại; tổ chức không cần.
- **Ownership là tiền điều kiện**, không trộn vào luồng — phối hợp với UC-06 thay vì lặp lại check.
- **Quy tắc nghiệp vụ đầy đủ để suy ra Requirement**: từ các quy tắc có thể sinh TC bao phủ sửa Title (reset), sửa Cover (reset), sửa AlbumId đơn thuần (không reset), sửa bài người khác (từ chối).
- **Hậu điều kiện đo được**: Status + badge UI + tính khả kiến trên trang chủ — đều quan sát được mà không cần debug.
- **Tính module**: chỉ sửa metadata, không nhúng việc thay file audio (sẽ là UC mới nếu phát sinh nhu cầu) — giữ trách nhiệm đơn nhất.

### Mức hiện thực (Concrete / Implementation-level) — *Bản code đã chạy đúng chưa?*

- Khi Artist sửa Title hoặc CoverImage của bài hát đang Approved, Status có **reset về Pending** và badge UI có chuyển vàng không?
- Khi Artist chỉ đổi AlbumId (không đổi Title/CoverImage), Status có **giữ nguyên** (không reset) không?
- Khi Artist cố sửa bài hát của Artist khác, server có trả lỗi "Không tìm thấy bài hát hoặc bạn không có quyền." không?
- Title có được trim khoảng trắng trước khi lưu không?
- AlbumId có thể được set null (bỏ bài khỏi album) không?
- Sau khi bài chuyển về Pending, bài đó có **không còn xuất hiện** trên trang chủ cho user khác, nhưng vẫn hiển thị cho chủ bài và Admin không?
- Endpoint sửa bài hát có yêu cầu xác thực và check ownership ở tầng service, không chỉ dựa vào UI không?
- AC-01 chạy thực tế (sửa Title bài Approved → Status Pending, badge vàng) có cho kết quả khớp expected không?

## Acceptance Criteria

| AC | Mô tả | TC liên kết |
|----|-------|-------------|
| AC-01 | Artist sửa Title bài hát Approved -> Status reset về Pending, badge chuyển vàng | TC-17 |

## Ánh xạ code

| Thành phần | Đường dẫn |
|------------|-----------|
| Song Service (Update) | `Services/SongService.cs` -> `UpdateAsync()` |
| API Endpoint | `Controllers/TestApiController.cs` -> `PUT /api/songs/{id}` |
| UpdateSongDto | `DTOs/UpdateSongDto` |
| SongStatus Enum | `Enums/SongStatus.cs` |

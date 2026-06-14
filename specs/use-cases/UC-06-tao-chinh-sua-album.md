# UC-06: Tạo / Chỉnh sửa Album

## Thông tin

| Thuộc tính | Giá trị |
|------------|---------|
| Mã UC | UC-06 |
| Tên | Tạo / Chỉnh sửa Album |
| Actor chính | Artist |
| Trigger | Artist truy cập trang /artist/albums và nhấn Tạo album hoặc Edit album |
| Mức độ ưu tiên | Medium |

## Mô tả

Artist tổ chức bài hát thành album. Artist có thể tạo album mới, chỉnh sửa tên/ảnh bìa album, thêm hoặc xóa bài hát khỏi album, và xóa album. Khi xóa album, bài hát trong album trở thành single (AlbumId = null) nhưng không bị xóa.

## Điều kiện trước

- Artist đã đăng nhập vào hệ thống
- Artist có vai trò Role = Artist hoặc Role = Admin

## Luồng chính — Tạo album mới

1. Artist truy cập trang /artist/albums
2. Artist nhấn nút "Tạo album"
3. Modal hiện ô nhập tên album và upload ảnh bìa (tùy chọn)
4. Artist nhập tên album (bắt buộc)
5. Artist tùy chọn upload ảnh bìa (JPG/PNG/WebP, tối đa 2MB)
6. Artist nhấn "Lưu"
7. Hệ thống validate:
   - Tên album không được trống (trim)
   - Ảnh bìa hợp lệ (nếu có)
8. Hệ thống tạo bản ghi Album: { Name (trim), CoverImage (nullable), ArtistId, CreatedAt = now }
9. Hệ thống hiển thị toast: "Đã tạo album!"

## Luồng thay thế — Chỉnh sửa album

1. Artist nhấn Edit (overlay) trên album của mình
2. Modal hiện thông tin album hiện tại
3. Artist thay đổi tên và/hoặc ảnh bìa
4. Artist nhấn "Lưu"
5. Hệ thống kiểm tra ownership: album.ArtistId == userId
6. Hệ thống cập nhật Name (trim), CoverImage
7. Hệ thống hiển thị toast: "Đã cập nhật album!"

## Luồng thay thế — Thêm bài hát vào album

1. Artist gọi API thêm bài hát vào album
2. Hệ thống kiểm tra:
   - Album thuộc Artist (album.ArtistId == artistId)
   - Bài hát thuộc Artist (song.ArtistId == artistId)
3. Hệ thống gán song.AlbumId = albumId
4. Nếu bài đã thuộc album đó, không làm gì (idempotent)

## Luồng thay thế — Xóa bài hát khỏi album

1. Artist xóa bài hát khỏi album
2. Hệ thống kiểm tra: bài hát thuộc album và thuộc Artist
3. Hệ thống set song.AlbumId = null (bài hát trở thành single)

## Luồng thay thế — Xóa album

1. Artist nhấn "Xóa" trên album
2. Hệ thống hiển thị ConfirmDialog: "Bạn có chắc muốn xóa album? Các bài hát trong album vẫn được giữ lại nhưng sẽ trở thành single."
3. Artist xác nhận xóa
4. Hệ thống kiểm tra ownership
5. Hệ thống xóa album (SET NULL AlbumId cho các bài trong album)
6. Hệ thống hiển thị toast: "Đã xóa album."

## Luồng ngoại lệ

### R1: Tên album trống

- Tại bước 7 (tạo) hoặc bước 6 (sửa), tên album chỉ chứa khoảng trắng
- Hệ thống hiển thị lỗi: "Vui lòng nhập tên album."

### R2: Album không thuộc Artist

- Service truy vấn: `WHERE Id = albumId AND ArtistId = userId` -> không tìm thấy
- Trả về lỗi: "Không tìm thấy album hoặc bạn không có quyền."

### R3: Thêm bài hát không thuộc Artist vào album

- Bài hát có ArtistId != artistId hiện tại
- Trả về lỗi: "Bài hát không hợp lệ."

### R4: Ảnh bìa không hợp lệ

- Content type không thuộc {image/jpeg, image/png, image/webp}
- Hệ thống hiển thị lỗi: "Chỉ chấp nhận ảnh JPG, PNG hoặc WebP."

### R5: Ảnh bìa vượt quá 2MB

- Hệ thống hiển thị lỗi: "Ảnh bìa tối đa 2MB."

## Điều kiện sau

- Album được tạo/cập nhật/xóa đúng
- Bài hát thêm vào album có AlbumId đúng
- Bài hát xóa khỏi album có AlbumId = null
- Xóa album không xóa bài hát bên trong

## Quy tắc nghiệp vụ

| Mã | Quy tắc |
|----|---------|
| BR-01 | Tên album bắt buộc, được trim khoảng trắng trước khi lưu |
| BR-02 | Chỉ Artist sở hữu album mới được thao tác (album.ArtistId == userId) |
| BR-03 | Chỉ thêm được bài hát thuộc Artist vào album thuộc Artist đó |
| BR-04 | Ảnh bìa album: chỉ chấp nhận JPG/PNG/WebP, tối đa 2MB |
| BR-05 | Xóa album: bài hát trong album trở thành single (AlbumId = null), không bị xóa |
| BR-06 | Thêm bài hát đã thuộc album đó là idempotent (không lỗi, không tăng count) |

## Acceptance Criteria

| AC | Mô tả | TC liên kết |
|----|-------|-------------|
| AC-01 | Artist tạo album hợp lệ -> album hiển thị trên trang /artist/albums | - |
| AC-02 | Artist sửa album (tên/ảnh bìa) -> cập nhật thành công | - |
| AC-03 | Artist thêm bài hát thuộc mình vào album -> thành công | - |
| AC-04 | Artist thêm bài hát người khác vào album -> lỗi "Bài hát không hợp lệ." | TC-20 |
| AC-05 | Artist xóa album -> bài hát trong album trở thành single | - |

## Ánh xạ code

| Thành phần | Đường dẫn |
|------------|-----------|
| Album Service | `Services/AlbumService.cs` |
| ArtistAlbums Component | `Components/Artist/ArtistAlbums.razor` |
| API Endpoint (AddSong) | `Controllers/TestApiController.cs` -> `POST /api/albums/{id}/songs` |
| Album Entity | `Entities/Album.cs` |

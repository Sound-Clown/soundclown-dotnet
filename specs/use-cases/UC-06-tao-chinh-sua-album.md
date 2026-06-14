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

## Tiêu chí chất lượng

### Mức ý niệm (Essential / Conceptual) — *UC này có cần thiết không?*

- **Mục tiêu nghiệp vụ**: cho Artist tổ chức bài hát thành album — phản ánh đúng mô hình phát hành nhạc trong ngành.
- **Tách biệt "nội dung" và "tổ chức"**: album là lớp tổ chức bên trên bài hát; xóa album không xóa bài hát → bảo toàn nội dung gốc.
- **CRUD hoàn chỉnh trong một UC**: tạo / sửa / xóa album + thêm / xóa bài khỏi album — vòng đời album đầy đủ trong một use case.
- **Actor đúng vai**: chỉ Artist (chủ sở hữu) mới thao tác → đồng bộ với UC-07 (quản lý bài hát).
- **Truy vết tới BR**: UC-06 ↔ BR-01 (Quản lý bài hát).
- **Trung lập công nghệ**: chưa nói tới Cloudinary, SET NULL, hay endpoint cụ thể.

### Mức thiết kế (Design-level) — *Thiết kế xử lý UC này có hợp lý không?*

- **5 luồng đối ngẫu**: tạo, sửa, thêm bài, xóa bài, xóa album — bao phủ toàn bộ vòng đời.
- **5 nhánh ngoại lệ R1–R5** rõ ràng: tên trống, album không thuộc Artist, bài người khác, ảnh sai loại, ảnh quá size.
- **Tính idempotent khi thêm bài đã thuộc album**: thiết kế đã tính tới việc thao tác lặp không gây lỗi (BR-06).
- **Quy tắc xóa "mềm" có chủ đích**: xóa album → bài thành single (AlbumId = null), không xóa cascade bài hát → bảo vệ dữ liệu Artist.
- **Confirm dialog cho thao tác hủy**: thiết kế UX có rào chắn cho hành động bất khả phục hồi.
- **Quy tắc nghiệp vụ đầy đủ để suy ra Requirement**: có thể sinh TC bao phủ tạo album thành công, sửa thành công, thêm bài hợp lệ, thêm bài người khác (negative), xóa album.
- **Tính module**: chỉ xử lý vòng đời album, không nhúng việc duyệt nội dung (UC-09) hay phát nhạc.

### Mức hiện thực (Concrete / Implementation-level) — *Bản code đã chạy đúng chưa?*

- Khi Artist tạo album với tên hợp lệ, album có được lưu vào DB với Name đã trim và xuất hiện trên trang /artist/albums không?
- Khi Artist sửa tên/ảnh bìa album của mình, cập nhật có được lưu đúng không?
- Khi Artist cố sửa album của Artist khác, server có trả "Không tìm thấy album hoặc bạn không có quyền." không?
- Khi tên album để trống (chỉ khoảng trắng), hệ thống có chặn với "Vui lòng nhập tên album." không?
- Khi Artist thêm bài hát thuộc về mình vào album thuộc mình, song.AlbumId có được gán đúng không?
- Khi Artist cố thêm bài hát người khác vào album, server có trả "Bài hát không hợp lệ." (HTTP 400) không?
- Khi thêm bài đã thuộc album đó (idempotent), code có không lỗi và không tăng count gì không?
- Khi xóa bài khỏi album, song.AlbumId có chuyển null không?
- Khi xóa album, các bài hát trong album có giữ lại (AlbumId = null), không bị xóa theo không?
- Ảnh bìa album: code có validate đúng content-type ∈ {JPG, PNG, WebP} và size ≤ 2MB không?
- Tất cả AC-01 → AC-05 chạy thực tế cho kết quả khớp expected không?

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

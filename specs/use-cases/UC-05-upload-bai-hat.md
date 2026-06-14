# UC-05: Upload bài hát

## Thông tin

| Thuộc tính | Giá trị |
|------------|---------|
| Mã UC | UC-05 |
| Tên | Upload bài hát |
| Actor chính | Artist |
| Actor phụ | Cloudinary (hệ thống lưu trữ media) |
| Trigger | Artist truy cập trang /artist/upload và nhấn Upload bài hát |
| Mức độ ưu tiên | High |

## Mô tả

Artist upload file audio MP3 kèm ảnh bìa (tùy chọn) để tạo bài hát mới. Bài hát sau khi upload sẽ có trạng thái Pending, chờ Admin duyệt trước khi công khai.

## Điều kiện trước

- Artist đã đăng nhập vào hệ thống
- Artist có vai trò Role = Artist hoặc Role = Admin

## Luồng chính

1. Artist truy cập trang /artist/upload
2. Artist nhập Title bài hát
3. Artist chọn hoặc kéo file audio MP3 vào vùng upload
4. Artist tùy chọn upload ảnh bìa (JPG/PNG/WebP)
5. Artist tùy chọn chọn Album (chỉ album thuộc về mình)
6. Artist nhấn nút Upload bài hát
7. Hệ thống validate file audio:
   - Kiểm tra content type: chỉ chấp nhận `audio/mpeg` hoặc `audio/mp3`
   - Kiểm tra dung lượng: tối đa 10MB
8. Hệ thống validate ảnh bìa (nếu có):
   - Kiểm tra content type: chỉ chấp nhận `image/jpeg`, `image/png`, `image/webp`
   - Kiểm tra dung lượng: tối đa 2MB
9. Hệ thống upload file audio lên Cloudinary (folder: soundclown/audio)
10. Hệ thống upload ảnh bìa lên Cloudinary (folder: soundclown/covers, tự resize 600x600 crop fill)
11. Hệ thống tạo bản ghi Song với:
    - Status = Pending
    - AudioFile = URL Cloudinary của audio
    - CoverImage = URL Cloudinary của ảnh bìa (nullable)
    - ArtistId = ID của Artist hiện tại
    - AlbumId = ID album đã chọn (nullable)
12. Hệ thống hiển thị toast: "Upload thành công! Bài hát đang chờ duyệt."

## Luồng thay thế

### R1: File audio không hợp lệ (không phải MP3)

- Tại bước 7, nếu content type khác `audio/mpeg` hoặc `audio/mp3`
- Hệ thống hiển thị lỗi ngay khi chọn file: "Chỉ chấp nhận file MP3."
- Upload không được thực hiện

### R2: File audio vượt quá 10MB

- Tại bước 7, nếu dung lượng file > 10MB
- Hệ thống hiển thị lỗi: "File âm thanh tối đa 10MB."
- Upload không được thực hiện

### R3: File ảnh bìa không hợp lệ

- Tại bước 8, nếu content type không thuộc danh sách cho phép
- Hệ thống hiển thị lỗi: "Chỉ chấp nhận ảnh JPG, PNG hoặc WebP."

### R4: File ảnh bìa vượt quá 2MB

- Tại bước 8, nếu dung lượng ảnh > 2MB
- Hệ thống hiển thị lỗi: "Ảnh bìa tối đa 2MB."

### R5: Album không thuộc Artist

- Tại bước 11, nếu AlbumId được chọn nhưng album đó không thuộc Artist hiện tại
- Hệ thống trả lỗi: "Album không hợp lệ."

### R6: Upload Cloudinary thất bại

- Tại bước 9 hoặc 10, nếu Cloudinary trả về error
- Hệ thống hiển thị lỗi: "Upload thất bại: [Cloudinary error message]"

## Điều kiện sau

- Bài hát mới được tạo trong DB với Status = Pending
- File audio và ảnh bìa (nếu có) được lưu trên Cloudinary
- Artist có thể thấy bài hát trên trang /artist/songs với badge Pending (vàng)

## Quy tắc nghiệp vụ

| Mã | Quy tắc |
|----|---------|
| BR-01 | Chỉ chấp nhận file audio có content type `audio/mpeg` hoặc `audio/mp3` |
| BR-02 | Dung lượng file audio tối đa 10MB (10,485,760 bytes) |
| BR-03 | Chỉ chấp nhận ảnh bìa có content type `image/jpeg`, `image/png`, `image/webp` |
| BR-04 | Dung lượng ảnh bìa tối đa 2MB |
| BR-05 | Bài hát mới tạo luôn có Status = Pending |
| BR-06 | AlbumId phải thuộc về Artist đang upload, nếu không sẽ bị từ chối |
| BR-07 | Title bài hát được trim khoảng trắng trước khi lưu |
| BR-08 | CoverImage và AlbumId là nullable (không bắt buộc) |

## Acceptance Criteria

| AC | Mô tả | TC liên kết |
|----|-------|-------------|
| AC-01 | Artist upload file MP3 hợp lệ (<10MB) -> tạo bài hát Pending, hiển thị toast thành công | TC-01 |
| AC-02 | Artist upload file .exe đổi đuôi .mp3 -> lỗi "Chỉ chấp nhận file MP3." ngay khi chọn file | TC-02 |
| AC-03 | Artist upload file MP3 đúng 10MB -> upload thành công | TC-06 |
| AC-04 | Artist upload file MP3 vượt 10MB -> lỗi "File âm thanh tối đa 10MB." | TC-07 |

## Ánh xạ code

| Thành phần | Đường dẫn |
|------------|-----------|
| Upload Service | `Services/UploadService.cs` |
| Song Service (Create) | `Services/SongService.cs` -> `CreateAsync()` |
| Song Entity | `Entities/Song.cs` |
| SongStatus Enum | `Enums/SongStatus.cs` |

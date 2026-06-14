# UC-01: Upload bài hát

## Thông tin

| Thuộc tính | Giá trị |
|------------|---------|
| Mã UC | UC-01 |
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

## Tiêu chí chất lượng

### Mức ý niệm (Essential / Conceptual) — *UC này để làm gì, tại sao tồn tại?*

- **Mục tiêu nghiệp vụ rõ ràng**: cho Artist đóng góp nội dung mới vào kho nhạc, đồng thời đảm bảo nội dung chưa được công khai trước khi qua kiểm duyệt.
- **Actor & quyền hợp lệ**: chỉ Artist (hoặc Admin) đã đăng nhập mới khởi tạo được — phản ánh đúng vai trò "người tạo nội dung" trong hệ thống.
- **Hậu điều kiện đúng triết lý nghiệp vụ**: bài hát luôn ở trạng thái Pending sau khi tạo (BR-05) → khớp với workflow "contributor → moderator".
- **Truy vết tới BR**: UC-01 ↔ BR-01 (Quản lý bài hát).
- **Trung lập công nghệ ở mức này**: chưa nói tới Cloudinary, MP3 codec, hay MB cụ thể — chỉ nêu "file âm thanh kèm ảnh bìa tùy chọn".

### Mức thiết kế (Design-level) — *Thiết kế xử lý UC này có hợp lý không?*

- **Luồng chính tuần tự, rõ điểm vào – điểm ra**: bắt đầu từ truy cập trang upload, kết thúc bằng thông báo thành công; mỗi bước có hành động cụ thể của actor hoặc hệ thống.
- **Bao phủ đủ các điểm thất bại có thể xảy ra**: sai loại audio, quá size audio, sai loại ảnh, quá size ảnh, album không thuộc artist, lỗi external service — mỗi điểm thất bại đều có nhánh ngoại lệ riêng và thông điệp phản hồi cụ thể.
- **Quy tắc nghiệp vụ định lượng được**: ngưỡng 10MB, 2MB, danh sách content-type cụ thể, Status mặc định = Pending — không có quy tắc mơ hồ kiểu "file đủ nhỏ".
- **Quy tắc nghiệp vụ đầy đủ để suy ra Requirement**: từ các quy tắc này có thể sinh ra TC bao phủ Positive (upload hợp lệ), Negative (sai loại file), Boundary (đúng / vượt ngưỡng 10MB).
- **Tính module**: UC chỉ làm việc "tạo bài hát mới", tách bạch với việc duyệt (UC-04) và sửa (UC-08).
- **Tiền/hậu điều kiện rõ**: tiền điều kiện về role và đăng nhập đủ để xác định bối cảnh; hậu điều kiện về trạng thái bài hát + vị trí hiển thị (badge Pending) là quan sát được.

### Mức hiện thực (Concrete / Implementation-level) — *Bản code đã chạy đúng chưa?*

- Code validation file audio có thực thi đúng quy tắc content-type ∈ {audio/mpeg, audio/mp3} và size ≤ 10MB như đặc tả không?
- Code validation ảnh bìa có thực thi đúng quy tắc content-type ∈ {image/jpeg, image/png, image/webp} và size ≤ 2MB không?
- Sau khi upload thành công, bản ghi Song có được lưu đúng trạng thái Pending kèm đầy đủ ArtistId, AudioFile (URL), CoverImage (nullable), AlbumId (nullable) không?
- Khi external service (Cloudinary) trả lỗi, hệ thống có xử lý graceful (bắt exception, trả message rõ cho user, không crash request) không?
- Ràng buộc ownership: nếu Artist chọn AlbumId không thuộc về mình, code có từ chối với message đúng spec không?
- Title có được trim khoảng trắng trước khi lưu vào DB không?
- Thông báo toast hiển thị đúng nội dung như mô tả AC ("Upload thành công! Bài hát đang chờ duyệt.") không?
- Tất cả AC-01 → AC-04 chạy thực tế đều cho kết quả khớp với "Kết quả mong đợi" trong từng TC không?

## Acceptance Criteria

| AC | Mô tả | TC liên kết |
|----|-------|-------------|
| AC-01 | Artist upload file MP3 hợp lệ (<10MB) -> tạo bài hát Pending, hiển thị toast thành công | TC-01 |
| AC-02 | Artist upload file .exe đổi đuôi .mp3 -> lỗi "Chỉ chấp nhận file MP3." ngay khi chọn file | TC-02 |
| AC-03 | Artist upload file MP3 đúng 10MB -> upload thành công | TC-06 |
| AC-04 | Artist upload file MP3 vượt 10MB -> lỗi "File âm thanh tối đa 10MB." | TC-07 |

## Test Cases

### TC-01 — Upload bài hát thành công

- **Loại**: Black-box, Positive, Functional
- **AC liên kết**: AC-01
- **Tài khoản**: `artist@demo.com` / `Artist123!`
- **File test**: `tests/fixtures/files/sample_5mb.mp3`
- **Các bước**:
  1. Đăng nhập `artist@demo.com`
  2. Vào /artist/upload
  3. Nhập Title: "Giấc Mơ Mùa Hè"
  4. Chọn file `sample_5mb.mp3` vào vùng upload audio
  5. (Tùy chọn) Upload ảnh bìa
  6. Nhấn Upload bài hát
- **Kết quả mong đợi**:
  - Toast: "Upload thành công! Bài hát đang chờ duyệt."
  - Trang /artist/songs thấy bài mới với badge Pending (vàng)

### TC-02 — Upload file .exe đổi đuôi thành .mp3

- **Loại**: Black-box, Negative, Validation
- **AC liên kết**: AC-02
- **Tài khoản**: `artist@demo.com` / `Artist123!`
- **File test**: `tests/fixtures/files/malicious.mp3` (file .exe đã đổi tên)
- **Các bước**:
  1. Đăng nhập `artist@demo.com`
  2. Vào /artist/upload
  3. Chọn file `malicious.mp3`
  4. Quan sát phản hồi
- **Kết quả mong đợi**: Lỗi hiển thị ngay khi chọn file — "Chỉ chấp nhận file MP3."

### TC-06 — Upload file đúng 10MB

- **Loại**: Boundary Testing
- **AC liên kết**: AC-03
- **Tài khoản**: `artist@demo.com` / `Artist123!`
- **File test**: `tests/fixtures/files/exact_10mb.mp3` (10,485,760 bytes)
- **Các bước**:
  1. Đăng nhập `artist@demo.com`
  2. Vào /artist/upload
  3. Chọn file `exact_10mb.mp3`
  4. Nhấn Upload bài hát
- **Kết quả mong đợi**: Upload thành công (size = 10MB, không vượt ngưỡng)

### TC-07 — Upload file vượt 10MB

- **Loại**: Boundary Testing
- **AC liên kết**: AC-04
- **Tài khoản**: `artist@demo.com` / `Artist123!`
- **File test**: `tests/fixtures/files/over_10mb.mp3` (~10.5MB)
- **Các bước**:
  1. Chọn file `over_10mb.mp3` vào form upload
  2. Quan sát phản hồi
- **Kết quả mong đợi**: Lỗi "File âm thanh tối đa 10MB."

## Ánh xạ code

| Thành phần | Đường dẫn |
|------------|-----------|
| Upload Service | `Services/UploadService.cs` |
| Song Service (Create) | `Services/SongService.cs` -> `CreateAsync()` |
| Song Entity | `Entities/Song.cs` |
| SongStatus Enum | `Enums/SongStatus.cs` |

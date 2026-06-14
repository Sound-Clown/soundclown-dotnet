# UC-03: Like / Unlike bài hát

## Thông tin

| Thuộc tính | Giá trị |
|------------|---------|
| Mã UC | UC-03 |
| Tên | Like / Unlike bài hát |
| Actor chính | Listener, Artist |
| Trigger | User nhấn nút Like (♡) trên bài hát |
| Mức độ ưu tiên | Medium |

## Mô tả

User đã đăng nhập có thể toggle trạng thái like/unlike cho bài hát. Thao tác này là idempotent: nhấn like lần đầu sẽ like, nhấn lần nữa sẽ unlike. Like count phản ánh đúng số lượng user đã like.

## Điều kiện trước

- User đã đăng nhập vào hệ thống
- Bài hát tồn tại trong hệ thống

## Luồng chính — Like (chưa like trước đó)

1. User nhấn nút ♡ trên bài hát chưa like
2. Hệ thống kiểm tra: chưa tồn tại bản ghi Like cho (UserId, SongId)
3. Hệ thống tạo bản ghi Like mới: { UserId, SongId, CreatedAt = now }
4. Hệ thống tăng Song.LikeCount thêm 1
5. Hệ thống lưu thay đổi vào DB
6. Trả về kết quả: `{ liked: true, newCount: <số like mới> }`
7. Icon ♡ chuyển sang trạng thái đã like (đỏ)

## Luồng thay thế — Unlike (đã like trước đó)

1. User nhấn nút ♡ trên bài hát đã like
2. Hệ thống kiểm tra: tồn tại bản ghi Like cho (UserId, SongId)
3. Hệ thống xóa bản ghi Like
4. Hệ thống giảm Song.LikeCount thêm 1 (minimum = 0, không cho count âm)
5. Hệ thống lưu thay đổi vào DB
6. Trả về kết quả: `{ liked: false, newCount: <số like mới> }`
7. Icon ♡ chuyển về trạng thái chưa like (trắng/xám)

## Luồng ngoại lệ

### R1: Bài hát không tồn tại

- Hệ thống trả lỗi: "Không tìm thấy bài hát."

## Điều kiện sau

- Bảng Likes được cập nhật (thêm/xóa bản ghi)
- Song.LikeCount phản ánh đúng số lượng like hiện tại
- UI hiển thị đúng trạng thái like/unlike

## Quy tắc nghiệp vụ

| Mã | Quy tắc |
|----|---------|
| BR-01 | Mỗi user chỉ được like 1 bài hát 1 lần (PK ghép UserId + SongId trên bảng Likes) |
| BR-02 | Thao tác Like là toggle (idempotent): nhấn lại sẽ unlike |
| BR-03 | LikeCount không bao giờ âm: `Math.Max(0, LikeCount - 1)` khi unlike |
| BR-04 | Khi xóa user hoặc xóa bài hát, tất cả like liên quan bị xóa theo (CASCADE DELETE) |

## Acceptance Criteria

| AC | Mô tả | TC liên kết |
|----|-------|-------------|
| AC-01 | User like bài hát lần đầu -> liked = true, likeCount tăng 1 | TC-03 |
| AC-02 | User unlike bài hát đã like -> liked = false, likeCount giảm 1 | TC-04 |
| AC-03 | User nhấn like 5 lần liên tiếp trong 3 giây -> like count chỉ tăng 1, trạng thái cuối là đã like | TC-10 |

## Ánh xạ code

| Thành phần | Đường dẫn |
|------------|-----------|
| Song Service (ToggleLike) | `Services/SongService.cs` -> `ToggleLikeAsync()` |
| Like Entity | `Entities/Like.cs` |
| API Endpoint | `Controllers/TestApiController.cs` -> `POST /api/songs/{id}/like` |

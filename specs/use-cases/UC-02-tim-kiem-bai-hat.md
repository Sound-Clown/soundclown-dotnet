# UC-02: Tìm kiếm bài hát

## Thông tin

| Thuộc tính | Giá trị |
|------------|---------|
| Mã UC | UC-02 |
| Tên | Tìm kiếm bài hát |
| Actor chính | Listener, Artist |
| Trigger | User nhập từ khóa trên trang /search hoặc thanh tìm kiếm |
| Mức độ ưu tiên | Medium |

## Mô tả

User tìm kiếm bài hát và artist theo từ khóa. Hệ thống tìm kiếm không phân biệt hoa thường (case-insensitive) trên tiêu đề bài hát và tên artist. Chỉ bài hát đã Approved mới xuất hiện trong kết quả. Tìm kiếm có debounce 300ms để tránh gửi quá nhiều request.

## Điều kiện trước

- User đã đăng nhập vào hệ thống

## Luồng chính — Tìm kiếm có kết quả

1. User truy cập trang /search hoặc nhập vào thanh tìm kiếm
2. User nhập từ khóa (tối thiểu 2 ký tự)
3. Hệ thống chờ debounce 300ms sau khi user ngừng gõ
4. Hệ thống tìm kiếm:
   - Bài hát: `Song.Status = Approved` AND (`Song.Title` ILIKE `%query%` OR `Artist.Username` ILIKE `%query%`)
   - Artist: `User.IsActive = true` AND (`User.Username` ILIKE `%query%` OR `User.Email` ILIKE `%query%`)
5. Hệ thống trả về:
   - Tối đa 50 bài hát, sắp xếp theo Title
   - Tối đa 6 artist
6. UI hiển thị danh sách kết quả (bài hát + artist)

## Luồng thay thế — Tìm kiếm không có kết quả

1. User nhập từ khóa không khớp với bài hát hoặc artist nào
2. Hệ thống trả về danh sách rỗng
3. UI hiển thị EmptyState: "Không tìm thấy kết quả"

## Luồng ngoại lệ

### R1: Từ khóa quá ngắn

- Nếu query trim có độ dài < 2 ký tự
- Hệ thống trả về kết quả rỗng (không thực hiện tìm kiếm)

## Điều kiện sau

- Danh sách bài hát khớp từ khóa (chỉ bài Approved) được hiển thị
- Danh sách artist khớp từ khóa được hiển thị
- Thông tin liked state của mỗi bài hát được map đúng cho user hiện tại

## Quy tắc nghiệp vụ

| Mã | Quy tắc |
|----|---------|
| BR-01 | Từ khóa tối thiểu 2 ký tự sau khi trim, ngược lại trả về rỗng |
| BR-02 | Chỉ tìm kiếm bài hát đã Approved (Status = Approved) |
| BR-03 | Tìm kiếm case-insensitive (ILIKE / ToLower) |
| BR-04 | Tìm kiếm trên cả Song.Title và Artist.Username |
| BR-05 | Giới hạn kết quả: tối đa 50 bài hát, 6 artist |
| BR-06 | Debounce 300ms trên client trước khi gửi request tìm kiếm |
| BR-07 | Từ khóa được trim khoảng trắng trước khi tìm kiếm |

## Acceptance Criteria

| AC | Mô tả | TC liên kết |
|----|-------|-------------|
| AC-01 | Nhập từ khóa khớp bài hát/artist -> hiển thị danh sách kết quả | TC-18 |
| AC-02 | Nhập từ khóa không khớp -> hiển thị EmptyState "Không tìm thấy kết quả" | TC-19 |

## Ánh xạ code

| Thành phần | Đường dẫn |
|------------|-----------|
| Song Service (Search) | `Services/SongService.cs` -> `SearchAsync()` |
| Search Component | `Components/Main/Search.razor` |
| JS Debounce | Client-side debounce 300ms |

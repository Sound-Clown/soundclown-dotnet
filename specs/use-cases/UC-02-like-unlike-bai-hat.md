# UC-02: Like/Unlike bài hát

## Thông tin

| Thuộc tính | Giá trị |
|------------|---------|
| Mã UC | UC-02 |
| Tên | Like/Unlike bài hát |
| Actor chính | Listener, Artist, Admin (bất kỳ user đã đăng nhập) |
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

## Tiêu chí chất lượng

### Mức ý niệm (Essential / Conceptual) — *UC này để làm gì, tại sao tồn tại?*

- **Mục tiêu nghiệp vụ**: cho user đã đăng nhập bày tỏ sự yêu thích với bài hát; số lượng like là tín hiệu mức độ phổ biến.
- **Bản chất idempotent**: cùng một thao tác (nhấn ♡) phản ánh ý định "toggle trạng thái yêu thích", không phải "luôn tăng" → khái niệm đúng nghiệp vụ.
- **Actor rộng**: bất kỳ user đã đăng nhập (Listener/Artist/Admin) — phản ánh đúng tính phổ thông của hành động tương tác.
- **Truy vết tới BR**: UC-02 ↔ BR-02 (Tương tác bài hát).
- **Trung lập công nghệ**: chưa nói tới bảng Likes, PK ghép, hay endpoint cụ thể.

### Mức thiết kế (Design-level) — *Thiết kế xử lý UC này có hợp lý không?*

- **Hai luồng đối xứng** (Like / Unlike) cùng entry point — phản ánh đúng tính toggle, tránh thiết kế kiểu "2 endpoint riêng" dễ lệch trạng thái.
- **Bất biến được mô tả tường minh**: LikeCount ≥ 0, mỗi user chỉ 1 like / 1 bài hát — sẵn sàng map xuống ràng buộc DB ở mức hiện thực.
- **Có quy tắc xử lý concurrency/idempotency**: thiết kế đã tính tới trường hợp user nhấn liên tục (yêu cầu trạng thái cuối nhất quán), không bỏ ngỏ.
- **CASCADE khi xóa user/bài hát**: thiết kế nêu rõ phạm vi ảnh hưởng, tránh để lại dữ liệu rác (orphan like).
- **Quy tắc nghiệp vụ đầy đủ để suy ra Requirement**: từ 4 quy tắc có thể sinh TC bao phủ Positive insert, Positive delete, và stress concurrency.
- **Tính module**: chỉ xử lý like, không nhúng việc gửi thông báo cho Artist hay cập nhật trending.
- **Hậu điều kiện đo được**: state cuối + likeCount + UI icon — đều có thể quan sát qua DB/UI.

### Mức hiện thực (Concrete / Implementation-level) — *Bản code đã chạy đúng chưa?*

- Code thực thi đúng ngữ nghĩa toggle: lần đầu nhấn → like, lần kế → unlike, không bị "luôn tăng count"?
- Sau khi like, response có trả về đúng `{ liked: true, newCount }` và DB có sinh đúng 1 bản ghi Like không?
- Sau khi unlike, response có trả về `{ liked: false, newCount }` và bản ghi Like có thực sự bị xóa không?
- Cùng một user không thể tạo nhiều bản ghi like cho cùng một bài hát (ràng buộc unique trên (UserId, SongId)) — code có đảm bảo không?
- LikeCount có **không bao giờ âm** trong mọi tình huống unlike, kể cả khi dữ liệu cũ bị sai lệch không?
- Khi user nhấn like nhiều lần liên tiếp (5 lần trong 3 giây — AC-03), trạng thái cuối cùng và LikeCount có đúng không (không bị race condition)?
- Khi bài hát không tồn tại, code có trả lỗi "Không tìm thấy bài hát." không?
- Khi user hoặc bài hát bị xóa, các bản ghi Like liên quan có bị xóa theo (CASCADE) không?
- Endpoint có yêu cầu xác thực (chỉ user đã đăng nhập gọi được, UserId lấy từ token chứ không từ client) không?
- Tất cả AC-01, AC-02, AC-03 chạy thực tế cho kết quả khớp với expected không?

## Acceptance Criteria

| AC | Mô tả | TC liên kết |
|----|-------|-------------|
| AC-01 | User like bài hát lần đầu -> liked = true, likeCount tăng 1 | TC-03 |
| AC-02 | User unlike bài hát đã like -> liked = false, likeCount giảm 1 | TC-04 |
| AC-03 | User nhấn like 5 lần liên tiếp trong 3 giây -> like count chỉ tăng 1, trạng thái cuối cùng là đã like | TC-10 |

## Ánh xạ code

| Thành phần | Đường dẫn |
|------------|-----------|
| Song Service (ToggleLike) | `Services/SongService.cs` -> `ToggleLikeAsync()` |
| Like Entity | `Entities/Like.cs` |
| API Endpoint | `Controllers/TestApiController.cs` -> `POST /api/songs/{id}/like` |

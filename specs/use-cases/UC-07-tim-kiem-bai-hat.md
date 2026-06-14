# UC-07: Tìm kiếm bài hát

## Thông tin

| Thuộc tính | Giá trị |
|------------|---------|
| Mã UC | UC-07 |
| Tên | Tìm kiếm bài hát |
| Actor chính | Bất kỳ user đã đăng nhập |
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

## Tiêu chí chất lượng

### Mức ý niệm (Essential / Conceptual) — *UC này để làm gì, tại sao tồn tại?*

- **Mục tiêu nghiệp vụ**: cho user khám phá nội dung và artist trong kho nhạc qua từ khóa tự do.
- **Phạm vi tìm kiếm có chủ đích**: chỉ trả về bài hát đã Approved (BR-02) → không lộ bài Pending/Rejected ra ngoài.
- **Ý định "discovery"**: ưu tiên độ phủ rộng (cả bài hát + artist) thay vì exact match.
- **Truy vết tới BR**: UC-07 ↔ BR-06 (Tìm kiếm).
- **Trung lập công nghệ**: chưa nói tới ILIKE, debounce JS, hay limit cụ thể.

### Mức thiết kế (Design-level) — *Thiết kế xử lý UC này có hợp lý không?*

- **Hai luồng đối ngẫu** (có kết quả / không có kết quả) + nhánh ngoại lệ (từ khóa quá ngắn) → bao phủ trải nghiệm người dùng.
- **Quy tắc input định lượng**: tối thiểu 2 ký tự sau trim → tránh truy vấn vô nghĩa và tải DB không cần thiết.
- **Giới hạn kết quả định lượng**: tối đa 50 bài + 6 artist → cân bằng giữa độ phủ và hiệu năng, ngăn DDoS qua truy vấn rộng.
- **Phạm vi tìm kiếm rõ**: chỉ bài Approved → đồng bộ với chính sách kiểm duyệt nội dung (phối hợp UC-04, UC-06).
- **Trải nghiệm gõ phím mượt**: debounce phía client với ngưỡng cụ thể (300ms) — tránh gửi request liên tục khi user đang gõ.
- **Quy tắc nghiệp vụ đầy đủ để suy ra Requirement**: từ các quy tắc có thể sinh TC bao phủ Positive (khớp), Empty (không khớp), Boundary (đúng 2 ký tự, dưới 2 ký tự), case-insensitive.
- **Tính module**: chỉ trả danh sách + liked-state, không xử lý phát nhạc hay like — giữ trách nhiệm đơn nhất.

### Mức hiện thực (Concrete / Implementation-level) — *Bản code đã chạy đúng chưa?*

- Khi user nhập từ khóa khớp tiêu đề bài hát hoặc tên artist, kết quả có hiển thị đúng danh sách không?
- Kết quả tìm kiếm có **chỉ bao gồm bài hát Status = Approved**, không lộ Pending/Rejected ra ngoài không?
- Tìm kiếm có **case-insensitive** thật sự (gõ "GIAC", "giac", "Giac" đều ra cùng kết quả) không?
- Khi từ khóa < 2 ký tự (sau trim), hệ thống có trả kết quả rỗng và không thực hiện truy vấn DB không?
- Khi từ khóa không khớp gì, UI có hiển thị EmptyState "Không tìm thấy kết quả" không?
- Số lượng kết quả có bị giới hạn đúng ở server (tối đa 50 bài hát, 6 artist) không?
- Từ khóa có được trim khoảng trắng trước khi xử lý không?
- Debounce 300ms phía client có thực sự giảm số lượng request khi user gõ liên tục không?
- Trạng thái liked của mỗi bài hát có được map đúng cho user hiện tại (icon ♡ hiển thị đúng đỏ/xám ngay khi render kết quả) không?
- Tất cả AC-01, AC-02 chạy thực tế cho kết quả khớp với expected không?

## Acceptance Criteria

| AC | Mô tả | TC liên kết |
|----|-------|-------------|
| AC-01 | Nhập từ khóa khớp bài hát/artist -> hiển thị danh sách kết quả | TC-18 |
| AC-02 | Nhập từ khóa không khớp -> hiển thị EmptyState "Không tìm thấy kết quả" | TC-19 |

## Ánh xạ code

| Thành phần | Đường dẫn |
|------------|-----------|
| Song Service (Search) | `Services/SongService.cs` -> `SearchAsync()` |
| Search Component | `Components/` (Search page) |
| JS Debounce | Client-side debounce 300ms |

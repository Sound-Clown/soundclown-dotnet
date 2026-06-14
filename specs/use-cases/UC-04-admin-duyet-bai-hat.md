# UC-04: Admin duyệt bài hát

## Thông tin

| Thuộc tính | Giá trị |
|------------|---------|
| Mã UC | UC-04 |
| Tên | Admin duyệt bài hát |
| Actor chính | Admin |
| Trigger | Admin truy cập trang /admin/songs và thao tác approve/reject bài Pending |
| Mức độ ưu tiên | High |

## Mô tả

Admin duyệt hoặc từ chối bài hát đang ở trạng thái Pending. Bài được approve sẽ công khai trên trang chủ. Bài bị reject sẽ kèm lý do từ chối để Artist biết và chỉnh sửa.

## Điều kiện trước

- Admin đã đăng nhập vào hệ thống
- Admin có vai trò Role = Admin
- Có ít nhất 1 bài hát ở trạng thái Pending

## Luồng chính — Approve bài hát

1. Admin truy cập trang /admin/songs
2. Hệ thống hiển thị danh sách bài hát Pending (mới nhất trước)
3. Admin nhấn nút Duyệt (approve) trên bài hát Pending
4. Hệ thống cập nhật:
   - Song.Status = Approved
   - Song.RejectReason = null (xóa lý do từ chối cũ nếu có)
5. Hệ thống lưu thay đổi vào DB
6. Bài hát công khai trên trang chủ (trang Home, Search)
7. Trả về kết quả: `{ message: "Thành công.", action: "approve" }`

## Luồng thay thế — Reject bài hát

1. Admin nhấn nút Từ chối (reject) trên bài hát Pending
2. Modal hiện ô nhập lý do từ chối
3. Admin nhập lý do từ chối (bắt buộc)
4. Admin nhấn Xác nhận từ chối
5. Hệ thống cập nhật:
   - Song.Status = Rejected
   - Song.RejectReason = lý do đã nhập (trim khoảng trắng)
6. Hệ thống lưu thay đổi vào DB
7. Trả về kết quả: `{ message: "Thành công.", action: "reject" }`

## Luồng ngoại lệ

### R1: Reject không nhập lý do

- Tại bước 3 luồng reject, Admin để trống ô lý do
- Hệ thống hiển thị cảnh báo: "Vui lòng nhập lý do từ chối"
- Modal vẫn mở, không xử lý reject
- Trạng thái bài hát không thay đổi

### R2: Bài hát không tồn tại

- Hệ thống trả lỗi: "Không tìm thấy bài hát."

### R3: Hành động không hợp lệ

- Nếu action không phải "approve" hoặc "reject"
- Hệ thống trả lỗi: "Hành động không hợp lệ."

## Điều kiện sau

- Bài Approved: Status = Approved, công khai trên trang chủ
- Bài Rejected: Status = Rejected, RejectReason có giá trị
- Artist có thể thấy trạng thái và lý do từ chối trên /artist/songs

## Quy tắc nghiệp vụ

| Mã | Quy tắc |
|----|---------|
| BR-01 | Chỉ Admin mới có quyền duyệt bài hát (Role = Admin) |
| BR-02 | Reject bắt buộc phải nhập lý do (không được để trống hoặc chỉ khoảng trắng) |
| BR-03 | Khi approve, RejectReason được set null (xóa lý do cũ nếu có) |
| BR-04 | RejectReason được trim khoảng trắng trước khi lưu |
| BR-05 | Chỉ bài Pending mới xuất hiện trong danh sách duyệt |

## Tiêu chí chất lượng

### Mức ý niệm (Essential / Conceptual) — *UC này để làm gì, tại sao tồn tại?*

- **Mục tiêu nghiệp vụ**: kiểm duyệt nội dung do Artist gửi lên trước khi công khai → bảo vệ chất lượng và tuân thủ quy định nền tảng.
- **Hai ý định đối ngẫu**: chấp nhận (Approved) hoặc từ chối (Rejected); nếu từ chối thì bắt buộc có lý do để Artist hiểu và sửa.
- **Actor**: Admin — phản ánh đúng vai trò moderator.
- **Truy vết tới BR**: UC-04 ↔ BR-03 (Duyệt nội dung).
- **Trung lập công nghệ**: chưa nói tới enum SongStatus, modal UI, hay endpoint.

### Mức thiết kế (Design-level) — *Thiết kế xử lý UC này có hợp lý không?*

- **Hai luồng đối ngẫu** (Approve / Reject) cùng entry point và cùng cấu trúc đầu vào, phân biệt qua tham số action — tránh thiết kế 2 màn hình tách rời gây trùng lặp.
- **Bắt buộc lý do khi reject**: được mô tả tường minh ở cả luồng thay thế và nhánh ngoại lệ → ràng buộc không thể bỏ qua, không phụ thuộc UI validate.
- **Tính sạch trạng thái**: khi approve, lý do từ chối cũ được xóa → tránh dữ liệu rác từ lần reject trước.
- **Trạng thái rõ ràng (state machine)**: chỉ bài Pending mới được review — ngăn việc duyệt lại bài đã Approved/Rejected.
- **Quy tắc nghiệp vụ đầy đủ để suy ra Requirement**: từ các quy tắc có thể sinh TC bao phủ Approve happy path, Reject kèm lý do, Reject thiếu lý do.
- **Tính module**: chỉ xử lý duyệt, không nhúng việc chỉnh sửa nội dung (UC-08) hay khóa Artist (UC-05).
- **Phân quyền là tiền điều kiện**, không trộn vào luồng — đẩy trách nhiệm sang UC-06.

### Mức hiện thực (Concrete / Implementation-level) — *Bản code đã chạy đúng chưa?*

- Khi Admin approve bài Pending, Status có chuyển sang Approved và RejectReason có được reset null không?
- Sau khi approve, bài hát có thực sự xuất hiện công khai trên trang chủ và trang Search không?
- Khi Admin reject kèm lý do, Status có chuyển sang Rejected và RejectReason có lưu đúng (đã trim) không?
- Khi Admin reject mà bỏ trống lý do, hệ thống có chặn (hiển thị "Vui lòng nhập lý do từ chối"), không thay đổi Status không?
- Chỉ tài khoản có Role = Admin mới gọi được API review; các vai trò khác có nhận HTTP 403 không?
- Action không hợp lệ (không phải "approve"/"reject") có bị từ chối với message "Hành động không hợp lệ." không?
- Khi bài hát không tồn tại, response có là "Không tìm thấy bài hát." không?
- Artist (chủ bài) có thấy đúng trạng thái + lý do từ chối trên /artist/songs sau khi Admin review không?
- Tất cả AC-01, AC-02, AC-03 chạy thực tế cho kết quả khớp với expected không?

## Acceptance Criteria

| AC | Mô tả | TC liên kết |
|----|-------|-------------|
| AC-01 | Admin approve bài Pending -> Status = Approved, bài hát công khai | TC-11 |
| AC-02 | Admin reject bài Pending kèm lý do -> Status = Rejected, RejectReason được lưu | TC-12 |
| AC-03 | Admin reject bài Pending không nhập lý do -> cảnh báo "Vui lòng nhập lý do từ chối", bài không bị reject | TC-13 |

## Ánh xạ code

| Thành phần | Đường dẫn |
|------------|-----------|
| Admin Service | `Services/AdminService.cs` -> `ReviewSongAsync()` |
| API Endpoint | `Controllers/TestApiController.cs` -> `POST /api/admin/songs/{id}/review` |
| ReviewSongDto | `DTOs/ReviewSongDto` |
| SongStatus Enum | `Enums/SongStatus.cs` |

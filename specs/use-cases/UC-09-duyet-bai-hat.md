# UC-09: Duyệt bài hát

## Thông tin

| Thuộc tính | Giá trị |
|------------|---------|
| Mã UC | UC-09 |
| Tên | Duyệt bài hát |
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

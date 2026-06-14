# UC-10: Quản lý tài khoản

## Thông tin

| Thuộc tính | Giá trị |
|------------|---------|
| Mã UC | UC-10 |
| Tên | Quản lý tài khoản |
| Actor chính | Admin |
| Trigger | Admin truy cập trang /admin/users và thao tác khóa/mở khóa user |
| Mức độ ưu tiên | Medium |

## Mô tả

Admin có thể khóa hoặc mở khóa tài khoản user. User bị khóa (IsActive = false) không thể đăng nhập. Admin không được khóa chính tài khoản của mình.

## Điều kiện trước

- Admin đã đăng nhập vào hệ thống
- Admin có vai trò Role = Admin

## Luồng chính — Khóa/Mở khóa user

1. Admin truy cập trang /admin/users
2. Hệ thống hiển thị danh sách user (phân trang, mới nhất trước)
3. Admin nhấn nút Khóa/Mở khóa trên dòng user khác (không phải mình)
4. Hệ thống toggle trạng thái `User.IsActive`:
   - Nếu IsActive = true -> chuyển sang false (khóa)
   - Nếu IsActive = false -> chuyển sang true (mở khóa)
5. Hệ thống lưu thay đổi vào DB
6. Trả về kết quả:
   - Khóa: `{ message: "Đã khóa.", isActive: false }`
   - Mở khóa: `{ message: "Đã mở khóa.", isActive: true }`

## Luồng ngoại lệ

### R1: Admin tự khóa tài khoản mình

- Admin nhấn nút Khóa trên dòng tài khoản của chính mình
- Hệ thống từ chối: "Không thể khóa tài khoản của chính mình."
- Trạng thái IsActive không thay đổi
- UI: dòng tài khoản Admin hiện tại không hiển thị nút Khóa/Mở khóa, thay vào đó hiển thị `—`

### R2: User không tồn tại

- Hệ thống trả lỗi: "Không tìm thấy người dùng."

## Điều kiện sau

- User bị khóa: IsActive = false, không thể đăng nhập
- User được mở khóa: IsActive = true, có thể đăng nhập bình thường
- Admin hiện tại luôn giữ IsActive = true

## Quy tắc nghiệp vụ

| Mã | Quy tắc |
|----|---------|
| BR-01 | Admin không được khóa tài khoản của chính mình (targetUserId != adminId) |
| BR-02 | Thao tác là toggle: khóa -> mở khóa và ngược lại |
| BR-03 | User bị khóa (IsActive = false) không thể đăng nhập |
| BR-04 | UI ẩn nút Khóa/Mở khóa trên dòng tài khoản Admin hiện tại, hiển thị `—` |

## Acceptance Criteria

| AC | Mô tả | TC liên kết |
|----|-------|-------------|
| AC-01 | Admin tự khóa mình -> lỗi "Không thể khóa tài khoản của chính mình." | TC-14 |
| AC-02 | Admin khóa/mở khóa user khác -> thành công, isActive toggle đúng | TC-15 |

## Ánh xạ code

| Thành phần | Đường dẫn |
|------------|-----------|
| Admin Service | `Services/AdminService.cs` -> `ToggleLockUserAsync()` |
| API Endpoint | `Controllers/TestApiController.cs` -> `POST /api/admin/users/{id}/toggle-lock` |
| User Entity | `Entities/User.cs` (IsActive) |

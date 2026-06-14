# UC-05: Admin quản lý user

## Thông tin

| Thuộc tính | Giá trị |
|------------|---------|
| Mã UC | UC-05 |
| Tên | Admin quản lý user |
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

## Tiêu chí chất lượng

### Mức ý niệm (Essential / Conceptual) — *UC này để làm gì, tại sao tồn tại?*

- **Mục tiêu nghiệp vụ**: Admin có công cụ vô hiệu hóa tài khoản vi phạm (spam, gian lận, vi phạm điều khoản).
- **Bất biến an toàn**: Admin không được tự khóa chính mình → tránh tình huống "khóa hết admin, không ai mở khóa được" (lockout).
- **Bản chất toggle**: cùng một hành động cho cả khóa và mở khóa, phản ánh đúng ý định "đổi trạng thái".
- **Truy vết tới BR**: UC-05 ↔ BR-04 (Quản lý user).
- **Trung lập công nghệ**: chưa nói tới field `IsActive`, JWT, hay UI button.

### Mức thiết kế (Design-level) — *Thiết kế xử lý UC này có hợp lý không?*

- **Luồng chính ngắn gọn** + nhánh ngoại lệ tách bạch (self-lock, user không tồn tại) → bao phủ đủ rủi ro mà không làm luồng chính phức tạp.
- **Quy tắc tự vệ ở 2 tầng**: server từ chối self-lock và UI ẩn nút trên dòng admin hiện tại → phòng vệ chiều sâu, không phụ thuộc chỉ vào UI.
- **Hậu quả của khóa được nêu tường minh**: user bị khóa không đăng nhập được — giúp suy ra Requirement liên đới ở khâu xác thực, không bị bỏ sót.
- **Quy tắc nghiệp vụ đầy đủ để suy ra Requirement**: từ các quy tắc có thể sinh TC bao phủ self-lock (negative critical) và toggle user khác (positive happy path).
- **Tính toggle**: cùng một hành động phục vụ cả khóa và mở khóa → thiết kế đối xứng, tránh trùng lặp endpoint.
- **Tính module**: chỉ toggle IsActive, không gộp xóa user / đổi role / reset password — mỗi nhiệm vụ có UC riêng nếu cần.

### Mức hiện thực (Concrete / Implementation-level) — *Bản code đã chạy đúng chưa?*

- Khi Admin nhấn Khóa trên user khác đang active, IsActive có chuyển từ true → false và response có là "Đã khóa." không?
- Khi Admin mở khóa user đang bị khóa, IsActive có chuyển false → true và response có là "Đã mở khóa." không?
- Khi Admin cố tình gửi request khóa chính mình (qua API, bỏ qua UI), server có từ chối với "Không thể khóa tài khoản của chính mình." không?
- UI trên trang /admin/users có thực sự ẩn nút Khóa/Mở khóa trên dòng tài khoản Admin hiện tại (hiển thị `—`) không?
- Sau khi user bị khóa, user đó có không đăng nhập lại được không (middleware từ chối)?
- Sau khi user được mở khóa, user đó có đăng nhập lại bình thường không?
- Chỉ tài khoản Role = Admin mới gọi được endpoint toggle-lock; các vai trò khác có nhận 403 không?
- Khi user không tồn tại, response có là "Không tìm thấy người dùng." không?
- Tất cả AC-01, AC-02 chạy thực tế cho kết quả khớp với expected không?

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

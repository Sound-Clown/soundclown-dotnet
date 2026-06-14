# Entity: User

## Định nghĩa

User đại diện cho tài khoản người dùng trong hệ thống. Có 3 vai trò: Listener, Artist, Admin.

## Thuộc tính

| Thuộc tính | Kiểu | Ràng buộc | Mô tả |
|------------|------|-----------|-------|
| Id | int | PK, auto-increment | Định danh duy nhất |
| Username | string | UK, not null | Tên đăng nhập, duy nhất |
| Email | string | UK, not null | Email, duy nhất |
| PasswordHash | string | not null | Mật khẩu đã hash bằng BCrypt (cost 12) |
| Role | Role (enum) | not null, default Listener | Vai trò: Listener=0, Artist=1, Admin=2 |
| IsActive | bool | not null, default true | Trạng thái khóa/mở khóa |
| CreatedAt | DateTime | not null, default UTC now | Thời gian tạo |

## Enum: Role

| Giá trị | Số | Mô tả |
|---------|-----|-------|
| Listener | 0 | Nghe nhạc, tương tác (like, search) |
| Artist | 1 | Tất cả quyền Listener + upload, sửa bài hát, quản lý album |
| Admin | 2 | Tất cả quyền Artist + duyệt bài, quản lý user |

## Quan hệ

| Quan hệ | Entity đích | Loại | Xóa |
|---------|-------------|------|-----|
| Songs | Song | One-to-Many | CASCADE DELETE |
| Albums | Album | One-to-Many | CASCADE DELETE |
| Likes | Like | One-to-Many | CASCADE DELETE |
| PasswordResetToken | PasswordResetToken | One-to-One | CASCADE DELETE |

## Ánh xạ code

`Entities/User.cs`

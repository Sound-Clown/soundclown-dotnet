# Entity: PasswordResetToken

## Định nghĩa

PasswordResetToken đại diện cho token reset mật khẩu. Mỗi user tối đa 1 token đang hiệu lực.

## Thuộc tính

| Thuộc tính | Kiểu | Ràng buộc | Mô tả |
|------------|------|-----------|-------|
| Id | int | PK, auto-increment | Định danh duy nhất |
| UserId | int | FK -> Users.Id, UNIQUE | ID của User (mỗi user tối đa 1 token) |
| Token | string | UK, not null | Chuỗi token duy nhất |
| ExpiresAt | DateTime | not null | Thời gian hết hạn (30 phút sau khi tạo) |
| CreatedAt | DateTime | not null, default UTC now | Thời gian tạo |

## Quan hệ

| Quan hệ | Entity đích | Loại | Xóa |
|---------|-------------|------|-----|
| User | User | One-to-One | CASCADE DELETE |

## Ánh xạ code

`Entities/PasswordResetToken.cs`

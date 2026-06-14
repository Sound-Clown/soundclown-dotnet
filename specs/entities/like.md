# Entity: Like

## Định nghĩa

Like đại diện cho lượt thích bài hát của user. Mỗi user chỉ like 1 bài hát 1 lần (toggle).

## Thuộc tính

| Thuộc tính | Kiểu | Ràng buộc | Mô tả |
|------------|------|-----------|-------|
| UserId | int | PK, FK -> Users.Id | ID của User like |
| SongId | int | PK, FK -> Songs.Id | ID của Bài hát được like |
| CreatedAt | DateTime | not null, default UTC now | Thời gian like |

## Ràng buộc

- Khóa chính ghép: (UserId, SongId) -- đảm bảo mỗi user chỉ like 1 bài 1 lần
- CASCADE DELETE từ cả Users và Songs

## Quan hệ

| Quan hệ | Entity đích | Loại | Xóa |
|---------|-------------|------|-----|
| User | User | Many-to-One | CASCADE DELETE |
| Song | Song | Many-to-One | CASCADE DELETE |

## Ánh xạ code

`Entities/Like.cs`

# Entity: Album

## Định nghĩa

Album đại diện cho tập hợp bài hát của Artist. Bài hát có thể thuộc album hoặc là single (AlbumId = null).

## Thuộc tính

| Thuộc tính | Kiểu | Ràng buộc | Mô tả |
|------------|------|-----------|-------|
| Id | int | PK, auto-increment | Định danh duy nhất |
| Name | string | not null | Tên album (trim trước khi lưu) |
| CoverImage | string? | nullable | URL Cloudinary của ảnh bìa album |
| ArtistId | int | FK -> Users.Id, not null | ID của Artist sở hữu album |
| CreatedAt | DateTime | not null, default UTC now | Thời gian tạo |

## Quan hệ

| Quan hệ | Entity đích | Loại | Xóa | Ghi chú |
|---------|-------------|------|-----|---------|
| Artist | User | Many-to-One | CASCADE DELETE | Xóa user -> xóa tất cả album |
| Songs | Song | One-to-Many | SET NULL (phía Song) | Xóa album -> bài hát thành single |

## Ánh xạ code

`Entities/Album.cs`

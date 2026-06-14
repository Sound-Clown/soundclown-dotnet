# Entity: Song

## Định nghĩa

Song đại diện cho bài hát trong hệ thống. Bài hát trải qua vòng đời: Pending -> Approved/Rejected.

## Thuộc tính

| Thuộc tính | Kiểu | Ràng buộc | Mô tả |
|------------|------|-----------|-------|
| Id | int | PK, auto-increment | Định danh duy nhất |
| Title | string | not null | Tiêu đề bài hát (trim trước khi lưu) |
| AudioFile | string | not null | URL Cloudinary của file audio |
| CoverImage | string? | nullable | URL Cloudinary của ảnh bìa |
| ArtistId | int | FK -> Users.Id, not null | ID của Artist upload bài |
| AlbumId | int? | FK -> Albums.Id, nullable | ID của Album (null = single) |
| Status | SongStatus (enum) | not null, default Pending | Trạng thái duyệt |
| RejectReason | string? | nullable | Lý do từ chối (chỉ khi Status = Rejected) |
| PlayCount | int | not null, default 0 | Số lượt nghe (tăng khi nghe >= 30s) |
| LikeCount | int | not null, default 0 | Số lượt like (denormalized từ bảng Likes) |
| CreatedAt | DateTime | not null, default UTC now | Thời gian tạo |

## Enum: SongStatus

| Giá trị | Số | Mô tả |
|---------|-----|-------|
| Pending | 0 | Chờ Admin duyệt |
| Approved | 1 | Đã duyệt, công khai trên trang chủ |
| Rejected | 2 | Bị từ chối, kèm RejectReason |

## Quan hệ

| Quan hệ | Entity đích | Loại | Xóa | Ghi chú |
|---------|-------------|------|-----|---------|
| Artist | User | Many-to-One | CASCADE DELETE | Xóa user -> xóa tất cả bài |
| Album | Album | Many-to-One | SET NULL | Xóa album -> bài thành single |
| Likes | Like | One-to-Many | CASCADE DELETE | Xóa bài -> xóa tất cả like |

## Index phụ

- `songs.Status` -- tối ưu truy vấn lọc bài đã duyệt
- `songs.ArtistId` -- tối ưu truy vấn bài của artist

## Ánh xạ code

`Entities/Song.cs`, `Enums/SongStatus.cs`

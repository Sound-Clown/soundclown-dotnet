# BR-07: Quản lý album

## Mô tả

Artist tổ chức bài hát thành album. Tạo/sửa/xóa album, thêm/xóa bài hát khỏi album. Xóa album không xóa bài hát bên trong. Chỉ thêm được bài hát thuộc Artist vào album thuộc Artist đó.

## Nguồn

- UC-06: Tạo / Chỉnh sửa Album

## Quy tắc nghiệp vụ

| Mã | Quy tắc |
|----|---------|
| BR-07-01 | Tên album bắt buộc, được trim khoảng trắng |
| BR-07-02 | Chỉ Artist sở hữu album mới được thao tác |
| BR-07-03 | Chỉ thêm được bài hát thuộc Artist vào album thuộc Artist đó |
| BR-07-04 | Ảnh bìa album: JPG/PNG/WebP, tối đa 2MB |
| BR-07-05 | Xóa album: bài hát trở thành single (AlbumId = null), không bị xóa |

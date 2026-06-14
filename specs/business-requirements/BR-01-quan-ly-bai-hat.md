# BR-01: Quản lý bài hát

## Mô tả

Hệ thống cho phép Artist upload, sửa bài hát. Bài hát trải qua vòng đời: Pending -> Approved/Rejected. Chỉ bài Approved mới công khai. Artist sửa bài Approved sẽ reset về Pending để duyệt lại.

## Nguồn

- UC-01: Upload bài hát
- UC-08: Artist sửa bài hát

## Quy tắc nghiệp vụ

| Mã | Quy tắc |
|----|---------|
| BR-01-01 | File audio chỉ chấp nhận MP3, tối đa 10MB |
| BR-01-02 | Ảnh bìa chỉ chấp nhận JPG/PNG/WebP, tối đa 2MB |
| BR-01-03 | Bài hát mới tạo luôn có Status = Pending |
| BR-01-04 | Khi sửa Title hoặc CoverImage, Status reset về Pending |
| BR-01-05 | Chỉ thay đổi AlbumId không trigger reset Status |
| BR-01-06 | AlbumId phải thuộc Artist hiện tại |

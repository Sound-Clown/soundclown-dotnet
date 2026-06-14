# BR-05: Phân quyền truy cập

## Mô tả

Hệ thống kiểm soát quyền truy cập ở hai tầng: route-level (Role-based) và service-level (Ownership). User chỉ thao tác được trên tài nguyên thuộc quyền mình.

## Nguồn

- UC-07: Quản lý bài hát

## Quy tắc nghiệp vụ

| Mã | Quy tắc |
|----|---------|
| BR-05-01 | API Admin chỉ accessible bởi Role = Admin |
| BR-05-02 | Artist chỉ sửa/xóa bài hát thuộc về mình |
| BR-05-03 | Artist chỉ thêm bài hát thuộc về mình vào album của mình |
| BR-05-04 | Bài Pending/Rejected chỉ hiển thị cho chủ bài và Admin |

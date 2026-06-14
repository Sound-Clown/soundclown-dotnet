# BR-04: Quản lý user

## Mô tả

Admin có thể khóa/mở khóa tài khoản user. User bị khóa không thể đăng nhập. Admin không được tự khóa tài khoản của mình.

## Nguồn

- UC-05: Admin quản lý user

## Quy tắc nghiệp vụ

| Mã | Quy tắc |
|----|---------|
| BR-04-01 | Admin không được khóa tài khoản của chính mình |
| BR-04-02 | Thao tác khóa/mở khóa là toggle |
| BR-04-03 | User bị khóa (IsActive = false) không thể đăng nhập |
| BR-04-04 | UI ẩn nút hành động trên dòng Admin hiện tại |

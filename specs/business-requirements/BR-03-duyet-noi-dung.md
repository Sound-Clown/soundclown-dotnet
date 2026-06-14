# BR-03: Duyệt nội dung

## Mô tả

Admin duyệt (approve) hoặc từ chối (reject) bài hát Pending. Reject bắt buộc nhập lý do. Bài Approved công khai, bài Rejected kèm lý do để Artist biết.

## Nguồn

- UC-09: Duyệt bài hát

## Quy tắc nghiệp vụ

| Mã | Quy tắc |
|----|---------|
| BR-03-01 | Chỉ Admin mới có quyền duyệt bài hát |
| BR-03-02 | Reject bắt buộc nhập lý do (không trống/khoảng trắng) |
| BR-03-03 | Khi approve, RejectReason được set null |
| BR-03-04 | RejectReason được trim khoảng trắng trước khi lưu |

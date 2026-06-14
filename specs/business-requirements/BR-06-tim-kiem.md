# BR-06: Tìm kiếm

## Mô tả

User tìm kiếm bài hát và artist theo từ khóa. Tìm kiếm case-insensitive, chỉ trả về bài Approved, có debounce 300ms.

## Nguồn

- UC-07: Tìm kiếm bài hát

## Quy tắc nghiệp vụ

| Mã | Quy tắc |
|----|---------|
| BR-06-01 | Từ khóa tối thiểu 2 ký tự |
| BR-06-02 | Chỉ tìm kiếm bài hát đã Approved |
| BR-06-03 | Tìm kiếm case-insensitive trên Title và Username |
| BR-06-04 | Giới hạn: tối đa 50 bài hát, 6 artist |
| BR-06-05 | Debounce 300ms trên client |

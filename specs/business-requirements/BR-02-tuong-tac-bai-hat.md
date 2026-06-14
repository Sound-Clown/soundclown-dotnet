# BR-02: Tương tác bài hát

## Mô tả

User đã đăng nhập có thể like/unlike bài hát và phát nhạc. Like là toggle idempotent. Play count chỉ tăng khi nghe đủ 30 giây.

## Nguồn

- UC-02: Like/Unlike bài hát
- UC-03: Phát nhạc & đếm lượt nghe

## Quy tắc nghiệp vụ

| Mã | Quy tắc |
|----|---------|
| BR-02-01 | Mỗi user chỉ like 1 bài hát 1 lần (toggle) |
| BR-02-02 | LikeCount không bao giờ âm |
| BR-02-03 | PlayCount chỉ tăng khi nghe >= 30 giây |
| BR-02-04 | PlayCount không tăng khi nghe < 30 giây |

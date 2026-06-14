# BR-02: Tương tác bài hát

## Mô tả

User đã đăng nhập có thể phát nhạc, like/unlike bài hát, và chia sẻ link. Like là toggle idempotent. Play count chỉ tăng khi nghe đủ 30 giây. Chia sẻ copy link vào clipboard.

## Nguồn

- UC-01: Phát nhạc
- UC-03: Like / Unlike bài hát
- UC-04: Chia sẻ bài hát

## Quy tắc nghiệp vụ

| Mã | Quy tắc |
|----|---------|
| BR-02-01 | Mỗi user chỉ like 1 bài hát 1 lần (toggle) |
| BR-02-02 | LikeCount không bao giờ âm |
| BR-02-03 | PlayCount chỉ tăng khi nghe >= 30 giây |
| BR-02-04 | PlayCount không tăng khi nghe < 30 giây |

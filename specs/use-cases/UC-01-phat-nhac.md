# UC-01: Phát nhạc

## Thông tin

| Thuộc tính | Giá trị |
|------------|---------|
| Mã UC | UC-01 |
| Tên | Phát nhạc |
| Actor chính | Listener, Artist |
| Trigger | User nhấn nút Play trên bài hát |
| Mức độ ưu tiên | High |

## Mô tả

User phát bài hát qua HTML5 `<audio>` player. Hệ thống sử dụng JS Interop để điều khiển phát nhạc, tự động chuyển bài tiếp theo trong queue. Lượt nghe (PlayCount) chỉ được tăng khi user nghe đủ 30 giây trở lên, chống gian lận.

## Điều kiện trước

- User đã đăng nhập vào hệ thống
- Có ít nhất 1 bài hát Approved trong hệ thống

## Luồng chính — Phát nhạc & đếm lượt nghe

1. User nhấn nút Play trên bài hát
2. Hệ thống thiết lập Player Queue: danh sách bài hát, chỉ số bài hiện tại, nguồn phát
3. Hệ thống gọi JS Interop để phát audio qua HTML5 `<audio>`
4. Player tự động chuyển bài tiếp theo khi bài hiện tại kết thúc
5. Khi bài hát phát đủ >= 30 giây, JS client gửi request tăng PlayCount
6. Hệ thống tăng `Song.PlayCount` thêm 1 và lưu vào DB

## Luồng thay thế — Nghe dưới 30 giây

1. User nhấn nút Play trên bài hát
2. User dừng phát, chuyển bài khác, hoặc đóng tab trước 30 giây
3. Hệ thống KHÔNG tăng PlayCount

## Luồng thay thế — Điều khiển player

1. **Next**: Nếu chưa phải bài cuối queue, chuyển sang bài tiếp theo
2. **Prev**: Nếu chưa phải bài đầu queue, chuyển về bài trước
3. **Toggle Play/Pause**: Chuyển đổi trạng thái phát/tạm dừng

## Luồng ngoại lệ

### R1: Bài hát không tồn tại

- Hệ thống trả lỗi khi gọi `IncrementPlayCountAsync`: "Không tìm thấy bài hát."

## Điều kiện sau

- Player Queue hiển thị đúng bài đang phát
- PlayCount chỉ tăng khi nghe >= 30 giây
- PlayCount không tăng khi nghe < 30 giây

## Quy tắc nghiệp vụ

| Mã | Quy tắc |
|----|---------|
| BR-01 | PlayCount chỉ tăng khi thời gian phát >= 30 giây |
| BR-02 | PlayCount không tăng nếu thời gian phát < 30 giây (kể cả đóng tab, chuyển bài) |
| BR-03 | Mỗi lần nghe đủ 30 giây, PlayCount tăng đúng 1 đơn vị |
| BR-04 | Queue tự động chuyển bài tiếp theo khi bài hiện tại kết thúc |
| BR-05 | Debounce 300ms cho thao tác tìm kiếm/tương tác tránh gửi request liên tục |

## Acceptance Criteria

| AC | Mô tả | TC liên kết |
|----|-------|-------------|
| AC-01 | Phát nhạc >= 30 giây -> PlayCount tăng 1 | TC-08 |
| AC-02 | Phát nhạc < 30 giây rồi dừng/đóng -> PlayCount không đổi | TC-09 |

## Ánh xạ code

| Thành phần | Đường dẫn |
|------------|-----------|
| Song Service (IncrementPlayCount) | `Services/SongService.cs` -> `IncrementPlayCountAsync()` |
| Player Service | `Services/PlayerService.cs` |
| JS Interop | `wwwroot/js/player.js` |
| Song Entity | `Entities/Song.cs` (PlayCount) |

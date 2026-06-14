# UC-03: Phát nhạc & đếm lượt nghe

## Thông tin

| Thuộc tính | Giá trị |
|------------|---------|
| Mã UC | UC-03 |
| Tên | Phát nhạc & đếm lượt nghe |
| Actor chính | Listener (hoặc bất kỳ user đã đăng nhập) |
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

## Tiêu chí chất lượng

### Mức ý niệm (Essential / Conceptual) — *UC này để làm gì, tại sao tồn tại?*

- **Mục tiêu nghiệp vụ**: cho user trải nghiệm bài hát; đồng thời đo lường mức độ tiếp cận thực sự qua PlayCount.
- **Triết lý "lượt nghe thực"**: chỉ đếm khi user nghe đủ một ngưỡng có ý nghĩa (30 giây) → chống gian lận inflate count bằng auto-click.
- **Actor**: bất kỳ user đã đăng nhập, không phân biệt vai trò — phù hợp với hành vi tiêu thụ nội dung phổ quát.
- **Truy vết tới BR**: UC-03 ↔ BR-02 (Tương tác bài hát).
- **Trung lập công nghệ**: chưa nói tới HTML5 audio, JS Interop, hay endpoint API.

### Mức thiết kế (Design-level) — *Thiết kế xử lý UC này có hợp lý không?*

- **Tách 2 trách nhiệm rõ trong cùng UC**: (a) điều khiển phát nhạc (queue, next/prev, play/pause) và (b) đếm lượt nghe — decoupled, dễ thay đổi độc lập.
- **Quy tắc đếm có ngưỡng định lượng**: 30 giây — đo lường được, không mơ hồ kiểu "nghe đủ lâu".
- **Quy tắc nghiệp vụ đầy đủ để suy ra Requirement**: từ ngưỡng 30s có thể sinh TC bao phủ Positive (đủ ngưỡng), Negative/Boundary (dưới ngưỡng, đúng 30s).
- **Điều khiển queue được mô tả tường minh**: Next/Prev/Toggle Play-Pause + auto-next khi hết bài — không bỏ sót thao tác cơ bản của một music player.
- **Debounce 300ms**: thiết kế tương tác có ràng buộc giảm tải request, không để client gửi liên tục.
- **Tính idempotent của việc tăng count**: mỗi lượt nghe đủ ngưỡng chỉ tăng đúng 1, không bị double-count.
- **Tách bạch khỏi UC khác**: không nhúng việc tính trending, không xử lý like — đúng tinh thần "1 UC một trách nhiệm".

### Mức hiện thực (Concrete / Implementation-level) — *Bản code đã chạy đúng chưa?*

- Khi user phát bài hát đủ ≥ 30 giây, PlayCount trên DB có tăng đúng 1 đơn vị không?
- Khi user phát bài hát < 30 giây rồi dừng / chuyển bài / đóng tab, PlayCount có **không thay đổi** không?
- Mỗi lượt nghe đủ ngưỡng chỉ tăng đúng 1 đơn vị, không bị tăng 2 lần do code gọi nhầm không?
- Player có tự động chuyển sang bài kế tiếp trong queue khi bài hiện tại kết thúc không?
- Các thao tác Next / Prev / Toggle Play-Pause có hoạt động đúng và không vượt biên (Next bài cuối, Prev bài đầu) không?
- Khi bài hát không tồn tại nhưng client vẫn gửi request tăng count, server có trả lỗi "Không tìm thấy bài hát." không?
- Endpoint tăng PlayCount có yêu cầu xác thực không?
- Debounce 300ms cho thao tác tìm kiếm/tương tác có thực sự giảm số lượng request không?
- Tất cả AC-01, AC-02 chạy thực tế cho kết quả khớp với expected không?

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

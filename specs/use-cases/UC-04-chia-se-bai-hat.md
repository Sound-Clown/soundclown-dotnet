# UC-04: Chia sẻ bài hát

## Thông tin

| Thuộc tính | Giá trị |
|------------|---------|
| Mã UC | UC-04 |
| Tên | Chia sẻ bài hát |
| Actor chính | Listener, Artist |
| Trigger | User nhấn nút Chia sẻ trên trang chi tiết bài hát |
| Mức độ ưu tiên | Low |

## Mô tả

User lấy link bài hát để chia sẻ bên ngoài (copy link to clipboard). Link dẫn trực tiếp đến trang chi tiết bài hát `/songs/{id}`. Người nhận link có thể nghe bài hát (nếu bài đã Approved và người nhận đã đăng nhập).

## Điều kiện trước

- User đã đăng nhập vào hệ thống
- User đang xem trang chi tiết bài hát `/songs/{Id}`

## Luồng chính — Chia sẻ link bài hát

1. User truy cập trang `/songs/{Id}` của bài hát
2. User nhấn nút "Chia sẻ"
3. Hệ thống tạo URL: `{App:BaseUrl}/songs/{Id}`
4. Hệ thống gọi JS Interop `copyToClipboard(url)` để copy link vào clipboard
5. Hệ thống hiển thị toast: "Đã copy link!"
6. User có thể paste link vào tin nhắn, mạng xã hội, v.v.

## Luồng thay thế — Người nhận mở link

1. Người nhận (đã đăng nhập) mở link `/songs/{Id}`
2. Hệ thống hiển thị trang chi tiết bài hát với nút Play, Like, Share
3. Người nhận có thể phát nhạc, like bài hát

## Luồng ngoại lệ

### R1: Bài hát không tồn tại hoặc chưa duyệt

- Người nhận mở link nhưng bài hát không tồn tại hoặc chưa Approved
- Nếu không tồn tại: hiển thị EmptyState "Không tìm thấy bài hát"
- Nếu Pending/Rejected và không phải chủ bài/Admin: trả lỗi "Không tìm thấy bài hát"

### R2: Clipboard API không khả dụng

- Trình duyệt không hỗ trợ `navigator.clipboard`
- Hệ thống sử dụng fallback `document.execCommand("copy")`

## Điều kiện sau

- Link bài hát được copy vào clipboard
- Toast xác nhận hiển thị

## Quy tắc nghiệp vụ

| Mã | Quy tắc |
|----|---------|
| BR-01 | URL chia sẻ có dạng `{App:BaseUrl}/songs/{Id}` |
| BR-02 | Chỉ bài hát đã Approved mới hiển thị đầy đủ cho người nhận không phải chủ bài |
| BR-03 | Bài hát Pending/Rejected chỉ hiển thị cho chủ bài (ArtistId == userId) và Admin |

## Tiêu chí chất lượng

### Mức ý niệm (Essential / Conceptual) — *UC này có cần thiết không?*

- **Mục tiêu nghiệp vụ**: cho user lan tỏa nội dung ra bên ngoài hệ thống (mạng xã hội, tin nhắn) — chức năng growth, tăng tiếp cận tự nhiên.
- **Cách tiếp cận đơn giản**: copy link vào clipboard thay vì tích hợp share API phức tạp — phù hợp với độ ưu tiên Low của tính năng.
- **Bảo toàn chính sách kiểm duyệt**: link công khai chỉ hiển thị đầy đủ cho người nhận khi bài Approved → không bypass UC-09 (duyệt).
- **Actor**: bất kỳ user đã đăng nhập — phản ánh tính phổ thông.
- **Truy vết tới BR**: UC-04 ↔ BR-02 (Tương tác bài hát).
- **Trung lập công nghệ**: chưa nói tới `navigator.clipboard`, JS Interop, fallback `execCommand`.

### Mức thiết kế (Design-level) — *Thiết kế xử lý UC này có hợp lý không?*

- **Luồng chính ngắn gọn 6 bước** + 1 luồng phía người nhận + 2 ngoại lệ (bài không tồn tại/chưa duyệt, clipboard API không khả dụng) → bao phủ kịch bản thực tế.
- **Có fallback graceful**: thiết kế đã tính tới trường hợp trình duyệt không hỗ trợ Clipboard API → fallback `execCommand("copy")` thay vì lỗi cứng.
- **URL có cấu trúc rõ**: `{App:BaseUrl}/songs/{Id}` — dễ test, dễ thay đổi base URL khi deploy môi trường khác.
- **Visibility rule rõ**: bài Pending/Rejected chỉ chủ bài và Admin xem được — đồng bộ với UC-07.
- **Quy tắc nghiệp vụ đầy đủ để suy ra Requirement**: có thể sinh TC bao phủ copy thành công, mở link bài Approved, mở link bài Pending/không tồn tại.
- **Tính module**: chỉ xử lý copy link, không nhúng việc track số lượt share (nếu cần sẽ là UC mới).

### Mức hiện thực (Concrete / Implementation-level) — *Bản code đã chạy đúng chưa?*

- Khi user nhấn nút "Chia sẻ", link đúng định dạng `{BaseUrl}/songs/{Id}` có được copy vào clipboard không?
- Toast "Đã copy link!" có hiển thị sau khi copy thành công không?
- Trên trình duyệt không hỗ trợ `navigator.clipboard`, fallback `execCommand("copy")` có hoạt động không?
- Khi người nhận (đã đăng nhập) mở link bài Approved, trang chi tiết bài hát có hiển thị đầy đủ (Play, Like, Share) không?
- Khi người nhận mở link bài không tồn tại, UI có hiển thị EmptyState "Không tìm thấy bài hát" không?
- Khi người nhận **không phải chủ bài/Admin** mở link bài Pending/Rejected, hệ thống có trả "Không tìm thấy bài hát" (không lộ tồn tại) không?
- Khi chính chủ bài hoặc Admin mở link bài Pending/Rejected, họ vẫn xem được không?
- AC-01 chạy thực tế cho kết quả khớp expected không?

## Acceptance Criteria

| AC | Mô tả | TC liên kết |
|----|-------|-------------|
| AC-01 | User nhấn Chia sẻ -> link copy vào clipboard, toast "Đã copy link!" | - |

## Ánh xạ code

| Thành phần | Đường dẫn |
|------------|-----------|
| SongDetail Component (Share) | `Components/Main/SongDetail.razor` -> `Share()` |
| JS Interop (copyToClipboard) | `wwwroot/js/` |
| App:BaseUrl Config | `appsettings.json` |

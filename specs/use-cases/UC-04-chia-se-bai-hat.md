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

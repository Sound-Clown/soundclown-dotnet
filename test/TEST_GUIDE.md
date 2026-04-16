# Hướng Dẫn Thực Hiện Kiểm Thử — SoundClown

> **Mục tiêu**: Thực hiện đầy đủ 20 testcase cho báo cáo Đảm Bảo Chất Lượng Phần Mềm.
> **Công cụ**: Trình duyệt (chụp ảnh thủ công) + Postman (test API).
> **Tiền đề**: App đang chạy tại `http://localhost:5000`, PostgreSQL đang chạy.

---

## 📋 Tổng Quan Testcase

| Tool | Số lượng | Testcase |
|------|----------|----------|
| 🖥️ Trình duyệt (thủ công) | 12 | TC-01,02,06,07,10,13,14,17,18,19 + TC-08,09 (timing) |
| 📮 Postman (API) | 8 | TC-03,04,05,11,12,15,16,20 |
| **Tổng** | **20** | |

---

## 🖥️ PHẦN 1 — Test Thủ Công (12 TC)

### Cách chụp ảnh nhanh

**Windows**: `Win + Shift + S` → kéo chọn vùng → `Ctrl + V` paste vào Word/PowerPoint.

**VS Code** (chụp code đẹp): Cài extension **Polacode** → chuột phải vào code → "Polacode".

**Trình duyệt** (toàn trang): Cài extension **GoFullPage** → F12 → chụp.

> **Quy tắc**: Mỗi testcase cần chụp **ít nhất 1 ảnh kết quả** (thành công HOẶC lỗi tùy case).

---

### TC-01 — Upload bài hát thành công ✅

**Loại**: Black-box, Positive, Functional

**Tài khoản**: `artist@demo.com` / `Artist123!`

**File test**: `test/files/sample_5mb.mp3` (đã chuẩn bị sẵn)

**Các bước**:

1. Đăng nhập `artist@demo.com`
2. Vào **/artist/upload**
3. Nhập Title: `Giấc Mơ Mùa Hè`
4. Drop hoặc chọn file `sample_5mb.mp3` vào vùng upload audio
5. *(Tùy chọn)* Upload ảnh bìa `test/files/sample_cover.jpg`
6. Nhấn **Upload bài hát**
7. Quan sát toast thành công

**Kết quả mong đợi**:
- Toast: "Upload thành công! Bài hát đang chờ duyệt."
- Sang **/artist/songs** thấy bài mới với badge **Pending** (màu vàng)

**Ảnh cần chụp**: (1) Toast thành công | (2) Trang My Songs thấy bài Pending

---

### TC-02 — Upload file .exe đổi đuôi thành .mp3 ❌

**Loại**: Black-box, Negative, Validation

**File test**: `test/files/malicious.exe` (file .exe đã đổi tên thành `malicious.mp3`)

**Các bước**:

1. Đăng nhập `artist@demo.com`
2. Vào **/artist/upload**
3. Chọn file `malicious.mp3`
4. Quan sát phản hồi

**Kết quả mong đợi**: Lỗi hiển thị ngay khi chọn file — "Chỉ chấp nhận file MP3."

**Ảnh cần chụp**: Thông báo lỗi validation.

---

### TC-06 — Upload file đúng 10MB ✅

**Loại**: Boundary Testing

**File test**: `test/files/exact_10mb.mp3` (file chính xác 10,485,760 bytes)

**Các bước**:

1. Đăng nhập `artist@demo.com`
2. Vào **/artist/upload**
3. Chọn file `exact_10mb.mp3`
4. Nhấn **Upload bài hát**

**Kết quả mong đợi**: Upload thành công (vì size = 10MB, không vượt ngưỡng).

**Ảnh cần chụp**: Thành công hoặc lỗi.

---

### TC-07 — Upload file vượt 10MB ❌

**Loại**: Boundary Testing

**File test**: `test/files/over_10mb.mp3` (~10.5MB)

**Các bước**:

1. Chọn file `over_10mb.mp3` vào form upload
2. Quan sát phản hồi

**Kết quả mong đợi**: Lỗi "File âm thanh tối đa 10MB."

**Ảnh cần chụp**: Thông báo lỗi.

---

### TC-10 — Like 5 lần liên tiếp

**Loại**: Black-box, Idempotency

**Các bước**:

1. Đăng nhập `listener@demo.com`
2. Vào **trang Home**
3. Nhấn nút ♡ (Like) **5 lần liên tiếp** vào cùng 1 bài hát trong vòng 3 giây
4. Quan sát icon ♡ và số like

**Kết quả mong đợi**: Like count chỉ tăng **1**, icon ♡ hiển thị trạng thái cuối cùng (đã like).

**Ảnh cần chụp**: Số like count hiển thị sau 5 lần click.

---

### TC-13 — Admin Reject không nhập lý do ❌

**Loại**: Black-box, Validation

**Tài khoản**: `admin@music.com` / `Admin123456!`

**Các bước**:

1. Đăng nhập `admin@music.com`
2. Vào **/admin/songs**
3. Chọn 1 bài đang **Pending**
4. Nhấn nút **Từ chối** (màu đỏ)
5. Để trống ô lý do
6. Nhấn **Từ chối** lần 2

**Kết quả mong đợi**: Button **Từ chối** bị disable (UI) HOẶC hiển thị lỗi "Vui lòng nhập lý do từ chối."

**Ảnh cần chụp**: Button disabled HOẶC thông báo lỗi.

---

### TC-14 — Admin tự khóa tài khoản mình ❌

**Loại**: Black-box, Security

**Tài khoản**: `admin@music.com` / `Admin123456!`

**Các bước**:

1. Đăng nhập `admin@music.com`
2. Vào **/admin/users**
3. Tìm dòng tài khoản của **chính mình** (có badge "Bạn")
4. Quan sát nút hành động

**Kết quả mong đợi**: Không có nút **Khóa/Mở khóa** trên dòng tài khoản của mình. Dòng hiển thị `—`.

**Ảnh cần chụp**: Dòng admin hiện tại — không có nút hành động.

---

### TC-17 — Artist sửa title bài đã Approved → Reset Pending

**Loại**: Black-box, Functional

**Tài khoản**: `artist@demo.com` / `Artist123!`

**Các bước**:

1. Đăng nhập `artist@demo.com`
2. Vào **/artist/songs**
3. Tìm 1 bài đang **Approved** (badge xanh)
4. Nhấn **Edit** (biểu tượng bút)
5. Đổi Title → nhấn **Lưu**
6. Quan sát lại badge của bài đó

**Kết quả mong đợi**: Badge chuyển từ **Approved** (xanh) → **Pending** (vàng).

**Ảnh cần chụp**: (1) Trước khi sửa — badge xanh | (2) Sau khi sửa — badge vàng.

---

### TC-18 — Tìm kiếm có kết quả

**Loại**: Black-box, Functional

**Các bước**:

1. Vào **/search**
2. Nhập từ khóa khớp với ít nhất 1 bài hát (VD: "Summer" nếu có bài "Summer Vibes")
3. Chờ kết quả (debounce ~300ms)

**Kết quả mong đợi**: Hiển thị danh sách bài hát khớp.

**Ảnh cần chụp**: Kết quả tìm kiếm.

---

### TC-19 — Tìm kiếm không khớp

**Loại**: Black-box, Edge Case

**Các bước**:

1. Vào **/search**
2. Nhập `xyz_abc_123_not_exist`
3. Chờ kết quả

**Kết quả mong đợi**: Hiển thị **EmptyState** — "Không tìm thấy kết quả"

**Ảnh cần chụp**: EmptyState.

---

### TC-08 — Play ≥30 giây → Play count tăng 1 ⏱️

**Loại**: Black-box, Functional (timing-sensitive)

**Các bước**:

1. Đăng nhập `listener@demo.com`
2. Vào trang Home, nhấn **Play** 1 bài hát
3. **Bấm đồng hồ**: chờ đủ **35 giây**
4. Vào **/artist/stats** (nếu là artist) HOẶC kiểm tra trong database

**Verify DB**:
```sql
-- Xem play_count trước
SELECT id, title, play_count FROM songs WHERE id = <songId>;

-- Sau 35 giây, kiểm tra lại
SELECT id, title, play_count FROM songs WHERE id = <songId>;
```

**Kết quả mong đợi**: `play_count` tăng thêm **1**.

**Ảnh cần chụp**: (1) Đồng hồ đếm 35s | (2) Số play_count tăng.

---

### TC-09 — Play <30 giây → Play count không đổi ⏱️

**Loại**: Black-box, Negative (timing-sensitive)

**Các bước**:

1. Ghi lại `play_count` hiện tại của 1 bài (VD: bài A có `play_count = 5`)
2. Nhấn **Play** bài A
3. **Bấm đồng hồ**: chờ **15 giây**
4. **Đóng tab trình duyệt** hoặc chuyển bài khác
5. Kiểm tra `play_count` của bài A

**Kết quả mong đợi**: `play_count` của bài A **không thay đổi** (vẫn = 5).

**Ảnh cần chụp**: (1) play_count trước | (2) play_count sau khi đóng tab.

---

## 📮 PHẦN 2 — Test API bằng Postman (8 TC)

### Bước 1 — Cài đặt Postman

```
https://www.postman.com/downloads/
→ Download → Install → Đăng ký tài khoản (hoặc Skip)
```

### Bước 2 — Import Collection

1. Mở Postman
2. Nhấn **Import** (góc trên trái)
3. Kéo file `test/postman/SoundClown_Test_Collection.json` vào
4. Collection **"SoundClown - Test Collection"** xuất hiện trong sidebar

### Bước 3 — Tạo Environment

1. Nhấn biểu tượng **⚙️** (gears) góc trên phải → **Manage Environments**
2. Nhấn **Add**
3. Điền:

| Variable | Initial Value | Current Value |
|----------|-------------|---------------|
| `baseUrl` | `http://localhost:5000` | `http://localhost:5000` |
| `tokenListener` | *(trống)* | *(trống)* |
| `tokenArtist` | *(trống)* | *(trống)* |
| `tokenAdmin` | *(trống)* | *(trống)* |
| `songId` | *(trống)* | *(trống)* |
| `albumId` | *(trống)* | *(trống)* |

4. Chọn environment vừa tạo là **Active**

### Bước 4 — Chạy Setup (lấy token + IDs)

1. Mở folder **"=== 0. SETUP ==="**
2. Chạy lần lượt từ trên xuống:
   - **[A] Login Listener** → tokenListener được set tự động
   - **[B] Login Artist** → tokenArtist được set tự động
   - **[C] Login Admin** → tokenAdmin được set tự động
   - **[D] Get Songs** → songId được set tự động
   - **[E] Get Albums** → albumId được set tự động

3. Kiểm tra biến đã được set: click **Environment** (góc trên phải) → các biến `token*` và `songId` đã có giá trị.

> **Lưu ý**: `/auth/login` trả về JWT token trong body JSON (không redirect nữa). Token được tự động set vào biến Postman bởi script trong tab **Tests** của mỗi request Login.

> **Nếu Get Songs trả về mảng rỗng**: Cần upload ít nhất 1 bài hát trước qua giao diện web (TC-01) để có `songId` test.

### Bước 5 — Chạy các Test Request

#### TC-03 — Like (nhánh Insert) ✅

1. Folder **"=== 1. LIKE / UNLIKE ==="**
2. Mở **"TC-03 Like (nhánh Insert)"** — Bearer token đã được cấu hình sẵn trong request
3. Nhấn **Send**

**Kết quả mong đợi**: HTTP 200, Body:
```json
{ "isLiked": true, "likeCount": 11 }
```

**Ảnh cần chụp**: Response body JSON trong Postman.

---

#### TC-04 — Unlike (nhánh Delete) ✅

1. **"TC-04 Unlike (nhánh Delete)"**
2. Token: `{{tokenAdmin}}`
3. Nhấn **Send** (ngay sau TC-03, cùng bài)

**Kết quả mong đợi**: HTTP 200, Body:
```json
{ "isLiked": false, "likeCount": 10 }
```

**Ảnh cần chụp**: Response body JSON.

---

#### TC-05 — Listener/Artist gọi API Admin → 403 ❌

1. Folder **"=== 2. ADMIN REVIEW ==="**
2. **"TC-05 Listener gọi API Admin → 403"** — Token: `{{tokenListener}}`
   - Nhấn **Send**
   - **Kết quả**: HTTP **403**
   - **Ảnh cần chụp**: Response 403 trong Postman
3. **"TC-05 Artist gọi API Admin → 403"** — Token: `{{tokenArtist}}`
   - Nhấn **Send**
   - **Kết quả**: HTTP **403**

---

#### TC-11 — Review Approve (nhánh Approve) ✅

1. **"TC-11 Review Approve (nhánh approve)"**
2. Token: `{{tokenAdmin}}`
3. Nhấn **Send**

**Kết quả mong đợi**: HTTP 200:
```json
{ "message": "Thành công.", "action": "approve" }
```

> **Lưu ý**: Cần có bài Pending trong DB. Nếu không có, upload bài mới trước (TC-01).

**Ảnh cần chụp**: Response body JSON.

---

#### TC-12 — Review Reject (nhánh Reject) ✅

1. **"TC-12 Review Reject (nhánh reject)"**
2. Token: `{{tokenAdmin}}`
3. Body: `{"action":"reject","rejectReason":"Chất lượng audio kém"}`
4. Nhấn **Send**

**Kết quả mong đợi**: HTTP 200:
```json
{ "message": "Thành công.", "action": "reject" }
```

**Ảnh cần chụp**: Response body JSON.

---

#### TC-14 — Admin tự khóa mình → Lỗi ❌

1. Folder **"=== 3. LOCK USER ==="**
2. **"TC-14 Admin tự khóa mình → lỗi"**
3. Token: `{{tokenAdmin}}`
4. ID `1` = tài khoản admin đầu tiên (admin@music.com)
5. Nhấn **Send**

**Kết quả mong đợi**: HTTP 400 hoặc 403:
```json
{ "error": "Không thể khóa tài khoản của chính mình." }
```

**Ảnh cần chụp**: Response lỗi trong Postman.

---

#### TC-15 — ToggleLockUser hợp lệ ✅

1. **"TC-15 ToggleLockUser hợp lệ"**
2. Token: `{{tokenAdmin}}`
3. ID `2` = listener@demo.com (user khác)
4. Nhấn **Send**

**Kết quả mong đợi**: HTTP 200:
```json
{ "message": "Đã khóa.", "isActive": false }
```
*(Chạy lần 2 sẽ toggle ngược: "Đã mở khóa.", "isActive": true)*

**Ảnh cần chụp**: Response body JSON.

---

#### TC-16 — Artist A sửa bài Artist B → 403 ❌

1. Folder **"=== 4. SECURITY ==="**
2. **"TC-16 Artist A sửa bài Artist B → 403"**
3. Token: `{{tokenListener}}` (listener = user khác, không phải chủ bài)
4. Body: `{"title":"Hacked Title"}`
5. `{{songId}}` = bài của artist khác (bài được upload bởi `artist@demo.com`)
6. Nhấn **Send**

**Kết quả mong đợi**: HTTP **403**:
```json
{ "error": "Không tìm thấy bài hát hoặc bạn không có quyền." }
```

**Ảnh cần chụp**: Response 403 trong Postman.

---

#### TC-20 — Artist thêm bài người khác vào album ❌

1. **"TC-20 Artist thêm bài người khác vào album"**
2. Token: `{{tokenArtist}}`
3. `{{albumId}}` = album thuộc `artist@demo.com`
4. `songId` trong body = bài **không thuộc** artist đó (VD: bài của listener)
5. Nhấn **Send**

**Kết quả mong đợi**: HTTP **400**:
```json
{ "error": "Bài hát không hợp lệ." }
```

**Ảnh cần chụp**: Response lỗi trong Postman.

---

## 🔧 PHẦN 3 — Bonus: Kiểm Tra Database

### Cách 1 — psql trong terminal

```bash
# Kết nối vào container PostgreSQL
docker exec -it soundclown-db psql -U postgres -d soundclown

# Danh sách bảng
\dt

# Xem bài hát (trạng thái, play count)
SELECT id, title, status, play_count, like_count, artist_id
FROM songs ORDER BY id DESC LIMIT 10;

# Xem likes
SELECT * FROM likes LIMIT 10;

# Xem users
SELECT id, username, email, role, is_active FROM users;
```

### Cách 2 — DBeaver (GUI)

```
https://dbeaver.io/download/
```

Connection:
```
Host:     localhost
Port:     5432
Database: soundclown
Username: postgres
Password: postgres
```

---

## ✅ Checklist Tổng Hợp

```
☐ TC-01  [Manual]   Upload thành công → chụp toast + My Songs (badge Pending)
☐ TC-02  [Manual]   Upload .exe đổi .mp3 → chụp lỗi MIME
☐ TC-03  [Postman]  POST /api/songs/{id}/like → chụp JSON {isLiked: true}
☐ TC-04  [Postman]  POST /api/songs/{id}/like → chụp JSON {isLiked: false}
☐ TC-05  [Postman]  POST /api/admin/... với Listener → chụp HTTP 403
☐ TC-05  [Postman]  POST /api/admin/... với Artist → chụp HTTP 403
☐ TC-06  [Manual]   Upload đúng 10MB → chụp thành công
☐ TC-07  [Manual]   Upload 10.5MB → chụp lỗi size
☐ TC-08  [Manual]   Play 35s → chụp play_count tăng (DB verify)
☐ TC-09  [Manual]   Play 15s → chụp play_count không đổi
☐ TC-10  [Manual]   Like 5 lần → chụp count chỉ +1
☐ TC-11  [Postman]  Review Approve → chụp JSON {action: "approve"}
☐ TC-12  [Postman]  Review Reject → chụp JSON {action: "reject"}
☐ TC-13  [Manual]   Reject không nhập lý do → chụp button disabled / lỗi
☐ TC-14  [Manual]   Admin khóa mình → chụp dòng "—" không có nút
☐ TC-15  [Postman]  POST /api/admin/users/2/toggle-lock → chụp JSON
☐ TC-16  [Postman]  PUT /api/songs/{id} → chụp HTTP 403
☐ TC-17  [Manual]   Sửa title bài Approved → chụp badge chuyển Pending
☐ TC-18  [Manual]   Tìm kiếm có kết quả → chụp danh sách
☐ TC-19  [Manual]   Tìm kiếm không kết quả → chụp EmptyState
☐ TC-20  [Postman]  POST /api/albums/{id}/songs → chụp lỗi

Tổng: 11 Manual + 8 Postman = 19 test thực hiện (TC-14 + TC-05 mỗi cái cần 2 tài khoản)
```

---

## 📁 Cấu Trúc Thư Mục

```
soundclown-mvp/
├── test/
│   ├── TEST_GUIDE.md                         ← Hướng dẫn này
│   ├── create_test_files.sh                   ← Script tạo file test (chạy 1 lần)
│   ├── postman/
│   │   └── SoundClown_Test_Collection.json  ← Import vào Postman
│   └── files/                               ← File test (tạo bằng script trên)
│       ├── malicious.mp3                     ← file .exe đổi tên — TC-02
│       ├── exact_10mb.mp3                   ← file 10MB — TC-06
│       ├── over_10mb.mp3                    ← file 10.5MB — TC-07
│       └── sample_5mb.mp3                  ← file 5MB — TC-01
├── Controllers/
│   ├── AuthController.cs                    ← Đã thêm JWT token trong response login
│   └── TestApiController.cs                 ← API endpoints cho Postman test
└── Program.cs                               ← Đã thêm JWT Bearer authentication
```

**Tạo file test** (chạy 1 lần duy nhất):
```bash
bash test/create_test_files.sh
```

---

## ⚠️ Lưu Ý Quan Trọng

1. **Postman + JWT Bearer Auth**: App đã được cấu hình JWT Bearer authentication. Sau khi login thành công, `/auth/login` trả về `{ token: "...", user: {...} }` — token này được tự động lưu vào biến Postman bởi script trong tab Tests của mỗi request Login. Dùng token này trong header `Authorization: Bearer <token>` cho các API test.

2. **Thứ tự test**: Chạy Setup (0) trước để lấy `songId`. Nếu DB rỗng (chưa có bài nào Approved), các API test sẽ fail → cần upload bài qua UI trước.

3. **TC-08/TC-09**: Hai testcase này cần timing thực tế. Không thể test bằng Postman vì cần browser phát audio. Chạy thủ công trên trình duyệt.

4. **File test**: Các file trong `test/files/` cần được tạo thủ công (xem phần "Tạo file test" bên dưới).

---

## 🛠️ Tạo File Test

Chạy script tạo file (cần Linux/macOS/WSL):

```bash
cd soundclown-mvp
bash test/create_test_files.sh
```

Script sẽ tạo 4 file trong `test/files/`:
- `malicious.mp3` — 2KB random (mô phỏng .exe đổi tên) → TC-02
- `exact_10mb.mp3` — 10,485,760 bytes → TC-06
- `over_10mb.mp3` — 11,010,048 bytes → TC-07
- `sample_5mb.mp3` — 5,242,880 bytes → TC-01

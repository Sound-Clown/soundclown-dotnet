# SoundClown — Specification Index

## Cấu trúc folder

```
/specs
├── README.md                       ← File này
├── business-requirements/          ← Nghiệp vụ yêu cầu
│   ├── BR-01-quan-ly-bai-hat.md
│   ├── BR-02-tuong-tac-bai-hat.md
│   ├── BR-03-duyet-noi-dung.md
│   ├── BR-04-quan-ly-tai-khoan.md
│   ├── BR-05-phan-quyen-truy-cap.md
│   ├── BR-06-tim-kiem.md
│   ├── BR-07-quan-ly-album.md
│   └── BR-08-thong-ke-tac-pham.md
├── use-cases/                      ← Đặc tả use case
│   ├── UC-01-phat-nhac.md
│   ├── UC-02-tim-kiem-bai-hat.md
│   ├── UC-03-like-unlike-bai-hat.md
│   ├── UC-04-chia-se-bai-hat.md
│   ├── UC-05-upload-bai-hat.md
│   ├── UC-06-tao-chinh-sua-album.md
│   ├── UC-07-quan-ly-bai-hat.md
│   ├── UC-08-xem-thong-ke-luot-nghe.md
│   ├── UC-09-duyet-bai-hat.md
│   └── UC-10-quan-ly-tai-khoan.md
├── entities/                       ← Đặc tả entity
│   ├── user.md
│   ├── song.md
│   ├── album.md
│   ├── like.md
│   └── password-reset-token.md
├── diagrams/                        ← Diagrams (Mermaid)
│   ├── erd.md
│   ├── song-lifecycle.md
│   └── like-toggle.md
└── tests/                           ← Test specs (mỗi AC 1 file .test)
    └── use-cases/
        ├── UC-01-phat-nhac/              AC-1.test … AC-2.test
        ├── UC-02-tim-kiem-bai-hat/       AC-1.test … AC-2.test
        ├── UC-03-like-unlike-bai-hat/    AC-1.test … AC-3.test
        ├── UC-04-chia-se-bai-hat/        AC-1.test
        ├── UC-05-upload-bai-hat/         AC-1.test … AC-4.test
        ├── UC-06-tao-chinh-sua-album/    AC-1.test … AC-5.test
        ├── UC-07-quan-ly-bai-hat/        AC-1.test … AC-4.test
        ├── UC-08-xem-thong-ke-luot-nghe/ AC-1.test … AC-2.test
        ├── UC-09-duyet-bai-hat/          AC-1.test … AC-3.test
        └── UC-10-quan-ly-tai-khoan/      AC-1.test … AC-2.test
```

## Traceability Chain

```
Business Requirement → Use Case → Acceptance Criteria → Source Code → Test
```

## BR → UC Mapping

| BR | Tên | UC liên kết |
|----|-----|-------------|
| BR-01 | Quản lý bài hát | UC-05, UC-07 |
| BR-02 | Tương tác bài hát | UC-01, UC-03, UC-04 |
| BR-03 | Duyệt nội dung | UC-09 |
| BR-04 | Quản lý tài khoản | UC-10 |
| BR-05 | Phân quyền truy cập | UC-07 |
| BR-06 | Tìm kiếm | UC-02 |
| BR-07 | Quản lý album | UC-06 |
| BR-08 | Thống kê tác phẩm | UC-08 |

## Use Cases

| UC | Tên | Actor chính | Mục đích | AC | TC liên kết | File |
|----|-----|-------------|----------|----|-------------|------|
| UC-01 | Phát nhạc | Listener, Artist | Nghe bài hát liên tục với queue tự động | 2 | TC-08, TC-09 | [UC-01](use-cases/UC-01-phat-nhac.md) |
| UC-02 | Tìm kiếm bài hát | Listener, Artist | Tìm bài hát theo tên hoặc tên nghệ sĩ | 2 | TC-18, TC-19 | [UC-02](use-cases/UC-02-tim-kiem-bai-hat.md) |
| UC-03 | Like / Unlike bài hát | Listener, Artist | Đánh dấu bài hát yêu thích | 3 | TC-03, TC-04, TC-10 | [UC-03](use-cases/UC-03-like-unlike-bai-hat.md) |
| UC-04 | Chia sẻ bài hát | Listener, Artist | Lấy link bài hát để chia sẻ bên ngoài | 1 | - | [UC-04](use-cases/UC-04-chia-se-bai-hat.md) |
| UC-05 | Upload bài hát | Artist | Đăng tải tác phẩm lên nền tảng | 4 | TC-01, TC-02, TC-06, TC-07 | [UC-05](use-cases/UC-05-upload-bai-hat.md) |
| UC-06 | Tạo / Chỉnh sửa Album | Artist | Tổ chức bài hát thành album | 5 | TC-20 | [UC-06](use-cases/UC-06-tao-chinh-sua-album.md) |
| UC-07 | Quản lý bài hát | Artist | Xem, sửa, xóa bài hát đã đăng | 4 | TC-05, TC-16, TC-17, TC-20 | [UC-07](use-cases/UC-07-quan-ly-bai-hat.md) |
| UC-08 | Xem thống kê lượt nghe | Artist | Theo dõi độ phổ biến tác phẩm | 2 | - | [UC-08](use-cases/UC-08-xem-thong-ke-luot-nghe.md) |
| UC-09 | Duyệt bài hát | Admin | Kiểm soát nội dung trước khi phát hành | 3 | TC-11, TC-12, TC-13 | [UC-09](use-cases/UC-09-duyet-bai-hat.md) |
| UC-10 | Quản lý tài khoản | Admin | Khóa/mở khóa tài khoản người dùng | 2 | TC-14, TC-15 | [UC-10](use-cases/UC-10-quan-ly-tai-khoan.md) |

**Tổng**: 8 Business Requirement, 10 Use Case, 28 Acceptance Criteria, 20 Test Case

## TC → UC → BR Mapping

| TC | Mô tả | UC | BR |
|----|-------|----|----|
| TC-01 | Upload thành công | UC-05 | BR-01 |
| TC-02 | Upload .exe đổi .mp3 | UC-05 | BR-01 |
| TC-03 | Like (nhánh Insert) | UC-03 | BR-02 |
| TC-04 | Unlike (nhánh Delete) | UC-03 | BR-02 |
| TC-05 | Listener/Artist gọi API Admin → 403 | UC-07 | BR-05 |
| TC-06 | Upload đúng 10MB | UC-05 | BR-01 |
| TC-07 | Upload vượt 10MB | UC-05 | BR-01 |
| TC-08 | Play >= 30s → count tăng | UC-01 | BR-02 |
| TC-09 | Play < 30s → count không đổi | UC-01 | BR-02 |
| TC-10 | Like 5 lần liên tiếp | UC-03 | BR-02 |
| TC-11 | Review Approve | UC-09 | BR-03 |
| TC-12 | Review Reject | UC-09 | BR-03 |
| TC-13 | Reject không nhập lý do | UC-09 | BR-03 |
| TC-14 | Admin tự khóa mình | UC-10 | BR-04 |
| TC-15 | ToggleLockUser hợp lệ | UC-10 | BR-04 |
| TC-16 | Artist A sửa bài Artist B → 403 | UC-07 | BR-05 |
| TC-17 | Sửa title Approved → Pending | UC-07 | BR-01 |
| TC-18 | Tìm kiếm có kết quả | UC-02 | BR-06 |
| TC-19 | Tìm kiếm không khớp | UC-02 | BR-06 |
| TC-20 | Artist thêm bài người khác vào album | UC-06, UC-07 | BR-05, BR-07 |

## Vai trò hệ thống

| Vai trò | Quyền |
|---------|-------|
| Listener | Nghe nhạc, Like/Unlike, Tìm kiếm, Chia sẻ |
| Artist | Tất cả quyền Listener + Upload, Sửa bài hát, Quản lý Album, Xem thống kê |
| Admin | Tất cả quyền Artist + Duyệt bài hát, Quản lý tài khoản |

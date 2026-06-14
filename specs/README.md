# SoundClown — Specification Index

## Cấu trúc folder

```
/specs
├── README.md                       ← File này
├── business-requirements/          ← Nghiệp vụ yêu cầu
│   ├── BR-01-quan-ly-bai-hat.md
│   ├── BR-02-tuong-tac-bai-hat.md
│   ├── BR-03-duyet-noi-dung.md
│   ├── BR-04-quan-ly-user.md
│   ├── BR-05-phan-quyen-truy-cap.md
│   └── BR-06-tim-kiem.md
├── use-cases/                      ← Đặc tả use case
│   ├── UC-01-upload-bai-hat.md
│   ├── UC-02-like-unlike-bai-hat.md
│   ├── UC-03-phat-nhac-dem-luot-nghe.md
│   ├── UC-04-admin-duyet-bai-hat.md
│   ├── UC-05-admin-quan-ly-user.md
│   ├── UC-06-phan-quyen-truy-cap.md
│   ├── UC-07-tim-kiem-bai-hat.md
│   └── UC-08-artist-sua-bai-hat.md
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
        ├── UC-01-upload-bai-hat/         AC-1.test … AC-4.test
        ├── UC-02-like-unlike-bai-hat/    AC-1.test … AC-3.test
        ├── UC-03-phat-nhac-dem-luot-nghe/  AC-1.test … AC-2.test
        ├── UC-04-admin-duyet-bai-hat/    AC-1.test … AC-3.test
        ├── UC-05-admin-quan-ly-user/     AC-1.test … AC-2.test
        ├── UC-06-phan-quyen-truy-cap/    AC-1.test … AC-3.test
        ├── UC-07-tim-kiem-bai-hat/       AC-1.test … AC-2.test
        └── UC-08-artist-sua-bai-hat/     AC-1.test
```

## Traceability Chain

```
Business Requirement → Use Case → Acceptance Criteria → Source Code → Test
```

## BR → UC Mapping

| BR | Tên | UC liên kết |
|----|-----|-------------|
| BR-01 | Quản lý bài hát | UC-01, UC-08 |
| BR-02 | Tương tác bài hát | UC-02, UC-03 |
| BR-03 | Duyệt nội dung | UC-04 |
| BR-04 | Quản lý user | UC-05 |
| BR-05 | Phân quyền truy cập | UC-06 |
| BR-06 | Tìm kiếm | UC-07 |

## Use Cases

| UC | Tên | AC | TC liên kết | File |
|----|-----|----|-------------|------|
| UC-01 | Upload bài hát | 4 | TC-01, TC-02, TC-06, TC-07 | [UC-01](use-cases/UC-01-upload-bai-hat.md) |
| UC-02 | Like/Unlike bài hát | 3 | TC-03, TC-04, TC-10 | [UC-02](use-cases/UC-02-like-unlike-bai-hat.md) |
| UC-03 | Phát nhạc & đếm lượt nghe | 2 | TC-08, TC-09 | [UC-03](use-cases/UC-03-phat-nhac-dem-luot-nghe.md) |
| UC-04 | Admin duyệt bài hát | 3 | TC-11, TC-12, TC-13 | [UC-04](use-cases/UC-04-admin-duyet-bai-hat.md) |
| UC-05 | Admin quản lý user | 2 | TC-14, TC-15 | [UC-05](use-cases/UC-05-admin-quan-ly-user.md) |
| UC-06 | Phân quyền truy cập | 3 | TC-05, TC-16, TC-20 | [UC-06](use-cases/UC-06-phan-quyen-truy-cap.md) |
| UC-07 | Tìm kiếm bài hát | 2 | TC-18, TC-19 | [UC-07](use-cases/UC-07-tim-kiem-bai-hat.md) |
| UC-08 | Artist sửa bài hát | 1 | TC-17 | [UC-08](use-cases/UC-08-artist-sua-bai-hat.md) |

**Tong**: 6 Business Requirement, 8 Use Case, 20 Acceptance Criteria, 20 Test Case

## TC → UC → BR Mapping

| TC | Mô tả | UC | BR |
|----|-------|----|----|
| TC-01 | Upload thành công | UC-01 | BR-01 |
| TC-02 | Upload .exe đổi .mp3 | UC-01 | BR-01 |
| TC-03 | Like (nhánh Insert) | UC-02 | BR-02 |
| TC-04 | Unlike (nhánh Delete) | UC-02 | BR-02 |
| TC-05 | Listener/Artist gọi API Admin → 403 | UC-06 | BR-05 |
| TC-06 | Upload đúng 10MB | UC-01 | BR-01 |
| TC-07 | Upload vượt 10MB | UC-01 | BR-01 |
| TC-08 | Play >= 30s → count tăng | UC-03 | BR-02 |
| TC-09 | Play < 30s → count không đổi | UC-03 | BR-02 |
| TC-10 | Like 5 lần liên tiếp | UC-02 | BR-02 |
| TC-11 | Review Approve | UC-04 | BR-03 |
| TC-12 | Review Reject | UC-04 | BR-03 |
| TC-13 | Reject không nhập lý do | UC-04 | BR-03 |
| TC-14 | Admin tự khóa mình | UC-05 | BR-04 |
| TC-15 | ToggleLockUser hợp lệ | UC-05 | BR-04 |
| TC-16 | Artist A sửa bài Artist B → 403 | UC-06 | BR-05 |
| TC-17 | Sửa title Approved → Pending | UC-08 | BR-01 |
| TC-18 | Tìm kiếm có kết quả | UC-07 | BR-06 |
| TC-19 | Tìm kiếm không khớp | UC-07 | BR-06 |
| TC-20 | Artist thêm bài người khác vào album | UC-06 | BR-05 |

## Vai trò hệ thống

| Vai trò | Quyền |
|---------|-------|
| Listener | Nghe nhạc, Like/Unlike, Tìm kiếm |
| Artist | Tất cả quyền Listener + Upload, Sửa bài hát, Quản lý Album |
| Admin | Tất cả quyền Artist + Duyệt bài hát, Quản lý user |

# Song Lifecycle — State Diagram

```mermaid
stateDiagram-v2
    [*] --> Pending : Artist upload bài hát

    Pending --> Approved : Admin approve
    Pending --> Rejected : Admin reject (kèm lý do)

    Approved --> Pending : Artist sửa Title/CoverImage

    Rejected --> Pending : Artist sửa Title/CoverImage

    Approved --> [*] : Artist xóa bài / Admin xóa
    Rejected --> [*] : Artist xóa bài / Admin xóa
    Pending --> [*] : Artist xóa bài / Admin xóa
```

## Mô tả chuyển trạng thái

| Chuyển | Trigger | Điều kiện | UC liên kết |
|--------|---------|-----------|-------------|
| [new] -> Pending | Artist upload bài hát | File audio hợp lệ, size <= 10MB | UC-01 |
| Pending -> Approved | Admin nhấn Duyệt | Role = Admin | UC-04 |
| Pending -> Rejected | Admin nhấn Từ chối | Role = Admin, lý do không trống | UC-04 |
| Approved -> Pending | Artist sửa Title hoặc CoverImage | ArtistId == userId | UC-08 |
| Rejected -> Pending | Artist sửa Title hoặc CoverImage | ArtistId == userId | UC-08 |
| [any] -> [deleted] | Artist xóa bài hát | ArtistId == userId | UC-08 |

## Lưu ý

- Chỉ thay đổi AlbumId KHÔNG trigger reset Status
- Bài Approved công khai trên trang chủ và tìm kiếm
- Bài Pending/Rejected chỉ hiển thị cho chủ bài và Admin

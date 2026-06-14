# ERD — Entity Relationship Diagram

```mermaid
erDiagram
    users ||--o{ songs : "uploads"
    users ||--o{ albums : "owns"
    users ||--o{ likes : "makes"
    users ||--o| password_reset_tokens : "has"
    albums ||--o{ songs : "groups"
    songs ||--o{ likes : "receives"

    users {
        int Id PK "auto-increment"
        string Username UK "unique, not null"
        string Email UK "unique, not null"
        string PasswordHash "BCrypt cost 12"
        enum Role "Listener=0, Artist=1, Admin=2"
        bool IsActive "default true"
        datetime CreatedAt "UTC"
    }

    songs {
        int Id PK "auto-increment"
        string Title "not null, trimmed"
        string AudioFile "URL Cloudinary"
        string CoverImage "nullable"
        int ArtistId FK "to users, CASCADE"
        int AlbumId FK "to albums, SET NULL, nullable"
        enum Status "Pending=0, Approved=1, Rejected=2"
        string RejectReason "nullable"
        int PlayCount "default 0"
        int LikeCount "default 0, denormalized"
        datetime CreatedAt "UTC"
    }

    albums {
        int Id PK "auto-increment"
        string Name "not null, trimmed"
        string CoverImage "nullable"
        int ArtistId FK "to users, CASCADE"
        datetime CreatedAt "UTC"
    }

    likes {
        int UserId PK_FK "to users, CASCADE"
        int SongId PK_FK "to songs, CASCADE"
        datetime CreatedAt "UTC"
    }

    password_reset_tokens {
        int Id PK "auto-increment"
        int UserId FK_UK "to users, unique, CASCADE"
        string Token UK "unique"
        datetime ExpiresAt "30 min after creation"
        datetime CreatedAt "UTC"
    }
```

## Ràng buộc xóa

| Quan hệ | Hành vi xóa | Giải thích |
|---------|-------------|------------|
| User -> Song | CASCADE | Xóa user -> xóa toàn bộ bài hát |
| User -> Album | CASCADE | Xóa user -> xóa toàn bộ album |
| User -> Like | CASCADE | Xóa user -> xóa toàn bộ like |
| User -> PasswordResetToken | CASCADE | Xóa user -> xóa token reset |
| Album -> Song | SET NULL | Xóa album -> bài hát thành single (AlbumId = null) |
| Song -> Like | CASCADE | Xóa bài hát -> xóa toàn bộ like |

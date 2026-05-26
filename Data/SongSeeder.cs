using Microsoft.EntityFrameworkCore;
using MusicApp.Entities;
using MusicApp.Enums;

namespace MusicApp.Data;

/// <summary>
/// Seeds artists, albums, and ~1000 songs with placeholder audio (SoundHelix)
/// and deterministic cover images (Picsum). Idempotent — skips if any Song exists.
/// </summary>
public static class SongSeeder
{
    private const int TargetSongs = 1000;
    private const int AlbumCount = 80;

    // SoundHelix CC-licensed demo MP3s — rotated across all songs
    private static readonly string[] AudioUrls = Enumerable.Range(1, 10)
        .Select(i => $"https://www.soundhelix.com/examples/mp3/SoundHelix-Song-{i}.mp3")
        .ToArray();

    private static readonly (string DisplayName, string Username)[] SeedArtists =
    {
        ("Hoàng Mai",     "hoangmai"),
        ("Lan Anh",       "lananh"),
        ("Minh Tú",       "minhtu"),
        ("Bảo Trâm",      "baotram"),
        ("Đức Huy",       "duchuy"),
        ("Mạnh Quân",     "manhquan"),
        ("Phương Linh",   "phuonglinh"),
        ("Hà Anh",        "haanh"),
    };

    private static readonly string[] Moods   = { "Buồn", "Vui", "Lặng", "Yêu", "Nhớ", "Quên", "Mơ", "Hoài", "Cô đơn", "Xa", "Gần", "Vắng" };
    private static readonly string[] Times   = { "Đêm", "Sáng", "Chiều", "Hè", "Đông", "Khuya", "Bình minh", "Hoàng hôn", "Mùa thu", "Mùa xuân" };
    private static readonly string[] Nouns   = { "Mưa", "Nắng", "Gió", "Trăng", "Sao", "Em", "Anh", "Tình yêu", "Kỷ niệm", "Phố", "Biển", "Mây", "Sương", "Phố cũ" };
    private static readonly string[] Verbs   = { "Nhớ", "Yêu", "Quên", "Mơ", "Chờ", "Tìm", "Gặp", "Mất", "Hỏi", "Thương", "Ngắm", "Hát" };
    private static readonly string[] Concepts = { "Hành trình", "Khoảnh khắc", "Ký ức", "Mùa", "Nhịp đập", "Lời chưa nói", "Bình yên", "Vũ điệu", "Tiếng vọng", "Hơi thở", "Dòng sông", "Cánh diều", "Bản tình ca" };

    private static readonly string[] RejectReasons =
    {
        "Chất lượng âm thanh không đạt.",
        "Nội dung vi phạm chính sách.",
        "Trùng lặp với bài đã có.",
        "Ảnh bìa không phù hợp.",
        "Thiếu thông tin nghệ sĩ.",
    };

    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Songs.AnyAsync()) return;

        var rng = new Random(42); // deterministic

        // 1. Artists — include existing artist@demo.com + 8 seed artists
        var artists = new List<User>();
        var demoArtist = await db.Users.FirstOrDefaultAsync(u => u.Email == "artist@demo.com");
        if (demoArtist != null) artists.Add(demoArtist);

        var existingEmails = await db.Users
            .Where(u => u.Role == Role.Artist)
            .Select(u => u.Email)
            .ToListAsync();

        for (int i = 0; i < SeedArtists.Length; i++)
        {
            var (display, username) = SeedArtists[i];
            var email = $"{username}@seed.local";
            if (existingEmails.Contains(email)) continue;
            var user = new User
            {
                Username = username,
                Email = email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Seed12345!"),
                Role = Role.Artist,
                IsActive = true,
                CreatedAt = DateTime.UtcNow.AddDays(-rng.Next(30, 400))
            };
            db.Users.Add(user);
            artists.Add(user);
        }
        await db.SaveChangesAsync();

        // 2. Albums (~80)
        var albums = new List<Album>();
        for (int i = 0; i < AlbumCount; i++)
        {
            var artist = artists[rng.Next(artists.Count)];
            var album = new Album
            {
                Name = GenerateAlbumName(rng, i),
                CoverImage = $"https://picsum.photos/seed/album{i + 1}/600/600",
                ArtistId = artist.Id,
                CreatedAt = DateTime.UtcNow.AddDays(-rng.Next(7, 300))
            };
            db.Albums.Add(album);
            albums.Add(album);
        }
        await db.SaveChangesAsync();

        // 3. Songs (1000) — 80% in album, 20% singles
        var songs = new List<Song>(TargetSongs);
        for (int i = 0; i < TargetSongs; i++)
        {
            Album? album = rng.Next(100) < 80 ? albums[rng.Next(albums.Count)] : null;
            int artistId = album?.ArtistId ?? artists[rng.Next(artists.Count)].Id;

            int statusRoll = rng.Next(100);
            var status = statusRoll < 85 ? SongStatus.Approved
                       : statusRoll < 95 ? SongStatus.Pending
                       :                   SongStatus.Rejected;

            int plays = status == SongStatus.Approved
                ? (int)(Math.Pow(rng.NextDouble(), 3) * 100_000)
                : 0;
            int likes = plays > 0 ? plays / rng.Next(15, 30) : 0;

            songs.Add(new Song
            {
                Title = GenerateSongTitle(rng),
                AudioFile = AudioUrls[i % AudioUrls.Length],
                CoverImage = album?.CoverImage ?? $"https://picsum.photos/seed/song{i + 1}/600/600",
                ArtistId = artistId,
                AlbumId = album?.Id,
                Status = status,
                RejectReason = status == SongStatus.Rejected
                    ? RejectReasons[rng.Next(RejectReasons.Length)]
                    : null,
                PlayCount = plays,
                LikeCount = likes,
                CreatedAt = DateTime.UtcNow.AddDays(-rng.Next(1, 200)).AddMinutes(-rng.Next(0, 1440))
            });
        }

        // Bulk insert in batches to avoid huge SaveChanges transaction
        const int batchSize = 200;
        for (int i = 0; i < songs.Count; i += batchSize)
        {
            db.Songs.AddRange(songs.Skip(i).Take(batchSize));
            await db.SaveChangesAsync();
        }
    }

    private static string GenerateSongTitle(Random rng) => rng.Next(6) switch
    {
        0 => $"{Pick(rng, Moods)} {Pick(rng, Times)}",
        1 => $"{Pick(rng, Verbs)} {Pick(rng, Nouns)}",
        2 => $"{Pick(rng, Moods)} {Pick(rng, Nouns)}",
        3 => $"{Pick(rng, Times)} {Pick(rng, Nouns)}",
        4 => $"{Pick(rng, Verbs)} em mãi",
        _ => $"{Pick(rng, Nouns)} và {Pick(rng, Nouns).ToLowerInvariant()}"
    };

    private static string GenerateAlbumName(Random rng, int idx)
    {
        var concept = Pick(rng, Concepts);
        return rng.Next(3) switch
        {
            0 => $"{concept} {Pick(rng, Moods).ToLowerInvariant()}",
            1 => $"{concept} Vol. {rng.Next(1, 8)}",
            _ => concept
        };
    }

    private static string Pick(Random rng, string[] arr) => arr[rng.Next(arr.Length)];
}

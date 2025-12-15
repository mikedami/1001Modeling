using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace _1001;


//All properties here look great, nice work!
//The use of navigation properties is well done and will help with data retrieval
// Overall, the models are well-structured and follow best practices for Entity Framework Core.

// Songs table
[Table("songs")]
public class Song
{
    [Key]
    [Column("song_id")]
    public int SongId { get; set; }

    [Column("title")]
    [Required]
    public string Title { get; set; } = string.Empty;

    [Column("release_date")]
    public DateOnly? ReleaseDate { get; set; }

    [Column("duration_seconds")]
    public int? DurationSeconds { get; set; }

    [Column("genre")]
    public string? Genre { get; set; }

    [Column("bpm")]
    public int? Bpm { get; set; }

    // Navigation properties
    public ICollection<SongArtist> SongArtists { get; set; } = new List<SongArtist>();
    public ICollection<SetSong> SetSongs { get; set; } = new List<SetSong>();
}

// Artists table
[Table("artists")]
public class Artist
{
    [Key]
    [Column("artist_id")]
    public int ArtistId { get; set; }

    [Column("display_name")]
    [Required]
    public string DisplayName { get; set; } = string.Empty;

    [Column("country")]
    public string? Country { get; set; }

    // Navigation properties
    public ICollection<SongArtist> SongArtists { get; set; } = new List<SongArtist>();
    public ICollection<DjSet> DjSets { get; set; } = new List<DjSet>();
}

// Song_Artists junction table (many-to-many)
[Table("song_artists")]
public class SongArtist
{
    [Key]
    [Column("song_id")]
    [ForeignKey(nameof(Song))]
    public int SongId { get; set; }

    [Key]
    [Column("artist_id")]
    [ForeignKey(nameof(Artist))]
    public int ArtistId { get; set; }

    // Navigation properties
    public Song Song { get; set; } = null!;
    public Artist Artist { get; set; } = null!;
}

// DJ Sets table
[Table("dj_sets")]
public class DjSet
{
    [Key]
    [Column("dj_set_id")]
    public int DjSetId { get; set; }

    [Column("artist_id")]
    [ForeignKey(nameof(Artist))]
    public int ArtistId { get; set; }

    [Column("title")]
    public string? Title { get; set; }

    [Column("set_datetime")]
    public DateTime? SetDatetime { get; set; }

    [Column("duration_minutes")]
    public int? DurationMinutes { get; set; }

    [Column("source_url")]
    public string? SourceUrl { get; set; }

    [Column("venue_id")]
    [ForeignKey(nameof(Venue))]
    public int? VenueId { get; set; }

    // Navigation properties
    public Artist Artist { get; set; } = null!;
    public Venue? Venue { get; set; }
    public ICollection<SetSong> SetSongs { get; set; } = new List<SetSong>();
    public SetAnalytics? SetAnalytics { get; set; }
}

// Set_Songs junction table
[Table("set_songs")]
public class SetSong
{
    [Key]
    [Column("set_song_id")]
    public int SetSongId { get; set; }

    [Column("song_id")]
    [ForeignKey(nameof(Song))]
    public int SongId { get; set; }

    [Column("dj_set_id")]
    [ForeignKey(nameof(DjSet))]
    public int DjSetId { get; set; }

    [Column("timestamp_in_set_seconds")]
    public int? TimestampInSetSeconds { get; set; }

    // Navigation properties
    public Song Song { get; set; } = null!;
    public DjSet DjSet { get; set; } = null!;
}

// Set_Analytics table
[Table("set_analytics")]
public class SetAnalytics
{
    [Key]
    [Column("dj_set_id")]
    [ForeignKey(nameof(DjSet))]
    public int DjSetId { get; set; }

    [Column("tickets_sold")]
    public int? TicketsSold { get; set; }

    [Column("attendance_count")]
    public int? AttendanceCount { get; set; }

    [Column("gross_revenue")]
    public int? GrossRevenue { get; set; }

    [Column("stream_count")]
    public int? StreamCount { get; set; }

    [Column("like_count")]
    public int? LikeCount { get; set; }

    // Navigation property
    public DjSet DjSet { get; set; } = null!;
}

// Venues table
[Table("venues")]
public class Venue
{
    [Key]
    [Column("venue_id")]
    public int VenueId { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("capacity")]
    public int? Capacity { get; set; }

    [Column("address")]
    public string? Address { get; set; }

    // Navigation property
    public ICollection<DjSet> DjSets { get; set; } = new List<DjSet>();
}

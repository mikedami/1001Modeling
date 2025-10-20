using Microsoft.EntityFrameworkCore;

namespace _1001;

class AppDbContext(string dbName = "1001") : DbContext
{
    private readonly string _connectionHost = "localhost";
    private readonly string _connectionDbName = dbName;

    // DbSet properties for all tables
    public DbSet<Song> Songs { get; set; }
    public DbSet<Artist> Artists { get; set; }
    public DbSet<SongArtist> SongArtists { get; set; }
    public DbSet<DjSet> DjSets { get; set; }
    public DbSet<SetSong> SetSongs { get; set; }
    public DbSet<SetAnalytics> SetAnalytics { get; set; }
    public DbSet<Venue> Venues { get; set; }

    // Tell context how to connect to the database
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql($"Host={_connectionHost};Database={_connectionDbName}");
    }


    // Configure composite keys and relationships
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure composite primary key for SongArtist (many-to-many junction table)
        modelBuilder.Entity<SongArtist>()
            .HasKey(sa => new { sa.SongId, sa.ArtistId });

        // Configure Song <-> SongArtist relationship
        modelBuilder.Entity<SongArtist>()
            .HasOne(sa => sa.Song)
            .WithMany(s => s.SongArtists)
            .HasForeignKey(sa => sa.SongId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure Artist <-> SongArtist relationship
        modelBuilder.Entity<SongArtist>()
            .HasOne(sa => sa.Artist)
            .WithMany(a => a.SongArtists)
            .HasForeignKey(sa => sa.ArtistId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure DjSet <-> Artist relationship
        modelBuilder.Entity<DjSet>()
            .HasOne(ds => ds.Artist)
            .WithMany(a => a.DjSets)
            .HasForeignKey(ds => ds.ArtistId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure DjSet <-> Venue relationship
        modelBuilder.Entity<DjSet>()
            .HasOne(ds => ds.Venue)
            .WithMany(v => v.DjSets)
            .HasForeignKey(ds => ds.VenueId)
            .OnDelete(DeleteBehavior.SetNull);

        // Configure SetSong <-> Song relationship
        modelBuilder.Entity<SetSong>()
            .HasOne(ss => ss.Song)
            .WithMany(s => s.SetSongs)
            .HasForeignKey(ss => ss.SongId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure SetSong <-> DjSet relationship
        modelBuilder.Entity<SetSong>()
            .HasOne(ss => ss.DjSet)
            .WithMany(ds => ds.SetSongs)
            .HasForeignKey(ss => ss.DjSetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Configure SetAnalytics <-> DjSet relationship (one-to-one)
        modelBuilder.Entity<SetAnalytics>()
            .HasOne(sa => sa.DjSet)
            .WithOne(ds => ds.SetAnalytics)
            .HasForeignKey<SetAnalytics>(sa => sa.DjSetId)
            .OnDelete(DeleteBehavior.Cascade);
    }
    // Alternatively, separating each configuration 
    // into its own method could enhance reusability if this management system was developed further,
    // but this approach keeps everything concise and looks super clean!

}

// Clear, readable, consice relationships between tables. 
// The flow is intuitive and easy to follow as a user!

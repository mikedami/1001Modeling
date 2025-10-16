using _1001;
using Microsoft.EntityFrameworkCore;


public static class Program
{
    static void Main(string[] args)
    {
        using var context = new AppDbContext();
        
        // Ensure database is created
        context.Database.EnsureCreated();
        
        Console.WriteLine("=== DJ Set Management System ===\n");
        
        while (true)
        {
            Console.WriteLine("\n--- Main Menu ---");
            Console.WriteLine("1. Add new DJ set");
            Console.WriteLine("2. Query database");
            Console.WriteLine("3. Exit");
            Console.Write("\nSelect an option: ");
            
            string? choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    AddDjSet(context);
                    Console.WriteLine("\nDJ Set added successfully!");
                    break;
                case "2":
                    QueryDatabase(context);
                    break;
                case "3":
                    Console.WriteLine("\nGoodbye!");
                    return;
                default:
                    Console.WriteLine("\nInvalid option. Please try again.");
                    break;
            }
        }
    }

    static void QueryDatabase(AppDbContext context)
    {
        while (true)
        {
            Console.WriteLine("\n--- Query Database ---");
            Console.WriteLine("1. Search DJ sets by artist");
            Console.WriteLine("2. Search DJ sets by venue");
            Console.WriteLine("3. Search DJ sets by date range");
            Console.WriteLine("4. View all DJ sets");
            Console.WriteLine("5. Search songs by title");
            Console.WriteLine("6. Search songs by artist");
            Console.WriteLine("7. Search songs by genre");
            Console.WriteLine("8. View DJ set details (with songs)");
            Console.WriteLine("9. View set analytics");
            Console.WriteLine("10. Back to main menu");
            Console.Write("\nSelect query option: ");
            
            string? choice = Console.ReadLine();
            
            switch (choice)
            {
                case "1":
                    SearchDjSetsByArtist(context);
                    break;
                case "2":
                    SearchDjSetsByVenue(context);
                    break;
                case "3":
                    SearchDjSetsByDateRange(context);
                    break;
                case "4":
                    ViewAllDjSets(context);
                    break;
                case "5":
                    SearchSongsByTitle(context);
                    break;
                case "6":
                    SearchSongsByArtist(context);
                    break;
                case "7":
                    SearchSongsByGenre(context);
                    break;
                case "8":
                    ViewDjSetDetails(context);
                    break;
                case "9":
                    ViewSetAnalytics(context);
                    break;
                case "10":
                    return;
                default:
                    Console.WriteLine("\nInvalid option. Please try again.");
                    break;
            }
        }
    }

    static void SearchDjSetsByArtist(AppDbContext context)
    {
        Console.Write("\nEnter artist name to search: ");
        string? searchName = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(searchName))
        {
            Console.WriteLine("Search term cannot be empty.");
            return;
        }
        
        var djSets = context.DjSets
            .Include(ds => ds.Artist)
            .Include(ds => ds.Venue)
            .Where(ds => ds.Artist.DisplayName.Contains(searchName))
            .OrderByDescending(ds => ds.SetDatetime)
            .ToList();
        
        if (!djSets.Any())
        {
            Console.WriteLine($"\nNo DJ sets found for artist containing '{searchName}'.");
            return;
        }
        
        Console.WriteLine($"\n--- Found {djSets.Count} DJ Set(s) ---");
        foreach (var djSet in djSets)
        {
            DisplayDjSetSummary(djSet);
        }
    }

    static void SearchDjSetsByVenue(AppDbContext context)
    {
        Console.Write("\nEnter venue name to search: ");
        string? searchName = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(searchName))
        {
            Console.WriteLine("Search term cannot be empty.");
            return;
        }
        
        var djSets = context.DjSets
            .Include(ds => ds.Artist)
            .Include(ds => ds.Venue)
            .Where(ds => ds.Venue != null && ds.Venue.Name != null && ds.Venue.Name.Contains(searchName))
            .OrderByDescending(ds => ds.SetDatetime)
            .ToList();
        
        if (!djSets.Any())
        {
            Console.WriteLine($"\nNo DJ sets found for venue containing '{searchName}'.");
            return;
        }
        
        Console.WriteLine($"\n--- Found {djSets.Count} DJ Set(s) ---");
        foreach (var djSet in djSets)
        {
            DisplayDjSetSummary(djSet);
        }
    }

    static void SearchDjSetsByDateRange(AppDbContext context)
    {
        Console.Write("\nEnter start date (yyyy-MM-dd) or press Enter to skip: ");
        string? startInput = Console.ReadLine();
        DateTime? startDate = null;
        if (!string.IsNullOrWhiteSpace(startInput) && DateTime.TryParse(startInput, out var start))
        {
            startDate = DateTime.SpecifyKind(start, DateTimeKind.Utc);
        }
        
        Console.Write("Enter end date (yyyy-MM-dd) or press Enter to skip: ");
        string? endInput = Console.ReadLine();
        DateTime? endDate = null;
        if (!string.IsNullOrWhiteSpace(endInput) && DateTime.TryParse(endInput, out var end))
        {
            endDate = DateTime.SpecifyKind(end.AddDays(1).AddSeconds(-1), DateTimeKind.Utc);
        }
        
        var query = context.DjSets
            .Include(ds => ds.Artist)
            .Include(ds => ds.Venue)
            .AsQueryable();
        
        if (startDate.HasValue)
        {
            query = query.Where(ds => ds.SetDatetime >= startDate.Value);
        }
        
        if (endDate.HasValue)
        {
            query = query.Where(ds => ds.SetDatetime <= endDate.Value);
        }
        
        var djSets = query.OrderByDescending(ds => ds.SetDatetime).ToList();
        
        if (!djSets.Any())
        {
            Console.WriteLine("\nNo DJ sets found in the specified date range.");
            return;
        }
        
        Console.WriteLine($"\n--- Found {djSets.Count} DJ Set(s) ---");
        foreach (var djSet in djSets)
        {
            DisplayDjSetSummary(djSet);
        }
    }

    static void ViewAllDjSets(AppDbContext context)
    {
        var djSets = context.DjSets
            .Include(ds => ds.Artist)
            .Include(ds => ds.Venue)
            .OrderByDescending(ds => ds.SetDatetime)
            .ToList();
        
        if (!djSets.Any())
        {
            Console.WriteLine("\nNo DJ sets found in the database.");
            return;
        }
        
        Console.WriteLine($"\n--- All DJ Sets ({djSets.Count} total) ---");
        foreach (var djSet in djSets)
        {
            DisplayDjSetSummary(djSet);
        }
    }

    static void SearchSongsByTitle(AppDbContext context)
    {
        Console.Write("\nEnter song title to search: ");
        string? searchTitle = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(searchTitle))
        {
            Console.WriteLine("Search term cannot be empty.");
            return;
        }
        
        var songs = context.Songs
            .Include(s => s.SongArtists)
            .ThenInclude(sa => sa.Artist)
            .Where(s => s.Title.Contains(searchTitle))
            .ToList();
        
        if (!songs.Any())
        {
            Console.WriteLine($"\nNo songs found with title containing '{searchTitle}'.");
            return;
        }
        
        Console.WriteLine($"\n--- Found {songs.Count} Song(s) ---");
        foreach (var song in songs)
        {
            DisplaySongSummary(song);
        }
    }

    static void SearchSongsByArtist(AppDbContext context)
    {
        Console.Write("\nEnter artist name to search: ");
        string? searchName = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(searchName))
        {
            Console.WriteLine("Search term cannot be empty.");
            return;
        }
        
        var songs = context.Songs
            .Include(s => s.SongArtists)
            .ThenInclude(sa => sa.Artist)
            .Where(s => s.SongArtists.Any(sa => sa.Artist.DisplayName.Contains(searchName)))
            .ToList();
        
        if (!songs.Any())
        {
            Console.WriteLine($"\nNo songs found for artist containing '{searchName}'.");
            return;
        }
        
        Console.WriteLine($"\n--- Found {songs.Count} Song(s) ---");
        foreach (var song in songs)
        {
            DisplaySongSummary(song);
        }
    }

    static void SearchSongsByGenre(AppDbContext context)
    {
        Console.Write("\nEnter genre to search: ");
        string? searchGenre = Console.ReadLine();
        
        if (string.IsNullOrWhiteSpace(searchGenre))
        {
            Console.WriteLine("Search term cannot be empty.");
            return;
        }
        
        var songs = context.Songs
            .Include(s => s.SongArtists)
            .ThenInclude(sa => sa.Artist)
            .Where(s => s.Genre != null && s.Genre.Contains(searchGenre))
            .ToList();
        
        if (!songs.Any())
        {
            Console.WriteLine($"\nNo songs found with genre containing '{searchGenre}'.");
            return;
        }
        
        Console.WriteLine($"\n--- Found {songs.Count} Song(s) ---");
        foreach (var song in songs)
        {
            DisplaySongSummary(song);
        }
    }

    static void ViewDjSetDetails(AppDbContext context)
    {
        Console.Write("\nEnter DJ set ID: ");
        if (!int.TryParse(Console.ReadLine(), out int setId))
        {
            Console.WriteLine("Invalid DJ set ID.");
            return;
        }
        
        var djSet = context.DjSets
            .Include(ds => ds.Artist)
            .Include(ds => ds.Venue)
            .Include(ds => ds.SetSongs)
            .ThenInclude(ss => ss.Song)
            .ThenInclude(s => s.SongArtists)
            .ThenInclude(sa => sa.Artist)
            .FirstOrDefault(ds => ds.DjSetId == setId);
        
        if (djSet == null)
        {
            Console.WriteLine($"\nDJ set with ID {setId} not found.");
            return;
        }
        
        Console.WriteLine("\n=== DJ Set Details ===");
        Console.WriteLine($"ID: {djSet.DjSetId}");
        Console.WriteLine($"Title: {djSet.Title ?? "N/A"}");
        Console.WriteLine($"Artist: {djSet.Artist.DisplayName}");
        Console.WriteLine($"Date/Time: {(djSet.SetDatetime.HasValue ? djSet.SetDatetime.Value.ToString("yyyy-MM-dd HH:mm") : "N/A")}");
        Console.WriteLine($"Duration: {(djSet.DurationMinutes.HasValue ? $"{djSet.DurationMinutes.Value} minutes" : "N/A")}");
        Console.WriteLine($"Venue: {djSet.Venue?.Name ?? "N/A"}");
        Console.WriteLine($"Source URL: {djSet.SourceUrl ?? "N/A"}");
        
        if (djSet.SetSongs.Any())
        {
            Console.WriteLine($"\n--- Tracklist ({djSet.SetSongs.Count} songs) ---");
            foreach (var setSong in djSet.SetSongs.OrderBy(ss => ss.TimestampInSetSeconds ?? int.MaxValue))
            {
                var artists = string.Join(", ", setSong.Song.SongArtists.Select(sa => sa.Artist.DisplayName));
                var timestamp = setSong.TimestampInSetSeconds.HasValue 
                    ? $"[{TimeSpan.FromSeconds(setSong.TimestampInSetSeconds.Value):mm\\:ss}] " 
                    : "";
                Console.WriteLine($"{timestamp}{setSong.Song.Title} - {artists}");
            }
        }
        else
        {
            Console.WriteLine("\nNo songs in this set.");
        }
    }

    static void ViewSetAnalytics(AppDbContext context)
    {
        Console.Write("\nEnter DJ set ID: ");
        if (!int.TryParse(Console.ReadLine(), out int setId))
        {
            Console.WriteLine("Invalid DJ set ID.");
            return;
        }
        
        var djSet = context.DjSets
            .Include(ds => ds.Artist)
            .Include(ds => ds.SetAnalytics)
            .FirstOrDefault(ds => ds.DjSetId == setId);
        
        if (djSet == null)
        {
            Console.WriteLine($"\nDJ set with ID {setId} not found.");
            return;
        }
        
        Console.WriteLine("\n=== DJ Set Analytics ===");
        Console.WriteLine($"Set: {djSet.Title ?? "N/A"} by {djSet.Artist.DisplayName}");
        
        if (djSet.SetAnalytics == null)
        {
            Console.WriteLine("\nNo analytics data available for this set.");
            return;
        }
        
        Console.WriteLine($"\nTickets Sold: {djSet.SetAnalytics.TicketsSold?.ToString() ?? "N/A"}");
        Console.WriteLine($"Attendance: {djSet.SetAnalytics.AttendanceCount?.ToString() ?? "N/A"}");
        Console.WriteLine($"Gross Revenue: {(djSet.SetAnalytics.GrossRevenue.HasValue ? $"${djSet.SetAnalytics.GrossRevenue.Value}" : "N/A")}");
        Console.WriteLine($"Stream Count: {djSet.SetAnalytics.StreamCount?.ToString() ?? "N/A"}");
        Console.WriteLine($"Like Count: {djSet.SetAnalytics.LikeCount?.ToString() ?? "N/A"}");
    }

    static void DisplayDjSetSummary(DjSet djSet)
    {
        Console.WriteLine($"\nID: {djSet.DjSetId} | {djSet.Title ?? "Untitled"}");
        Console.WriteLine($"  Artist: {djSet.Artist.DisplayName}");
        Console.WriteLine($"  Date: {(djSet.SetDatetime.HasValue ? djSet.SetDatetime.Value.ToString("yyyy-MM-dd HH:mm") : "N/A")}");
        Console.WriteLine($"  Venue: {djSet.Venue?.Name ?? "N/A"}");
        Console.WriteLine($"  Duration: {(djSet.DurationMinutes.HasValue ? $"{djSet.DurationMinutes.Value} min" : "N/A")}");
    }

    static void DisplaySongSummary(Song song)
    {
        var artists = string.Join(", ", song.SongArtists.Select(sa => sa.Artist.DisplayName));
        Console.WriteLine($"\nID: {song.SongId} | {song.Title}");
        Console.WriteLine($"  Artist(s): {artists}");
        Console.WriteLine($"  Genre: {song.Genre ?? "N/A"} | BPM: {song.Bpm?.ToString() ?? "N/A"}");
        Console.WriteLine($"  Duration: {(song.DurationSeconds.HasValue ? TimeSpan.FromSeconds(song.DurationSeconds.Value).ToString(@"mm\:ss") : "N/A")}");
        Console.WriteLine($"  Release Date: {(song.ReleaseDate.HasValue ? song.ReleaseDate.Value.ToString("yyyy-MM-dd") : "N/A")}");
    }

    static void AddDjSet(AppDbContext context)
    {
        // Get or create artist for the DJ set
        Console.WriteLine("--- DJ Set Artist ---");
        var artist = GetOrCreateArtist(context);
        
        // Get DJ set basic information
        Console.WriteLine("\n--- DJ Set Information ---");
        Console.Write("Enter DJ set title: ");
        string? title = Console.ReadLine();
        
        Console.Write("Enter set date and time (yyyy-MM-dd HH:mm) or press Enter to skip: ");
        string? dateInput = Console.ReadLine();
        DateTime? setDateTime = null;
        if (!string.IsNullOrWhiteSpace(dateInput) && DateTime.TryParse(dateInput, out var parsedDate))
        {
            setDateTime = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
        }
        
        Console.Write("Enter duration in minutes: ");
        int? durationMinutes = null;
        if (int.TryParse(Console.ReadLine(), out var duration))
        {
            durationMinutes = duration;
        }
        
        Console.Write("Enter source URL (or press Enter to skip): ");
        string? sourceUrl = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(sourceUrl)) sourceUrl = null;
        
        // Get or create venue (optional)
        Console.WriteLine("\n--- Venue Information ---");
        Console.Write("Add venue? (y/n): ");
        Venue? venue = null;
        if (Console.ReadLine()?.ToLower() == "y")
        {
            venue = GetOrCreateVenue(context);
        }
        
        // Create the DJ set
        var djSet = new DjSet
        {
            Artist = artist,
            Title = title,
            SetDatetime = setDateTime,
            DurationMinutes = durationMinutes,
            SourceUrl = sourceUrl,
            Venue = venue
        };
        
        context.DjSets.Add(djSet);
        context.SaveChanges();
        
        // Add songs to the set
        Console.WriteLine("\n--- Add Songs to Set ---");
        AddSongsToSet(context, djSet);
        
        // Optionally add analytics
        Console.WriteLine("\n--- Analytics (Optional) ---");
        Console.Write("Add analytics data? (y/n): ");
        if (Console.ReadLine()?.ToLower() == "y")
        {
            AddSetAnalytics(context, djSet);
        }
        
        context.SaveChanges();
    }

    static Artist GetOrCreateArtist(AppDbContext context)
    {
        Console.Write("Search for existing artist by name (or press Enter to create new): ");
        string? searchName = Console.ReadLine();
        
        if (!string.IsNullOrWhiteSpace(searchName))
        {
            var existingArtists = context.Artists
                .Where(a => a.DisplayName.Contains(searchName))
                .ToList();
            
            if (existingArtists.Any())
            {
                Console.WriteLine("\nFound artists:");
                for (int i = 0; i < existingArtists.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {existingArtists[i].DisplayName} ({existingArtists[i].Country ?? "N/A"})");
                }
                
                Console.Write("Select artist (number) or 0 to create new: ");
                if (int.TryParse(Console.ReadLine(), out int selection) && selection > 0 && selection <= existingArtists.Count)
                {
                    return existingArtists[selection - 1];
                }
            }
        }
        
        // Create new artist
        Console.WriteLine("\n--- Create New Artist ---");
        Console.Write("Enter artist display name: ");
        string displayName = Console.ReadLine() ?? "Unknown Artist";
        
        Console.Write("Enter country (or press Enter to skip): ");
        string? country = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(country)) country = null;
        
        var newArtist = new Artist
        {
            DisplayName = displayName,
            Country = country
        };
        
        context.Artists.Add(newArtist);
        context.SaveChanges();
        
        return newArtist;
    }

    static Venue? GetOrCreateVenue(AppDbContext context)
    {
        Console.Write("Search for existing venue by name (or press Enter to create new): ");
        string? searchName = Console.ReadLine();
        
        if (!string.IsNullOrWhiteSpace(searchName))
        {
            var existingVenues = context.Venues
                .Where(v => v.Name != null && v.Name.Contains(searchName))
                .ToList();
            
            if (existingVenues.Any())
            {
                Console.WriteLine("\nFound venues:");
                for (int i = 0; i < existingVenues.Count; i++)
                {
                    Console.WriteLine($"{i + 1}. {existingVenues[i].Name} - Capacity: {existingVenues[i].Capacity ?? 0}");
                }
                
                Console.Write("Select venue (number) or 0 to create new: ");
                if (int.TryParse(Console.ReadLine(), out int selection) && selection > 0 && selection <= existingVenues.Count)
                {
                    return existingVenues[selection - 1];
                }
            }
        }
        
        // Create new venue
        Console.WriteLine("\n--- Create New Venue ---");
        Console.Write("Enter venue name: ");
        string? name = Console.ReadLine();
        
        Console.Write("Enter capacity: ");
        int? capacity = null;
        if (int.TryParse(Console.ReadLine(), out var cap))
        {
            capacity = cap;
        }
        
        Console.Write("Enter address: ");
        string? address = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(address)) address = null;
        
        var newVenue = new Venue
        {
            Name = name,
            Capacity = capacity,
            Address = address
        };
        
        context.Venues.Add(newVenue);
        context.SaveChanges();
        
        return newVenue;
    }

    static void AddSongsToSet(AppDbContext context, DjSet djSet)
    {
        while (true)
        {
            Console.Write("\nAdd a song to the set? (y/n): ");
            if (Console.ReadLine()?.ToLower() != "y") break;
            
            Console.Write("Search for existing song by title (or press Enter to create new): ");
            string? searchTitle = Console.ReadLine();
            
            Song? song = null;
            
            if (!string.IsNullOrWhiteSpace(searchTitle))
            {
                var existingSongs = context.Songs
                    .Include(s => s.SongArtists)
                    .ThenInclude(sa => sa.Artist)
                    .Where(s => s.Title.Contains(searchTitle))
                    .ToList();
                
                if (existingSongs.Any())
                {
                    Console.WriteLine("\nFound songs:");
                    for (int i = 0; i < existingSongs.Count; i++)
                    {
                        var artists = string.Join(", ", existingSongs[i].SongArtists.Select(sa => sa.Artist.DisplayName));
                        Console.WriteLine($"{i + 1}. {existingSongs[i].Title} by {artists}");
                    }
                    
                    Console.Write("Select song (number) or 0 to create new: ");
                    if (int.TryParse(Console.ReadLine(), out int selection) && selection > 0 && selection <= existingSongs.Count)
                    {
                        song = existingSongs[selection - 1];
                    }
                }
            }
            
            // Create new song if not selected
            if (song == null)
            {
                song = CreateNewSong(context);
            }
            
            // Add song to set with timestamp
            Console.Write("Enter timestamp in set (seconds) or press Enter to skip: ");
            int? timestamp = null;
            if (int.TryParse(Console.ReadLine(), out var ts))
            {
                timestamp = ts;
            }
            
            var setSong = new SetSong
            {
                Song = song,
                DjSet = djSet,
                TimestampInSetSeconds = timestamp
            };
            
            context.SetSongs.Add(setSong);
            context.SaveChanges();
            
            Console.WriteLine($"Added '{song.Title}' to the set!");
        }
    }

    static Song CreateNewSong(AppDbContext context)
    {
        Console.WriteLine("\n--- Create New Song ---");
        Console.Write("Enter song title: ");
        string title = Console.ReadLine() ?? "Untitled";
        
        Console.Write("Enter release date (yyyy-MM-dd) or press Enter to skip: ");
        DateOnly? releaseDate = null;
        if (DateOnly.TryParse(Console.ReadLine(), out var date))
        {
            releaseDate = date;
        }
        
        Console.Write("Enter duration in seconds: ");
        int? duration = null;
        if (int.TryParse(Console.ReadLine(), out var dur))
        {
            duration = dur;
        }
        
        Console.Write("Enter genre: ");
        string? genre = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(genre)) genre = null;
        
        Console.Write("Enter BPM: ");
        int? bpm = null;
        if (int.TryParse(Console.ReadLine(), out var bpmVal))
        {
            bpm = bpmVal;
        }
        
        var song = new Song
        {
            Title = title,
            ReleaseDate = releaseDate,
            DurationSeconds = duration,
            Genre = genre,
            Bpm = bpm
        };
        
        context.Songs.Add(song);
        context.SaveChanges();
        
        // Add artists to song
        Console.WriteLine("\n--- Add Artists to Song ---");
        while (true)
        {
            Console.Write("Add an artist to this song? (y/n): ");
            if (Console.ReadLine()?.ToLower() != "y") break;
            
            var artist = GetOrCreateArtist(context);
            
            var songArtist = new SongArtist
            {
                Song = song,
                Artist = artist
            };
            
            context.SongArtists.Add(songArtist);
            context.SaveChanges();
            
            Console.WriteLine($"Added artist '{artist.DisplayName}' to song!");
        }
        
        return song;
    }

    static void AddSetAnalytics(AppDbContext context, DjSet djSet)
    {
        Console.Write("Enter tickets sold: ");
        int? ticketsSold = null;
        if (int.TryParse(Console.ReadLine(), out var tickets))
        {
            ticketsSold = tickets;
        }
        
        Console.Write("Enter attendance count: ");
        int? attendance = null;
        if (int.TryParse(Console.ReadLine(), out var att))
        {
            attendance = att;
        }
        
        Console.Write("Enter gross revenue: ");
        int? revenue = null;
        if (int.TryParse(Console.ReadLine(), out var rev))
        {
            revenue = rev;
        }
        
        Console.Write("Enter stream count: ");
        int? streams = null;
        if (int.TryParse(Console.ReadLine(), out var str))
        {
            streams = str;
        }
        
        Console.Write("Enter like count: ");
        int? likes = null;
        if (int.TryParse(Console.ReadLine(), out var lik))
        {
            likes = lik;
        }
        
        var analytics = new SetAnalytics
        {
            DjSet = djSet,
            TicketsSold = ticketsSold,
            AttendanceCount = attendance,
            GrossRevenue = revenue,
            StreamCount = streams,
            LikeCount = likes
        };
        
        context.SetAnalytics.Add(analytics);
        context.SaveChanges();
        
        Console.WriteLine("Analytics added successfully!");
    }
}


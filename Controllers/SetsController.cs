using Microsoft.AspNetCore.Mvc;
using _1001;
using Microsoft.EntityFrameworkCore;

namespace Notesbin.Controllers;

public class SetsController : Controller
{
    private readonly AppDbContext _context;

    public SetsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: Sets
    public async Task<IActionResult> Index()
    {
        var sets = await _context.DjSets
            .Include(s => s.Artist)
            .Include(s => s.Venue)
            .Include(s => s.SetAnalytics)
            .OrderByDescending(s => s.SetDatetime)
            .ToListAsync();
        return View(sets);
    }

    // GET: Sets/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null) return NotFound();

        var set = await _context.DjSets
            .Include(s => s.Artist)
            .Include(s => s.Venue)
            .Include(s => s.SetSongs)
                .ThenInclude(ss => ss.Song)
                    .ThenInclude(song => song.SongArtists)
                        .ThenInclude(sa => sa.Artist)
            .Include(s => s.SetAnalytics)
            .FirstOrDefaultAsync(m => m.DjSetId == id);

        if (set == null) return NotFound();

        return View(set);
    }

    // GET: Sets/Create
    public IActionResult Create()
    {
        return View();
    }

    //This is a large HTTP post, it might be good to add some console debugging logs throughout
    //the post, just to have peace of mind if anything goes wrong at a certain point. Sort of like
    //having checkpoints to see where things might be failing.

    // POST: Sets/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateSetViewModel model)
    {
        // 1. Handle Artist

        //for example, you could wrap these in try catch blocks to catch any exceptions that may arise
        var artist = await _context.Artists.FirstOrDefaultAsync(a => a.DisplayName == model.ArtistName);

        try
        {
            if (artist == null)
            {
                artist = new Artist { DisplayName = model.ArtistName };
                _context.Artists.Add(artist);
                await _context.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling artist: {ex.Message}");
            //or write your own error handling
        }

        // 2. Handle Venue
        var venue = await _context.Venues.FirstOrDefaultAsync(v => v.Name == model.VenueName);
        if (venue == null)
        {
            venue = new Venue { Name = model.VenueName };
            _context.Venues.Add(venue);
            await _context.SaveChangesAsync();
        }

        // 3. Create Set
        var djSet = new DjSet
        {
            Title = model.Title,
            ArtistId = artist.ArtistId,
            SetDatetime = model.SetDatetime.ToUniversalTime(),
            VenueId = venue.VenueId
        };
        _context.DjSets.Add(djSet);
        await _context.SaveChangesAsync();

        // 4. Analytics
        var analytics = new SetAnalytics
        {
            DjSetId = djSet.DjSetId,
            TicketsSold = model.TicketsSold
        };
        _context.SetAnalytics.Add(analytics);

        // 5. Tracklist
        if (model.Tracklist != null)
        {
            foreach (var entry in model.Tracklist)
            {
                if (string.IsNullOrWhiteSpace(entry.SongTitle)) continue;

                // Handle Song
                var song = await _context.Songs.FirstOrDefaultAsync(s => s.Title == entry.SongTitle);
                if (song == null)
                {
                    song = new Song { Title = entry.SongTitle };
                    _context.Songs.Add(song);
                    await _context.SaveChangesAsync();
                }

                // Handle Song Artist (if provided)
                if (!string.IsNullOrWhiteSpace(entry.ArtistName))
                {
                    var songArtist = await _context.Artists.FirstOrDefaultAsync(a => a.DisplayName == entry.ArtistName);
                    if (songArtist == null)
                    {
                        songArtist = new Artist { DisplayName = entry.ArtistName };
                        _context.Artists.Add(songArtist);
                        await _context.SaveChangesAsync();
                    }

                    // Link Song and Artist if not already linked
                    var existingLink = await _context.SongArtists.FindAsync(song.SongId, songArtist.ArtistId);
                    if (existingLink == null)
                    {
                        _context.SongArtists.Add(new SongArtist
                        {
                            SongId = song.SongId,
                            ArtistId = songArtist.ArtistId
                        });
                        await _context.SaveChangesAsync();
                    }
                }

                //This is great, good handling of the tracklist entries
                //Nice to link things together

                // Link Song to Set
                _context.SetSongs.Add(new SetSong
                {
                    DjSetId = djSet.DjSetId,
                    SongId = song.SongId
                });
            }
            await _context.SaveChangesAsync();
        }

        return RedirectToAction(nameof(Index));
    }
}

//Good organization to have the view model classes down here

public class CreateSetViewModel
{
    public string Title { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public DateTime SetDatetime { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public int TicketsSold { get; set; }
    public List<TracklistEntry> Tracklist { get; set; } = new List<TracklistEntry>();
}

public class TracklistEntry
{
    public string SongTitle { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
}

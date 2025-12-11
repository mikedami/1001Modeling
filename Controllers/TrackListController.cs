using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using _1001;
using Microsoft.EntityFrameworkCore;

namespace Notesbin.Controllers;

[ApiController]
[Route("[controller]")]
public class TrackListController : Controller
{
    private readonly AppDbContext _context;

    public TrackListController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /TrackList/Sets
    [HttpGet("sets")]
    public async Task<IActionResult> Sets()
    {
        // Include navigation properties needed for the view
        var sets = await _context.DjSets
            .Include(s => s.Artist)
            .Include(s => s.Venue)
            .Include(s => s.SetSongs)
                .ThenInclude(ss => ss.Song)
            .Include(s => s.SetAnalytics)
            .ToListAsync();

        return View(sets);
    }

    // GET: /TrackList/Sets/{id}
    [HttpGet("sets/{id}")]
    public async Task<IActionResult> Sets(int id)
    {
        var set = await _context.DjSets
            .Include(s => s.Artist)
            .Include(s => s.Venue)
            .Include(s => s.SetSongs)
                .ThenInclude(ss => ss.Song)
            .Include(s => s.SetAnalytics)
            .FirstOrDefaultAsync(s => s.DjSetId == id);

        if (set == null)
            return NotFound();

        return View("SetsDetails", set);
    }

    // POST: /TrackList/AddSet
    [HttpPost("AddSet")]
    public async Task<IActionResult> AddSet([FromBody] AddSetRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // 1. Handle Artist (DJ)
        var artist = await _context.Artists.FirstOrDefaultAsync(a => a.DisplayName == request.Artist);
        if (artist == null)
        {
            artist = new Artist { DisplayName = request.Artist };
            _context.Artists.Add(artist);
            await _context.SaveChangesAsync();
        }

        // 2. Handle Venue
        var venue = await _context.Venues.FirstOrDefaultAsync(v => v.Name == request.Venue);
        if (venue == null)
        {
            venue = new Venue { Name = request.Venue };
            _context.Venues.Add(venue);
            await _context.SaveChangesAsync();
        }

        // 3. Create DjSet
        var djSet = new DjSet
        {
            Title = request.Title,
            ArtistId = artist.ArtistId,
            SetDatetime = request.Date,
            VenueId = venue.VenueId
        };
        _context.DjSets.Add(djSet);
        await _context.SaveChangesAsync();

        // 4. Handle Analytics
        var analytics = new SetAnalytics
        {
            DjSetId = djSet.DjSetId,
            TicketsSold = request.TicketsSold
        };
        _context.SetAnalytics.Add(analytics);

        // 5. Handle Tracklist
        if (request.Tracklist != null)
        {
            foreach (var songTitle in request.Tracklist)
            {
                var song = await _context.Songs.FirstOrDefaultAsync(s => s.Title == songTitle);
                if (song == null)
                {
                    song = new Song { Title = songTitle };
                    _context.Songs.Add(song);
                    await _context.SaveChangesAsync();
                }

                var setSong = new SetSong
                {
                    DjSetId = djSet.DjSetId,
                    SongId = song.SongId
                };
                _context.SetSongs.Add(setSong);
            }
        }

        await _context.SaveChangesAsync();

        var response = new DjSetResponse
        {
            DjSetId = djSet.DjSetId,
            Title = djSet.Title ?? string.Empty,
            ArtistName = artist.DisplayName,
            SetDatetime = djSet.SetDatetime,
            VenueName = venue.Name ?? string.Empty,
            TicketsSold = analytics.TicketsSold,
            Tracklist = request.Tracklist
        };

        return Ok(response);
    }

    // POST: /TrackList/AddSong
    [HttpPost("AddSong")]
    public async Task<IActionResult> AddSong([FromBody] AddSongRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // 1. Create Song
        var song = new Song
        {
            Title = request.Name,
            Bpm = request.BPM
        };
        _context.Songs.Add(song);
        await _context.SaveChangesAsync();

        // 2. Handle Artist
        var artist = await _context.Artists.FirstOrDefaultAsync(a => a.DisplayName == request.Artist);
        if (artist == null)
        {
            artist = new Artist { DisplayName = request.Artist };
            _context.Artists.Add(artist);
            await _context.SaveChangesAsync();
        }

        // 3. Link Song and Artist
        var songArtist = new SongArtist
        {
            SongId = song.SongId,
            ArtistId = artist.ArtistId
        };
        _context.SongArtists.Add(songArtist);
        await _context.SaveChangesAsync();

        var response = new SongResponse
        {
            SongId = song.SongId,
            Title = song.Title,
            Bpm = song.Bpm,
            ArtistName = artist.DisplayName
        };

        return Ok(response);
    }
}

public class AddSetRequest
{
    public string Title { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Venue { get; set; } = string.Empty;
    public int TicketsSold { get; set; }
    public List<string> Tracklist { get; set; } = new List<string>();
}

public class AddSongRequest
{
    public string Name { get; set; } = string.Empty;
    public string Artist { get; set; } = string.Empty;
    public int BPM { get; set; }
    public string Key { get; set; } = string.Empty;
}

public class DjSetResponse
{
    public int DjSetId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ArtistName { get; set; } = string.Empty;
    public DateTime? SetDatetime { get; set; }
    public string VenueName { get; set; } = string.Empty;
    public int? TicketsSold { get; set; }
    public List<string> Tracklist { get; set; } = new List<string>();
}

public class SongResponse
{
    public int SongId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int? Bpm { get; set; }
    public string ArtistName { get; set; } = string.Empty;
}
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

    // GET: /TrackList/Sets/{id}
    [HttpGet("sets/artists/{id}")]
    public async Task<IActionResult> GetSetsByArtist(int id)
    {
        var sets = await _context.DjSets
            .Include(s => s.Artist)
            .Include(s => s.Venue)
            .Include(s => s.SetSongs)
                .ThenInclude(ss => ss.Song)
            .Include(s => s.SetAnalytics)
            .Where(s => s.ArtistId == id)
            .ToListAsync();

        return View("Sets", sets);
    }

    // GET: /TrackList/Venue/{id}
    [HttpGet("sets/venue/{id}")]
    public async Task<IActionResult> GetSetsByVenue(int id)
    {
        var sets = await _context.DjSets
            .Include(s => s.Artist)
            .Include(s => s.Venue)
            .Include(s => s.SetSongs)
                .ThenInclude(ss => ss.Song)
            .Include(s => s.SetAnalytics)
            .Where(s => s.VenueId == id)
            .ToListAsync();

        return View("Sets", sets);
    }

    // GET: /TrackList/Sets/Date
    [HttpGet("sets/date/{start}/{end}")]
    public async Task<IActionResult> GetSetsByArtist(DateTime start, DateTime? end = null)
    {
        if (end == null)
        {
            end = DateTime.MaxValue;
        }
        var sets = await _context.DjSets
            .Include(s => s.Artist)
            .Include(s => s.Venue)
            .Include(s => s.SetSongs)
                .ThenInclude(ss => ss.Song)
            .Include(s => s.SetAnalytics)
            .Where(s => s.SetDatetime >= start && s.SetDatetime <= end)
            .ToListAsync();

        return View("Sets", sets);
    }
}
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using _1001;
using Microsoft.EntityFrameworkCore;

namespace Notesbin.Controllers;


public class TrackListController : Controller
{
    private readonly AppDbContext _context;

    public TrackListController(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(await _context.DjSets.ToListAsync());
    }
}
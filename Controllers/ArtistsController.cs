using Microsoft.AspNetCore.Mvc;
using _1001;
using System.Threading.Tasks;

namespace Notesbin.Controllers;

[ApiController]
[Route("[controller]")]
public class ArtistsController : ControllerBase
{
    private readonly AppDbContext _context;

    public ArtistsController(AppDbContext context) => _context = context;

    // POST /Artists
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] string name)
    {
      
        var artist = new Artist { DisplayName = name };
        _context.Artists.Add(artist);
        await _context.SaveChangesAsync();

        return Ok(artist);
    }
}

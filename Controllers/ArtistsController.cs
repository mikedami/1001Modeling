using Microsoft.AspNetCore.Mvc;
using _1001;
using System.Threading.Tasks;

namespace Notesbin.Controllers;

//I like how you split up the controllers into their own files for better organization
//This is great, instead of having one large controller file

//API route here looks good, well done, simple and effective

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

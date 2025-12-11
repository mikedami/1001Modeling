using Microsoft.AspNetCore.Mvc;
using _1001;
using System.Linq;
using System.Threading.Tasks;

namespace Notesbin.Controllers;

[ApiController]
[Route("[controller]")]
public class VenuesController : ControllerBase
{
    private readonly AppDbContext _context;

    public VenuesController(AppDbContext context) => _context = context;

    // POST /Venues
    [HttpPost]
    public async Task<IActionResult> Post([FromBody] VenueInput input)
    {
        var addressParts = new[] { input.Location, input.City, input.Country }
                            .Where(s => !string.IsNullOrWhiteSpace(s));
        var venue = new Venue
        {
            Name = input.Name,
            Address = addressParts.Any() ? string.Join(", ", addressParts) : null,
            Capacity = input.Capacity
        };

        _context.Venues.Add(venue);
        await _context.SaveChangesAsync();

        return Ok(venue);
    }

    public record VenueInput(string Name, string? Location, string? City, string? Country, int? Capacity);
}

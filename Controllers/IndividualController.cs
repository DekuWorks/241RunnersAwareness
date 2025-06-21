using Microsoft.AspNetCore.Mvc;
using _241RunnersAwareness.BackendAPI.Data;
using _241RunnersAwareness.BackendAPI.Models; // make sure this matches your project
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;
using _241RunnersAwareness.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Security.Cryptography;
using System.Text;

namespace _241RunnersAwareness.BackendAPI.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class IndividualController : ControllerBase
    {
        private readonly RunnersDbContext _context;

        public IndividualController(RunnersDbContext context)
        {
            _context = context;
        }

        // GET: api/Individual/mycase
        [HttpGet("mycase")]
        public async Task<ActionResult<Individual>> GetMyCase()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized();
            }

            var individual = await _context.Individuals
                .Include(i => i.EmergencyContacts)
                .FirstOrDefaultAsync(i => i.Id == userId);

            if (individual == null)
            {
                return NotFound();
            }

            return individual;
        }

        // GET: api/Individual
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Individual>>> GetIndividuals()
        {
            return _context.Individuals.ToList();
        }

        // POST: api/individual
        [HttpPost]
        public async Task<ActionResult<Individual>> PostIndividual(Individual individual)
        {
            _context.Individuals.Add(individual);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetIndividuals), new { id = individual.IndividualId }, individual);
        }

        [HttpPost("{id}/photo")]
        public async Task<IActionResult> UploadPhoto(string id, IFormFile file)
        {
            var individual = await _context.Individuals.FindAsync(id);
            if (individual == null)
            {
                return NotFound();
            }

            // In a real application, you would upload the file to a cloud storage
            // service (e.g., Azure Blob Storage, AWS S3) and get back a URL.
            // For this example, we'll just simulate it.
            var photoUrl = $"/images/{id}-{Guid.NewGuid()}-{file.FileName}";

            individual.PhotoUrl = photoUrl;
            individual.LastPhotoUpdate = DateTime.UtcNow;

            _context.Individuals.Update(individual);
            await _context.SaveChangesAsync();

            return Ok(new { photoUrl = individual.PhotoUrl });
        }
    }
}

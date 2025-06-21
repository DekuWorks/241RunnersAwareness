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
using Microsoft.EntityFrameworkCore;
using _241RunnersAwareness.DTOs;

namespace _241RunnersAwareness.BackendAPI.Controllers
{
    [Authorize(Roles = "Admin")]
    [ApiController]
    [Route("api/[controller]")]
    public class IndividualController : ControllerBase
    {
        private readonly RunnersDbContext _context;

        public IndividualController(RunnersDbContext context)
        {
            _context = context;
        }

        // GET: api/Individual
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Individual>>> GetIndividuals()
        {
            return await _context.Individuals.ToListAsync();
        }

        // GET: api/Individual/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Individual>> GetIndividual(Guid id)
        {
            var individual = await _context.Individuals.FindAsync(id);
            if (individual == null)
            {
                return NotFound();
            }

            return individual;
        }

        // PUT: api/Individual/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateIndividual(Guid id, [FromBody] AdminCaseUpdateDto updateDto)
        {
            var individual = await _context.Individuals.FindAsync(id);
            if (individual == null)
            {
                return NotFound();
            }

            individual.FullName = updateDto.FullName;
            individual.CurrentStatus = updateDto.CurrentStatus;

            _context.Entry(individual).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Individuals.Any(e => e.IndividualId == id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
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
        public async Task<IActionResult> UploadPhoto(Guid id, IFormFile file)
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

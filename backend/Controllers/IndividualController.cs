using Microsoft.AspNetCore.Mvc;
using _241RunnersAwareness.BackendAPI.Data;
using _241RunnersAwareness.BackendAPI.Models; // make sure this matches your project
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace _241RunnersAwareness.BackendAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class IndividualController : ControllerBase
    {
        private readonly RunnersDbContext _context;

        public IndividualController(RunnersDbContext context)
        {
            _context = context;
        }

        // GET: api/individual
        [HttpGet]
        public ActionResult<IEnumerable<Individual>> GetIndividuals()
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
    }
}

using Blaga_Alexia_Denisa_Lab4.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Blaga_Alexia_Denisa_Lab4.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PredictionApiController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PredictionApiController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/PredictionApi
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var predictions = await _context.PredictionHistories
                .AsNoTracking()
                .ToListAsync();

            return Ok(predictions);
        }

        // DELETE: api/PredictionApi/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteById(int id)
        {
            var entity = await _context.PredictionHistories.FindAsync(id);

            if (entity == null)
                return NotFound(new { message = $"Prediction with id={id} not found." });

            _context.PredictionHistories.Remove(entity);
            await _context.SaveChangesAsync();

            return NoContent(); // 204
        }
    }
}


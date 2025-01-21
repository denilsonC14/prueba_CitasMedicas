// ProcedimientosController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionCitasMedicas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProcedimientosController : ControllerBase
    {
        private readonly AppDBContext _dbContext;

        public ProcedimientosController(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetProcedimientos()
        {
            return Ok(await _dbContext.Procedimientos.Include(p => p.Cita).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProcedimiento(int id)
        {
            var procedimiento = await _dbContext.Procedimientos
                .Include(p => p.Cita)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (procedimiento == null) return NotFound("El procedimiento no existe");
            return Ok(procedimiento);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProcedimiento(Procedimiento procedimiento)
        {
            var cita = await _dbContext.Citas.FindAsync(procedimiento.CitaId);
            if (cita == null) return BadRequest("La cita no existe");

            if (procedimiento.Costo < 0)
                return BadRequest("El costo no puede ser negativo");

            _dbContext.Procedimientos.Add(procedimiento);
            await _dbContext.SaveChangesAsync();
            return Ok(procedimiento);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProcedimiento(Procedimiento procedimiento)
        {
            var procedimientoExistente = await _dbContext.Procedimientos
                .FindAsync(procedimiento.Id);
            if (procedimientoExistente == null) return NotFound("Procedimiento no existe");

            var cita = await _dbContext.Citas.FindAsync(procedimiento.CitaId);
            if (cita == null) return BadRequest("La cita no existe");

            if (procedimiento.Costo < 0)
                return BadRequest("El costo no puede ser negativo");

            procedimientoExistente.Descripcion = procedimiento.Descripcion;
            procedimientoExistente.Costo = procedimiento.Costo;
            procedimientoExistente.CitaId = procedimiento.CitaId;

            await _dbContext.SaveChangesAsync();
            return Ok(procedimientoExistente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProcedimiento(int id)
        {
            var procedimiento = await _dbContext.Procedimientos.FindAsync(id);
            if (procedimiento == null) return NotFound("Procedimiento no existe");

            _dbContext.Procedimientos.Remove(procedimiento);
            await _dbContext.SaveChangesAsync();
            return Ok(procedimiento);
        }
    }
}
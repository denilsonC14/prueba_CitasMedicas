// CitasController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionCitasMedicas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CitasController : ControllerBase
    {
        private readonly AppDBContext _dbContext;

        public CitasController(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetCitas()
        {
            return Ok(await _dbContext.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Doctor)
                .Include(c => c.Procedimientos)
                .ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCita(int id)
        {
            var cita = await _dbContext.Citas
                .Include(c => c.Paciente)
                .Include(c => c.Doctor)
                .Include(c => c.Procedimientos)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cita == null) return NotFound("La cita no existe");
            return Ok(cita);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCita(Cita cita)
        {
            var paciente = await _dbContext.Pacientes.FindAsync(cita.PacienteId);
            if (paciente == null) return BadRequest("El paciente no existe");

            var doctor = await _dbContext.Doctores.FindAsync(cita.DoctorId);
            if (doctor == null) return BadRequest("El doctor no existe");

            _dbContext.Citas.Add(cita);
            await _dbContext.SaveChangesAsync();
            return Ok(cita);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateCita(Cita cita)
        {
            var citaExistente = await _dbContext.Citas.FindAsync(cita.Id);
            if (citaExistente == null) return NotFound("Cita no existe");

            var paciente = await _dbContext.Pacientes.FindAsync(cita.PacienteId);
            if (paciente == null) return BadRequest("El paciente no existe");

            var doctor = await _dbContext.Doctores.FindAsync(cita.DoctorId);
            if (doctor == null) return BadRequest("El doctor no existe");

            citaExistente.Fecha = cita.Fecha;
            citaExistente.Descripcion = cita.Descripcion;
            citaExistente.PacienteId = cita.PacienteId;
            citaExistente.DoctorId = cita.DoctorId;

            await _dbContext.SaveChangesAsync();
            return Ok(citaExistente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCita(int id)
        {
            var cita = await _dbContext.Citas
                .Include(c => c.Procedimientos)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cita == null) return NotFound("Cita no existe");

            if (cita.Procedimientos.Count > 0)
                return BadRequest("No se puede eliminar la cita porque tiene procedimientos asociados");


            _dbContext.Citas.Remove(cita);
            await _dbContext.SaveChangesAsync();
            return Ok(cita);
        }
    }
}
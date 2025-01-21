// PacientesController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionCitasMedicas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PacientesController : ControllerBase
    {
        private readonly AppDBContext _dbContext;

        public PacientesController(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetPacientes()
        {
            return Ok(await _dbContext.Pacientes.Include(p => p.Citas).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPaciente(int id)
        {
            var paciente = await _dbContext.Pacientes.Include(p => p.Citas)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (paciente == null) return NotFound("El paciente no existe");
            return Ok(paciente);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaciente(Paciente paciente)
        {
            if (paciente.FechaNacimiento > DateTime.Now)
                return BadRequest("La fecha de nacimiento no puede ser mayor a la fecha actual");
            _dbContext.Pacientes.Add(paciente);
            await _dbContext.SaveChangesAsync();
            return Ok(paciente);
        }

        [HttpPut]
        public async Task<IActionResult> UpdatePaciente(Paciente paciente)
        {
            var pacienteExistente = await _dbContext.Pacientes.FindAsync(paciente.Id);
            if (pacienteExistente == null) return NotFound("Paciente no existe");

            if (paciente.FechaNacimiento > DateTime.Now)
                return BadRequest("La fecha de nacimiento no puede ser futura");

            pacienteExistente.Nombre = paciente.Nombre;
            pacienteExistente.Apellido = paciente.Apellido;
            await _dbContext.SaveChangesAsync();
            return Ok(pacienteExistente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePaciente(int id)
        {
            var paciente = await _dbContext.Pacientes.Include(p => p.Citas)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (paciente == null) return NotFound("Paciente no existe");

            //preferencia utlizar count
            if(paciente.Citas.Count > 0)
                return BadRequest("No se puede eliminar el paciente porque tiene citas asociadas");


            _dbContext.Pacientes.Remove(paciente);
            await _dbContext.SaveChangesAsync();
            return Ok(paciente);
        }
    }
}
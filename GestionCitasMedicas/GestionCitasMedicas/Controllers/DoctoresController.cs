// DoctoresController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GestionCitasMedicas.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DoctoresController : ControllerBase
    {
        private readonly AppDBContext _dbContext;

        public DoctoresController(AppDBContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetDoctores()
        {
            return Ok(await _dbContext.Doctores.Include(d => d.Citas).ToListAsync());
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDoctor(int id)
        {
            var doctor = await _dbContext.Doctores.Include(d => d.Citas)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null) return NotFound("El doctor no existe");
            return Ok(doctor);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDoctor(Doctor doctor)
        { 
                // Validar datos duplicados de nombre y especialidad
                var doctorExistente = await _dbContext.Doctores 
                    .FirstOrDefaultAsync(d => d.Nombre == doctor.Nombre && d.Especialidad == doctor.Especialidad);
            if (doctorExistente != null) return BadRequest("Ya existe un doctor con el mismo nombre y especialidad");

        
            _dbContext.Doctores.Add(doctor);
            await _dbContext.SaveChangesAsync();
            return Ok(doctor);
        }

        [HttpPut]
        public async Task<IActionResult> UpdateDoctor(Doctor doctor)
        {
            var doctorExistente = await _dbContext.Doctores.FindAsync(doctor.Id);
            if (doctorExistente == null) return NotFound("Doctor no existe");

            doctorExistente.Nombre = doctor.Nombre;
            doctorExistente.Especialidad = doctor.Especialidad;
            await _dbContext.SaveChangesAsync();
            return Ok(doctorExistente);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _dbContext.Doctores.Include(d => d.Citas)
                .FirstOrDefaultAsync(d => d.Id == id);
            if (doctor == null) return NotFound("Doctor no existe");
            //preferencia utlizar count

            if(doctor.Citas.Count > 0)
                return BadRequest("No se puede eliminar el doctor porque tiene citas asociadas");

            _dbContext.Doctores.Remove(doctor);
            await _dbContext.SaveChangesAsync();
            return Ok(doctor);
        }
    }
}
// AppDBContext.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace GestionCitasMedicas
{
    public class AppDBContext : DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext> options) : base(options)
        {
        }

        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Doctor> Doctores { get; set; }
        public DbSet<Cita> Citas { get; set; }
        public DbSet<Procedimiento> Procedimientos { get; set; }
    }

    public class Paciente
    {
        public  int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Apellido { get; set; }

        public DateTime? FechaNacimiento { get; set; }

        [JsonIgnore]
        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }

    public class Doctor
    {
        public  int Id { get; set; }
        public string? Nombre { get; set; }
        public string? Especialidad { get; set; }

        [JsonIgnore]
        public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    }

    public class Cita
    {
        public  int Id { get; set; }

       
        public DateTime? Fecha { get; set; }

        public string? Descripcion { get; set; }

        
        public int? PacienteId { get; set; }

        
        public int? DoctorId { get; set; }

        [JsonIgnore]
        public Paciente? Paciente { get; set; }

        [JsonIgnore]
        public Doctor? Doctor { get; set; }

        public ICollection<Procedimiento> Procedimientos { get; set; } = new List<Procedimiento>();
    }

    public class Procedimiento
    {
        public  int Id { get; set; }
        public string? Descripcion { get; set; }

       
        public decimal? Costo { get; set; }

        
        public int? CitaId { get; set; }

        [JsonIgnore]
        public Cita? Cita { get; set; }
    }

}
using GestionCitasMedicas;
using GestionCitasMedicas.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TestProjectGestionCitasMedicas
{
    public class CitasControllerTest
    {
        private DbContextOptions<AppDBContext> _dbContextOptions;

        public CitasControllerTest()
        {
            _dbContextOptions = new DbContextOptionsBuilder<AppDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabaseCitas")
                .Options;
        }

        private async Task LimpiarBaseDeDatos()
        {
            using (var context = new AppDBContext(_dbContextOptions))
            {
                context.Citas.RemoveRange(context.Citas);
                context.Pacientes.RemoveRange(context.Pacientes);
                context.Doctores.RemoveRange(context.Doctores);
                context.Procedimientos.RemoveRange(context.Procedimientos);
                await context.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task GetCitas_ReturnsEmptyList_WhenNoAppointmentsExist()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var controller = new CitasController(context);
                var result = await controller.GetCitas();

                var okResult = Assert.IsType<OkObjectResult>(result);
                var citas = Assert.IsType<List<Cita>>(okResult.Value);
                Assert.Empty(citas);
            }
        }

        [Fact]
        public async Task GetCitas_ReturnsAppointmentList_WhenAppointmentsExist()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var paciente = new Paciente { Nombre = "Juan", Apellido = "Perez", FechaNacimiento = new DateTime(1980, 1, 1) };
                var doctor = new Doctor { Nombre = "Dr. Smith", Especialidad = "Cardiología" };
                context.Pacientes.Add(paciente);
                context.Doctores.Add(doctor);
                await context.SaveChangesAsync();

                context.Citas.Add(new Cita
                {
                    PacienteId = paciente.Id,
                    DoctorId = doctor.Id,
                    Fecha = DateTime.Now,
                    Descripcion = "Consulta"
                });
                await context.SaveChangesAsync();

                var controller = new CitasController(context);
                var result = await controller.GetCitas();

                var okResult = Assert.IsType<OkObjectResult>(result);
                var citas = Assert.IsType<List<Cita>>(okResult.Value);
                Assert.Single(citas);
            }
        }

        [Fact]
        public async Task GetCita_ReturnsNotFound_WhenAppointmentDoesNotExist()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var controller = new CitasController(context);
                var result = await controller.GetCita(1);

                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal("La cita no existe", notFoundResult.Value);
            }
        }

        [Fact]
        public async Task CreateCita_ReturnsBadRequest_WhenPatientDoesNotExist()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var doctor = new Doctor { Nombre = "Dr. Smith", Especialidad = "Cardiología" };
                context.Doctores.Add(doctor);
                await context.SaveChangesAsync();

                var controller = new CitasController(context);
                var cita = new Cita
                {
                    PacienteId = 999,
                    DoctorId = doctor.Id,
                    Fecha = DateTime.Now
                };

                var result = await controller.CreateCita(cita);

                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("El paciente no existe", badRequestResult.Value);
            }
        }

        [Fact]
        public async Task CreateCita_ReturnsBadRequest_WhenDoctorDoesNotExist()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var paciente = new Paciente { Nombre = "Juan", Apellido = "Perez", FechaNacimiento = new DateTime(1980, 1, 1) };
                context.Pacientes.Add(paciente);
                await context.SaveChangesAsync();

                var controller = new CitasController(context);
                var cita = new Cita
                {
                    PacienteId = paciente.Id,
                    DoctorId = 999,
                    Fecha = DateTime.Now
                };

                var result = await controller.CreateCita(cita);

                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("El doctor no existe", badRequestResult.Value);
            }
        }

        [Fact]
        public async Task DeleteCita_ReturnsBadRequest_WhenAppointmentHasProcedures()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var paciente = new Paciente { Nombre = "Juan", Apellido = "Perez", FechaNacimiento = new DateTime(1980, 1, 1) };
                var doctor = new Doctor { Nombre = "Dr. Smith", Especialidad = "Cardiología" };
                context.Pacientes.Add(paciente);
                context.Doctores.Add(doctor);
                await context.SaveChangesAsync();

                var cita = new Cita
                {
                    PacienteId = paciente.Id,
                    DoctorId = doctor.Id,
                    Fecha = DateTime.Now
                };
                context.Citas.Add(cita);
                await context.SaveChangesAsync();

                context.Procedimientos.Add(new Procedimiento
                {
                    CitaId = cita.Id,
                    Descripcion = "Procedimiento test",
                    Costo = 100
                });
                await context.SaveChangesAsync();

                var controller = new CitasController(context);
                var result = await controller.DeleteCita(cita.Id);

                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("No se puede eliminar la cita porque tiene procedimientos asociados", badRequestResult.Value);
            }
        }
    }
}
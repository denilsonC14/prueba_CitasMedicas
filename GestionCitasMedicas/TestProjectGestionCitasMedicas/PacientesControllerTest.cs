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
    public class PacientesControllerTest
    {
        private DbContextOptions<AppDBContext> _dbContextOptions;

        public PacientesControllerTest()
        {
            _dbContextOptions = new DbContextOptionsBuilder<AppDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
        }

        private async Task LimpiarBaseDeDatos()
        {
            using (var context = new AppDBContext(_dbContextOptions))
            {
                context.Pacientes.RemoveRange(context.Pacientes);
                context.Citas.RemoveRange(context.Citas);
                await context.SaveChangesAsync();
            }
        }

        // Test GetPacientes
        [Fact]
        public async Task GetPacientes_ReturnsEmptyList_WhenNoPatientsExist()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var controller = new PacientesController(context);
                var result = await controller.GetPacientes();

                var okResult = Assert.IsType<OkObjectResult>(result);
                var pacientes = Assert.IsType<List<Paciente>>(okResult.Value);
                Assert.Empty(pacientes);
            }
        }

        [Fact]
        public async Task GetPacientes_ReturnsPatientList_WhenPatientsExist()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                context.Pacientes.Add(new Paciente { Nombre = "Juan", Apellido = "Perez", FechaNacimiento = new DateTime(1980, 1, 1) });
                await context.SaveChangesAsync();

                var controller = new PacientesController(context);
                var result = await controller.GetPacientes();

                var okResult = Assert.IsType<OkObjectResult>(result);
                var pacientes = Assert.IsType<List<Paciente>>(okResult.Value);
                Assert.Single(pacientes);
            }
        }

        [Fact]
        public async Task GetPacientes_IncludesCitas_WhenPatientsHaveAppointments()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var paciente = new Paciente { Nombre = "Juan", Apellido = "Perez", FechaNacimiento = new DateTime(1980, 1, 1) };
                context.Pacientes.Add(paciente);
                await context.SaveChangesAsync();

                context.Citas.Add(new Cita { PacienteId = paciente.Id, Fecha = DateTime.Now });
                await context.SaveChangesAsync();

                var controller = new PacientesController(context);
                var result = await controller.GetPacientes();

                var okResult = Assert.IsType<OkObjectResult>(result);
                var pacientes = Assert.IsType<List<Paciente>>(okResult.Value);
                Assert.Single(pacientes);
                Assert.Single(pacientes[0].Citas);
            }
        }

        // Test GetPaciente
        [Fact]
        public async Task GetPaciente_ReturnsNotFound_WhenPatientDoesNotExist()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var controller = new PacientesController(context);
                var result = await controller.GetPaciente(1);

                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal("El paciente no existe", notFoundResult.Value);
            }
        }

        [Fact]
        public async Task GetPaciente_ReturnsPatient_WhenPatientExists()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var paciente = new Paciente { Nombre = "Juan", Apellido = "Perez", FechaNacimiento = new DateTime(1980, 1, 1) };
                context.Pacientes.Add(paciente);
                await context.SaveChangesAsync();

                var controller = new PacientesController(context);
                var result = await controller.GetPaciente(paciente.Id);

                var okResult = Assert.IsType<OkObjectResult>(result);
                var pacienteReturned = Assert.IsType<Paciente>(okResult.Value);
                Assert.Equal(paciente.Id, pacienteReturned.Id);
            }
        }

        // Test CreatePaciente
        [Fact]
        public async Task CreatePaciente_ReturnsBadRequest_WhenBirthdateIsInFuture()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var controller = new PacientesController(context);
                var paciente = new Paciente { Nombre = "Juan", Apellido = "Perez", FechaNacimiento = DateTime.Now.AddDays(1) };

                var result = await controller.CreatePaciente(paciente);

                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("La fecha de nacimiento no puede ser mayor a la fecha actual", badRequestResult.Value);
            }
        }

        [Fact]
        public async Task CreatePaciente_ReturnsBadRequest_WhenNameIsEmpty()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var controller = new PacientesController(context);
                var paciente = new Paciente { Nombre = "", Apellido = "Perez", FechaNacimiento = new DateTime(1980, 1, 1) };

                var result = await controller.CreatePaciente(paciente);

                Assert.IsType<BadRequestObjectResult>(result);
            }
        }

        [Fact]
        public async Task CreatePaciente_ReturnsOk_WhenPatientIsValid()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var controller = new PacientesController(context);
                var paciente = new Paciente { Nombre = "Juan", Apellido = "Perez", FechaNacimiento = new DateTime(1980, 1, 1) };

                var result = await controller.CreatePaciente(paciente);

                var okResult = Assert.IsType<OkObjectResult>(result);
                var pacienteReturned = Assert.IsType<Paciente>(okResult.Value);
                Assert.Equal(paciente.Nombre, pacienteReturned.Nombre);
            }
        }

        // Test UpdatePaciente
        [Fact]
        public async Task UpdatePaciente_ReturnsNotFound_WhenPatientDoesNotExist()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var controller = new PacientesController(context);
                var paciente = new Paciente { Id = 1, Nombre = "Juan", Apellido = "Perez", FechaNacimiento = new DateTime(1980, 1, 1) };

                var result = await controller.UpdatePaciente(paciente);

                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal("Paciente no existe", notFoundResult.Value);
            }
        }

        [Fact]
        public async Task UpdatePaciente_ReturnsBadRequest_WhenBirthdateIsInFuture()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var paciente = new Paciente { Nombre = "Juan", Apellido = "Perez", FechaNacimiento = new DateTime(1980, 1, 1) };
                context.Pacientes.Add(paciente);
                await context.SaveChangesAsync();

                var controller = new PacientesController(context);
                paciente.FechaNacimiento = DateTime.Now.AddDays(1);

                var result = await controller.UpdatePaciente(paciente);

                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("La fecha de nacimiento no puede ser futura", badRequestResult.Value);
            }
        }

        [Fact]
        public async Task UpdatePaciente_ReturnsBadRequest_WhenLastNameIsEmpty()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var paciente = new Paciente { Nombre = "Juan", Apellido = "Perez", FechaNacimiento = new DateTime(1980, 1, 1) };
                context.Pacientes.Add(paciente);
                await context.SaveChangesAsync();

                var controller = new PacientesController(context);
                paciente.Apellido = "";

                var result = await controller.UpdatePaciente(paciente);

                Assert.IsType<BadRequestObjectResult>(result);
            }
        }

        [Fact]
        public async Task UpdatePaciente_ReturnsOk_WhenPatientIsUpdated()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var paciente = new Paciente { Nombre = "Juan", Apellido = "Perez", FechaNacimiento = new DateTime(1980, 1, 1) };
                context.Pacientes.Add(paciente);
                await context.SaveChangesAsync();

                var controller = new PacientesController(context);
                paciente.Nombre = "Carlos";

                var result = await controller.UpdatePaciente(paciente);

                var okResult = Assert.IsType<OkObjectResult>(result);
                var pacienteReturned = Assert.IsType<Paciente>(okResult.Value);
                Assert.Equal("Carlos", pacienteReturned.Nombre);
            }
        }

        // Test DeletePaciente
        [Fact]
        public async Task DeletePaciente_ReturnsNotFound_WhenPatientDoesNotExist()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var controller = new PacientesController(context);
                var result = await controller.DeletePaciente(1);

                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal("Paciente no existe", notFoundResult.Value);
            }
        }

        [Fact]
        public async Task DeletePaciente_ReturnsBadRequest_WhenPatientHasCitas()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var paciente = new Paciente { Nombre = "Juan", Apellido = "Perez", FechaNacimiento = new DateTime(1980, 1, 1) };
                context.Pacientes.Add(paciente);
                await context.SaveChangesAsync();

                context.Citas.Add(new Cita { PacienteId = paciente.Id });
                await context.SaveChangesAsync();

                var controller = new PacientesController(context);
                var result = await controller.DeletePaciente(paciente.Id);

                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("No se puede eliminar el paciente porque tiene citas asociadas", badRequestResult.Value);
            }
        }

        [Fact]
        public async Task DeletePaciente_ReturnsOk_WhenPatientIsDeleted()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var paciente = new Paciente { Nombre = "Juan", Apellido = "Perez", FechaNacimiento = new DateTime(1980, 1, 1) };
                context.Pacientes.Add(paciente);
                await context.SaveChangesAsync();

                var controller = new PacientesController(context);
                var result = await controller.DeletePaciente(paciente.Id);

                var okResult = Assert.IsType<OkObjectResult>(result);
                var pacienteReturned = Assert.IsType<Paciente>(okResult.Value);
                Assert.Equal(paciente.Id, pacienteReturned.Id);
            }
        }
    }
}
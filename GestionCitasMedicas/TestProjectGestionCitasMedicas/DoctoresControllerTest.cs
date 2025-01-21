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
    public class DoctoresControllerTest
    {
        private DbContextOptions<AppDBContext> _dbContextOptions;

        public DoctoresControllerTest()
        {
            _dbContextOptions = new DbContextOptionsBuilder<AppDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabaseDoctores")
                .Options;
        }

        private async Task LimpiarBaseDeDatos()
        {
            using (var context = new AppDBContext(_dbContextOptions))
            {
                context.Doctores.RemoveRange(context.Doctores);
                context.Citas.RemoveRange(context.Citas);
                await context.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task GetDoctores_ReturnsEmptyList_WhenNoDoctorsExist()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var controller = new DoctoresController(context);
                var result = await controller.GetDoctores();

                var okResult = Assert.IsType<OkObjectResult>(result);
                var doctores = Assert.IsType<List<Doctor>>(okResult.Value);
                Assert.Empty(doctores);
            }
        }

        [Fact]
        public async Task CreateDoctor_ReturnsBadRequest_WhenDoctorAlreadyExists()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var doctor = new Doctor { Nombre = "Dr. Smith", Especialidad = "Cardiología" };
                context.Doctores.Add(doctor);
                await context.SaveChangesAsync();

                var controller = new DoctoresController(context);
                var newDoctor = new Doctor { Nombre = "Dr. Smith", Especialidad = "Cardiología" };

                var result = await controller.CreateDoctor(newDoctor);

                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("Ya existe un doctor con el mismo nombre y especialidad", badRequestResult.Value);
            }
        }

        [Fact]
        public async Task UpdateDoctor_ReturnsNotFound_WhenDoctorDoesNotExist()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var controller = new DoctoresController(context);
                var doctor = new Doctor { Id = 1, Nombre = "Dr. Smith", Especialidad = "Cardiología" };

                var result = await controller.UpdateDoctor(doctor);

                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal("Doctor no existe", notFoundResult.Value);
            }
        }

        [Fact]
        public async Task DeleteDoctor_ReturnsBadRequest_WhenDoctorHasAppointments()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var doctor = new Doctor { Nombre = "Dr. Smith", Especialidad = "Cardiología" };
                context.Doctores.Add(doctor);
                await context.SaveChangesAsync();

                context.Citas.Add(new Cita { DoctorId = doctor.Id });
                await context.SaveChangesAsync();

                var controller = new DoctoresController(context);
                var result = await controller.DeleteDoctor(doctor.Id);

                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("No se puede eliminar el doctor porque tiene citas asociadas", badRequestResult.Value);
            }
        }
    }
}
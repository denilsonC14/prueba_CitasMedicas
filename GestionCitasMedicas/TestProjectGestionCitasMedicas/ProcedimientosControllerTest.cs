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
    public class ProcedimientosControllerTest
    {
        private DbContextOptions<AppDBContext> _dbContextOptions;

        public ProcedimientosControllerTest()
        {
            _dbContextOptions = new DbContextOptionsBuilder<AppDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabaseProcedimientos")
                .Options;
        }

        private async Task LimpiarBaseDeDatos()
        {
            using (var context = new AppDBContext(_dbContextOptions))
            {
                context.Procedimientos.RemoveRange(context.Procedimientos);
                context.Citas.RemoveRange(context.Citas);
                await context.SaveChangesAsync();
            }
        }

        [Fact]
        public async Task GetProcedimientos_ReturnsEmptyList_WhenNoProceduresExist()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var controller = new ProcedimientosController(context);
                var result = await controller.GetProcedimientos();

                var okResult = Assert.IsType<OkObjectResult>(result);
                var procedimientos = Assert.IsType<List<Procedimiento>>(okResult.Value);
                Assert.Empty(procedimientos);
            }
        }

        [Fact]
        public async Task GetProcedimientos_ReturnsProceduresList_WhenProceduresExist()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var cita = new Cita { Id = 1 };
                context.Citas.Add(cita);
                await context.SaveChangesAsync();

                var procedimiento = new Procedimiento
                {
                    Id=1,
                    CitaId = cita.Id,
                    Descripcion = "Procedimiento test",
                    Costo = 100
                };
                context.Procedimientos.Add(procedimiento);
                await context.SaveChangesAsync();

                var controller = new ProcedimientosController(context);
                var result = await controller.GetProcedimientos();

                var okResult = Assert.IsType<OkObjectResult>(result);
                var procedimientos = Assert.IsType<List<Procedimiento>>(okResult.Value);
                Assert.Single(procedimientos);
                Assert.Equal("Procedimiento test", procedimientos[0].Descripcion);
            }
        }

        [Fact]
        public async Task GetProcedimiento_ReturnsNotFound_WhenProcedureDoesNotExist()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var controller = new ProcedimientosController(context);
                var result = await controller.GetProcedimiento(1);

                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal("El procedimiento no existe", notFoundResult.Value);
            }
        }

        [Fact]
        public async Task GetProcedimiento_ReturnsOk_WhenProcedureExists()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var cita = new Cita { Id = 1 };
                context.Citas.Add(cita);
                var procedimiento = new Procedimiento
                {
                    Id = 1,
                    CitaId = cita.Id,
                    Descripcion = "Procedimiento test",
                    Costo = 100
                };
                context.Procedimientos.Add(procedimiento);
                await context.SaveChangesAsync();

                var controller = new ProcedimientosController(context);
                var result = await controller.GetProcedimiento(procedimiento.Id);

                var okResult = Assert.IsType<OkObjectResult>(result);
                var procedimientoRetornado = Assert.IsType<Procedimiento>(okResult.Value);
                Assert.Equal(procedimiento.Id, procedimientoRetornado.Id);
                Assert.Equal("Procedimiento test", procedimientoRetornado.Descripcion);
            }
        }

        [Fact]
        public async Task CreateProcedimiento_ReturnsBadRequest_WhenAppointmentDoesNotExist()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var controller = new ProcedimientosController(context);
                var procedimiento = new Procedimiento
                {
                    Id = 1,
                    CitaId = 999,
                    Descripcion = "Procedimiento test",
                    Costo = 100
                };

                var result = await controller.CreateProcedimiento(procedimiento);

                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("La cita no existe", badRequestResult.Value);
            }
        }

        [Fact]
        public async Task CreateProcedimiento_ReturnsBadRequest_WhenCostIsNegative()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var cita = new Cita { Id = 1 };
                context.Citas.Add(cita);
                await context.SaveChangesAsync();

                var controller = new ProcedimientosController(context);
                var procedimiento = new Procedimiento
                {
                    Id = 1,
                    CitaId = cita.Id,
                    Descripcion = "Procedimiento test",
                    Costo = -100
                };

                var result = await controller.CreateProcedimiento(procedimiento);

                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("El costo no puede ser negativo", badRequestResult.Value);
            }
        }

        [Fact]
        public async Task CreateProcedimiento_ReturnsOk_WhenProcedureIsValid()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var cita = new Cita { Id = 1 };
                context.Citas.Add(cita);
                await context.SaveChangesAsync();

                var controller = new ProcedimientosController(context);
                var procedimiento = new Procedimiento
                {
                    Id = 1,
                    CitaId = cita.Id,
                    Descripcion = "Procedimiento test",
                    Costo = 100
                };

                var result = await controller.CreateProcedimiento(procedimiento);

                var okResult = Assert.IsType<OkObjectResult>(result);
                var procedimientoCreado = Assert.IsType<Procedimiento>(okResult.Value);
                Assert.Equal("Procedimiento test", procedimientoCreado.Descripcion);
                Assert.Equal(100, procedimientoCreado.Costo);
            }
        }

        [Fact]
        public async Task UpdateProcedimiento_ReturnsNotFound_WhenProcedureDoesNotExist()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var controller = new ProcedimientosController(context);
                var procedimiento = new Procedimiento
                {
                    Id = 1,
                    CitaId = 1,
                    Descripcion = "Procedimiento test",
                    Costo = 100
                };

                var result = await controller.UpdateProcedimiento(procedimiento);

                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal("Procedimiento no existe", notFoundResult.Value);
            }
        }

        [Fact]
        public async Task UpdateProcedimiento_ReturnsBadRequest_WhenAppointmentDoesNotExist()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var cita = new Cita { Id = 1 };
                context.Citas.Add(cita);
                var procedimiento = new Procedimiento
                {
                    Id = 1,
                    CitaId = cita.Id,
                    Descripcion = "Procedimiento test",
                    Costo = 100
                };
                context.Procedimientos.Add(procedimiento);
                await context.SaveChangesAsync();

                var controller = new ProcedimientosController(context);
                procedimiento.CitaId = 999;

                var result = await controller.UpdateProcedimiento(procedimiento);

                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("La cita no existe", badRequestResult.Value);
            }
        }

        [Fact]
        public async Task UpdateProcedimiento_ReturnsBadRequest_WhenCostIsNegative()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var cita = new Cita { Id = 1 };
                context.Citas.Add(cita);
                var procedimiento = new Procedimiento
                {
                    Id = 1,
                    CitaId = cita.Id,
                    Descripcion = "Procedimiento test",
                    Costo = 100
                };
                context.Procedimientos.Add(procedimiento);
                await context.SaveChangesAsync();

                var controller = new ProcedimientosController(context);
                procedimiento.Costo = -100;

                var result = await controller.UpdateProcedimiento(procedimiento);

                var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
                Assert.Equal("El costo no puede ser negativo", badRequestResult.Value);
            }
        }

        [Fact]
        public async Task UpdateProcedimiento_ReturnsOk_WhenProcedureIsUpdated()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var cita = new Cita { Id = 1 };
                context.Citas.Add(cita);
                var procedimiento = new Procedimiento
                {
                    Id = 1,
                    CitaId = cita.Id,
                    Descripcion = "Procedimiento test",
                    Costo = 100
                };
                context.Procedimientos.Add(procedimiento);
                await context.SaveChangesAsync();

                var controller = new ProcedimientosController(context);
                procedimiento.Descripcion = "Procedimiento actualizado";
                procedimiento.Costo = 200;

                var result = await controller.UpdateProcedimiento(procedimiento);

                var okResult = Assert.IsType<OkObjectResult>(result);
                var procedimientoActualizado = Assert.IsType<Procedimiento>(okResult.Value);
                Assert.Equal("Procedimiento actualizado", procedimientoActualizado.Descripcion);
                Assert.Equal(200, procedimientoActualizado.Costo);
            }
        }

        [Fact]
        public async Task DeleteProcedimiento_ReturnsNotFound_WhenProcedureDoesNotExist()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var controller = new ProcedimientosController(context);
                var result = await controller.DeleteProcedimiento(1);

                var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
                Assert.Equal("Procedimiento no existe", notFoundResult.Value);
            }
        }

        [Fact]
        public async Task DeleteProcedimiento_ReturnsOk_WhenProcedureIsDeleted()
        {
            await LimpiarBaseDeDatos();
            using (var context = new AppDBContext(_dbContextOptions))
            {
                var cita = new Cita { Id = 1 };
                context.Citas.Add(cita);
                var procedimiento = new Procedimiento
                {
                    Id = 1,
                    CitaId = cita.Id,
                    Descripcion = "Procedimiento test",
                    Costo = 100
                };
                context.Procedimientos.Add(procedimiento);
                await context.SaveChangesAsync();

                var controller = new ProcedimientosController(context);
                var result = await controller.DeleteProcedimiento(procedimiento.Id);

                var okResult = Assert.IsType<OkObjectResult>(result);
                var procedimientoEliminado = Assert.IsType<Procedimiento>(okResult.Value);
                Assert.Equal(procedimiento.Id, procedimientoEliminado.Id);
            }
        }
    }
}

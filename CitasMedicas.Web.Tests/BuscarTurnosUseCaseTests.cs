using CitasMedicas.Web.Modules.Agenda.BuscarTurnos;
using CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure;
using CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades;
using CitasMedicas.Web.Controllers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace CitasMedicas.Web.Tests;

public sealed class BuscarTurnosUseCaseTests
{
    [Fact]
    public async Task EjecutarAsync_FiltraPorEspecialidadFechaFranjaYDisponibilidad()
    {
        await using var dbContext = CrearContexto();
        dbContext.TurnosDisponibles.AddRange(
            Turno(1, new DateOnly(2030, 4, 10), new TimeOnly(9, 0), disponible: true),
            Turno(1, new DateOnly(2030, 4, 10), new TimeOnly(10, 0), disponible: false),
            Turno(2, new DateOnly(2030, 4, 10), new TimeOnly(9, 30), disponible: true),
            Turno(1, new DateOnly(2030, 4, 11), new TimeOnly(9, 30), disponible: true),
            Turno(1, new DateOnly(2030, 4, 10), new TimeOnly(12, 0), disponible: true));
        await dbContext.SaveChangesAsync();

        var resultados = await new BuscarTurnosUseCase(dbContext).EjecutarAsync(
            1, new DateOnly(2030, 4, 10), FranjaHoraria.Manana);

        var turno = Assert.Single(resultados);
        Assert.Equal(new TimeOnly(9, 0), turno.HoraInicio);
    }

    [Fact]
    public async Task EjecutarAsync_DevuelveTurnosDeLaTardeEnOrden()
    {
        await using var dbContext = CrearContexto();
        dbContext.TurnosDisponibles.AddRange(
            Turno(1, new DateOnly(2030, 4, 10), new TimeOnly(17, 0), disponible: true),
            Turno(1, new DateOnly(2030, 4, 10), new TimeOnly(12, 0), disponible: true),
            Turno(1, new DateOnly(2030, 4, 10), new TimeOnly(18, 0), disponible: true));
        await dbContext.SaveChangesAsync();

        var resultados = await new BuscarTurnosUseCase(dbContext).EjecutarAsync(
            1, new DateOnly(2030, 4, 10), FranjaHoraria.Tarde);

        Assert.Equal(2, resultados.Count);
        Assert.Equal(new TimeOnly(12, 0), resultados[0].HoraInicio);
        Assert.Equal(new TimeOnly(17, 0), resultados[1].HoraInicio);
    }

    [Fact]
    public async Task BuscarTurnos_SinCoincidencias_ConservaLosFiltrosYDevuelveListaVacia()
    {
        await using var dbContext = CrearContexto();
        dbContext.Especialidades.Add(new Especialidad
        {
            Id = 1,
            Nombre = "Cardiología",
            Descripcion = "Atención cardiológica"
        });
        await dbContext.SaveChangesAsync();

        var controller = new EspecialidadesController(
            new CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades.ListarEspecialidadesUseCase(dbContext),
            new CitasMedicas.Web.Modules.CatalogoMedico.CrearEspecialidad.CrearEspecialidadUseCase(dbContext),
            new BuscarTurnosUseCase(dbContext))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
        controller.Request.QueryString = QueryString.Create(new Dictionary<string, string?>
        {
            ["fecha"] = "2030-04-10",
            ["franja"] = "Manana"
        });

        var result = await controller.BuscarTurnos(1, new DateOnly(2030, 4, 10), FranjaHoraria.Manana, default);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<BuscarTurnosViewModel>(view.Model);
        Assert.True(model.BusquedaRealizada);
        Assert.Equal(new DateOnly(2030, 4, 10), model.Fecha);
        Assert.Equal(FranjaHoraria.Manana, model.Franja);
        Assert.Empty(model.Turnos);
    }

    private static ClinicaDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<ClinicaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ClinicaDbContext(options);
    }

    private static TurnoDisponible Turno(int especialidadId, DateOnly fecha, TimeOnly hora, bool disponible) => new()
    {
        EspecialidadId = especialidadId,
        ProfesionalNombre = "Profesional de prueba",
        Fecha = fecha,
        HoraInicio = hora,
        HoraFin = hora.AddMinutes(30),
        Disponible = disponible
    };
}

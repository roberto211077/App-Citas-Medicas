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

    [Fact]
    public async Task SeleccionarTurno_Disponible_MuestraLaConfirmacionConSusDatos()
    {
        await using var dbContext = CrearContexto();
        dbContext.Especialidades.Add(new Especialidad
        {
            Id = 1,
            Nombre = "Cardiología",
            Descripcion = "Atención cardiológica"
        });
        dbContext.TurnosDisponibles.Add(new TurnoDisponible
        {
            Id = 42,
            EspecialidadId = 1,
            ProfesionalNombre = "Ana Pérez",
            Fecha = new DateOnly(2030, 4, 10),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(9, 30),
            Disponible = true
        });
        await dbContext.SaveChangesAsync();

        var controller = CrearController(dbContext);

        var result = await controller.SeleccionarTurno(42, default);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("ConfirmarTurno", view.ViewName);
        var turno = Assert.IsType<TurnoDisponible>(view.Model);
        Assert.Equal("Cardiología", turno.Especialidad.Nombre);
        Assert.Equal("Ana Pérez", turno.ProfesionalNombre);
        Assert.Equal(new DateOnly(2030, 4, 10), turno.Fecha);
        Assert.Equal(new TimeOnly(9, 0), turno.HoraInicio);
        Assert.Equal(new TimeOnly(9, 30), turno.HoraFin);
    }

    [Fact]
    public async Task SeleccionarTurno_InexistenteONoDisponible_DevuelveNotFound()
    {
        await using var dbContext = CrearContexto();
        dbContext.Especialidades.Add(new Especialidad
        {
            Id = 1,
            Nombre = "Cardiología",
            Descripcion = "Atención cardiológica"
        });
        var turnoNoDisponible = Turno(1, new DateOnly(2030, 4, 10), new TimeOnly(9, 0), disponible: false);
        turnoNoDisponible.Id = 1;
        dbContext.TurnosDisponibles.Add(turnoNoDisponible);
        await dbContext.SaveChangesAsync();

        var controller = CrearController(dbContext);

        Assert.IsType<NotFoundResult>(await controller.SeleccionarTurno(999, default));
        Assert.IsType<NotFoundResult>(await controller.SeleccionarTurno(1, default));
    }

    private static ClinicaDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<ClinicaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ClinicaDbContext(options);
    }

    private static EspecialidadesController CrearController(ClinicaDbContext dbContext)
    {
        return new EspecialidadesController(
            new CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades.ListarEspecialidadesUseCase(dbContext),
            new CitasMedicas.Web.Modules.CatalogoMedico.CrearEspecialidad.CrearEspecialidadUseCase(dbContext),
            new BuscarTurnosUseCase(dbContext));
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

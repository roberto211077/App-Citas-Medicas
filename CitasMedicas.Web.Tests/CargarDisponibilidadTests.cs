using CitasMedicas.Web.Controllers;
using CitasMedicas.Web.Modules.Agenda.BuscarTurnos;
using CitasMedicas.Web.Modules.Agenda.CargarDisponibilidad;
using CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure;
using CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades;
using CitasMedicas.Web.Modules.CatalogoMedico.ListarMedicos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CitasMedicas.Web.Tests;

public sealed class CargarDisponibilidadTests
{
    [Fact]
    public async Task EjecutarAsync_RegistraTurnoDisponibleYLoIncluyeEnLaBusqueda()
    {
        await using var dbContext = CrearContexto();
        dbContext.Especialidades.Add(new Especialidad
        {
            Id = 1,
            Nombre = "Cardiología",
            Descripcion = "Atención cardiológica"
        });
        dbContext.Medicos.Add(new Medico
        {
            Id = 7,
            Nombre = "Ana Pérez",
            EspecialidadId = 1
        });
        await dbContext.SaveChangesAsync();

        var resultado = await new CargarDisponibilidadUseCase(dbContext).EjecutarAsync(new CargarDisponibilidadRequest
        {
            MedicoId = 7,
            Fecha = new DateOnly(2030, 4, 10),
            HoraInicio = new TimeOnly(9, 0)
        });

        Assert.Equal(CargarDisponibilidadResultado.Registrado, resultado);
        var turno = await dbContext.TurnosDisponibles.SingleAsync();
        Assert.Equal(7, turno.MedicoId);
        Assert.Equal(1, turno.EspecialidadId);
        Assert.Equal("Ana Pérez", turno.ProfesionalNombre);
        Assert.Equal(new DateOnly(2030, 4, 10), turno.Fecha);
        Assert.Equal(new TimeOnly(9, 0), turno.HoraInicio);
        Assert.Equal(new TimeOnly(9, 30), turno.HoraFin);
        Assert.True(turno.Disponible);

        var encontrados = await new BuscarTurnosUseCase(dbContext).EjecutarAsync(
            1, new DateOnly(2030, 4, 10), FranjaHoraria.Manana);
        Assert.Equal(turno.Id, Assert.Single(encontrados).Id);
    }

    [Fact]
    public async Task EjecutarAsync_MedicoInexistente_NoRegistraTurno()
    {
        await using var dbContext = CrearContexto();
        var resultado = await new CargarDisponibilidadUseCase(dbContext).EjecutarAsync(new CargarDisponibilidadRequest
        {
            MedicoId = 99,
            Fecha = new DateOnly(2030, 4, 10),
            HoraInicio = new TimeOnly(9, 0)
        });

        Assert.Equal(CargarDisponibilidadResultado.MedicoNoEncontrado, resultado);
        Assert.Empty(await dbContext.TurnosDisponibles.ToListAsync());
    }

    [Fact]
    public async Task EjecutarAsync_HoraQueExtiendeElTurnoAlDiaSiguiente_NoRegistraTurno()
    {
        await using var dbContext = CrearContexto();
        var resultado = await new CargarDisponibilidadUseCase(dbContext).EjecutarAsync(new CargarDisponibilidadRequest
        {
            MedicoId = 7,
            Fecha = new DateOnly(2030, 4, 10),
            HoraInicio = new TimeOnly(23, 45)
        });

        Assert.Equal(CargarDisponibilidadResultado.HorarioInvalido, resultado);
        Assert.Empty(await dbContext.TurnosDisponibles.ToListAsync());
    }

    [Theory]
    [InlineData(7, 30)]
    [InlineData(14, 0)]
    [InlineData(14, 45)]
    [InlineData(21, 45)]
    [InlineData(22, 0)]
    public async Task EjecutarAsync_HorarioFueraDeLasFranjasPermitidas_NoRegistraTurno(int hora, int minuto)
    {
        await using var dbContext = CrearContexto();
        await AgregarMedicoYTurnoExistenteAsync(dbContext);

        var resultado = await new CargarDisponibilidadUseCase(dbContext).EjecutarAsync(new CargarDisponibilidadRequest
        {
            MedicoId = 7,
            Fecha = new DateOnly(2030, 4, 10),
            HoraInicio = new TimeOnly(hora, minuto)
        });

        Assert.Equal(CargarDisponibilidadResultado.HorarioFueraDeFranja, resultado);
        Assert.Single(await dbContext.TurnosDisponibles.ToListAsync());
    }

    [Theory]
    [InlineData(8, 0)]
    [InlineData(13, 30)]
    [InlineData(15, 0)]
    [InlineData(21, 30)]
    public async Task EjecutarAsync_TurnoCompletoDentroDeUnaFranja_PermiteLaCarga(int hora, int minuto)
    {
        await using var dbContext = CrearContexto();
        await AgregarMedicoYTurnoExistenteAsync(dbContext);

        var resultado = await new CargarDisponibilidadUseCase(dbContext).EjecutarAsync(new CargarDisponibilidadRequest
        {
            MedicoId = 7,
            Fecha = new DateOnly(2030, 4, 10),
            HoraInicio = new TimeOnly(hora, minuto)
        });

        Assert.Equal(CargarDisponibilidadResultado.Registrado, resultado);
        Assert.Equal(2, await dbContext.TurnosDisponibles.CountAsync());
    }

    [Theory]
    [InlineData(9, 0)]
    [InlineData(9, 45)]
    public async Task EjecutarAsync_TurnoDuplicadoOSinTreintaMinutosLibres_RechazaLaCarga(int hora, int minuto)
    {
        await using var dbContext = CrearContexto();
        await AgregarMedicoYTurnoExistenteAsync(dbContext);

        var resultado = await new CargarDisponibilidadUseCase(dbContext).EjecutarAsync(new CargarDisponibilidadRequest
        {
            MedicoId = 7,
            Fecha = new DateOnly(2030, 4, 10),
            HoraInicio = new TimeOnly(hora, minuto)
        });

        Assert.Equal(CargarDisponibilidadResultado.TurnoEnConflicto, resultado);
        Assert.Single(await dbContext.TurnosDisponibles.ToListAsync());
    }

    [Fact]
    public async Task EjecutarAsync_ConTreintaMinutosLibresEntreTurnos_PermiteLaCarga()
    {
        await using var dbContext = CrearContexto();
        await AgregarMedicoYTurnoExistenteAsync(dbContext);

        var resultado = await new CargarDisponibilidadUseCase(dbContext).EjecutarAsync(new CargarDisponibilidadRequest
        {
            MedicoId = 7,
            Fecha = new DateOnly(2030, 4, 10),
            HoraInicio = new TimeOnly(10, 0)
        });

        Assert.Equal(CargarDisponibilidadResultado.Registrado, resultado);
        Assert.Equal(2, await dbContext.TurnosDisponibles.CountAsync());
    }

    [Fact]
    public async Task CargarDisponibilidad_PostConModelStateInvalido_ConservaFormularioYMedicos()
    {
        await using var dbContext = CrearContexto();
        dbContext.Especialidades.Add(new Especialidad
        {
            Id = 1,
            Nombre = "Cardiología",
            Descripcion = "Atención cardiológica"
        });
        dbContext.Medicos.Add(new Medico { Id = 7, Nombre = "Ana Pérez", EspecialidadId = 1 });
        await dbContext.SaveChangesAsync();

        var controller = new AgendaController(
            new ListarMedicosUseCase(dbContext),
            new CargarDisponibilidadUseCase(dbContext))
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() }
        };
        controller.ModelState.AddModelError(nameof(CargarDisponibilidadRequest.Fecha), "Fecha inválida");
        var request = new CargarDisponibilidadRequest
        {
            MedicoId = 7,
            Fecha = new DateOnly(2030, 4, 10),
            HoraInicio = new TimeOnly(9, 0)
        };

        var result = await controller.CargarDisponibilidad(request, default);

        var view = Assert.IsType<ViewResult>(result);
        Assert.Same(request, view.Model);
        Assert.Single(Assert.IsAssignableFrom<IReadOnlyList<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>>(
            controller.ViewBag.Medicos));
        Assert.Empty(await dbContext.TurnosDisponibles.ToListAsync());
    }

    private static ClinicaDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<ClinicaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ClinicaDbContext(options);
    }

    private static async Task AgregarMedicoYTurnoExistenteAsync(ClinicaDbContext dbContext)
    {
        dbContext.Especialidades.Add(new Especialidad
        {
            Id = 1,
            Nombre = "Cardiología",
            Descripcion = "Atención cardiológica"
        });
        dbContext.Medicos.Add(new Medico
        {
            Id = 7,
            Nombre = "Ana Pérez",
            EspecialidadId = 1
        });
        dbContext.TurnosDisponibles.Add(new TurnoDisponible
        {
            Id = 12,
            MedicoId = 7,
            EspecialidadId = 1,
            ProfesionalNombre = "Ana Pérez",
            Fecha = new DateOnly(2030, 4, 10),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(9, 30),
            Disponible = true
        });
        await dbContext.SaveChangesAsync();
    }
}

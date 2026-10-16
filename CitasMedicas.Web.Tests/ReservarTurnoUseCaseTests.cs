using CitasMedicas.Web.Modules.Agenda.BuscarTurnos;
using CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure;
using CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades;
using CitasMedicas.Web.Modules.Reservas.Application.ReservarTurno;
using CitasMedicas.Web.Modules.Reservas.Domain;
using CitasMedicas.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CitasMedicas.Web.Tests;

public sealed class ReservarTurnoUseCaseTests
{
    [Fact]
    public async Task EjecutarAsync_CreaPacienteReservaYMarcaTurnoNoDisponible()
    {
        await using var dbContext = CrearContexto();
        PrepararTurno(dbContext);

        var resultado = await new ReservarTurnoUseCase(dbContext).EjecutarAsync(Request());

        Assert.True(resultado.Exitoso);
        var paciente = await dbContext.Pacientes.SingleAsync();
        var reserva = await dbContext.Reservas.Include(item => item.Paciente).SingleAsync();
        Assert.Equal("00123456", paciente.Dni);
        Assert.Equal("María", paciente.Nombre);
        Assert.Equal(paciente.Id, reserva.PacienteId);
        Assert.Equal(1, reserva.TurnoDisponibleId);
        Assert.False((await dbContext.TurnosDisponibles.SingleAsync()).Disponible);
    }

    [Fact]
    public async Task EjecutarAsync_ReutilizaLosDatosDelPacienteExistente()
    {
        await using var dbContext = CrearContexto();
        PrepararTurno(dbContext);
        dbContext.Pacientes.Add(new Paciente
        {
            Dni = "00123456",
            Nombre = "Ana",
            Apellido = "López",
            Telefono = "1111"
        });
        await dbContext.SaveChangesAsync();

        var request = Request();
        var resultado = await new ReservarTurnoUseCase(dbContext).EjecutarAsync(request);

        Assert.True(resultado.Exitoso);
        Assert.Equal(1, await dbContext.Pacientes.CountAsync());
        var pacienteReservado = await dbContext.Reservas.Include(item => item.Paciente)
            .Select(item => item.Paciente).SingleAsync();
        Assert.Equal("Ana", pacienteReservado.Nombre);
        Assert.Equal("López", pacienteReservado.Apellido);
        Assert.Equal("1111", pacienteReservado.Telefono);
    }

    [Fact]
    public async Task EjecutarAsync_RechazaTurnoOcupadoSinCrearPaciente()
    {
        await using var dbContext = CrearContexto();
        PrepararTurno(dbContext, disponible: false);

        var resultado = await new ReservarTurnoUseCase(dbContext).EjecutarAsync(Request());

        Assert.False(resultado.Exitoso);
        Assert.Empty(await dbContext.Pacientes.ToListAsync());
        Assert.Empty(await dbContext.Reservas.ToListAsync());
    }

    [Fact]
    public async Task EjecutarAsync_SegundaSolicitudParaElMismoTurnoNoCreaDuplicados()
    {
        await using var dbContext = CrearContexto();
        PrepararTurno(dbContext);
        var useCase = new ReservarTurnoUseCase(dbContext);

        Assert.True((await useCase.EjecutarAsync(Request())).Exitoso);
        var resultadoDuplicado = await useCase.EjecutarAsync(Request());

        Assert.False(resultadoDuplicado.Exitoso);
        Assert.Single(await dbContext.Pacientes.ToListAsync());
        Assert.Single(await dbContext.Reservas.ToListAsync());
    }

    [Fact]
    public async Task SeleccionarTurno_ConDniExistenteCargaDatosGuardados()
    {
        await using var dbContext = CrearContexto();
        PrepararTurno(dbContext);
        dbContext.Pacientes.Add(new Paciente
        {
            Dni = "00123456",
            Nombre = "Ana",
            Apellido = "López",
            Telefono = "1111"
        });
        await dbContext.SaveChangesAsync();

        var controller = new EspecialidadesController(
            new CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades.ListarEspecialidadesUseCase(dbContext),
            new CitasMedicas.Web.Modules.CatalogoMedico.CrearEspecialidad.CrearEspecialidadUseCase(dbContext),
            new BuscarTurnosUseCase(dbContext),
            new ReservarTurnoUseCase(dbContext));

        var result = await controller.SeleccionarTurno(1, "00123456");

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ReservarTurnoViewModel>(view.Model);
        Assert.NotNull(model.PacienteEncontrado);
        Assert.Equal("Ana", model.Request.Nombre);
        Assert.Equal("López", model.Request.Apellido);
        Assert.Equal("1111", model.Request.Telefono);
    }

    private static ReservarTurnoRequest Request() => new()
    {
        TurnoDisponibleId = 1,
        Dni = " 00123456 ",
        Nombre = "María",
        Apellido = "Gómez",
        Telefono = "11223344"
    };

    private static void PrepararTurno(ClinicaDbContext dbContext, bool disponible = true)
    {
        dbContext.Especialidades.Add(new Especialidad
        {
            Id = 1,
            Nombre = "Clínica",
            Descripcion = "Atención clínica"
        });
        dbContext.TurnosDisponibles.Add(new TurnoDisponible
        {
            Id = 1,
            EspecialidadId = 1,
            ProfesionalNombre = "Dra. Prueba",
            Fecha = new DateOnly(2030, 4, 10),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(9, 30),
            Disponible = disponible
        });
        dbContext.SaveChanges();
    }

    private static ClinicaDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<ClinicaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ClinicaDbContext(options);
    }
}

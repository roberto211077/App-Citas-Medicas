using CitasMedicas.Web.Modules.Agenda.BuscarTurnos;
using CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure;
using CitasMedicas.Web.Modules.Reservas.ReservarTurno;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CitasMedicas.Web.Tests;

public sealed class ReservarTurnoTests
{
    [Fact]
    public async Task EjecutarAsync_TurnoDisponible_CreaPacienteReservaYMarcaTurnoReservado()
    {
        await using var dbContext = CrearContexto();
        dbContext.TurnosDisponibles.Add(new TurnoDisponible
        {
            Id = 12,
            EspecialidadId = 1,
            MedicoId = 7,
            Fecha = new DateOnly(2030, 4, 10),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(9, 30),
            Disponible = true
        });
        await dbContext.SaveChangesAsync();

        var resultado = await new ReservarTurnoUseCase(dbContext).EjecutarAsync(new ReservarTurnoRequest
        {
            TurnoId = 12,
            Dni = "  12345678  ",
            PacienteNombre = "  Juan  ",
            PacienteApellido = " Pérez ",
            PacienteTelefono = "  01112345678  ",
            PacienteCorreo = "  juan@example.com  "
        });

        var turno = await dbContext.TurnosDisponibles.SingleAsync();
        var paciente = await dbContext.Pacientes.SingleAsync();
        var reserva = await dbContext.Reservas.SingleAsync();
        Assert.Equal(ReservarTurnoResultado.Reservado, resultado);
        Assert.False(turno.Disponible);
        Assert.Equal("12345678", paciente.Dni);
        Assert.Equal("Juan", paciente.Nombre);
        Assert.Equal("Pérez", paciente.Apellido);
        Assert.Equal("01112345678", paciente.Telefono);
        Assert.Equal("juan@example.com", paciente.Correo);
        Assert.Equal(turno.Id, reserva.TurnoDisponibleId);
        Assert.Equal(paciente.Id, reserva.PacienteId);
    }

    [Fact]
    public async Task EjecutarAsync_TurnoYaReservado_RechazaSegundaReserva()
    {
        await using var dbContext = CrearContexto();
        dbContext.TurnosDisponibles.Add(new TurnoDisponible
        {
            Id = 12,
            EspecialidadId = 1,
            MedicoId = 7,
            Fecha = new DateOnly(2030, 4, 10),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(9, 30),
            Disponible = false
        });
        await dbContext.SaveChangesAsync();

        var resultado = await new ReservarTurnoUseCase(dbContext).EjecutarAsync(new ReservarTurnoRequest
        {
            TurnoId = 12,
            Dni = "98765432",
            PacienteNombre = "Otra",
            PacienteApellido = "Persona",
            PacienteTelefono = "01112345678",
            PacienteCorreo = "otra@example.com"
        });

        Assert.Equal(ReservarTurnoResultado.TurnoYaReservado, resultado);
        Assert.Empty(await dbContext.Reservas.ToListAsync());
        Assert.Empty(await dbContext.Pacientes.ToListAsync());
    }

    [Fact]
    public async Task EjecutarAsync_DniExistente_ReutilizaPacienteYConservaSusDatos()
    {
        await using var dbContext = CrearContexto();
        dbContext.Pacientes.Add(new CitasMedicas.Web.Modules.Reservas.Domain.Paciente
        {
            Dni = "12345678",
            Nombre = "María",
            Apellido = "Gómez",
            Telefono = "01155551234",
            Correo = "maria@example.com"
        });
        dbContext.TurnosDisponibles.Add(new TurnoDisponible
        {
            Id = 22,
            EspecialidadId = 1,
            MedicoId = 7,
            Fecha = new DateOnly(2030, 4, 10),
            HoraInicio = new TimeOnly(9, 0),
            HoraFin = new TimeOnly(9, 30),
            Disponible = true
        });
        await dbContext.SaveChangesAsync();
        var pacienteOriginal = await dbContext.Pacientes.SingleAsync();

        var resultado = await new ReservarTurnoUseCase(dbContext).EjecutarAsync(new ReservarTurnoRequest
        {
            TurnoId = 22,
            Dni = "12345678",
            PacienteNombre = "Nombre alterado",
            PacienteApellido = "Apellido alterado",
            PacienteTelefono = "000",
            PacienteCorreo = "otro@example.com"
        });

        var pacienteActual = await dbContext.Pacientes.SingleAsync();
        var reserva = await dbContext.Reservas.SingleAsync();
        Assert.Equal(ReservarTurnoResultado.Reservado, resultado);
        Assert.Equal(1, await dbContext.Pacientes.CountAsync());
        Assert.Equal("María", pacienteActual.Nombre);
        Assert.Equal("Gómez", pacienteActual.Apellido);
        Assert.Equal("01155551234", pacienteActual.Telefono);
        Assert.Equal("maria@example.com", pacienteActual.Correo);
        Assert.Equal(pacienteOriginal.Id, reserva.PacienteId);
    }

    [Fact]
    public async Task EjecutarAsync_TurnoInexistente_DevuelveNoEncontrado()
    {
        await using var dbContext = CrearContexto();

        var resultado = await new ReservarTurnoUseCase(dbContext).EjecutarAsync(new ReservarTurnoRequest
        {
            TurnoId = 404
        });

        Assert.Equal(ReservarTurnoResultado.TurnoNoEncontrado, resultado);
    }

    private static ClinicaDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<ClinicaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ClinicaDbContext(options);
    }
}

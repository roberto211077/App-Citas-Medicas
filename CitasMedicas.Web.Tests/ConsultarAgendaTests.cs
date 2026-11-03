using CitasMedicas.Web.Controllers;
using CitasMedicas.Web.Modules.Agenda.BuscarTurnos;
using CitasMedicas.Web.Modules.Agenda.ConsultarAgenda;
using CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure;
using CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades;
using CitasMedicas.Web.Modules.Reservas.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace CitasMedicas.Web.Tests;

public sealed class ConsultarAgendaTests
{
    [Fact]
    public async Task EjecutarAsync_FiltraMedicoIncluyeDisponiblesYReservadosEnOrdenCronologico()
    {
        await using var dbContext = CrearContexto();
        dbContext.Especialidades.Add(new Especialidad
        {
            Id = 1,
            Nombre = "Cardiología",
            Descripcion = "Atención cardiológica"
        });
        dbContext.Medicos.AddRange(
            new Medico { Id = 7, Nombre = "Ana Pérez", EspecialidadId = 1 },
            new Medico { Id = 8, Nombre = "Luis Gómez", EspecialidadId = 1 });
        var pacientes = new[]
        {
            Paciente(1, "María", "Dos"),
            Paciente(2, "Juan", "Uno"),
            Paciente(3, "Lucía", "Tres")
        };
        dbContext.Pacientes.AddRange(pacientes);
        var turnos = new[]
        {
            Turno(7, new DateOnly(2030, 4, 11), new TimeOnly(10, 0), false),
            Turno(7, new DateOnly(2030, 4, 10), new TimeOnly(11, 0), true),
            Turno(7, new DateOnly(2030, 4, 10), new TimeOnly(9, 0), false),
            Turno(8, new DateOnly(2030, 4, 10), new TimeOnly(8, 0), false)
        };
        dbContext.TurnosDisponibles.AddRange(turnos);
        dbContext.Reservas.AddRange(
            Reserva(1, pacientes[0], turnos[0]),
            Reserva(2, pacientes[1], turnos[2]),
            Reserva(3, pacientes[2], turnos[3]),
            Reserva(4, pacientes[1], turnos[1]));
        await dbContext.SaveChangesAsync();

        var resultados = await new ConsultarAgendaUseCase(dbContext).EjecutarAsync(7);

        Assert.Equal(3, resultados.Count);
        Assert.Equal("Juan Uno", resultados[0].PacienteNombre);
        Assert.Equal("01112345678", resultados[0].PacienteTelefono);
        Assert.Equal("juan.uno@example.com", resultados[0].PacienteCorreo);
        Assert.Equal("Ana Pérez", resultados[0].MedicoNombre);
        Assert.Equal(new TimeOnly(9, 0), resultados[0].HoraInicio);
        Assert.True(resultados[1].Disponible);
        Assert.Null(resultados[1].PacienteNombre);
        Assert.Equal(new TimeOnly(11, 0), resultados[1].HoraInicio);
        Assert.False(resultados[2].Disponible);
        Assert.Equal("María Dos", resultados[2].PacienteNombre);
        Assert.Equal(new DateOnly(2030, 4, 11), resultados[2].Fecha);
    }

    [Fact]
    public async Task ConsultarAgenda_SinMedico_MuestraSelectorSinResultados()
    {
        await using var dbContext = CrearContexto();
        dbContext.Especialidades.Add(new Especialidad
        {
            Id = 1,
            Nombre = "Cardiología",
            Descripcion = "Atención cardiológica"
        });
        dbContext.Medicos.AddRange(
            new Medico { Id = 7, Nombre = "Ana Pérez", EspecialidadId = 1 },
            new Medico { Id = 8, Nombre = "Luis Gómez", EspecialidadId = 1 });
        dbContext.TurnosDisponibles.Add(Turno(7, new DateOnly(2030, 4, 10), new TimeOnly(9, 0), true));
        await dbContext.SaveChangesAsync();

        var result = await CrearController(dbContext).ConsultarAgenda(null, default);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ConsultarAgendaViewModel>(view.Model);
        Assert.False(model.ConsultaRealizada);
        Assert.Equal(2, model.Medicos.Count);
        Assert.Empty(model.Turnos);
    }

    [Fact]
    public async Task ConsultarAgenda_MedicoExistenteSinTurnos_MuestraAgendaVacia()
    {
        await using var dbContext = CrearContexto();
        dbContext.Especialidades.Add(new Especialidad
        {
            Id = 1,
            Nombre = "Cardiología",
            Descripcion = "Atención cardiológica"
        });
        dbContext.Medicos.Add(new Medico { Id = 8, Nombre = "Luis Gómez", EspecialidadId = 1 });
        await dbContext.SaveChangesAsync();

        var result = await CrearController(dbContext).ConsultarAgenda(8, default);

        var view = Assert.IsType<ViewResult>(result);
        var model = Assert.IsType<ConsultarAgendaViewModel>(view.Model);
        Assert.True(model.ConsultaRealizada);
        Assert.Equal(8, model.MedicoId);
        Assert.Empty(model.Turnos);
    }

    [Fact]
    public async Task ConsultarAgenda_MedicoInexistente_DevuelveNotFound()
    {
        await using var dbContext = CrearContexto();
        var result = await CrearController(dbContext).ConsultarAgenda(404, default);
        Assert.IsType<NotFoundResult>(result);
    }

    private static ClinicaDbContext CrearContexto()
    {
        var options = new DbContextOptionsBuilder<ClinicaDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new ClinicaDbContext(options);
    }

    private static AgendaController CrearController(ClinicaDbContext dbContext)
    {
        return new AgendaController(
            new CitasMedicas.Web.Modules.CatalogoMedico.ListarMedicos.ListarMedicosUseCase(dbContext),
            new CitasMedicas.Web.Modules.Agenda.CargarDisponibilidad.CargarDisponibilidadUseCase(dbContext),
            new ConsultarAgendaUseCase(dbContext))
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    private static Paciente Paciente(int id, string nombre, string apellido) => new()
    {
        Id = id,
        Dni = $"{id:00000000}",
        Nombre = nombre,
        Apellido = apellido,
        Telefono = "01112345678",
        Correo = $"{nombre.ToLowerInvariant()}.{apellido.ToLowerInvariant()}@example.com"
    };

    private static Reserva Reserva(int id, Paciente paciente, TurnoDisponible turno) => new()
    {
        Id = id,
        Paciente = paciente,
        TurnoDisponible = turno,
        FechaCreacionUtc = new DateTime(2030, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    };

    private static TurnoDisponible Turno(int medicoId, DateOnly fecha, TimeOnly hora, bool disponible) => new()
    {
        MedicoId = medicoId,
        EspecialidadId = 1,
        Fecha = fecha,
        HoraInicio = hora,
        HoraFin = hora.AddMinutes(30),
        Disponible = disponible
    };
}

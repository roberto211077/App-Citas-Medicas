using CitasMedicas.Web.Modules.Agenda.BuscarTurnos;
using CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure;
using CitasMedicas.Web.Modules.Reservas.Domain;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicas.Web.Modules.Reservas.Application.ReservarTurno;

public sealed class ReservarTurnoUseCase(ClinicaDbContext dbContext)
{
    public Task<Paciente?> BuscarPacientePorDniAsync(string dni, CancellationToken cancellationToken = default)
    {
        var dniNormalizado = dni.Trim();
        return dbContext.Pacientes.AsNoTracking()
            .SingleOrDefaultAsync(paciente => paciente.Dni == dniNormalizado, cancellationToken);
    }

    public Task<TurnoDisponible?> BuscarTurnoDisponibleAsync(int turnoId, CancellationToken cancellationToken = default)
    {
        return dbContext.TurnosDisponibles.AsNoTracking()
            .Include(turno => turno.Especialidad)
            .SingleOrDefaultAsync(turno => turno.Id == turnoId && turno.Disponible, cancellationToken);
    }

    public async Task<ReservarTurnoResultado> EjecutarAsync(
        ReservarTurnoRequest request,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Dni)
            || string.IsNullOrWhiteSpace(request.Nombre)
            || string.IsNullOrWhiteSpace(request.Apellido)
            || string.IsNullOrWhiteSpace(request.Telefono))
        {
            return new(false, null, "Completá DNI, nombre, apellido y teléfono para continuar.");
        }

        var dni = request.Dni.Trim();
        var paciente = await dbContext.Pacientes.SingleOrDefaultAsync(
            existente => existente.Dni == dni, cancellationToken);
        var turno = await dbContext.TurnosDisponibles.SingleOrDefaultAsync(
            existente => existente.Id == request.TurnoDisponibleId && existente.Disponible,
            cancellationToken);

        if (turno is null)
        {
            return new(false, null, "El turno ya no está disponible. Elegí otro horario.");
        }

        if (paciente is null)
        {
            paciente = new Paciente
            {
                Dni = dni,
                Nombre = request.Nombre.Trim(),
                Apellido = request.Apellido.Trim(),
                Telefono = request.Telefono.Trim()
            };
            dbContext.Pacientes.Add(paciente);
        }

        turno.Disponible = false;
        var reserva = new Reserva
        {
            Paciente = paciente,
            TurnoDisponible = turno,
            FechaCreacionUtc = DateTime.UtcNow
        };
        dbContext.Reservas.Add(reserva);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return new(false, null, "No se pudo confirmar la reserva. El turno pudo haber sido tomado; intentá nuevamente.");
        }

        return new(true, reserva.Id, "La reserva se confirmó correctamente.");
    }

    public Task<Reserva?> BuscarReservaAsync(int reservaId, CancellationToken cancellationToken = default)
    {
        return dbContext.Reservas.AsNoTracking()
            .Include(reserva => reserva.Paciente)
            .Include(reserva => reserva.TurnoDisponible)
                .ThenInclude(turno => turno.Especialidad)
            .SingleOrDefaultAsync(reserva => reserva.Id == reservaId, cancellationToken);
    }
}

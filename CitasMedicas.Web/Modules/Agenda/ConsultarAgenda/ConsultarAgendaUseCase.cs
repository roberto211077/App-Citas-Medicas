using CitasMedicas.Web.Modules.Agenda.BuscarTurnos;
using CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicas.Web.Modules.Agenda.ConsultarAgenda;

public sealed class ConsultarAgendaUseCase(ClinicaDbContext dbContext)
{
    public async Task<List<TurnoAgenda>> EjecutarAsync(int medicoId, CancellationToken cancellationToken = default)
    {
        var turnos = await dbContext.TurnosDisponibles
            .AsNoTracking()
            .Where(turno => turno.MedicoId == medicoId)
            .OrderBy(turno => turno.Fecha)
            .ThenBy(turno => turno.HoraInicio)
            .Select(turno => new TurnoAgenda
            {
                TurnoId = turno.Id,
                MedicoNombre = turno.Medico.Nombre,
                Fecha = turno.Fecha,
                HoraInicio = turno.HoraInicio,
                HoraFin = turno.HoraFin,
                Disponible = turno.Disponible
            })
            .ToListAsync(cancellationToken);

        var reservasPorTurno = await dbContext.Reservas
            .AsNoTracking()
            .Where(reserva => reserva.TurnoDisponible.MedicoId == medicoId
                && !reserva.TurnoDisponible.Disponible)
            .Select(reserva => new
            {
                reserva.TurnoDisponibleId,
                PacienteNombre = reserva.Paciente.Nombre + " " + reserva.Paciente.Apellido,
                reserva.Paciente.Telefono,
                reserva.Paciente.Correo
            })
            .ToDictionaryAsync(reserva => reserva.TurnoDisponibleId, cancellationToken);

        foreach (var turno in turnos)
        {
            if (reservasPorTurno.TryGetValue(turno.TurnoId, out var reserva))
            {
                turno.PacienteNombre = reserva.PacienteNombre;
                turno.PacienteTelefono = reserva.Telefono;
                turno.PacienteCorreo = reserva.Correo;
            }
        }

        return turnos;
    }
}

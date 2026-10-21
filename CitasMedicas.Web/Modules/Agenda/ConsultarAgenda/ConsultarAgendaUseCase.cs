using CitasMedicas.Web.Modules.Agenda.BuscarTurnos;
using CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicas.Web.Modules.Agenda.ConsultarAgenda;

public sealed class ConsultarAgendaUseCase(ClinicaDbContext dbContext)
{
    public Task<List<TurnoReservado>> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Reservas
            .AsNoTracking()
            .Where(reserva => !reserva.TurnoDisponible.Disponible)
            .OrderBy(reserva => reserva.TurnoDisponible.Fecha)
            .ThenBy(reserva => reserva.TurnoDisponible.HoraInicio)
            .Select(reserva => new TurnoReservado
            {
                PacienteNombre = reserva.Paciente.Nombre + " " + reserva.Paciente.Apellido,
                PacienteTelefono = reserva.Paciente.Telefono,
                PacienteCorreo = reserva.Paciente.Correo,
                MedicoNombre = reserva.TurnoDisponible.Medico.Nombre,
                Fecha = reserva.TurnoDisponible.Fecha,
                HoraInicio = reserva.TurnoDisponible.HoraInicio,
                HoraFin = reserva.TurnoDisponible.HoraFin
            })
            .ToListAsync(cancellationToken);
    }
}

using CitasMedicas.Web.Modules.Agenda.BuscarTurnos;
using CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure;
using CitasMedicas.Web.Modules.Reservas.Domain;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicas.Web.Modules.Reservas.ReservarTurno;

public enum ReservarTurnoResultado
{
    Reservado,
    TurnoNoEncontrado,
    TurnoYaReservado
}

public sealed class ReservarTurnoUseCase(ClinicaDbContext dbContext)
{
    public Task<Paciente?> BuscarPacientePorDniAsync(string dni, CancellationToken cancellationToken = default)
    {
        var dniNormalizado = dni.Trim();
        return dbContext.Pacientes.AsNoTracking()
            .SingleOrDefaultAsync(paciente => paciente.Dni == dniNormalizado, cancellationToken);
    }

    public async Task<ReservarTurnoResultado> EjecutarAsync(
        ReservarTurnoRequest request,
        CancellationToken cancellationToken = default)
    {
        var turno = await dbContext.TurnosDisponibles
            .SingleOrDefaultAsync(turno => turno.Id == request.TurnoId, cancellationToken);

        if (turno is null)
        {
            return ReservarTurnoResultado.TurnoNoEncontrado;
        }

        if (!turno.Disponible)
        {
            return ReservarTurnoResultado.TurnoYaReservado;
        }

        var dni = request.Dni.Trim();
        var paciente = await dbContext.Pacientes
            .FirstOrDefaultAsync(paciente => paciente.Dni == dni, cancellationToken);

        if (paciente is null)
        {
            paciente = new Paciente { Dni = dni };
            dbContext.Pacientes.Add(paciente);
        }

        if (string.IsNullOrWhiteSpace(paciente.Nombre))
        {
            paciente.Nombre = request.PacienteNombre.Trim();
            paciente.Apellido = request.PacienteApellido.Trim();
            paciente.Telefono = request.PacienteTelefono.Trim();
            paciente.Correo = request.PacienteCorreo.Trim();
        }
        turno.Disponible = false;
        dbContext.Reservas.Add(new Reserva
        {
            Paciente = paciente,
            TurnoDisponible = turno,
            FechaCreacionUtc = DateTime.UtcNow
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return ReservarTurnoResultado.Reservado;
    }
}

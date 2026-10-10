using CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicas.Web.Modules.Agenda.BuscarTurnos;

public enum FranjaHoraria
{
    Manana,
    Tarde
}

public sealed class BuscarTurnosUseCase(ClinicaDbContext dbContext)
{
    public Task<List<TurnoDisponible>> EjecutarAsync(
        int especialidadId,
        DateOnly fecha,
        FranjaHoraria franja,
        CancellationToken cancellationToken = default)
    {
        var (desde, hasta) = franja switch
        {
            FranjaHoraria.Manana => (new TimeOnly(8, 0), new TimeOnly(12, 0)),
            FranjaHoraria.Tarde => (new TimeOnly(12, 0), new TimeOnly(18, 0)),
            _ => throw new ArgumentOutOfRangeException(nameof(franja))
        };

        return dbContext.TurnosDisponibles
            .AsNoTracking()
            .Where(turno => turno.EspecialidadId == especialidadId
                && turno.Fecha == fecha
                && turno.HoraInicio >= desde
                && turno.HoraInicio < hasta
                && turno.Disponible)
            .OrderBy(turno => turno.HoraInicio)
            .ToListAsync(cancellationToken);
    }

    public Task<TurnoDisponible?> BuscarDisponiblePorIdAsync(
        int turnoId,
        CancellationToken cancellationToken = default)
    {
        return dbContext.TurnosDisponibles
            .AsNoTracking()
            .Include(turno => turno.Especialidad)
            .SingleOrDefaultAsync(turno => turno.Id == turnoId && turno.Disponible, cancellationToken);
    }
}

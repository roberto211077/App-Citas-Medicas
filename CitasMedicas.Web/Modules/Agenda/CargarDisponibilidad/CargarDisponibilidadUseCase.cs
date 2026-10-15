using CitasMedicas.Web.Modules.Agenda.BuscarTurnos;
using CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicas.Web.Modules.Agenda.CargarDisponibilidad;

public sealed class CargarDisponibilidadUseCase(ClinicaDbContext dbContext)
{
    private static readonly TimeSpan DuracionTurno = TimeSpan.FromMinutes(30);
    private static readonly TimeSpan IntervaloMinimoEntreTurnos = TimeSpan.FromMinutes(30);
    private static readonly TimeOnly InicioManana = new(8, 0);
    private static readonly TimeOnly FinManana = new(14, 0);
    private static readonly TimeOnly InicioTarde = new(15, 0);
    private static readonly TimeOnly FinTarde = new(22, 0);
    private static readonly TimeOnly UltimaHoraDeInicio = new(23, 30);

    public async Task<CargarDisponibilidadResultado> EjecutarAsync(
        CargarDisponibilidadRequest request,
        CancellationToken cancellationToken = default)
    {
        if (!request.Fecha.HasValue || !request.HoraInicio.HasValue)
        {
            return CargarDisponibilidadResultado.HorarioInvalido;
        }

        if (request.HoraInicio.Value > UltimaHoraDeInicio)
        {
            return CargarDisponibilidadResultado.HorarioInvalido;
        }

        var horaInicio = request.HoraInicio.Value;
        var horaFin = horaInicio.Add(DuracionTurno);
        var dentroDeLaManana = horaInicio >= InicioManana && horaFin <= FinManana;
        var dentroDeLaTarde = horaInicio >= InicioTarde && horaFin <= FinTarde;
        if (!dentroDeLaManana && !dentroDeLaTarde)
        {
            return CargarDisponibilidadResultado.HorarioFueraDeFranja;
        }

        var medico = await dbContext.Medicos
            .AsNoTracking()
            .SingleOrDefaultAsync(medico => medico.Id == request.MedicoId, cancellationToken);
        if (medico is null)
        {
            return CargarDisponibilidadResultado.MedicoNoEncontrado;
        }

        var turnosDelMedicoEseDia = await dbContext.TurnosDisponibles
            .AsNoTracking()
            .Where(turno => turno.MedicoId == medico.Id && turno.Fecha == request.Fecha.Value)
            .Select(turno => new { turno.HoraInicio, turno.HoraFin })
            .ToListAsync(cancellationToken);

        var inicioSolicitadoTicks = horaInicio.ToTimeSpan().Ticks;
        var finSolicitadoConIntervaloTicks = inicioSolicitadoTicks
            + DuracionTurno.Ticks
            + IntervaloMinimoEntreTurnos.Ticks;
        var existeConflicto = turnosDelMedicoEseDia.Any(turno =>
        {
            var inicioExistente = turno.HoraInicio.ToTimeSpan().Ticks;
            var finExistente = turno.HoraFin.ToTimeSpan().Ticks;
            return inicioSolicitadoTicks < finExistente + IntervaloMinimoEntreTurnos.Ticks
                && inicioExistente < finSolicitadoConIntervaloTicks;
        });

        if (existeConflicto)
        {
            return CargarDisponibilidadResultado.TurnoEnConflicto;
        }

        dbContext.TurnosDisponibles.Add(new TurnoDisponible
        {
            MedicoId = medico.Id,
            EspecialidadId = medico.EspecialidadId,
            ProfesionalNombre = medico.Nombre,
            Fecha = request.Fecha.Value,
            HoraInicio = horaInicio,
            HoraFin = horaFin,
            Disponible = true
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return CargarDisponibilidadResultado.Registrado;
    }
}

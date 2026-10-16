using CitasMedicas.Web.Modules.Agenda.BuscarTurnos;
using CitasMedicas.Web.Modules.Reservas.Domain;

namespace CitasMedicas.Web.Modules.Reservas.Application.ReservarTurno;

public sealed class ReservarTurnoViewModel
{
    public required TurnoDisponible Turno { get; init; }
    public ReservarTurnoRequest Request { get; init; } = new();
    public Paciente? PacienteEncontrado { get; init; }
    public string? Mensaje { get; init; }
}

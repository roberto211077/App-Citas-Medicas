using CitasMedicas.Web.Modules.Agenda.BuscarTurnos;

namespace CitasMedicas.Web.Modules.Reservas.Domain;

public sealed class Reserva
{
    public int Id { get; set; }
    public int PacienteId { get; set; }
    public Paciente Paciente { get; set; } = null!;
    public int TurnoDisponibleId { get; set; }
    public TurnoDisponible TurnoDisponible { get; set; } = null!;
    public DateTime FechaCreacionUtc { get; set; }
}

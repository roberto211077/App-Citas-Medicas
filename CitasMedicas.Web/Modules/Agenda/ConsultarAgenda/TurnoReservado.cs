namespace CitasMedicas.Web.Modules.Agenda.ConsultarAgenda;

public sealed class TurnoReservado
{
    public string PacienteNombre { get; init; } = string.Empty;
    public string PacienteTelefono { get; init; } = string.Empty;
    public string? PacienteCorreo { get; init; }
    public string MedicoNombre { get; init; } = string.Empty;
    public DateOnly Fecha { get; init; }
    public TimeOnly HoraInicio { get; init; }
    public TimeOnly HoraFin { get; init; }
}

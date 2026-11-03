namespace CitasMedicas.Web.Modules.Agenda.ConsultarAgenda;

public sealed class TurnoAgenda
{
    public int TurnoId { get; init; }
    public string? PacienteNombre { get; set; }
    public string? PacienteTelefono { get; set; }
    public string? PacienteCorreo { get; set; }
    public string MedicoNombre { get; init; } = string.Empty;
    public DateOnly Fecha { get; init; }
    public TimeOnly HoraInicio { get; init; }
    public TimeOnly HoraFin { get; init; }
    public bool Disponible { get; init; }
}

namespace CitasMedicas.Web.Modules.Agenda.ConsultarAgenda;

public sealed class ConsultarAgendaViewModel
{
    public IReadOnlyList<MedicoAgendaOption> Medicos { get; init; } = [];
    public int? MedicoId { get; init; }
    public IReadOnlyList<TurnoAgenda> Turnos { get; init; } = [];
    public bool ConsultaRealizada => MedicoId.HasValue;
}

public sealed class MedicoAgendaOption
{
    public int Id { get; init; }
    public string Nombre { get; init; } = string.Empty;
    public string EspecialidadNombre { get; init; } = string.Empty;
}

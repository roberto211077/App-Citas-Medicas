namespace CitasMedicas.Web.Modules.Agenda.ConsultarAgenda;

public sealed class ConsultarAgendaViewModel
{
    public IReadOnlyList<TurnoReservado> Turnos { get; init; } = [];
}

using CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades;

namespace CitasMedicas.Web.Modules.Agenda.BuscarTurnos;

public sealed class TurnoDisponible
{
    public int Id { get; set; }
    public int EspecialidadId { get; set; }
    public Especialidad Especialidad { get; set; } = null!;
    public int MedicoId { get; set; }
    public Medico Medico { get; set; } = null!;
    public DateOnly Fecha { get; set; }
    public TimeOnly HoraInicio { get; set; }
    public TimeOnly HoraFin { get; set; }
    public bool Disponible { get; set; }
}

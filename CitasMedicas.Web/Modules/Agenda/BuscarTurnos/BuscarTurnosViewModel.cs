using System.ComponentModel.DataAnnotations;

namespace CitasMedicas.Web.Modules.Agenda.BuscarTurnos;

public sealed class BuscarTurnosViewModel
{
    public int EspecialidadId { get; init; }
    public string EspecialidadNombre { get; init; } = string.Empty;
    public string EspecialidadDescripcion { get; init; } = string.Empty;

    [Required(ErrorMessage = "Seleccioná una fecha.")]
    public DateOnly? Fecha { get; init; }

    [Required(ErrorMessage = "Seleccioná una franja horaria.")]
    public FranjaHoraria? Franja { get; init; }

    public bool BusquedaRealizada { get; init; }
    public IReadOnlyList<TurnoDisponible> Turnos { get; init; } = [];
}

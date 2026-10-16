using System.ComponentModel.DataAnnotations;

namespace CitasMedicas.Web.Modules.Agenda.CargarDisponibilidad;

public sealed class CargarDisponibilidadRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccioná un médico disponible.")]
    [Display(Name = "Médico")]
    public int MedicoId { get; set; }

    [Required(ErrorMessage = "Seleccioná una fecha.")]
    [Display(Name = "Fecha")]
    public DateOnly? Fecha { get; set; }

    [Required(ErrorMessage = "Seleccioná una hora.")]
    [Display(Name = "Hora de inicio")]
    public TimeOnly? HoraInicio { get; set; }
}

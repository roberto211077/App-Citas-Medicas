using System.ComponentModel.DataAnnotations;

namespace CitasMedicas.Web.Modules.Reservas.Application.ReservarTurno;

public sealed class ReservarTurnoRequest
{
    public int TurnoDisponibleId { get; set; }

    [Required(ErrorMessage = "Ingresá el DNI.")]
    [StringLength(20)]
    public string Dni { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá el nombre.")]
    [StringLength(100)]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá el apellido.")]
    [StringLength(100)]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá el teléfono.")]
    [StringLength(30)]
    public string Telefono { get; set; } = string.Empty;
}

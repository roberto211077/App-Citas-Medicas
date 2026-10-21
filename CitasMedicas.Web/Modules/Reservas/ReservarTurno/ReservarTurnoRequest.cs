using System.ComponentModel.DataAnnotations;
using CitasMedicas.Web.Modules.Agenda.BuscarTurnos;

namespace CitasMedicas.Web.Modules.Reservas.ReservarTurno;

public sealed class ReservarTurnoRequest
{
    public int TurnoId { get; init; }

    [Required(ErrorMessage = "Ingresá el DNI del paciente.")]
    [StringLength(20)]
    public string Dni { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá el nombre del paciente.")]
    [StringLength(150)]
    public string PacienteNombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá el apellido del paciente.")]
    [StringLength(100)]
    public string PacienteApellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá el teléfono del paciente.")]
    [Phone(ErrorMessage = "Ingresá un teléfono válido.")]
    [StringLength(30)]
    public string PacienteTelefono { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "Ingresá un correo válido.")]
    [StringLength(254)]
    public string PacienteCorreo { get; set; } = string.Empty;

    public bool PacienteEncontrado { get; set; }

    public TurnoDisponible? Turno { get; set; }
}

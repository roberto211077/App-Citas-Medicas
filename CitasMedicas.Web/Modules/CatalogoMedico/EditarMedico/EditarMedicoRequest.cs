using System.ComponentModel.DataAnnotations;

namespace CitasMedicas.Web.Modules.CatalogoMedico.EditarMedico;

public sealed class EditarMedicoRequest
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ingresá el nombre del médico.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    [Display(Name = "Nombre del médico")]
    public string Nombre { get; set; } = string.Empty;

    [Range(1, int.MaxValue, ErrorMessage = "Seleccioná una especialidad disponible.")]
    [Display(Name = "Especialidad")]
    public int EspecialidadId { get; set; }
}

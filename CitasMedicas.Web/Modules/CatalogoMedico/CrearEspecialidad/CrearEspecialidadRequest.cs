using System.ComponentModel.DataAnnotations;

namespace CitasMedicas.Web.Modules.CatalogoMedico.CrearEspecialidad;

public sealed class CrearEspecialidadRequest
{
    [Required(ErrorMessage = "Ingresá el nombre de la especialidad.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    [Display(Name = "Nombre")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "Ingresá una descripción.")]
    [StringLength(300, ErrorMessage = "La descripción no puede superar los 300 caracteres.")]
    [Display(Name = "Descripción")]
    public string Descripcion { get; set; } = string.Empty;
}

namespace CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades;

public sealed class Medico
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int EspecialidadId { get; set; }
    public Especialidad Especialidad { get; set; } = null!;
}

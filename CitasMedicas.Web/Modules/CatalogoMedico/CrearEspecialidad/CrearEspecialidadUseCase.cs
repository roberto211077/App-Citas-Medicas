using CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure;
using CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades;

namespace CitasMedicas.Web.Modules.CatalogoMedico.CrearEspecialidad;

public sealed class CrearEspecialidadUseCase(ClinicaDbContext dbContext)
{
    public async Task EjecutarAsync(CrearEspecialidadRequest request, CancellationToken cancellationToken = default)
    {
        var especialidad = new Especialidad
        {
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion.Trim()
        };

        dbContext.Especialidades.Add(especialidad);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}

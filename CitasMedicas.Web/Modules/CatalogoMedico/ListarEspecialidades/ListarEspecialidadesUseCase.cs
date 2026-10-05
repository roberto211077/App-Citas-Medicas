using CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades;

public sealed class ListarEspecialidadesUseCase(ClinicaDbContext dbContext)
{
    public Task<List<Especialidad>> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Especialidades
            .AsNoTracking()
            .OrderBy(especialidad => especialidad.Nombre)
            .ToListAsync(cancellationToken);
    }

    public Task<Especialidad?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return dbContext.Especialidades
            .AsNoTracking()
            .FirstOrDefaultAsync(especialidad => especialidad.Id == id, cancellationToken);
    }
}

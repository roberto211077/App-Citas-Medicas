using CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure;
using CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicas.Web.Modules.CatalogoMedico.ListarMedicos;

public sealed class ListarMedicosUseCase(ClinicaDbContext dbContext)
{
    public Task<List<Medico>> EjecutarAsync(CancellationToken cancellationToken = default)
    {
        return dbContext.Medicos
            .AsNoTracking()
            .Include(medico => medico.Especialidad)
            .OrderBy(medico => medico.Nombre)
            .ToListAsync(cancellationToken);
    }
}

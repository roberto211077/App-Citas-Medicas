using CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure;
using CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicas.Web.Modules.CatalogoMedico.RegistrarMedico;

public sealed class RegistrarMedicoUseCase(ClinicaDbContext dbContext)
{
    public async Task<bool> EjecutarAsync(RegistrarMedicoRequest request, CancellationToken cancellationToken = default)
    {
        var especialidadExiste = await dbContext.Especialidades
            .AnyAsync(especialidad => especialidad.Id == request.EspecialidadId, cancellationToken);
        if (!especialidadExiste)
        {
            return false;
        }

        dbContext.Medicos.Add(new Medico
        {
            Nombre = request.Nombre.Trim(),
            EspecialidadId = request.EspecialidadId
        });
        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }
}

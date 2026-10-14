using CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicas.Web.Modules.CatalogoMedico.EditarMedico;

public sealed class EditarMedicoUseCase(ClinicaDbContext dbContext)
{
    public async Task<EditarMedicoRequest?> BuscarPorIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return await dbContext.Medicos
            .AsNoTracking()
            .Where(medico => medico.Id == id)
            .Select(medico => new EditarMedicoRequest
            {
                Id = medico.Id,
                Nombre = medico.Nombre,
                EspecialidadId = medico.EspecialidadId
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<EditarMedicoResultado> EjecutarAsync(
        EditarMedicoRequest request,
        CancellationToken cancellationToken = default)
    {
        var medico = await dbContext.Medicos
            .SingleOrDefaultAsync(medico => medico.Id == request.Id, cancellationToken);
        if (medico is null)
        {
            return EditarMedicoResultado.MedicoNoEncontrado;
        }

        var especialidadExiste = await dbContext.Especialidades
            .AnyAsync(especialidad => especialidad.Id == request.EspecialidadId, cancellationToken);
        if (!especialidadExiste)
        {
            return EditarMedicoResultado.EspecialidadNoEncontrada;
        }

        medico.Nombre = request.Nombre.Trim();
        medico.EspecialidadId = request.EspecialidadId;
        await dbContext.SaveChangesAsync(cancellationToken);
        return EditarMedicoResultado.Actualizado;
    }
}

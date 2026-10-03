using CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure;

public sealed class ClinicaDbContext(DbContextOptions<ClinicaDbContext> options) : DbContext(options)
{
    public DbSet<Especialidad> Especialidades => Set<Especialidad>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Especialidad>(entity =>
        {
            entity.ToTable("Especialidades");
            entity.HasKey(especialidad => especialidad.Id);
            entity.Property(especialidad => especialidad.Id).UseIdentityColumn(seed: 1, increment: 1);
            entity.Property(especialidad => especialidad.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(especialidad => especialidad.Descripcion).HasMaxLength(300).IsRequired();
        });
    }
}

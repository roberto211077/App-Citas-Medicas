using CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades;
using CitasMedicas.Web.Modules.Agenda.BuscarTurnos;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure;

public sealed class ClinicaDbContext(DbContextOptions<ClinicaDbContext> options) : DbContext(options)
{
    public DbSet<Especialidad> Especialidades => Set<Especialidad>();
    public DbSet<TurnoDisponible> TurnosDisponibles => Set<TurnoDisponible>();

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

        modelBuilder.Entity<TurnoDisponible>(entity =>
        {
            entity.ToTable("TurnosDisponibles");
            entity.HasKey(turno => turno.Id);
            entity.Property(turno => turno.Id).UseIdentityColumn();
            entity.Property(turno => turno.ProfesionalNombre).HasMaxLength(150).IsRequired();
            entity.Property(turno => turno.Fecha).HasColumnType("date").IsRequired();
            entity.Property(turno => turno.HoraInicio).HasColumnType("time").IsRequired();
            entity.Property(turno => turno.HoraFin).HasColumnType("time").IsRequired();
            entity.Property(turno => turno.Disponible).IsRequired();
            entity.HasOne(turno => turno.Especialidad)
                .WithMany()
                .HasForeignKey(turno => turno.EspecialidadId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

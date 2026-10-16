using CitasMedicas.Web.Modules.CatalogoMedico.ListarEspecialidades;
using CitasMedicas.Web.Modules.Agenda.BuscarTurnos;
using CitasMedicas.Web.Modules.Reservas.Domain;
using Microsoft.EntityFrameworkCore;

namespace CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure;

public sealed class ClinicaDbContext(DbContextOptions<ClinicaDbContext> options) : DbContext(options)
{
    public DbSet<Especialidad> Especialidades => Set<Especialidad>();
    public DbSet<Medico> Medicos => Set<Medico>();
    public DbSet<TurnoDisponible> TurnosDisponibles => Set<TurnoDisponible>();
    public DbSet<Paciente> Pacientes => Set<Paciente>();
    public DbSet<Reserva> Reservas => Set<Reserva>();

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

        modelBuilder.Entity<Medico>(entity =>
        {
            entity.ToTable("Medicos");
            entity.HasKey(medico => medico.Id);
            entity.Property(medico => medico.Id).UseIdentityColumn();
            entity.Property(medico => medico.Nombre).HasMaxLength(150).IsRequired();
            entity.HasOne(medico => medico.Especialidad)
                .WithMany()
                .HasForeignKey(medico => medico.EspecialidadId)
                .OnDelete(DeleteBehavior.Restrict);
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
            entity.HasOne(turno => turno.Medico)
                .WithMany()
                .HasForeignKey(turno => turno.MedicoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Paciente>(entity =>
        {
            entity.ToTable("Pacientes");
            entity.HasKey(paciente => paciente.Id);
            entity.Property(paciente => paciente.Id).UseIdentityColumn();
            entity.Property(paciente => paciente.Dni).HasMaxLength(20).IsRequired();
            entity.HasIndex(paciente => paciente.Dni).IsUnique();
            entity.Property(paciente => paciente.Nombre).HasMaxLength(100).IsRequired();
            entity.Property(paciente => paciente.Apellido).HasMaxLength(100).IsRequired();
            entity.Property(paciente => paciente.Telefono).HasMaxLength(30).IsRequired();
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.ToTable("Reservas");
            entity.HasKey(reserva => reserva.Id);
            entity.Property(reserva => reserva.Id).UseIdentityColumn();
            entity.Property(reserva => reserva.FechaCreacionUtc).IsRequired();
            entity.HasIndex(reserva => reserva.TurnoDisponibleId).IsUnique();
            entity.HasOne(reserva => reserva.Paciente)
                .WithMany()
                .HasForeignKey(reserva => reserva.PacienteId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(reserva => reserva.TurnoDisponible)
                .WithMany()
                .HasForeignKey(reserva => reserva.TurnoDisponibleId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}

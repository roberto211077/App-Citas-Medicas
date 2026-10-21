using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UsarMedicoEnTurno : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO Medicos (Nombre, EspecialidadId)
                SELECT DISTINCT LTRIM(RTRIM(turno.ProfesionalNombre)), turno.EspecialidadId
                FROM TurnosDisponibles AS turno
                WHERE turno.MedicoId IS NULL
                  AND NOT EXISTS (
                      SELECT 1
                      FROM Medicos AS medico
                      WHERE medico.EspecialidadId = turno.EspecialidadId
                        AND medico.Nombre = LTRIM(RTRIM(turno.ProfesionalNombre)));

                UPDATE turno
                SET MedicoId = medico.Id
                FROM TurnosDisponibles AS turno
                INNER JOIN Medicos AS medico
                    ON medico.EspecialidadId = turno.EspecialidadId
                    AND medico.Nombre = LTRIM(RTRIM(turno.ProfesionalNombre))
                WHERE turno.MedicoId IS NULL;
                """);

            migrationBuilder.DropColumn(
                name: "ProfesionalNombre",
                table: "TurnosDisponibles");

            migrationBuilder.AlterColumn<int>(
                name: "MedicoId",
                table: "TurnosDisponibles",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "MedicoId",
                table: "TurnosDisponibles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<string>(
                name: "ProfesionalNombre",
                table: "TurnosDisponibles",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");
        }
    }
}

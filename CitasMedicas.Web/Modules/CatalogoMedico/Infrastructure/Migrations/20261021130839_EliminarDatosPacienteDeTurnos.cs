using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EliminarDatosPacienteDeTurnos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PacienteCorreo",
                table: "TurnosDisponibles");

            migrationBuilder.DropColumn(
                name: "PacienteNombre",
                table: "TurnosDisponibles");

            migrationBuilder.DropColumn(
                name: "PacienteTelefono",
                table: "TurnosDisponibles");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PacienteCorreo",
                table: "TurnosDisponibles",
                type: "nvarchar(254)",
                maxLength: 254,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PacienteNombre",
                table: "TurnosDisponibles",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PacienteTelefono",
                table: "TurnosDisponibles",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);
        }
    }
}

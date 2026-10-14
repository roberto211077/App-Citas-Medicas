using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CitasMedicas.Web.Modules.CatalogoMedico.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RegistrarMedicoYVincularTurnos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Medicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    EspecialidadId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medicos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Medicos_Especialidades_EspecialidadId",
                        column: x => x.EspecialidadId,
                        principalTable: "Especialidades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.AddColumn<int>(
                name: "MedicoId",
                table: "TurnosDisponibles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TurnosDisponibles_MedicoId",
                table: "TurnosDisponibles",
                column: "MedicoId");

            migrationBuilder.AddForeignKey(
                name: "FK_TurnosDisponibles_Medicos_MedicoId",
                table: "TurnosDisponibles",
                column: "MedicoId",
                principalTable: "Medicos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.CreateIndex(
                name: "IX_Medicos_EspecialidadId",
                table: "Medicos",
                column: "EspecialidadId");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TurnosDisponibles_Medicos_MedicoId",
                table: "TurnosDisponibles");

            migrationBuilder.DropIndex(
                name: "IX_TurnosDisponibles_MedicoId",
                table: "TurnosDisponibles");

            migrationBuilder.DropColumn(
                name: "MedicoId",
                table: "TurnosDisponibles");

            migrationBuilder.DropTable(
                name: "Medicos");
        }
    }
}

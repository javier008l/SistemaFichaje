using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaFichaje.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FichajeEventos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UsuarioExternoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    FechaHora = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    MetadatosDispositivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Geolocalizacion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    EsCorreccionManual = table.Column<bool>(type: "bit", nullable: false),
                    MotivoCorreccion = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FichajeEventos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FichajeEventos_UsuarioExternoId_FechaHora",
                table: "FichajeEventos",
                columns: new[] { "UsuarioExternoId", "FechaHora" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FichajeEventos");
        }
    }
}

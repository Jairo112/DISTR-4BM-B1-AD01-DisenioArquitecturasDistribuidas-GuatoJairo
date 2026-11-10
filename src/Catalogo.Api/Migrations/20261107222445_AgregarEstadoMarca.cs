using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Catalogo.Api.Migrations
{
    /// <inheritdoc />
    public partial class AgregarEstadoMarca : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Marca",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "ACTIVO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Marca");
        }
    }
}

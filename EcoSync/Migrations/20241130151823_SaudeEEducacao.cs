using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcoSync.Migrations
{
    /// <inheritdoc />
    public partial class SaudeEEducacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "EstruturaDeServicos",
                table: "Pontuacoes",
                newName: "Saude");

            migrationBuilder.AddColumn<double>(
                name: "Educacao",
                table: "Pontuacoes",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Educacao",
                table: "Pontuacoes");

            migrationBuilder.RenameColumn(
                name: "Saude",
                table: "Pontuacoes",
                newName: "EstruturaDeServicos");
        }
    }
}

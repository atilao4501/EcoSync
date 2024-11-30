using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace EcoSync.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pontuacoes",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AreaVerde = table.Column<double>(type: "double precision", nullable: false),
                    PoluicaoSonoraEArTransito = table.Column<double>(type: "double precision", nullable: false),
                    Educacao = table.Column<double>(type: "double precision", nullable: false),
                    Saude = table.Column<double>(type: "double precision", nullable: false),
                    DensidadePopulacional = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pontuacoes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Bairros",
                columns: table => new
                {
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Latitude = table.Column<double>(type: "double precision", nullable: false),
                    Longitude = table.Column<double>(type: "double precision", nullable: false),
                    Pontuacoesid = table.Column<int>(type: "integer", nullable: false),
                    DensidadePopulacional = table.Column<int>(type: "integer", nullable: false),
                    AreaEmKm2 = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bairros", x => x.Nome);
                    table.ForeignKey(
                        name: "FK_Bairros_Pontuacoes_Pontuacoesid",
                        column: x => x.Pontuacoesid,
                        principalTable: "Pontuacoes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bairros_Pontuacoesid",
                table: "Bairros",
                column: "Pontuacoesid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Bairros");

            migrationBuilder.DropTable(
                name: "Pontuacoes");
        }
    }
}

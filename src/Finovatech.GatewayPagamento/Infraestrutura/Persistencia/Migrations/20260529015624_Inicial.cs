using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finovatech.GatewayPagamento.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pagamentos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChaveIdempotencia = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    MoedaOrigem = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    MoedaDestino = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Valor = table.Column<decimal>(type: "numeric(18,4)", precision: 18, scale: 4, nullable: false),
                    ParceiroOrigemId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ParceiroDestinoId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Situacao = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagamentos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosOutbox",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Carga = table.Column<string>(type: "text", nullable: false),
                    CriadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Publicado = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    PublicadoEm = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosOutbox", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Pagamentos_ChaveIdempotencia",
                table: "Pagamentos",
                column: "ChaveIdempotencia",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosOutbox_Publicado",
                table: "RegistrosOutbox",
                column: "Publicado");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pagamentos");

            migrationBuilder.DropTable(
                name: "RegistrosOutbox");
        }
    }
}

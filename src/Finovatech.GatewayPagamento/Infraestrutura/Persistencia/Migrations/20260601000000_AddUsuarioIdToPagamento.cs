using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Finovatech.GatewayPagamento.Infraestrutura.Persistencia.Migrations
{
    /// <inheritdoc />
    public partial class AddUsuarioIdToPagamento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UsuarioId",
                table: "Pagamentos",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UsuarioId",
                table: "Pagamentos");
        }
    }
}

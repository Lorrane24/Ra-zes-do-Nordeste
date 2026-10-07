using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROJETOF.Migrations
{
    /// <inheritdoc />
    public partial class AddDataMovimentoEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "Data",
                table: "MovimentosEstoque",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Data",
                table: "MovimentosEstoque");
        }
    }
}

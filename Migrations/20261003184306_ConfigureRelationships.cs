using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PROJETOF.Migrations
{
    /// <inheritdoc />
    public partial class ConfigureRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProdutosUnidades_ProdutoId",
                table: "ProdutosUnidades",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_ProdutosUnidades_UnidadeId",
                table: "ProdutosUnidades",
                column: "UnidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_ClienteId",
                table: "Pedidos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedidos_UnidadeId",
                table: "Pedidos",
                column: "UnidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagamentos_PedidoId",
                table: "Pagamentos",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentosEstoque_ProdutoId",
                table: "MovimentosEstoque",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimentosEstoque_UnidadeId",
                table: "MovimentosEstoque",
                column: "UnidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensPedidos_PedidoId",
                table: "ItensPedidos",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensPedidos_ProdutoId",
                table: "ItensPedidos",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Fidelizacoes_ClienteId",
                table: "Fidelizacoes",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Estoques_ProdutoId",
                table: "Estoques",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_Estoques_UnidadeId",
                table: "Estoques",
                column: "UnidadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_UsuarioId",
                table: "Clientes",
                column: "UsuarioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Clientes_Usuarios_UsuarioId",
                table: "Clientes",
                column: "UsuarioId",
                principalTable: "Usuarios",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Estoques_Produtos_ProdutoId",
                table: "Estoques",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Estoques_Unidades_UnidadeId",
                table: "Estoques",
                column: "UnidadeId",
                principalTable: "Unidades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Fidelizacoes_Clientes_ClienteId",
                table: "Fidelizacoes",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItensPedidos_Pedidos_PedidoId",
                table: "ItensPedidos",
                column: "PedidoId",
                principalTable: "Pedidos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ItensPedidos_Produtos_ProdutoId",
                table: "ItensPedidos",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentosEstoque_Produtos_ProdutoId",
                table: "MovimentosEstoque",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimentosEstoque_Unidades_UnidadeId",
                table: "MovimentosEstoque",
                column: "UnidadeId",
                principalTable: "Unidades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pagamentos_Pedidos_PedidoId",
                table: "Pagamentos",
                column: "PedidoId",
                principalTable: "Pedidos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Clientes_ClienteId",
                table: "Pedidos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedidos_Unidades_UnidadeId",
                table: "Pedidos",
                column: "UnidadeId",
                principalTable: "Unidades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProdutosUnidades_Produtos_ProdutoId",
                table: "ProdutosUnidades",
                column: "ProdutoId",
                principalTable: "Produtos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ProdutosUnidades_Unidades_UnidadeId",
                table: "ProdutosUnidades",
                column: "UnidadeId",
                principalTable: "Unidades",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Clientes_Usuarios_UsuarioId",
                table: "Clientes");

            migrationBuilder.DropForeignKey(
                name: "FK_Estoques_Produtos_ProdutoId",
                table: "Estoques");

            migrationBuilder.DropForeignKey(
                name: "FK_Estoques_Unidades_UnidadeId",
                table: "Estoques");

            migrationBuilder.DropForeignKey(
                name: "FK_Fidelizacoes_Clientes_ClienteId",
                table: "Fidelizacoes");

            migrationBuilder.DropForeignKey(
                name: "FK_ItensPedidos_Pedidos_PedidoId",
                table: "ItensPedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_ItensPedidos_Produtos_ProdutoId",
                table: "ItensPedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimentosEstoque_Produtos_ProdutoId",
                table: "MovimentosEstoque");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimentosEstoque_Unidades_UnidadeId",
                table: "MovimentosEstoque");

            migrationBuilder.DropForeignKey(
                name: "FK_Pagamentos_Pedidos_PedidoId",
                table: "Pagamentos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Clientes_ClienteId",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedidos_Unidades_UnidadeId",
                table: "Pedidos");

            migrationBuilder.DropForeignKey(
                name: "FK_ProdutosUnidades_Produtos_ProdutoId",
                table: "ProdutosUnidades");

            migrationBuilder.DropForeignKey(
                name: "FK_ProdutosUnidades_Unidades_UnidadeId",
                table: "ProdutosUnidades");

            migrationBuilder.DropIndex(
                name: "IX_ProdutosUnidades_ProdutoId",
                table: "ProdutosUnidades");

            migrationBuilder.DropIndex(
                name: "IX_ProdutosUnidades_UnidadeId",
                table: "ProdutosUnidades");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_ClienteId",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_Pedidos_UnidadeId",
                table: "Pedidos");

            migrationBuilder.DropIndex(
                name: "IX_Pagamentos_PedidoId",
                table: "Pagamentos");

            migrationBuilder.DropIndex(
                name: "IX_MovimentosEstoque_ProdutoId",
                table: "MovimentosEstoque");

            migrationBuilder.DropIndex(
                name: "IX_MovimentosEstoque_UnidadeId",
                table: "MovimentosEstoque");

            migrationBuilder.DropIndex(
                name: "IX_ItensPedidos_PedidoId",
                table: "ItensPedidos");

            migrationBuilder.DropIndex(
                name: "IX_ItensPedidos_ProdutoId",
                table: "ItensPedidos");

            migrationBuilder.DropIndex(
                name: "IX_Fidelizacoes_ClienteId",
                table: "Fidelizacoes");

            migrationBuilder.DropIndex(
                name: "IX_Estoques_ProdutoId",
                table: "Estoques");

            migrationBuilder.DropIndex(
                name: "IX_Estoques_UnidadeId",
                table: "Estoques");

            migrationBuilder.DropIndex(
                name: "IX_Clientes_UsuarioId",
                table: "Clientes");
        }
    }
}

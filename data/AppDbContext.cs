using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Produto> Produtos { get; set; }
    public DbSet<Unidade> Unidades { get; set; }
    public DbSet<ProdutoUnidade> ProdutosUnidades { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<ItemPedido> ItensPedidos { get; set; }
    public DbSet<Estoque> Estoques { get; set; }
    public DbSet<MovimentoEstoque> MovimentosEstoque { get; set; }
    public DbSet<Pagamento> Pagamentos { get; set; }
    public DbSet<Fidelizacao> Fidelizacoes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Pedido → Cliente
        modelBuilder.Entity<Pedido>()
            .HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(p => p.ClienteId);

        // ItemPedido → Pedido
        modelBuilder.Entity<ItemPedido>()
            .HasOne<Pedido>()
            .WithMany()
            .HasForeignKey(i => i.PedidoId);

        // ItemPedido → Produto
        modelBuilder.Entity<ItemPedido>()
            .HasOne<Produto>()
            .WithMany()
            .HasForeignKey(i => i.ProdutoId);

        // Pagamento → Pedido
        modelBuilder.Entity<Pagamento>()
            .HasOne<Pedido>()
            .WithMany()
            .HasForeignKey(p => p.PedidoId);

        // Cliente → Usuario
        modelBuilder.Entity<Cliente>()
            .HasOne<Usuario>()
            .WithMany()
            .HasForeignKey(c => c.UsuarioId);

        // ProdutoUnidade → Produto
        modelBuilder.Entity<ProdutoUnidade>()
            .HasOne<Produto>()
            .WithMany()
            .HasForeignKey(pu => pu.ProdutoId);

        // ProdutoUnidade → Unidade
        modelBuilder.Entity<ProdutoUnidade>()
            .HasOne<Unidade>()
            .WithMany()
            .HasForeignKey(pu => pu.UnidadeId);

        // Estoque → Produto
        modelBuilder.Entity<Estoque>()
            .HasOne<Produto>()
            .WithMany()
            .HasForeignKey(e => e.ProdutoId);

        // Estoque → Unidade
        modelBuilder.Entity<Estoque>()
            .HasOne<Unidade>()
            .WithMany()
            .HasForeignKey(e => e.UnidadeId);

        // MovimentoEstoque → Produto
        modelBuilder.Entity<MovimentoEstoque>()
            .HasOne<Produto>()
            .WithMany()
            .HasForeignKey(m => m.ProdutoId);

        // MovimentoEstoque → Unidade
        modelBuilder.Entity<MovimentoEstoque>()
            .HasOne<Unidade>()
            .WithMany()
            .HasForeignKey(m => m.UnidadeId);

        // Fidelizacao → Cliente
        modelBuilder.Entity<Fidelizacao>()
            .HasOne<Cliente>()
            .WithMany()
            .HasForeignKey(f => f.ClienteId);

        // Pedido → Unidade
        modelBuilder.Entity<Pedido>()
            .HasOne<Unidade>()
            .WithMany()
            .HasForeignKey(p => p.UnidadeId);
    }
}
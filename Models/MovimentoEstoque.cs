public class MovimentoEstoque
{
    public int Id { get; set; }
    public int ProdutoId { get; set; }
    public int UnidadeId { get; set; }
    public int Quantidade { get; set; }
    public TipoMovimentoEstoque Tipo { get; set; }
    public DateTime Data { get; set; }
}
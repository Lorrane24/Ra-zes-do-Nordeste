public class Pagamento
{
    public int Id { get; set; }
    public int PedidoId { get; set; }
    public decimal Valor { get; set; }
    public StatusPagamento Status { get; set; }
    public string? TransacaoId { get; set; }
}
public class Pedido
{
    public int Id { get; set; }
    public int ClienteId { get; set; }
    public int UnidadeId { get; set; }
    public decimal ValorTotal { get; set; }
    public StatusPedido Status { get; set; }
    public CanalPedido Canal { get; set; }
    }
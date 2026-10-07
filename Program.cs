using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PROJETOF.Services;

var builder = WebApplication.CreateBuilder(args);

// Configuração do Banco de Dados e Serviços
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=raizes.db"));

builder.Services.AddOpenApi();
builder.Services.AddScoped<PasswordService>();

// ==========================================
// CONFIGURAÇÃO DO JWT (AUTENTICAÇÃO E AUTORIZAÇÃO)
// ==========================================

// Define a chave secreta de desenvolvimento (mínimo 32 caracteres)
var chaveSecreta = "SuaChaveSecretaSuperSeguraEDesenvolvimento123456!";
var key = Encoding.ASCII.GetBytes(chaveSecreta);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Apenas para ambiente de desenvolvimento
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = false,   // Pode ser alterado para true se configurar Issuer
        ValidateAudience = false, // Pode ser alterado para true se configurar Audience
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

// Configuração das políticas de acesso
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("ApenasAdmin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("ApenasCliente", policy => policy.RequireRole("Cliente"));
});

var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

/// =========================
// PRODUTOS
// =========================

app.MapGet("/produtos", async (AppDbContext db) =>
{
    return await db.Produtos.ToListAsync();
});

app.MapGet("/produtos/{id}", async (int id, AppDbContext db) =>
{
    var produto = await db.Produtos.FindAsync(id);

    if (produto == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(produto);
})
.RequireAuthorization("ApenasAdmin"); // <--- Atualizado para exigir a política ApenasAdmin

app.MapPost("/produtos", async (ProdutoDto dto, AppDbContext db) =>
{
    var produto = new Produto
    {
        Nome = dto.Nome,
        Preco = dto.Preco
    };

    db.Produtos.Add(produto);
    await db.SaveChangesAsync();

    return Results.Created($"/produtos/{produto.Id}", produto);
});

// =========================
// UNIDADES
// =========================

app.MapGet("/unidades", async (AppDbContext db) =>
{
    return await db.Unidades.ToListAsync();
});

app.MapPost("/unidades", async (Unidade unidade, AppDbContext db) =>
{
    db.Unidades.Add(unidade);
    await db.SaveChangesAsync();

    return Results.Created($"/unidades/{unidade.Id}", unidade);
});

// =========================
// PRODUTOS UNIDADES / CARDÁPIO
// =========================

app.MapGet("/produtos-unidades", async (AppDbContext db) =>
{
    return await db.ProdutosUnidades.ToListAsync();
});

// Endpoint de Cardápio por Unidade (Apenas produtos disponíveis)
app.MapGet("/unidades/{unidadeId}/cardapio", async (int unidadeId, AppDbContext db) =>
{
    var cardapio = await db.ProdutosUnidades
        .Where(pu => pu.UnidadeId == unidadeId && pu.Disponivel)
        .Join(
            db.Produtos,
            pu => pu.ProdutoId,
            p => p.Id,
            (pu, p) => new
            {
                p.Id,
                p.Nome,
                p.Preco
            })
        .ToListAsync();

    return Results.Ok(cardapio);
});

// =========================
// ESTOQUE
// =========================

app.MapGet("/estoques", async (AppDbContext db) =>
{
    return await db.Estoques.ToListAsync();
});

app.MapPost("/estoques", async (Estoque estoque, AppDbContext db) =>
{
    var produto = await db.Produtos.FindAsync(estoque.ProdutoId);

    if (produto == null)
    {
        return Results.BadRequest("Produto não encontrado.");
    }

    var unidade = await db.Unidades.FindAsync(estoque.UnidadeId);

    if (unidade == null)
    {
        return Results.BadRequest("Unidade não encontrada.");
    }

    if (estoque.Quantidade < 0)
    {
        return Results.BadRequest("A quantidade não pode ser negativa.");
    }

    db.Estoques.Add(estoque);
    await db.SaveChangesAsync();

    return Results.Created(
        $"/estoques/{estoque.Id}",
        estoque);
});

// =========================
// MOVIMENTAÇÕES DE ESTOQUE
// =========================

app.MapGet("/movimentos-estoque", async (AppDbContext db) =>
{
    return await db.MovimentosEstoque.ToListAsync();
});

app.MapPost("/movimentos-estoque", async (MovimentoEstoque movimento, AppDbContext db) =>
{
    var produto = await db.Produtos.FindAsync(movimento.ProdutoId);

    if (produto == null)
    {
        return Results.BadRequest("Produto não encontrado.");
    }

    var unidade = await db.Unidades.FindAsync(movimento.UnidadeId);

    if (unidade == null)
    {
        return Results.BadRequest("Unidade não encontrada.");
    }

    if (movimento.Quantidade <= 0)
    {
        return Results.BadRequest("A quantidade deve ser maior que zero.");
    }

    var estoque = await db.Estoques.FirstOrDefaultAsync(e =>
        e.ProdutoId == movimento.ProdutoId &&
        e.UnidadeId == movimento.UnidadeId);

    if (estoque == null)
    {
        return Results.BadRequest("Estoque não encontrado para este produto e unidade.");
    }

    if (movimento.Tipo == TipoMovimentoEstoque.Saida)
    {
        if (estoque.Quantidade < movimento.Quantidade)
        {
            return Results.BadRequest("Estoque insuficiente.");
        }

        estoque.Quantidade -= movimento.Quantidade;
    }
    else if (movimento.Tipo == TipoMovimentoEstoque.Entrada)
    {
        estoque.Quantidade += movimento.Quantidade;
    }

    movimento.Data = DateTime.Now;

    db.MovimentosEstoque.Add(movimento);

    await db.SaveChangesAsync();

    return Results.Created(
        $"/movimentos-estoque/{movimento.Id}",
        movimento);
});

// =========================
// USUÁRIOS
// =========================

app.MapPost("/usuarios", async (Usuario usuario, AppDbContext db, PasswordService passwordService) =>
{
    // 1. Valida se a senha foi informada
    if (string.IsNullOrEmpty(usuario.SenhaHash))
    {
        return Results.BadRequest("A senha é obrigatória.");
    }

    // 2. O operador ! (null-forgiving) garante ao compilador que 'usuario.SenhaHash' não é nulo aqui
    string senhaPura = usuario.SenhaHash;
    string hashGerado = passwordService.GerarHash(usuario, senhaPura);

    // 3. Atualiza o objeto e salva no banco
    usuario.SenhaHash = hashGerado;

    db.Usuarios.Add(usuario);
    await db.SaveChangesAsync();

    // 4. Retorna a resposta limpa sem expor o hash
    return Results.Created($"/usuarios/{usuario.Id}", new
    {
        usuario.Id,
        usuario.Nome,
        usuario.Email,
        usuario.Role
    });
});

// =========================
// CLIENTES
// =========================

app.MapGet("/clientes", async (AppDbContext db) =>
{
    return await db.Clientes.ToListAsync();
});

app.MapPost("/clientes", async (Cliente cliente, AppDbContext db) =>
{
    var usuario = await db.Usuarios.FindAsync(cliente.UsuarioId);

    if (usuario == null)
    {
        return Results.BadRequest("Usuário não encontrado.");
    }

    db.Clientes.Add(cliente);
    await db.SaveChangesAsync();

    return Results.Created($"/clientes/{cliente.Id}", cliente);
});

// =========================
// PEDIDOS
// =========================

app.MapGet("/pedidos", async (AppDbContext db) =>
{
    return await db.Pedidos.ToListAsync();
});

// Endpoint de filtro por canal (colocado ANTES de /pedidos/{id})
app.MapGet("/pedidos/canal/{canal}", async (CanalPedido canal, AppDbContext db) =>
{
    var pedidos = await db.Pedidos
        .Where(p => p.Canal == canal)
        .ToListAsync();

    return Results.Ok(pedidos);
});

app.MapGet("/pedidos/{id}", async (int id, AppDbContext db) =>
{
    var pedido = await db.Pedidos.FindAsync(id);

    if (pedido == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(pedido);
});

app.MapPost("/pedidos", async (Pedido pedido, AppDbContext db) =>
{
    var cliente = await db.Clientes.FindAsync(pedido.ClienteId);

    if (cliente == null)
    {
        return Results.BadRequest("Cliente não encontrado.");
    }

    var unidade = await db.Unidades.FindAsync(pedido.UnidadeId);

    if (unidade == null)
    {
        return Results.BadRequest("Unidade não encontrada.");
    }

    // Validação do Canal do Pedido
    if (!Enum.IsDefined(typeof(CanalPedido), pedido.Canal))
    {
        return Results.BadRequest("Canal do pedido inválido.");
    }

    pedido.Status = StatusPedido.EmPreparacao;

    db.Pedidos.Add(pedido);
    await db.SaveChangesAsync();

    return Results.Created($"/pedidos/{pedido.Id}", pedido);
});

// =========================
// ATUALIZAÇÃO DE STATUS DO PEDIDO
// =========================

app.MapPut("/pedidos/{id}/status", async (
    int id,
    StatusPedidoDto dto,
    AppDbContext db) =>
{
    var pedido = await db.Pedidos.FindAsync(id);

    if (pedido == null)
    {
        return Results.NotFound("Pedido não encontrado.");
    }

    var statusAtual = pedido.Status;
    var novoStatus = dto.Status;

    var transicaoPermitida =
        (statusAtual == StatusPedido.EmPreparacao &&
         (novoStatus == StatusPedido.Pronto ||
          novoStatus == StatusPedido.Cancelado))
        ||
        (statusAtual == StatusPedido.Pronto &&
         novoStatus == StatusPedido.Entregue);

    if (!transicaoPermitida)
    {
        return Results.BadRequest(
            "Transição de status não permitida.");
    }

    pedido.Status = novoStatus;

    await db.SaveChangesAsync();

    return Results.Ok(pedido);
});

// =========================
// ITENS DO PEDIDO
// =========================

app.MapGet("/itens-pedido", async (AppDbContext db) =>
{
    return await db.ItensPedidos.ToListAsync();
});

app.MapPost("/itens-pedido", async (ItemPedido item, AppDbContext db) =>
{
    var pedido = await db.Pedidos.FindAsync(item.PedidoId);

    if (pedido == null)
    {
        return Results.BadRequest("Pedido não encontrado.");
    }

    var produto = await db.Produtos.FindAsync(item.ProdutoId);

    if (produto == null)
    {
        return Results.BadRequest("Produto não encontrado.");
    }

    if (item.Quantidade <= 0)
    {
        return Results.BadRequest("A quantidade deve ser maior que zero.");
    }

    var produtoUnidade = await db.ProdutosUnidades.FirstOrDefaultAsync(pu =>
        pu.ProdutoId == item.ProdutoId &&
        pu.UnidadeId == pedido.UnidadeId);

    if (produtoUnidade == null || !produtoUnidade.Disponivel)
    {
        return Results.BadRequest(
            "Produto não está disponível nesta unidade.");
    }

    var estoque = await db.Estoques.FirstOrDefaultAsync(e =>
        e.ProdutoId == item.ProdutoId &&
        e.UnidadeId == pedido.UnidadeId);

    if (estoque == null)
    {
        return Results.BadRequest(
            "Estoque não encontrado para este produto e unidade.");
    }

    if (estoque.Quantidade < item.Quantidade)
    {
        return Results.BadRequest(
            "Estoque insuficiente para realizar o pedido.");
    }

    item.PrecoUnitario = produto.Preco;

    estoque.Quantidade -= item.Quantidade;

    var movimento = new MovimentoEstoque
    {
        ProdutoId = item.ProdutoId,
        UnidadeId = pedido.UnidadeId,
        Quantidade = item.Quantidade,
        Tipo = TipoMovimentoEstoque.Saida,
        Data = DateTime.Now
    };

    db.ItensPedidos.Add(item);
    db.MovimentosEstoque.Add(movimento);

    pedido.ValorTotal += item.Quantidade * item.PrecoUnitario;

    await db.SaveChangesAsync();

    return Results.Created(
        $"/itens-pedido/{item.Id}",
        item);
});

// =========================
// AUTENTICAÇÃO / LOGIN
// =========================

app.MapPost("/login", async (LoginDto loginDto, AppDbContext db, PasswordService passwordService) =>
{
    if (string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Senha))
    {
        return Results.BadRequest("Email e senha são obrigatórios.");
    }

    var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == loginDto.Email);

    if (usuario == null || string.IsNullOrEmpty(usuario.SenhaHash))
    {
        return Results.Unauthorized();
    }

    bool senhaCorreta = passwordService.VerificarSenha(usuario, usuario.SenhaHash, loginDto.Senha);

    if (!senhaCorreta)
    {
        return Results.Unauthorized();
    }

    // ==========================================
    // GERAÇÃO DO TOKEN JWT
    // ==========================================
    var chaveSecreta = "SuaChaveSecretaSuperSeguraEDesenvolvimento123456!";
    var key = Encoding.ASCII.GetBytes(chaveSecreta);

    var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
    var tokenDescriptor = new SecurityTokenDescriptor
    {
        Subject = new System.Security.Claims.ClaimsIdentity(new[]
        {
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, usuario.Email ?? ""),
            new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, usuario.Role ?? "Cliente")
        }),
        Expires = DateTime.UtcNow.AddHours(8),
        SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
    };

    var token = tokenHandler.CreateToken(tokenDescriptor);
    var tokenString = tokenHandler.WriteToken(token);

    return Results.Ok(new
    {
        Mensagem = "Login realizado com sucesso!",
        Token = tokenString,
        Usuario = new
        {
            usuario.Id,
            usuario.Nome,
            usuario.Email,
            usuario.Role
        }
    });
});

app.Run();

// =========================
// DTOs / MODELOS
// =========================

public class ProdutoDto
{
    public string? Nome { get; set; }
    public decimal Preco { get; set; }
}

public class StatusPedidoDto
{
    public StatusPedido Status { get; set; }
}

public record LoginDto(string Email, string Senha);
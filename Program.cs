    using System.Text;
    using System.Security.Claims;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IdentityModel.Tokens;
    using PROJETOF.Services;

    var builder = WebApplication.CreateBuilder(args);

    // Configuração do Banco de Dados e Serviços
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite("Data Source=raizes.db"));

    builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new Microsoft.OpenApi.OpenApiComponents();

        document.Components ??= new Microsoft.OpenApi.OpenApiComponents();

var esquemas = document.Components.SecuritySchemes;

if (esquemas is not null)
{
    esquemas["Bearer"] = new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Token obtido no endpoint /login."
    };
}
    new Microsoft.OpenApi.OpenApiSecurityScheme
    {
        Type = Microsoft.OpenApi.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Token obtido no endpoint /login."
    };
            new Microsoft.OpenApi.OpenApiSecurityScheme
            {
                Type = Microsoft.OpenApi.SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Token obtido no endpoint /login."
            };

        return Task.CompletedTask;
    });
});
    builder.Services.AddScoped<PasswordService>();

    // ==========================================
    // CONFIGURAÇÃO DO JWT (AUTENTICAÇÃO E AUTORIZAÇÃO)
    // ==========================================

    var chaveSecreta = "SuaChaveSecretaSuperSeguraEDesenvolvimento123456!";
    var key = Encoding.ASCII.GetBytes(chaveSecreta);

    builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; 
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,   
            ValidateAudience = false, 
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

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

    app.UseAuthentication();
    app.UseAuthorization();

    // =========================
    // USUÁRIOS
    // =========================

    // Endpoint para consultar quem está no banco
    app.MapGet("/usuarios", async (AppDbContext db) =>
    {
        return await db.Usuarios
            .Select(u => new
            {
                u.Id,
                u.Nome,
                u.Email,
                u.Role
            })
            .ToListAsync();
    });

    // Endpoint para CRIAR novos usuários
    app.MapPost("/usuarios", async (Usuario usuario, AppDbContext db, PasswordService passwordService) =>
    {
        // Verifica se já existe alguém com esse email
        if (await db.Usuarios.AnyAsync(u => u.Email == usuario.Email))
        {
            return Results.BadRequest("Este email já está cadastrado.");
        }

        // // Cadastros públicos sempre criam usuários comuns
        {
            usuario.Role = "Cliente";
        }

        // Verifica se a senha foi informada
        if (string.IsNullOrWhiteSpace(usuario.SenhaHash))
        {
            return Results.BadRequest("A senha é obrigatória.");
        }

        // Transforma a senha em hash antes de salvar
        usuario.SenhaHash = passwordService.GerarHash(usuario, usuario.SenhaHash);

        db.Usuarios.Add(usuario);
        await db.SaveChangesAsync();

        return Results.Created($"/usuarios/{usuario.Id}", new 
        { 
            usuario.Id, 
            usuario.Nome, 
            usuario.Email, 
            usuario.Role 
        });
    });


    // =========================
    // PRODUTOS
    // =========================

    app.MapGet("/produtos", async (AppDbContext db) =>
    {
        return await db.Produtos.ToListAsync();
    });

    app.MapGet("/produtos/{id:int}", async (int id, AppDbContext db) =>
    {
        var produto = await db.Produtos.FindAsync(id);

        if (produto == null)
        {
            return Results.NotFound("Produto não encontrado.");
        }

        return Results.Ok(produto);
    })
    .RequireAuthorization("ApenasAdmin");

    app.MapPut("/produtos/{id:int}", async (int id, ProdutoDto dto, AppDbContext db) =>
    {
        var produto = await db.Produtos.FindAsync(id);

        if (produto == null)
        {
            return Results.NotFound("Produto não encontrado.");
        }

        produto.Nome = dto.Nome;
        produto.Preco = dto.Preco;

        await db.SaveChangesAsync();

        return Results.Ok(produto);
    })
    .RequireAuthorization("ApenasAdmin");

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
    })
    .RequireAuthorization("ApenasAdmin");

    app.MapDelete("/produtos/{id:int}", async (int id, AppDbContext db) =>
    {
        var produto = await db.Produtos.FindAsync(id);

        if (produto == null)
        {
            return Results.NotFound("Produto não encontrado.");
        }

        db.Produtos.Remove(produto);
        await db.SaveChangesAsync();

        return Results.Ok("Produto excluído com sucesso.");
    })
    .RequireAuthorization("ApenasAdmin");
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
    })
    .RequireAuthorization("ApenasAdmin");
    // =========================
    // PRODUTOS UNIDADES / CARDÁPIO
    // =========================

    app.MapGet("/produtos-unidades", async (AppDbContext db) =>
    {
        return await db.ProdutosUnidades.ToListAsync();
    });

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
        return Results.Ok(await db.Estoques.ToListAsync());
    })
    .RequireAuthorization("ApenasAdmin");

    app.MapPost("/estoques", async (Estoque estoque, AppDbContext db) =>
    {
        var produto = await db.Produtos.FindAsync(estoque.ProdutoId);

        if (produto == null)
            return Results.BadRequest("Produto não encontrado.");

        var unidade = await db.Unidades.FindAsync(estoque.UnidadeId);

        if (unidade == null)
            return Results.BadRequest("Unidade não encontrada.");

        if (estoque.Quantidade < 0)
            return Results.BadRequest("A quantidade não pode ser negativa.");

        db.Estoques.Add(estoque);
        await db.SaveChangesAsync();

        return Results.Created($"/estoques/{estoque.Id}", estoque);
    })
    .RequireAuthorization("ApenasAdmin");

    // =========================
    // MOVIMENTAÇÕES DE ESTOQUE
    // =========================

    app.MapGet("/movimentos-estoque", async (AppDbContext db) =>
    {
        return Results.Ok(await db.MovimentosEstoque.ToListAsync());
    })
    .RequireAuthorization("ApenasAdmin");

    app.MapPost("/movimentos-estoque", async (MovimentoEstoque movimento, AppDbContext db) =>
    {
        var produto = await db.Produtos.FindAsync(movimento.ProdutoId);
        if (produto == null) return Results.BadRequest("Produto não encontrado.");

        var unidade = await db.Unidades.FindAsync(movimento.UnidadeId);
        if (unidade == null) return Results.BadRequest("Unidade não encontrada.");

        if (movimento.Quantidade <= 0) return Results.BadRequest("A quantidade deve ser maior que zero.");
        if (!Enum.IsDefined(typeof(TipoMovimentoEstoque), movimento.Tipo))
        return Results.BadRequest("Tipo de movimentação inválido.");

        var estoque = await db.Estoques.FirstOrDefaultAsync(e =>
            e.ProdutoId == movimento.ProdutoId &&
            e.UnidadeId == movimento.UnidadeId);

        if (estoque == null) return Results.BadRequest("Estoque não encontrado para este produto e unidade.");

        if (movimento.Tipo == TipoMovimentoEstoque.Saida)
        {
            if (estoque.Quantidade < movimento.Quantidade)
                return Results.BadRequest("Estoque insuficiente.");

            estoque.Quantidade -= movimento.Quantidade;
        }
        else if (movimento.Tipo == TipoMovimentoEstoque.Entrada)
        {
            estoque.Quantidade += movimento.Quantidade;
        }

        movimento.Data = DateTime.Now;
        db.MovimentosEstoque.Add(movimento);
        await db.SaveChangesAsync();

        return Results.Created($"/movimentos-estoque/{movimento.Id}", movimento);
    })
    .RequireAuthorization("ApenasAdmin");
    // =========================
    // CLIENTES
    // =========================


    app.MapGet("/clientes", async (AppDbContext db) =>
    {
        return Results.Ok(await db.Clientes.ToListAsync());
    })
    .RequireAuthorization("ApenasAdmin");

    app.MapPost("/clientes", async (Cliente cliente, AppDbContext db, ClaimsPrincipal usuarioAutenticado) =>
    {
        var usuarioIdClaim = usuarioAutenticado.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Results.Unauthorized();

        var ehAdmin = usuarioAutenticado.IsInRole("Admin");

        if (!ehAdmin && cliente.UsuarioId != usuarioId)
            return Results.Forbid();

        var usuario = await db.Usuarios.FindAsync(cliente.UsuarioId);

        if (usuario == null)
            return Results.BadRequest("Usuário não encontrado.");

        if (await db.Clientes.AnyAsync(c => c.UsuarioId == cliente.UsuarioId))
            return Results.BadRequest("Este usuário já possui cadastro de cliente.");

        db.Clientes.Add(cliente);
        await db.SaveChangesAsync();

        return Results.Created($"/clientes/{cliente.Id}", cliente);
    })
    .RequireAuthorization();;

    // =========================
    // FIDELIZAÇÃO
    // =========================

    app.MapGet("/fidelizacoes/{clienteId}", async (int clienteId, AppDbContext db, ClaimsPrincipal usuarioAutenticado) =>
    {
        var usuarioIdClaim = usuarioAutenticado.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Results.Unauthorized();

        var ehAdmin = usuarioAutenticado.IsInRole("Admin");

        if (!ehAdmin)
        {
            var cliente = await db.Clientes.FirstOrDefaultAsync(c =>
                c.Id == clienteId && c.UsuarioId == usuarioId);

            if (cliente == null)
                return Results.Forbid();
        }

        var fidelizacao = await db.Fidelizacoes.FirstOrDefaultAsync(f => f.ClienteId == clienteId);

        if (fidelizacao == null)
            return Results.NotFound("Fidelização não encontrada.");

        return Results.Ok(fidelizacao);
    })
    .RequireAuthorization();

    app.MapPut("/fidelizacoes/{clienteId}/adicionar-pontos", async (int clienteId, int pontos, AppDbContext db) =>
    {
        if (pontos <= 0) return Results.BadRequest("A quantidade de pontos deve ser maior que zero.");

        var fidelizacao = await db.Fidelizacoes.FirstOrDefaultAsync(f => f.ClienteId == clienteId);
        if (fidelizacao == null) return Results.NotFound("Fidelização não encontrada.");

        fidelizacao.Pontos += pontos;
        await db.SaveChangesAsync();

        return Results.Ok(fidelizacao);
        })
    .RequireAuthorization("ApenasAdmin");

    app.MapPut("/fidelizacoes/{clienteId}/resgatar", async (int clienteId, int pontos, AppDbContext db, ClaimsPrincipal usuarioAutenticado) =>
    {
        if (pontos <= 0)
            return Results.BadRequest("A quantidade de pontos deve ser maior que zero.");

        var usuarioIdClaim = usuarioAutenticado.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Results.Unauthorized();

        var ehAdmin = usuarioAutenticado.IsInRole("Admin");

        if (!ehAdmin)
        {
            var cliente = await db.Clientes.FirstOrDefaultAsync(c =>
                c.Id == clienteId && c.UsuarioId == usuarioId);

            if (cliente == null)
                return Results.Forbid();
        }

        var fidelizacao = await db.Fidelizacoes.FirstOrDefaultAsync(f => f.ClienteId == clienteId);

        if (fidelizacao == null)
            return Results.NotFound("Fidelização não encontrada.");

        if (fidelizacao.Pontos < pontos)
            return Results.BadRequest("Pontos insuficientes para realizar o resgate.");

        fidelizacao.Pontos -= pontos;
        await db.SaveChangesAsync();

        return Results.Ok(fidelizacao);
    })
    .RequireAuthorization();

    // =========================
    // PEDIDOS
    // =========================

    app.MapGet("/pedidos", async (AppDbContext db, ClaimsPrincipal usuarioAutenticado) =>
    {
        var usuarioIdClaim = usuarioAutenticado.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Results.Unauthorized();

        if (usuarioAutenticado.IsInRole("Admin"))
            return Results.Ok(await db.Pedidos.ToListAsync());

        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

        if (cliente == null)
            return Results.Ok(new List<Pedido>());

        var pedidos = await db.Pedidos
            .Where(p => p.ClienteId == cliente.Id)
            .ToListAsync();

        return Results.Ok(pedidos);
    })
    .RequireAuthorization();

    app.MapGet("/pedidos/canal/{canal}", async (CanalPedido canal, AppDbContext db, ClaimsPrincipal usuarioAutenticado) =>
    {
        if (!Enum.IsDefined(typeof(CanalPedido), canal))
            return Results.BadRequest("Canal do pedido inválido.");

        var usuarioIdClaim = usuarioAutenticado.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Results.Unauthorized();

        var consulta = db.Pedidos.Where(p => p.Canal == canal);

        if (!usuarioAutenticado.IsInRole("Admin"))
        {
            var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

            if (cliente == null)
                return Results.Ok(new List<Pedido>());

            consulta = consulta.Where(p => p.ClienteId == cliente.Id);
        }

        return Results.Ok(await consulta.ToListAsync());
    })
    .RequireAuthorization();

    app.MapGet("/pedidos/{id}", async (int id, AppDbContext db, ClaimsPrincipal usuarioAutenticado) =>
    {
        var pedido = await db.Pedidos.FindAsync(id);

        if (pedido == null)
            return Results.NotFound("Pedido não encontrado.");

        var usuarioIdClaim = usuarioAutenticado.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Results.Unauthorized();

        if (!usuarioAutenticado.IsInRole("Admin"))
        {
            var cliente = await db.Clientes.FirstOrDefaultAsync(c =>
                c.Id == pedido.ClienteId && c.UsuarioId == usuarioId);

            if (cliente == null)
                return Results.Forbid();
        }

        return Results.Ok(pedido);
    })
    .RequireAuthorization();
    app.MapPost("/pedidos", async (Pedido pedido, AppDbContext db, ClaimsPrincipal usuarioAutenticado) =>
    {
        var usuarioIdClaim = usuarioAutenticado.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Results.Unauthorized();

        var ehAdmin = usuarioAutenticado.IsInRole("Admin");

        var cliente = await db.Clientes.FindAsync(pedido.ClienteId);

        if (cliente == null)
            return Results.BadRequest("Cliente não encontrado.");

        if (!ehAdmin && cliente.UsuarioId != usuarioId)
            return Results.Forbid();

        var unidade = await db.Unidades.FindAsync(pedido.UnidadeId);

        if (unidade == null)
            return Results.BadRequest("Unidade não encontrada.");

        if (!unidade.Ativa)
            return Results.BadRequest("Esta unidade está inativa e não pode receber pedidos.");

        if (!Enum.IsDefined(typeof(CanalPedido), pedido.Canal))
            return Results.BadRequest("Canal do pedido inválido.");

        pedido.Status = StatusPedido.EmPreparacao;

        db.Pedidos.Add(pedido);
        await db.SaveChangesAsync();

        return Results.Created($"/pedidos/{pedido.Id}", pedido);
    })
    .RequireAuthorization();
    app.MapPut("/pedidos/{id}/status", async (int id, StatusPedidoDto dto, AppDbContext db) =>
    {
        var pedido = await db.Pedidos.FindAsync(id);
        if (pedido == null) return Results.NotFound("Pedido não encontrado.");

        var statusAtual = pedido.Status;
        var novoStatus = dto.Status;

        var transicaoPermitida =
            (statusAtual == StatusPedido.EmPreparacao && (novoStatus == StatusPedido.Pronto || novoStatus == StatusPedido.Cancelado)) ||
            (statusAtual == StatusPedido.Pronto && novoStatus == StatusPedido.Entregue);

        if (!transicaoPermitida)
            return Results.BadRequest("Transição de status não permitida.");

        
    if (novoStatus == StatusPedido.Cancelado)
    {
        var itens = await db.ItensPedidos
            .Where(i => i.PedidoId == pedido.Id)
            .ToListAsync();

        foreach (var item in itens)
        {
            var estoque = await db.Estoques.FirstOrDefaultAsync(e =>
                e.ProdutoId == item.ProdutoId &&
                e.UnidadeId == pedido.UnidadeId);

            if (estoque != null)
            {
                estoque.Quantidade += item.Quantidade;

                db.MovimentosEstoque.Add(new MovimentoEstoque
                {
                    ProdutoId = item.ProdutoId,
                    UnidadeId = pedido.UnidadeId,
                    Quantidade = item.Quantidade,
                    Tipo = TipoMovimentoEstoque.Entrada,
                    Data = DateTime.Now
                });
            }
        }
    }

    pedido.Status = novoStatus;
    await db.SaveChangesAsync();

        return Results.Ok(pedido);  
        })
    .RequireAuthorization("ApenasAdmin");

    // =========================
    // ITENS DO PEDIDO
    // =========================

    app.MapGet("/itens-pedido", async (AppDbContext db, ClaimsPrincipal usuarioAutenticado) =>
    {
        var usuarioIdClaim = usuarioAutenticado.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Results.Unauthorized();

        if (usuarioAutenticado.IsInRole("Admin"))
            return Results.Ok(await db.ItensPedidos.ToListAsync());

        var cliente = await db.Clientes.FirstOrDefaultAsync(c => c.UsuarioId == usuarioId);

        if (cliente == null)
            return Results.Ok(new List<ItemPedido>());

        var itens = await db.ItensPedidos
            .Where(i => db.Pedidos.Any(p => p.Id == i.PedidoId && p.ClienteId == cliente.Id))
            .ToListAsync();

        return Results.Ok(itens);
    })
    .RequireAuthorization();
    app.MapPost("/itens-pedido", async (ItemPedido item, AppDbContext db, ClaimsPrincipal usuarioAutenticado) =>
    {
        var pedido = await db.Pedidos.FindAsync(item.PedidoId);
        if (pedido == null) return Results.BadRequest("Pedido não encontrado.");

        var usuarioIdClaim = usuarioAutenticado.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Results.Unauthorized();

        var ehAdmin = usuarioAutenticado.IsInRole("Admin");

        if (!ehAdmin)
        {
            var cliente = await db.Clientes.FirstOrDefaultAsync(c =>
                c.Id == pedido.ClienteId && c.UsuarioId == usuarioId);

            if (cliente == null)
                return Results.Forbid();
        }

        if (pedido.Status != StatusPedido.EmPreparacao)
            return Results.BadRequest("Só é possível adicionar itens a pedidos em preparação.");

        var produto = await db.Produtos.FindAsync(item.ProdutoId);
        if (produto == null) return Results.BadRequest("Produto não encontrado.");

        if (item.Quantidade <= 0)
            return Results.BadRequest("A quantidade deve ser maior que zero.");

        var produtoUnidade = await db.ProdutosUnidades.FirstOrDefaultAsync(pu =>
            pu.ProdutoId == item.ProdutoId && pu.UnidadeId == pedido.UnidadeId);

        if (produtoUnidade == null || !produtoUnidade.Disponivel)
            return Results.BadRequest("Produto não está disponível nesta unidade.");

        var estoque = await db.Estoques.FirstOrDefaultAsync(e =>
            e.ProdutoId == item.ProdutoId && e.UnidadeId == pedido.UnidadeId);

        if (estoque == null)
            return Results.BadRequest("Estoque não encontrado para este produto e unidade.");

        if (estoque.Quantidade < item.Quantidade)
            return Results.BadRequest("Estoque insuficiente para realizar o pedido.");

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

        return Results.Created($"/itens-pedido/{item.Id}", item);
    })
    .RequireAuthorization();

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

        var chaveSecretaLocal = "SuaChaveSecretaSuperSeguraEDesenvolvimento123456!";
        var keyLocal = Encoding.ASCII.GetBytes(chaveSecretaLocal);

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
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(keyLocal), SecurityAlgorithms.HmacSha256Signature)
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

    // Consultar pagamentos
    app.MapGet("/pagamentos", async (
        AppDbContext db,
        ClaimsPrincipal usuarioAutenticado) =>
    {
        var usuarioIdClaim = usuarioAutenticado.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Results.Unauthorized();

        if (usuarioAutenticado.IsInRole("Admin"))
        {
            return Results.Ok(await db.Pagamentos.ToListAsync());
        }
            var pagamentos = await db.Pagamentos
            .Where(p => db.Pedidos.Any(pedido =>
                pedido.Id == p.PedidoId &&
                db.Clientes.Any(cliente =>
                    cliente.Id == pedido.ClienteId &&
                    cliente.UsuarioId == usuarioId)))
            .ToListAsync();

        return Results.Ok(pagamentos);
    });


    app.MapPost("/pagamentos", async (
        int pedidoId,
        AppDbContext db,
        ClaimsPrincipal usuarioAutenticado) =>
    {
        var pedido = await db.Pedidos.FindAsync(pedidoId);

        if (pedido == null)
            return Results.NotFound("Pedido não encontrado.");

        var usuarioIdClaim = usuarioAutenticado.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Results.Unauthorized();

        if (!usuarioAutenticado.IsInRole("Admin"))
        {
            var ehDono = await db.Clientes.AnyAsync(c =>
                c.Id == pedido.ClienteId &&
                c.UsuarioId == usuarioId);

            if (!ehDono)
                return Results.Forbid();
        }

        if (pedido.Status == StatusPedido.Cancelado)
            return Results.BadRequest("Não é possível pagar um pedido cancelado.");

        if (await db.Pagamentos.AnyAsync(p => p.PedidoId == pedidoId))
            return Results.BadRequest("Este pedido já possui um pagamento.");

        var pagamento = new Pagamento
        {
            PedidoId = pedido.Id,
            Valor = pedido.ValorTotal,
            Status = StatusPagamento.Pendente,
            TransacaoId = $"SIM-{Guid.NewGuid():N}"
        };

        db.Pagamentos.Add(pagamento);
        await db.SaveChangesAsync();

        return Results.Ok(pagamento);
    })
    .RequireAuthorization();

    app.MapPut("/pagamentos/{id}/processar", async (
        int id,
        bool aprovado,
        AppDbContext db,
        ClaimsPrincipal usuarioAutenticado) =>
    {
        var pagamento = await db.Pagamentos
            .FirstOrDefaultAsync(p => p.Id == id);

        if (pagamento == null)
            return Results.NotFound("Pagamento não encontrado.");

        var pedido = await db.Pedidos.FindAsync(pagamento.PedidoId);

        if (pedido == null)
            return Results.NotFound("Pedido não encontrado.");

        var usuarioIdClaim = usuarioAutenticado.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (!int.TryParse(usuarioIdClaim, out var usuarioId))
            return Results.Unauthorized();

        if (!usuarioAutenticado.IsInRole("Admin"))
        {
            var ehDono = await db.Clientes.AnyAsync(c =>
                c.Id == pedido.ClienteId &&
                c.UsuarioId == usuarioId);

            if (!ehDono)
                return Results.Forbid();
        }

        if (pagamento.Status != StatusPagamento.Pendente)
            return Results.BadRequest(
                "Somente pagamentos pendentes podem ser processados.");

        if (pedido.Status == StatusPedido.Cancelado)
            return Results.BadRequest(
                "Não é possível processar pagamento de pedido cancelado.");

        pagamento.Status = aprovado
            ? StatusPagamento.Aprovado
            : StatusPagamento.Recusado;

        if (aprovado)
            pedido.Status = StatusPedido.EmPreparacao;

        await db.SaveChangesAsync();

        return Results.Ok(new
        {
            mensagem = aprovado
                ? "Pagamento aprovado."
                : "Pagamento recusado.",
            pagamento,
            pedido
        });
    })
    .RequireAuthorization();

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
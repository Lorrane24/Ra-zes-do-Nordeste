# Raízes do Nordeste

Sistema de gerenciamento desenvolvido para uma rede de lanchonetes, como projeto acadêmico do curso de Análise e Desenvolvimento de Sistemas.

A aplicação tem como objetivo organizar e facilitar o gerenciamento das operações do estabelecimento, incluindo usuários, clientes, pedidos, pagamentos, estoque e fidelização.

## Tecnologias utilizadas

- **C#** — linguagem de programação.
- **.NET 10** — plataforma de desenvolvimento.
- **Entity Framework Core** — mapeamento objeto-relacional (ORM).
- **SQLite** — banco de dados.
- **JWT (JSON Web Token)** — autenticação e autorização.
- **OpenAPI** — documentação da API.

## Funcionalidades

O sistema contempla os seguintes módulos:

- Gerenciamento de usuários.
- Cadastro e gerenciamento de clientes.
- Registro e gerenciamento de pedidos.
- Controle de pagamentos.
- Programa de fidelização de clientes.
- Gerenciamento de estoque.
- Autenticação e controle de acesso por perfil.

## Perfis de acesso

O sistema trabalha com diferentes níveis de acesso:

- **Administrador:** acesso às funcionalidades administrativas.
- **Cliente:** acesso às funcionalidades destinadas aos clientes.

A autenticação utiliza JWT para controle de acesso aos recursos protegidos da API.

## Como executar o projeto

### Pré-requisitos

- .NET 10 SDK instalado.
- Visual Studio Code ou outra IDE compatível.
- Git instalado.

### 1. Clonar o repositório

```bash
git clone https://github.com/Lorrane24/Ra-zes-do-Nordeste.git
```

### 2. Acessar a pasta do projeto

```bash
cd Ra-zes-do-Nordeste
```

### 3. Restaurar as dependências

```bash
dotnet restore
```

### 4. Executar a aplicação

```bash
dotnet run
```

Após iniciar, consulte o endereço apresentado no terminal para acessar a aplicação e sua documentação da API, conforme a configuração do projeto.

## Banco de dados

O projeto utiliza SQLite para armazenamento das informações e Entity Framework Core para comunicação entre a aplicação e o banco de dados.

O banco de dados local é armazenado no arquivo `raizes.db`, que não é incluído no controle de versão.

## Testes e documentação

A documentação da API utiliza OpenAPI.

Os testes do projeto contemplam cenários positivos e negativos, incluindo validações das funcionalidades e respostas da API.

A coleção de requisições e as evidências dos testes serão disponibilizadas junto aos materiais de documentação do projeto.

## Estrutura do projeto

A aplicação está organizada em arquivos e diretórios responsáveis pelas funcionalidades e pela comunicação com o banco de dados.

Entre os principais arquivos estão:

- `Program.cs` — inicialização e configuração da aplicação.
- `data/AppDbContext.cs` — contexto de acesso ao banco de dados.

## Repositório

Código-fonte disponível em:

[GitHub — Raízes do Nordeste](https://github.com/Lorrane24/Ra-zes-do-Nordeste)

---

**Projeto acadêmico — Análise e Desenvolvimento de Sistemas.**

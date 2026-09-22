# RetroVibe

Plataforma de retrospectivas ágeis (TCC). Monorepo com front-end e back-end separados:

```
TCC/
├── front-end/       React + TypeScript + Tailwind (SPA)
├── back-end-dotnet/ .NET 10 + ASP.NET Core + EF Core, DDD + CQRS (MediatR), SQLite
├── docs/            Regras de negócio e contratos da API
└── docker-compose.yml
```

## Rodando localmente (sem Docker)

### Back-end

```bash
cd back-end-dotnet
dotnet run --project src/RetroVibe.Api   # http://localhost:3333 — migra e popula o banco automaticamente
```

### Front-end

```bash
cd front-end
npm install
cp .env.example .env   # VITE_API_URL=http://localhost:3333/api
npm run dev             # http://localhost:5173
```

### Testes automatizados

```bash
cd back-end-dotnet
dotnet test   # 18 cenários de integração (xUnit + WebApplicationFactory), banco SQLite em memória
```

## Rodando com Docker

```bash
docker compose up --build
```

- Back-end: http://localhost:3333 (roda migrations + seed automaticamente na primeira subida, o banco fica em um volume persistente)
- Front-end: http://localhost:5173

## Publicação no Railway

Veja [docs/railway.md](docs/railway.md) para publicar a interface e a API em um único serviço com SQLite persistente.

## Login

Três papéis (RF001): **Administrador**, **Facilitador** e **Participante**. Não há cadastro pela UI
para os dois primeiros — vêm do seed:

| Usuário | Senha | Papel | Squad |
|---|---|---|---|
| `marcos` | `123` | Administrador (`ADMIN`) — vê todos os squads | Phoenix |
| `joao` | `123` | Facilitador (`FACILITATOR`) — só vê o squad Cosmos | Cosmos |

**Participante**: não faz login. Um Administrador/Facilitador cria uma sessão, copia o link
"Convidar participantes" na tela da sessão (`/entrar/:id`) e compartilha — quem abre o link só digita
um nome e já entra contribuindo, sem conta (RF004).

## Arquitetura

- **Back-end**: Domain-Driven Design + CQRS. Projetos `RetroVibe.Domain` (entidades e regras de negócio), `RetroVibe.Application` (Commands/Queries via MediatR + FluentValidation), `RetroVibe.Infrastructure` (EF Core + SQLite, JWT, hashing) e `RetroVibe.Api` (controllers ASP.NET Core, autenticação/autorização). Veja [docs/regras-de-negocio.md](docs/regras-de-negocio.md) e [docs/contratos-api.md](docs/contratos-api.md).
- **Requisitos do TCC**: este projeto diverge do `REQUIREMENTS.pdf` oficial em alguns pontos (stack, features) — tudo documentado e justificado em [docs/desvios-requisitos.md](docs/desvios-requisitos.md). O back-end foi originalmente construído em Node/TypeScript e reescrito em .NET em 2026-08-25, convertendo a maior parte desse desvio em conformidade real.
- **Front-end**: React + Vite + Tailwind, organizado por módulos de domínio (`sessions`, `action-items`, `dashboard`, `catalog`, `user`), cada um com `api/queries.ts` (leituras via TanStack Query) e `api/mutations.ts` (escritas). Tem `ErrorBoundary` (RNF008), toasts de falha de rede (RNF009) e cache local do board pra sobreviver a um reload (RNF010). Não precisou de nenhuma alteração na reescrita do back-end — o contrato HTTP ficou idêntico.
- **Autenticação**: JWT Bearer (stateless), com hashing de senha via `Microsoft.AspNetCore.Identity.PasswordHasher` e autorização por policy (`Staff`/`Admin`) lendo claims do token.
- **Operação**: graceful shutdown nativo do ASP.NET Core (RNF014) e logs estruturados em JSON via Serilog, em `back-end-dotnet/logs/app.log` (RNF015).
- **Testes**: `back-end-dotnet/tests/RetroVibe.Api.IntegrationTests` — 18 cenários de integração (xUnit + `WebApplicationFactory`) rodando contra um SQLite em memória, cobrindo login, papéis, fases, pausa/retomada, edição e modo anônimo.

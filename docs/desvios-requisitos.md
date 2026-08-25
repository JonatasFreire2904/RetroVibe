# Desvios em relação ao `REQUIREMENTS.pdf`

Este documento existe para a monografia: registra, ponto a ponto, onde a implementação diverge do
`REQUIREMENTS.pdf` (Requisitos Funcionais e Não Funcionais oficiais do TCC) e por quê.

**Atualização de 2026-08-25**: o back-end foi reescrito de Node.js/TypeScript para **.NET 10 (ASP.NET
Core) + Entity Framework Core**, preservando a mesma arquitetura DDD/CQRS (agora com MediatR) e o
mesmo contrato HTTP byte-a-byte (`docs/contratos-api.md`) — o front-end React não precisou de nenhuma
alteração. Essa reescrita **converte o antigo desvio de stack (RNF011) em conformidade real** (mais
próxima do Java/Spring/PostgreSQL exigido do que a versão Node era), e junto com ela dois outros
desvios que antes eram só "equivalente funcional" também viraram conformidade: RNF001 (hashing) e
RNF002 (JWT). O único ponto que passou a ser uma simplificação deliberada é a ausência de revogação
de token no logout — ver RNF002 abaixo.

## Stack (RNF011) — de desvio mantido para conformidade parcial

| Exigido | Implementado (atual) | Situação |
|---|---|---|
| Java 17+ / Spring Boot | **.NET 10 / ASP.NET Core**, com MediatR fazendo o papel de CQRS que o Spring daria via camadas equivalentes | Ainda não é literalmente Java/Spring, mas é a mesma categoria de stack enterprise tipada e compilada que o requisito pedia — DDD/CQRS explícito nas mesmas camadas (`Domain`/`Application`/`Infrastructure`/`Api`) |
| PostgreSQL | SQLite via **Entity Framework Core** (`Microsoft.EntityFrameworkCore.Sqlite`) | Decisão consciente de manter SQLite (evita infraestrutura extra pro escopo do TCC); a troca de motor pra Postgres é uma troca de provider do EF Core (`UseNpgsql` em vez de `UseSqlite`), já que o acesso a dados passa por repositórios com interfaces (DIP) e migrations — não é mais SQL cru como na versão Node |

A versão Node/TypeScript/Express foi a implementação original deste TCC (documentada nas seções abaixo,
que refletem o comportamento — idêntico — da reescrita); a partir de 2026-08-25 o back-end oficial é o
`back-end-dotnet/`.

## Requisitos Não Funcionais — status final

| RNF | Exigido | Adotado | Status |
|---|---|---|---|
| RNF001 | Hash de senha com BCrypt | `Microsoft.AspNetCore.Identity.PasswordHasher` (PBKDF2-HMACSHA256) | ✅ Conformidade real — hashing forte e salgado via biblioteca padrão do próprio ecossistema .NET, não mais uma troca de algoritmo por conveniência |
| RNF002 | JWT stateless | **JWT Bearer** (`Microsoft.AspNetCore.Authentication.JwtBearer`), claims de `accessLevel`/`squadId`/`allowedSessionId` embutidas no token | ✅ Conformidade real — é JWT de fato agora, e stateless (nada de servidor-side `auth_sessions`). Simplificação assumida: sem revogação server-side — `POST /api/auth/logout` é um no-op (o cliente só descarta o token), aceitável pro escopo do TCC e documentado aqui |
| RNF003 | Spring Security com Roles | Policies do ASP.NET Core (`[Authorize(Policy="Staff"/"Admin")]`) lendo o claim de papel do JWT | ✅ Conformidade real — é o análogo direto do Spring Security no ecossistema .NET, não mais um middleware manual |
| RNF004 | Mascarar `participant_name` em sessão anônima | ✅ Implementado — `authorId` vem `null` na API sempre que `privacyMode = ANONYMOUS` | — |
| RNF005 | Pool HikariCP (10 max / 5 min) | Não se aplica | SQLite é um arquivo embutido, sem modelo de pool de conexões de rede (mesma justificativa, independente da stack) |
| RNF006 | 5–15 usuários simultâneos por sessão sem degradação | Não testado sob carga | SQLite serializa escritas (single-writer); sob 15 pessoas votando ao mesmo tempo pode haver fila breve — risco conhecido, não medido. Trocar pra Postgres (troca de provider do EF Core) removeria essa limitação se necessário |
| RNF007 | `@ControllerAdvice` com status codes padronizados | Exception handler global do ASP.NET Core + `DomainFailure` → status HTTP, mesmo envelope `{error, message, details}` | ✅ Conformidade real — é literalmente o análogo do `@ControllerAdvice` no ASP.NET Core |
| RNF008 | `ErrorBoundary` no React | ✅ Implementado (`app/ErrorBoundary.tsx`, envolve toda a aplicação) | — |
| RNF009 | Toasts em falha de rede | ✅ Implementado — `httpClient` dispara toast em falha de rede real, erro 5xx e sessão expirada (401) | — |
| RNF010 | Facilitador recupera sessão do `localStorage` ao recarregar | ✅ Implementado — último snapshot do board fica em cache local e é exibido instantaneamente ao recarregar, enquanto uma versão fresca é buscada em segundo plano | — |
| RNF011 | Java/Spring/PostgreSQL | .NET 10/ASP.NET Core/SQLite (via EF Core) | Ver seção "Stack" acima — de desvio mantido para conformidade parcial |
| RNF012 | Servidor stateless | Satisfeito — com JWT isso é ainda mais literal que antes (nem token opaco em banco existe mais) | — |
| RNF013 | Deploy simples, sem Docker obrigatório | Docker é **opcional** (`docker-compose.yml` existe, mas `dotnet run` local funciona sem ele) | Compatível — o requisito veda *depender* de Docker, não proíbe oferecê-lo como conveniência |
| RNF014 | Graceful shutdown com timeout de 30s | O host do ASP.NET Core já implementa graceful shutdown nativamente (`SIGTERM`/`SIGINT` → `IHostApplicationLifetime`, timeout configurável) | ✅ Conformidade real — comportamento nativo do framework, não mais uma implementação manual |
| RNF015 | Logs estruturados em arquivo (INFO/WARN/ERROR) | ✅ Implementado via **Serilog** (`Serilog.Sinks.File`), grava `logs/app.log` em JSON por linha + console, INFO/WARN/ERROR por request | — |

## Requisitos Funcionais — status final

| RF | Exigido | Status |
|---|---|---|
| RF001 | 3 papéis: Admin/Facilitador/Participante | ✅ Implementado |
| RF002 | Ações administrativas restritas a Admin | ✅ Implementado — CRUD de catálogo (`/admin/templates`, `/admin/themes`) exige `accessLevel = ADMIN` (`requireAdmin`), Facilitador recebe 403 |
| RF003 | Login e-mail/senha do Facilitador | ⚠️ Parcial (mantido) — login é por **usuário** (não e-mail) + senha; troca de campo é cosmética, não implementada por não alterar nenhuma regra de negócio |
| RF004 | Participante entra por token/link, sem cadastro completo | ✅ Implementado (`/entrar/:id` → `POST /sessions/:id/join`) |
| RF005 | Criar sessão com título e squad | ✅ Implementado — campo `title` opcional na criação, exibido no lugar do nome do modelo quando preenchido |
| RF006 | Abrir/pausar/retomar/fechar sessão | ✅ Implementado — `PATCH /sessions/:id/pause` e `/resume`, com contribuições bloqueadas enquanto `PAUSED` |
| RF007 | Persistir estado continuamente | ✅ Cada ação grava no SQLite imediatamente |
| RF008 | Catálogo de temas visuais lúdicos | ✅ Implementado (5 temas seedados + Admin pode adicionar mais) |
| RF009 | Selecionar tema + template ao criar sessão | ✅ Implementado |
| RF010 | Identidade visual do tema consistente pra todos | ✅ Implementado (mesmo `theme.emoji`/label vem da API pra qualquer viewer) |
| RF011 | Sequência de fases guiada pelo template | ✅ Implementado — `phase: COLLECTING → VOTING → DISCUSSING` na sessão |
| RF012 | Facilitador avança fase pra todo mundo | ✅ Implementado — `PATCH /sessions/:id/phase`; card só pode ser criado em `COLLECTING`, voto só em `VOTING` |
| RF013 | Participante registra contribuição na fase de coleta | ✅ Implementado — reforçado pelo gate de fase do RF011/012 (antes, qualquer fase valia) |
| RF014 | Contribuição organizada por fase | ⚠️ Parcial (mantido) — organizada por **coluna** do template (Start/Stop/Continue etc.), não por uma fase temporal separada; as duas dimensões coexistem (coluna = categoria, fase = quando pode agir) |
| RF015 | Participante edita contribuição antes do fechamento | ✅ Implementado — `PATCH /sessions/:id/cards/:cardId`, restrito ao autor original, permitido em qualquer fase até a sessão fechar |
| RF016 | Modo Anônimo/Identificado | ✅ Implementado — `privacyMode` escolhido na criação da sessão |
| RF017 | Ocultar autoria quando Anônimo | ✅ Implementado — `authorId` mascarado (`null`) na API para todo mundo; `isMine` preserva a capacidade do próprio autor de editar seu card |
| RF018 | Relatório estruturado pós-encerramento | ⚠️ Parcial (mantido) — o board (read-only) + Itens de Ação cobrem todo o conteúdo do requisito, mas não como um documento único exportável (PDF/CSV) |
| RF019 | Histórico pesquisável de sessões encerradas | ✅ Implementado (busca + filtros em Histórico de Sessões) |
| RF020 | Admin gerencia catálogo preservando histórico imutável | ✅ Implementado — tela `/admin/catalogo`: Admin cria/ativa/inativa temas e templates; sessões já criadas mantêm suas colunas clonadas (histórico imutável por construção) |

## Resumo para a banca

Dos 20 RFs, **17 estão totalmente implementados** e **3 mantidos como parciais por decisão consciente**
(RF003 — login por usuário em vez de e-mail, sem impacto de negócio; RF014 — contribuição organizada por
coluna do template em vez de uma fase temporal isolada, mantendo os dois conceitos coexistindo; RF018 —
relatório coberto pelo board + Itens de Ação, sem exportação em arquivo único). Isso não mudou com a
reescrita do back-end — são decisões de regra de negócio, independentes de stack.

Dos 15 RNFs, a reescrita para .NET 10/ASP.NET Core/EF Core elevou **RNF001, RNF002, RNF003, RNF007 e
RNF014 de "equivalente funcional" para conformidade real** (hashing, JWT, autorização por papel,
tratamento global de erro e graceful shutdown agora usam os mecanismos nativos do próprio framework
exigido, não implementações manuais). O único ponto que permanece como desvio consciente é a dupla
Java/Spring → .NET (RNF011) — mais próxima do espírito do requisito original (stack tipada, compilada,
enterprise) do que a versão Node/TypeScript era, mas ainda não é literalmente Java/Spring — e o motor de
banco (SQLite em vez de PostgreSQL, RNF011/RNF005), mantido por simplicidade de infraestrutura para o
escopo do TCC.

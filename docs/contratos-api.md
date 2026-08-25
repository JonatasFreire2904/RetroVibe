# Contratos da API — RetroVibe

REST simples (sem GraphQL). Internamente o back-end é organizado em **Commands** (escrita) e **Queries**
(leitura) seguindo CQRS, mas o contrato exposto ao front é HTTP comum: `GET` para queries, `POST`/`PATCH`
para commands. Base URL: `http://localhost:3333/api`.

Erros seguem sempre o formato:

```json
{ "error": "VALIDATION" | "NOT_FOUND" | "CONFLICT" | "FORBIDDEN" | "UNAUTHORIZED" | "INTERNAL_ERROR", "message": "string", "details": {} }
```

com status HTTP `400`, `404`, `409`, `403`, `401` ou `500` respectivamente.

## Autenticação

Todas as rotas abaixo, **exceto `/auth/login` e `/sessions/:id/join`**, exigem um header
`Authorization: Bearer <token>` obtido no login ou no join. Sem o header (ou com um token
inválido/expirado), a API responde `401`.

O token é um **JWT** assinado (`HS256`), com claims de `sub` (id do usuário), `accessLevel`, `squadId`
e `allowedSessionId` — é ele quem carrega tudo que a API precisa saber sobre quem está chamando, sem
consulta ao banco a cada request. Por ser stateless, `/auth/logout` não revoga nada no servidor: é um
no-op que só existe pra manter o contrato (`204`); a "saída" de fato é o cliente descartar o token.

| Rota | Método | Body | Retorno |
|---|---|---|---|
| `/auth/login` | POST | `{ username, password }` | `{ token, user: UserDto }` (200) — pública, ADMIN/FACILITATOR |
| `/auth/logout` | POST | — (usa o header `Authorization`) | 204 — no-op (JWT é stateless, nada a revogar) |
| `/sessions/:id/join` | POST | `{ displayName }` | `{ token, participant: {id,name,sessionId} }` (201) — pública, RF004 |

`UserDto`: `{ id, name, role, squad, avatarColor, accessLevel: "ADMIN" \| "FACILITATOR" \| "PARTICIPANT" }`

Usuários seedados para teste: `marcos` / `123` (ADMIN, squad Phoenix) e `joao` / `123` (FACILITATOR, squad Cosmos).
Um PARTICIPANT não tem usuário/senha fixos — é criado na hora pelo `/sessions/:id/join`, que só funciona
se a sessão estiver `ACTIVE` (`409` caso contrário).

### Regras de visibilidade por papel

- **ADMIN** enxerga e age sobre qualquer squad/sessão; filtros de `squadId` nas queries funcionam normalmente.
- **FACILITATOR** só enxerga e só age sobre o próprio squad. O back-end **ignora** qualquer `squadId`
  diferente do squad do usuário passado por query string, e retorna **`403 FORBIDDEN`** ao tentar
  acessar/alterar diretamente um recurso (sessão, item de ação) de outro squad — não é apenas uma
  filtragem de lista.
- **PARTICIPANT** só pode `GET /sessions/:id` (a sua) e `POST` em `/cards`, `/votes`, `/comments` dessa
  mesma sessão — todas as demais rotas (`/home`, `/sessions` sem `:id`, `/action-items`, `/dashboard`,
  `/me`, catálogo) retornam `403 FORBIDDEN` para esse papel.

## Catálogo (dados de referência) — ADMIN/FACILITATOR

| Rota | Método | Descrição |
|---|---|---|
| `/templates` | GET | Lista modelos **ativos** de retrospectiva (Start/Stop/Continue, Starfish, 4Ls, Personalizado) com suas colunas |
| `/themes` | GET | Lista temas **ativos** (Festa Junina, Natal, Dia dos Namorados, Páscoa, Sem Tema) |
| `/squads` | GET | Lista squads já usados |

`Template`: `{ id, key, label, icon, description, isCustom, active, columns: [{key,label,icon}] }`
`Theme`: `{ id, key, label, emoji, active }`

### Gestão de catálogo (RF002/RF020) — só ADMIN

| Rota | Método | Body | Retorno |
|---|---|---|---|
| `/admin/templates` | GET | — | `Template[]` — inclui inativos |
| `/admin/templates` | POST | `{ key?, label, icon, description, columns: [{key,label,icon}] }` | `Template` (201) |
| `/admin/templates/:id` | PATCH | `{ label?, icon?, description?, active?, columns? }` | `Template` |
| `/admin/themes` | GET | — | `Theme[]` — inclui inativos |
| `/admin/themes` | POST | `{ key?, label, emoji }` | `Theme` (201) |
| `/admin/themes/:id` | PATCH | `{ label?, emoji?, active? }` | `Theme` |

FACILITATOR recebe `403` em qualquer rota `/admin/*`. Inativar (`active: false`) não afeta sessões já
criadas — elas clonaram as colunas do template no momento da criação.

## Home — ADMIN/FACILITATOR

`GET /home` → `{ recentSessions: SessionSummary[], quickTemplates: Template[] }` — `recentSessions` já vem
restrito ao squad do usuário quando ele é FACILITATOR.

## Sessões de retrospectiva

| Rota | Método | Papel | Body | Retorno |
|---|---|---|---|---|
| `/sessions` | GET | ADMIN/FACILITATOR | query: `squadId?, templateId?, themeId?, search?` | `SessionSummary[]` |
| `/sessions/:id` | GET | ADMIN/FACILITATOR/**PARTICIPANT** (só a sua) | — | `SessionBoard`; `403` se for de outro squad/sessão |
| `/sessions` | POST | ADMIN/FACILITATOR | `{ title?, templateId, themeId?, squadName, privacyMode? }` | `SessionBoard` (201) — para um FACILITATOR, `squadName` é ignorado e o squad do próprio usuário é usado |
| `/sessions/:id/cards` | POST | ADMIN/FACILITATOR/**PARTICIPANT** (só a sua) | `{ columnId, text }` | `SessionCard` (201) — só na fase `COLLECTING` de uma sessão `ACTIVE` |
| `/sessions/:id/cards/:cardId` | PATCH | ADMIN/FACILITATOR/**PARTICIPANT** (autor original) | `{ text }` | `SessionBoard` — `403` se não for o autor; `409` se a sessão já estiver `COMPLETED` |
| `/sessions/:id/votes` | POST | ADMIN/FACILITATOR/**PARTICIPANT** (só a sua) | `{ cardId }` | `{ voted: boolean, votes: number }` — só na fase `VOTING` |
| `/sessions/:id/comments` | POST | ADMIN/FACILITATOR/**PARTICIPANT** (só a sua) | `{ cardId, text }` | `{ commentsCount: number }` |
| `/sessions/:id/close` | PATCH | ADMIN/FACILITATOR | `{ feedbackScore? }` | `SessionBoard` — muda status para `COMPLETED` |
| `/sessions/:id/pause` | PATCH | ADMIN/FACILITATOR | — | `SessionBoard` — status `PAUSED`, bloqueia contribuições |
| `/sessions/:id/resume` | PATCH | ADMIN/FACILITATOR | — | `SessionBoard` — volta para `ACTIVE` |
| `/sessions/:id/phase` | PATCH | ADMIN/FACILITATOR | — | `SessionBoard` — avança `COLLECTING → VOTING → DISCUSSING`; `409` se já estiver em `DISCUSSING` |

`SessionSummary`: `{ id, title, status: "ACTIVE"\|"PAUSED"\|"COMPLETED", template, theme, squad, date, participantsCount, actionItemsCount, durationMinutes }`

`SessionBoard`: igual ao summary + `phase: "COLLECTING"\|"VOTING"\|"DISCUSSING"`, `privacyMode: "ANONYMOUS"\|"IDENTIFIED"` +
`columns: [{ id, key, label, icon, cards: SessionCard[] }]`

`SessionCard`: `{ id, text, authorId: string|null, isMine, votes, commentsCount, createdAt }` — `authorId` é
sempre `null` quando `privacyMode = ANONYMOUS` (RF017/RNF004), para qualquer viewer; `isMine` diz ao
próprio autor que aquele card é dele (permite editar mesmo anônimo, sem expor a identidade a mais ninguém).

## Itens de Ação — ADMIN/FACILITATOR

| Rota | Método | Body | Retorno |
|---|---|---|---|
| `/action-items` | GET | query: `sessionId?, squadId?, templateId?, themeId?, status?` | `ActionItem[]` |
| `/action-items/sessions-summary` | GET | query: `squadId?, templateId?, themeId?` | `ActionItemSessionSummary[]` (para a barra lateral) |
| `/action-items` | POST | `{ sessionId, description, assigneeId?, dueDate? }` | `ActionItem` (201), status inicial `PLANNED`; `403` se a sessão não for do seu squad |
| `/action-items/:id` | PATCH | `{ status?, description?, assigneeId?, dueDate? }` | `ActionItem`; `403` se o item pertencer a uma sessão de outro squad |

`status` ∈ `PLANNED \| BACKLOG \| IN_PROGRESS \| DONE \| DISCARDED`

`ActionItem`: `{ id, sessionId, description, status, assignee: {id,name,avatarColor} | null, dueDate, commentsCount, createdAt }`

## Dashboard do Time — ADMIN/FACILITATOR

`GET /dashboard?squadId=` → (para FACILITATOR, `squadId` é sempre forçado ao próprio squad)

```ts
{
  averageParticipants: number;
  averageFeedbackScore: number;
  averageDurationMinutes: number;
  sessionsInPeriod: number;
  phaseAverages: { collectMinutes: number; voteMinutes: number; discussMinutes: number };
  templatesUsage: { label: string; count: number }[];
  themesUsage: { label: string; count: number }[];
  participantsSeries: { sessionLabel: string; participants: number }[];
}
```

Calculado apenas sobre sessões `COMPLETED`.

## Perfil (usuário autenticado) — ADMIN/FACILITATOR

| Rota | Método | Body | Retorno |
|---|---|---|---|
| `/me` | GET | — | `UserDto` do usuário do token |
| `/me` | PATCH | `{ name, role, squadName, avatarColor }` | `UserDto` atualizado |

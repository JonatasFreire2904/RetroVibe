# Regras de Negócio — RetroVibe

Este documento descreve as regras de domínio implementadas no back-end (camada `domain`), que valem
independentemente de qual UI ou cliente está consumindo a API.

## 1. Autenticação e autorização

O sistema tem **3 papéis de acesso** (`accessLevel`), alinhados ao RF001 do documento de requisitos:

| Papel | Como autentica | Escopo de acesso |
|---|---|---|
| **ADMIN** (Administrador) | Login usuário+senha (`POST /auth/login`) | Todos os squads, todas as telas |
| **FACILITATOR** (Facilitador) | Login usuário+senha (`POST /auth/login`) | Só o próprio squad |
| **PARTICIPANT** (Participante) | Entra por link/token (`POST /sessions/:id/join`), só com um nome | Só a **uma sessão** que entrou |

- Login de ADMIN/FACILITATOR retorna um token de sessão opaco (`Authorization: Bearer <token>`)
  válido para todas as chamadas seguintes.
- **Entrada de participante (RF004)**: qualquer pessoa com o link de uma sessão **`ACTIVE`** pode
  entrar informando só um nome de exibição — sem e-mail, sem senha, sem cadastro. Isso cria uma conta
  "convidada" (`username`/`passwordHash` nulos) presa a essa sessão via `allowedSessionId`, e emite um
  token igual ao de login normal — o resto da API não precisa de nenhum caso especial para lidar com
  participantes.
- **Regras de visibilidade, aplicadas no back-end (não só escondidas na UI)**:
  - ADMIN enxerga e age sobre qualquer squad/sessão.
  - FACILITATOR só enxerga/age sobre o **próprio squad** — em toda tela (Home, Histórico, Itens de
    Ação, Dashboard) e toda ação de gestão (criar sessão, encerrar sessão, criar/editar item de ação).
    Tentar acessar outro squad retorna `403 FORBIDDEN`, mesmo forçando outro `squadId` na requisição.
  - PARTICIPANT só pode ver e contribuir (adicionar card, votar, comentar) na **sessão exata** do seu
    token — nenhuma outra rota (Home, listar sessões, criar sessão, Itens de Ação, Dashboard, perfil)
    está disponível para esse papel; todas retornam `403 FORBIDDEN`.
- Um FACILITATOR que cria uma sessão sempre a cria no **próprio squad** — o squad informado na
  requisição é ignorado nesse caso (a UI já trava esse campo, mas a regra vale mesmo sem a UI).
  PARTICIPANT nunca pode criar sessão.
- Nem todo registro de usuário tem login: perfis usados apenas como responsáveis de itens de ação
  (assignees) podem existir sem `username`/senha, e nesse caso não conseguem autenticar.

## 2. Sessão de retrospectiva (`RetroSession`)

- Toda sessão pertence a exatamente um **Squad**, um **Modelo** (`Template`: Start/Stop/Continue,
  Starfish, 4Ls ou Personalizado) e um **Tema** (`Theme`); se nenhum tema for informado na criação,
  assume-se automaticamente o tema **"Sem Tema"**. Um **título** livre (RF005) é opcional — quando
  ausente, a UI usa o nome do modelo como título.
- Ao criar a sessão, as colunas do board são **clonadas** a partir do modelo escolhido no momento da
  criação. Alterar o catálogo de modelos depois não altera sessões já existentes.
- Toda sessão nasce com status **`ACTIVE`**, fase **`COLLECTING`**, 1 participante (quem criou) e
  privacidade **`IDENTIFIED`** (a menos que o criador escolha `ANONYMOUS` — ver seção 3).
- **Status** (RF006): `ACTIVE → PAUSED → ACTIVE → COMPLETED`. Pausar suspende toda contribuição
  (cards, votos, comentários, edição) até alguém retomar; encerrar é permitido a partir de `ACTIVE`
  ou `PAUSED`, mas não de uma sessão já `COMPLETED`.
- **Fase guiada** (RF011/012): `COLLECTING → VOTING → DISCUSSING`, avançada uma de cada vez pelo
  facilitador/admin para todo o time simultaneamente — não é possível voltar fase nem pular uma.
  Enquanto a sessão está `ACTIVE`:
  - **Adicionar card** só é permitido em `COLLECTING`.
  - **Votar** só é permitido em `VOTING`.
  - **Comentar** é permitido em qualquer fase (não é travado pela fase, apenas pelo status).
  - **Editar o próprio card** é permitido em qualquer fase, desde que a sessão não esteja `COMPLETED`.
- **Encerrar** uma sessão (`close`) muda o status para `COMPLETED`, calcula a **duração** total
  (diferença entre `createdAt` e `closedAt`) e aceita uma **nota de feedback** opcional (0 a 5).
  Sessões `COMPLETED` são somente leitura em toda a plataforma (histórico, dashboard).

## 3. Cards do board (`RetroCard`) e privacidade da sessão

- O texto do card é obrigatório e limitado a 500 caracteres.
- **Voto é um toggle por usuário**: cada usuário pode ter no máximo 1 voto por card; votar novamente
  remove o voto anterior (não existe "múltiplos votos" do mesmo usuário no mesmo card).
- **Edição (RF015)**: só o autor original pode editar o próprio card, e só antes do fechamento da
  sessão — qualquer outra pessoa (mesmo Admin) recebe `403 FORBIDDEN` ao tentar.
- Comentários incrementam um contador no card; nesta versão básica os comentários não são editáveis
  nem removíveis pela UI, apenas contabilizados.
- **Modo de privacidade (RF016/017)**: escolhido na criação da sessão — `IDENTIFIED` (padrão) ou
  `ANONYMOUS`. Quando `ANONYMOUS`, o campo de autoria (`authorId`) vem **sempre `null`** na API para
  **qualquer** viewer, inclusive Admin — a aplicação nunca expõe quem escreveu o quê. Para o próprio
  autor não perder a capacidade de editar seu card mesmo anônimo, todo card carrega também `isMine`
  (calculado no servidor a partir de quem fez a requisição), que não revela identidade a mais ninguém.

## 4. Itens de Ação (`ActionItem`)

- A descrição é obrigatória e limitada a 280 caracteres.
- Todo item nasce com status **`PLANNED`**.
- O status pode transitar **livremente** entre `PLANNED`, `BACKLOG`, `IN_PROGRESS`, `DONE` e
  `DISCARDED` — não há uma máquina de estados restritiva, refletindo a UI (dropdown que permite
  qualquer mudança a qualquer momento).
- Responsável (`assignee`) e prazo (`dueDate`) são opcionais e podem ser alterados a qualquer momento,
  independentemente do status.
- Um item de ação está sempre vinculado a uma sessão; não existem itens de ação "soltos".

## 5. Perfil de usuário

- Nome é obrigatório. Cor do avatar deve ser uma das 8 cores predefinidas na paleta da plataforma.
- Squad é opcional; ao salvar um novo nome de squad que ainda não existe, ele é **criado
  automaticamente** (find-or-create) — não existe uma tela separada de cadastro de squads.
- Um usuário pode alterar seu próprio squad livremente pela tela de Configurações — isso muda, a
  partir daí, qual squad ele enxerga caso seja FACILITATOR (não há restrição adicional sobre essa
  troca). Essa tela não existe para PARTICIPANT (conta convidada, sem perfil persistente).

## 6. Catálogo (Templates, Temas, Squads)

- **Templates** e **Temas** têm dados semeados (seed) mas também podem ser **gerenciados por um
  ADMIN** (RF002/RF020) na tela `/admin/catalogo`: criar novos, editar rótulo/ícone/emoji, e
  **inativar** (`active: false`) — nunca excluídos de fato.
- Um item inativo some das listas usadas para criar novas sessões, mas **sessões já criadas com ele
  continuam intactas**: as colunas do template são clonadas no momento da criação da sessão (ver
  seção 2), então inativar um template depois não afeta nem o histórico nem sessões em andamento —
  a imutabilidade do histórico é uma consequência direta desse design, não uma regra extra.
- Só ADMIN acessa essas rotas (`/admin/templates`, `/admin/themes`); FACILITATOR recebe `403`.
- **Squads** são criados sob demanda: tanto ao criar uma sessão quanto ao editar o perfil, se o nome
  informado não existir ainda, ele é criado automaticamente.

## 7. Dashboard do Time

- Todas as métricas agregadas (média de participantes, nota média de feedback, duração média, tempo
  médio por fase, modelos/temas mais usados, série de participantes por sessão) são calculadas
  **apenas sobre sessões `COMPLETED`** — sessões em andamento nunca distorcem os agregados.
- O filtro por squad é opcional para um ADMIN (sem filtro, agrega todos os squads); para um
  FACILITATOR o filtro é sempre forçado ao próprio squad. PARTICIPANT não acessa esta tela.
- O "tempo médio por fase" (coleta / votação / discussão) é uma quebra proporcional da duração total
  de cada sessão, registrada no momento do encerramento (`close`) — é um dado opcional por sessão.

## 8. Fluxo entre telas

- **Login** (ADMIN/FACILITATOR) → autentica e leva para a **Home**. Sem token válido, qualquer rota
  do painel redireciona para o login.
- **Home** → "Criar Sessão de Retrospectiva" (modal com squad/modelo/tema) ou "Usar este modelo"
  (1 clique, usa o squad do usuário atual e tema "Sem Tema") → ambos levam para o **board da sessão**
  recém-criada, já em modo interativo (`ACTIVE`).
- No board ativo, o facilitador vê uma caixa **"Convidar participantes"** com um link (`/entrar/:id`)
  para compartilhar, o indicador de **fase atual** com botão para avançar (Coleta → Votação →
  Discussão), e pode **pausar/retomar** ou **encerrar a sessão**. Ao encerrar, ela passa a aparecer
  no **Histórico de Sessões** e a contar para o **Dashboard do Time**.
- **Entrada de participante**: quem recebe o link vai para uma tela isolada (sem sidebar, sem acesso
  a mais nada da plataforma) — digita o nome, entra, e cai direto no board da sessão (`/participar/:id`),
  vendo a mesma fase guiada (sem poder avançá-la) e podendo contribuir de acordo com a fase atual. O
  board desse participante atualiza sozinho a cada poucos segundos para refletir contribuições de
  outras pessoas.
- **Itens de Ação** são criados a partir de uma sessão específica (painel lateral lista as sessões que
  já têm itens) e podem ser acompanhados independentemente de a sessão estar ativa ou encerrada —
  disponível só para ADMIN/FACILITATOR.

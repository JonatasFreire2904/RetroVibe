# Publicação no Railway

O `Dockerfile` da raiz compila o React e o inclui no ASP.NET Core. Interface e API
rodam no mesmo serviço e domínio; o front-end usa `/api`. O Docker Compose local
continua com seus dois serviços de desenvolvimento.

## Configuração

- Raiz do serviço: raiz deste repositório.
- Dockerfile: `Dockerfile`.
- Health check: `/health`.
- Uma réplica.
- Volume persistente montado em `/app/data`.
- `DATABASE_PATH=/app/data/retrovibe.db`.
- `Jwt__Key`: segredo aleatório exclusivo, com pelo menos 32 bytes.
- `Seed__AdminPassword`: senha aleatória exclusiva, com pelo menos 16 caracteres.
- `Seed__FacilitatorPassword`: outra senha aleatória, com pelo menos 16 caracteres.
- `ASPNETCORE_ENVIRONMENT=Production`.
- A aplicação usa a variável `PORT` fornecida pelo Railway.

As senhas são usadas na criação inicial do banco. Usuários: `marcos` (administrador)
e `joao` (facilitador). Alterar essas variáveis não redefine senhas de usuários já
existentes. Nunca versione os valores dos segredos. No Railway, a aplicação recusa
iniciar com as senhas locais ou a chave JWT padrão do repositório.

Antes de criar recursos, confira o e-mail autenticado e o ID do workspace pessoal.
Use IDs explícitos de projeto, ambiente e serviço nas operações de publicação.

## Verificação local

```bash
docker build --target test -t retrovibe-railway-tests .
docker build -t retrovibe-railway .
docker run --rm -p 3334:3333 retrovibe-railway
```

Sem as variáveis de senhas, a execução local mantém os usuários de demonstração
com senha `123`; a validação obrigatória dos segredos é ativada no ambiente Railway.
O acesso local da imagem de produção fica em `http://localhost:3334`.

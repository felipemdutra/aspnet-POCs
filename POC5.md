# POC5 — Da autenticação manual ao Keycloak com Blazor e 2FA

## Objetivo desta POC

Esta POC substitui `POC2.md`, `POC3.md` e `POC4.md`. Ela começa consolidando o que essas fases realmente entregam, separa compilação de execução, corrige a inicialização da API e apresenta uma solução mais apropriada para identidade: o Keycloak.

Ao concluir o roteiro, você consegue:

- subir o Keycloak e o banco dele com Docker Compose;
- criar um realm e um cliente OpenID Connect;
- permitir o registro de usuários;
- exigir troca de senha no primeiro acesso;
- exigir o cadastro de 2FA no Google Authenticator depois da troca de senha;
- criar um Blazor Web App com botões de login e logout;
- proteger uma área interna;
- mostrar nessa área os dados da conta autenticada;
- comprovar que os acessos seguintes exigem senha e código temporário.

Esta é uma atividade didática. O Keycloak usa `start-dev`, HTTP local e segredos de desenvolvimento. Essas escolhas não representam uma configuração de produção.

## Como ler os checklists

- `[x]` significa que a auditoria encontra evidência no código ou em uma execução já realizada.
- `[ ]` significa que ainda existe trabalho para o aluno executar ou validar.
- Build, inicialização e funcionamento são verificações diferentes. Uma aplicação pode compilar e mesmo assim não iniciar.

Os comandos `bash` deste roteiro devem ser executados no terminal Linux/macOS. Se o repositório estiver no WSL, abra o terminal da distribuição Linux — por exemplo, Ubuntu — e execute-os nele. Quando um comando for diferente no PowerShell, o roteiro apresenta a alternativa para Windows.

## Vocabulário essencial

### Compilar, iniciar e funcionar

`Build` ou compilação é a transformação do código-fonte em arquivos executáveis pelo .NET. Um build bem-sucedido prova que o compilador aceita o código. Ele não prova que configurações, banco de dados ou serviços externos estão disponíveis.

Iniciar significa criar o processo da aplicação e fazê-lo ouvir em uma porta, por exemplo `http://localhost:5291`.

Funcionar significa responder corretamente aos casos de uso. Para esta API, um exemplo é `GET /users` retornar `200 OK` depois de consultar o banco.

### Identidade, autenticação e autorização

Identidade é o conjunto de informações que representa uma pessoa ou sistema, como identificador, nome e email.

Autenticação responde: **quem é você?** Login, senha e segundo fator participam dessa confirmação.

Autorização responde: **o que você pode acessar?** Uma pessoa pode estar autenticada e ainda assim não ter permissão para uma página administrativa.

### Provedor de identidade

Um provedor de identidade, também chamado de IdP, é um sistema especializado em contas, credenciais, sessões e login. Nesta POC, o Keycloak assume esse papel.

O Blazor deixa de receber a senha. Ele redireciona o navegador para o Keycloak, recebe o resultado da autenticação e mantém uma sessão local protegida por cookie.

### OIDC, OAuth 2.0, token e claim

OpenID Connect, abreviado como OIDC, é um protocolo de autenticação construído sobre OAuth 2.0. Ele permite que uma aplicação delegue o login a um provedor de identidade.

Um token é um documento assinado digitalmente pelo provedor. Ele transporta informações verificáveis sobre uma autenticação.

Uma claim é uma afirmação dentro do token, como:

- `sub`: identificador estável do usuário no Keycloak;
- `preferred_username`: nome usado no login;
- `email`: email da conta;
- `given_name`: primeiro nome;
- `family_name`: sobrenome.

O valor `sub` é mais apropriado para vincular a identidade do Keycloak a um perfil local do que o email, porque o email pode mudar.

### Cookie de autenticação

Cookie é um pequeno valor que o navegador envia ao site em requisições posteriores. No Blazor Server desta POC, o ASP.NET Core cria um cookie protegido depois que o Keycloak conclui o login. A senha e o segredo do cliente não são colocados nesse cookie como texto aberto.

### 2FA, OTP e TOTP

2FA significa autenticação de dois fatores. Ela pede duas provas de categorias diferentes:

1. algo que você sabe: a senha;
2. algo que você possui: o celular configurado com o autenticador.

OTP significa senha de uso único. TOTP significa senha de uso único baseada em tempo. O Google Authenticator gera um código curto a partir de um segredo compartilhado e do horário atual. O código muda normalmente a cada 30 segundos.

O QR Code mostrado pelo Keycloak contém o segredo usado pelo autenticador. Não fotografe, não publique e não envie esse QR Code para outra pessoa.

### Realm e client

Realm é uma área isolada do Keycloak. Ele agrupa usuários, clientes, políticas e fluxos de autenticação. Esta POC usa o realm `poc5`.

Client é a representação de uma aplicação dentro do realm. Esta POC registra o Blazor como `user-registration-blazor`.

## Parte 1 — Auditoria consolidada das fases anteriores

### Evidência coletada em 29/08/2026

Os comandos executados na raiz do repositório apresentam o seguinte resultado:

```bash
dotnet build csharp-user-registration-poc.slnx --no-restore
```

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

O build está saudável. A execução, porém, falha:

```bash
dotnet run --no-build \
  --project src/UserRegistration.Api/UserRegistration.Api.csproj
```

```text
Unhandled exception. System.InvalidOperationException:
Missing 'ConnectionStrings:Default' in appsettings.json
```

O Docker Compose também denuncia variáveis ausentes:

```bash
docker compose -f src/UserRegistration.Api/compose.yaml config
```

```text
The "DB_USER" variable is not set. Defaulting to a blank string.
The "DB_PASSWORD" variable is not set. Defaulting to a blank string.
The "DB_NAME" variable is not set. Defaulting to a blank string.
```

Portanto, a frase correta é: **a solução compila, mas a API atual não inicia pelo caminho `dotnet run` e o Compose não possui configuração suficiente para iniciar corretamente.**

### POC2 — o que está concluído

- [x] A solução está organizada sob `src/`.
- [x] As Minimal APIs usam rotas de usuário e semântica HTTP coerente.
- [x] `POST` retorna `201 Created` e informa a localização do recurso.
- [x] `DELETE` usa `204 No Content` quando remove o usuário.
- [x] Requests e responses possuem DTOs separados.
- [x] `UserResponse` não contém senha.
- [x] Requests usam DataAnnotations para email, senha, telefone e endereço.
- [x] O domínio também protege suas invariantes.
- [x] O arquivo atual `user-registration.http` contém um caso de telefone inválido.
- [x] OpenAPI e Scalar estão configurados para Development.
- [x] ProblemDetails e tratamento central de erros estão configurados.
- [x] A entidade `User` está no projeto Domain.
- [x] Os projetos Api, Application, Domain e Infrastructure existem.
- [x] A API chama um serviço da Application em vez de criar e alterar `User` diretamente.

As antigas tarefas 04.1, 07 e 08 da POC2 deixam de ser pendentes. O código atual contém `IUserStore.Update`, `UserService`, `UserInput` e endpoints dependentes de `IUserService`.

### POC3 — o que está concluído

- [x] O cenário manual de telefone inválido foi adicionado.
- [x] `IUserStore` possui atualização explícita.
- [x] `InMemoryUserStore` e `EfUserStore` implementam atualização.
- [x] `UserService` orquestra criação, busca, listagem, atualização e remoção.
- [x] O endpoint não chama `User.Create` nem os métodos `Change*` diretamente.
- [x] Application não referencia Infrastructure.
- [x] Domain não referencia ASP.NET Core ou EF Core.
- [x] A refatoração preservou historicamente os contratos HTTP testados naquela fase.

A validação HTTP antiga não vale como validação do commit atual. Depois dela, o projeto troca armazenamento em memória/SQLite por PostgreSQL. Uma alteração desse tamanho exige um novo smoke test.

### POC4 — o que está concluído

- [x] O aluno adicionou defesas para as tarefas arquiteturais.
- [x] O caso de telefone inválido está no arquivo `.http`.
- [x] O serviço de Application está registrado na injeção de dependência.
- [x] Os endpoints estão mais finos.
- [x] O pacote `Microsoft.OpenApi` não está mais na versão vulnerável `2.0.0`.
- [x] O build atual não emite `NU1903`.

O checklist final de `POC4.md` ficou desatualizado: ele ainda marcava como pendentes itens que o código já implementava. Também apontava para `UserApplicationService.cs`, mas o tipo atual se chama `UserService` e implementa `IUserService`.

### O que não está concluído

- [ ] A API inicia com `dotnet run` sem configuração manual escondida.
- [ ] O Compose falha cedo quando as variáveis obrigatórias estão ausentes.
- [ ] Existe um `.env.example` seguro que ensina quais nomes configurar.
- [ ] O container da API espera o PostgreSQL ficar pronto.
- [ ] A migration existente é aplicada antes do primeiro acesso à tabela `Users`.
- [ ] O Dockerfile deixa de configurar uma connection string SQLite incompatível com Npgsql.
- [ ] Os pacotes SQLite remanescentes são removidos.
- [ ] Comentários que ainda dizem “SQLite” são corrigidos.
- [ ] O fluxo HTTP completo é revalidado depois da migração para PostgreSQL.
- [ ] Existe teste automatizado mínimo contra regressão. As POCs anteriores declaravam testes fora de escopo; portanto, isso não era uma entrega descumprida, mas continua sendo uma lacuna técnica.
- [ ] A aplicação deixa de armazenar senhas em texto aberto.
- [ ] Cadastro, login, logout e segundo fator são delegados ao Keycloak.
- [ ] Existe uma interface Blazor com área protegida e dados da conta.

## Parte 2 — Por que a API atual não roda

### Causa imediata

`Program.cs` executa esta verificação antes de construir a aplicação:

```csharp
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException(
        "Missing 'ConnectionStrings:Default' in appsettings.json");
```

`appsettings.json` não possui `ConnectionStrings:Default`. O valor aparece apenas no serviço `api` do Compose. Quando você usa `dotnet run`, o Compose não participa do processo e a variável não existe. A exceção é lançada antes de o Kestrel abrir a porta 5291.

### Problemas adicionais do Compose

O arquivo atual usa `DB_NAME`, `DB_USER` e `DB_PASSWORD`, mas não fornece `.env.example`. Como `.env` está ignorado pelo Git e não existe no repositório, o Compose substitui as variáveis por texto vazio.

Mesmo com as variáveis corretas, ainda faltam duas garantias:

1. o serviço da API pode tentar acessar o banco antes de o PostgreSQL aceitar conexões;
2. a migration `20260815162136_InitialCreate` existe, mas nenhum comando ou código a aplica.

Sem a migration, o processo pode iniciar, porém o primeiro endpoint que consulta usuários falha porque a tabela `Users` não existe. Isso é diferente da falha imediata da connection string, mas também impede o funcionamento da API.

### Resíduos da migração SQLite → PostgreSQL

O projeto usa `UseNpgsql`, mas ainda contém:

- `SQLitePCLRaw.lib.e_sqlite3` em projetos `.csproj`;
- comentários sobre SQLite em `EfUserStore` e `UserDbContext`;
- diretório, volume e connection string SQLite no Dockerfile.

O Compose sobrescreve a connection string do Dockerfile. Fora do Compose, porém, a imagem recebe `Data Source=/app/data/users.db`, que não é uma connection string PostgreSQL válida.

## Parte 3 — Como consertar a execução atual

Execute esta parte antes do Keycloak. Ela fecha a regressão introduzida pela migração para PostgreSQL.

### Tarefa 1 — Documentar as variáveis

Crie `.env.example` na raiz:

```dotenv
DB_NAME=user_registration
DB_USER=user_registration_app
DB_PASSWORD=[REDACTED_SECRET]

KC_BOOTSTRAP_ADMIN_USERNAME=admin-local
KC_BOOTSTRAP_ADMIN_PASSWORD=[REDACTED_SECRET]
KEYCLOAK_DB_NAME=keycloak
KEYCLOAK_DB_USER=keycloak_app
KEYCLOAK_DB_PASSWORD=[REDACTED_SECRET]
```

`[REDACTED_SECRET]` é um marcador, não uma senha para usar. Copie o arquivo e substitua cada marcador por um segredo local diferente:

Linux/macOS:

```bash
cp .env.example .env
```

Windows PowerShell:

```powershell
Copy-Item .env.example .env
```

Não faça commit de `.env`. O arquivo já está no `.gitignore` porque contém segredos.

### Tarefa 2 — Fazer o Compose falhar cedo e esperar o banco

Substitua `src/UserRegistration.Api/compose.yaml` por:

```yaml
services:
  db:
    image: postgres:18
    environment:
      POSTGRES_DB: ${DB_NAME:?defina DB_NAME no arquivo .env}
      POSTGRES_USER: ${DB_USER:?defina DB_USER no arquivo .env}
      POSTGRES_PASSWORD: ${DB_PASSWORD:?defina DB_PASSWORD no arquivo .env}
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U $$POSTGRES_USER -d $$POSTGRES_DB"]
      interval: 5s
      timeout: 5s
      retries: 12
    volumes:
      - api-db-data:/var/lib/postgresql

  api:
    build:
      context: ../..
      dockerfile: src/UserRegistration.Api/Dockerfile
    environment:
      ASPNETCORE_URLS: http://+:5291
      ConnectionStrings__Default: >-
        Host=db;Port=5432;Database=${DB_NAME};Username=${DB_USER};Password=${DB_PASSWORD}
    depends_on:
      db:
        condition: service_healthy
    ports:
      - "127.0.0.1:5291:5291"

volumes:
  api-db-data:
```

`${VAR:?mensagem}` significa “esta variável é obrigatória”. Em vez de aceitar valor vazio e falhar mais tarde, o Compose interrompe a leitura e explica o que está faltando.

`healthcheck` é uma verificação periódica de saúde. `pg_isready` pergunta ao PostgreSQL se ele já aceita conexões. `depends_on` faz a API aguardar esse estado.

### Tarefa 3 — Aplicar a migration no início da POC

Depois de `var app = builder.Build();` em `Program.cs`, adicione:

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    db.Database.Migrate();
}
```

`Migrate()` consulta quais migrations já foram aplicadas e executa somente as pendentes.

```csharp
// ponytail: migration no startup serve para uma única instância local;
// use um job exclusivo de migration antes de escalar ou publicar em produção.
```

Não combine `EnsureCreated()` com migrations. `EnsureCreated()` cria o schema diretamente e ignora o histórico de migrations.

### Tarefa 4 — Remover os resíduos de SQLite

Execute:

```bash
dotnet remove src/UserRegistration.Api/UserRegistration.Api.csproj \
  package SQLitePCLRaw.lib.e_sqlite3

dotnet remove src/UserRegistration.Infrastructure/UserRegistration.Infrastructure.csproj \
  package SQLitePCLRaw.lib.e_sqlite3
```

No Dockerfile, remova:

- `RUN mkdir -p /app/data ...`;
- `VOLUME ["/app/data"]`;
- `ENV ConnectionStrings__Default="Data Source=/app/data/users.db"`;
- comentários que descrevem persistência SQLite.

Atualize também os comentários de `EfUserStore.cs` e `UserDbContext.cs` para dizer PostgreSQL ou apenas banco relacional.

### Tarefa 5 — Validar a API reparada

Primeiro prove que todas as variáveis existem sem imprimir seus valores:

```bash
docker compose --env-file .env \
  -f src/UserRegistration.Api/compose.yaml config --quiet
```

Depois suba os serviços:

```bash
docker compose --env-file .env \
  -f src/UserRegistration.Api/compose.yaml up --build -d
```

Confira estado e logs:

```bash
docker compose --env-file .env \
  -f src/UserRegistration.Api/compose.yaml ps

docker compose --env-file .env \
  -f src/UserRegistration.Api/compose.yaml logs api
```

Teste:

```bash
curl -i http://localhost:5291/users
curl -i http://localhost:5291/users/999
curl -i http://localhost:5291/openapi/v1.json
curl -i http://localhost:5291/scalar
```

Critérios de aceite:

- [ ] `docker compose config --quiet` termina sem aviso de variável vazia.
- [ ] `db` fica `healthy`.
- [ ] `api` fica em execução.
- [ ] A migration aparece na tabela `__EFMigrationsHistory`.
- [ ] `GET /users` retorna `200`.
- [ ] Usuário inexistente retorna `404`.
- [ ] Payload inválido retorna `400`.
- [ ] OpenAPI e Scalar abrem em Development.

## Parte 4 — Por que substituir o cadastro manual pelo Keycloak

O projeto atual demonstra como um cadastro aparentemente simples cresce rapidamente. Para armazenar senha com responsabilidade, ainda seria necessário implementar ou manter:

- hash de senha com algoritmo adequado;
- sal, parâmetros de custo e atualização do hash;
- política de senha;
- troca e recuperação de senha;
- verificação de email;
- bloqueio e proteção contra tentativas repetidas;
- sessões, cookies e revogação;
- segundo fator;
- códigos de recuperação;
- auditoria de eventos de segurança;
- telas e mensagens seguras;
- correções contínuas quando protocolos e ameaças evoluem.

O código atual armazena `User.Password` diretamente na tabela. Tirar a senha das responses é correto, mas não protege o valor no banco. Se o banco vazar, as credenciais vazam junto.

Keycloak já implementa identidade, armazenamento protegido de credenciais, OIDC, sessões, políticas, ações obrigatórias e OTP. A aplicação continua responsável pelos dados de negócio, como endereço ou preferências, mas deixa de ser responsável pela senha.

### Arquitetura-alvo

```text
Navegador
   |
   | abre /login
   v
Blazor Web App ------ redireciona ------> Keycloak
   ^                                      |  senha
   |                                      |  troca de senha
   | recebe autorização                  |  cadastro do OTP
   +--------------------------------------+
   |
   | cookie local autenticado
   v
/conta protegida
```

O formulário que recebe login, senha e OTP pertence ao Keycloak. O Blazor oferece os controles “Entrar” e “Sair”, mas não cria uma cópia insegura do formulário de senha.

## Parte 5 — Subir o Keycloak com Docker Compose

### Tarefa adicional 1 — Criar `keycloak.compose.yaml`

Crie este arquivo na raiz do repositório:

```yaml
services:
  keycloak-db:
    image: postgres:18
    environment:
      POSTGRES_DB: ${KEYCLOAK_DB_NAME:?defina KEYCLOAK_DB_NAME no arquivo .env}
      POSTGRES_USER: ${KEYCLOAK_DB_USER:?defina KEYCLOAK_DB_USER no arquivo .env}
      POSTGRES_PASSWORD: ${KEYCLOAK_DB_PASSWORD:?defina KEYCLOAK_DB_PASSWORD no arquivo .env}
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U $$POSTGRES_USER -d $$POSTGRES_DB"]
      interval: 5s
      timeout: 5s
      retries: 12
    volumes:
      - keycloak-db-data:/var/lib/postgresql

  keycloak:
    image: quay.io/keycloak/keycloak:26.7.2
    command: start-dev
    environment:
      KC_BOOTSTRAP_ADMIN_USERNAME: ${KC_BOOTSTRAP_ADMIN_USERNAME:?defina KC_BOOTSTRAP_ADMIN_USERNAME no arquivo .env}
      KC_BOOTSTRAP_ADMIN_PASSWORD: ${KC_BOOTSTRAP_ADMIN_PASSWORD:?defina KC_BOOTSTRAP_ADMIN_PASSWORD no arquivo .env}
      KC_DB: postgres
      KC_DB_URL: jdbc:postgresql://keycloak-db:5432/${KEYCLOAK_DB_NAME:?defina KEYCLOAK_DB_NAME no arquivo .env}
      KC_DB_USERNAME: ${KEYCLOAK_DB_USER:?defina KEYCLOAK_DB_USER no arquivo .env}
      KC_DB_PASSWORD: ${KEYCLOAK_DB_PASSWORD:?defina KEYCLOAK_DB_PASSWORD no arquivo .env}
    depends_on:
      keycloak-db:
        condition: service_healthy
    ports:
      - "127.0.0.1:8080:8080"
    mem_limit: 1g

volumes:
  keycloak-db-data:
```

A versão fica fixada em `26.7.2`. Evite `latest`: ela pode baixar uma versão diferente amanhã e alterar telas ou comportamento no meio do exercício.

`start-dev` simplifica TLS e hostname para desenvolvimento local. Não use esse comando em produção.

### Tarefa adicional 2 — Iniciar e verificar

```bash
docker compose --env-file .env \
  -f keycloak.compose.yaml config --quiet

docker compose --env-file .env \
  -f keycloak.compose.yaml up -d

docker compose --env-file .env \
  -f keycloak.compose.yaml ps

docker compose --env-file .env \
  -f keycloak.compose.yaml logs -f keycloak
```

Quando o log informar que o Keycloak iniciou, interrompa apenas a visualização dos logs com `Ctrl+C`. Isso não para os containers.

Abra:

- console: <http://localhost:8080/admin/>;
- discovery do realm master: <http://localhost:8080/realms/master/.well-known/openid-configuration>.

Discovery é um documento JSON que publica os endpoints OIDC. Se ele não abre, não avance para o Blazor.

Critérios de aceite:

- [ ] `keycloak-db` está saudável.
- [ ] `keycloak` permanece em execução.
- [ ] O discovery retorna JSON.
- [ ] O console administrativo abre.
- [ ] O login administrativo aceita os valores definidos pelo aluno em `.env`.

## Parte 6 — Configurar o realm

### Tarefa adicional 3 — Criar o realm `poc5`

1. Entre no console administrativo.
2. No seletor de realm, abra `Create realm`.
3. Em `Realm name`, informe `poc5`.
4. Mantenha `Enabled` ligado.
5. Clique em `Create`.
6. Confirme que o canto superior mostra `poc5`, não `master`.

O realm `master` administra o próprio Keycloak. Não cadastre usuários da aplicação nele.

### Tarefa adicional 4 — Habilitar autorregistro

1. Abra `Realm settings`.
2. Abra a aba `Login`.
3. Ligue `User registration`.
4. Ligue `Login with email` se quiser permitir login por email.
5. Mantenha emails duplicados desabilitados.
6. Salve.

Depois dessa configuração, a página de login do realm apresenta o link `Register`.

### Tarefa adicional 5 — Configurar política de senha

1. Abra `Authentication`.
2. Abra `Policies` e depois `Password policy`.
3. Adicione pelo menos:
   - tamanho mínimo de 12 caracteres;
   - uma letra maiúscula;
   - uma letra minúscula;
   - um número;
   - um caractere especial;
   - senha diferente do username;
   - senha diferente do email.
4. Salve.

Política de senha não substitui 2FA. Ela melhora o primeiro fator; o autenticador fornece o segundo.

## Parte 7 — Configurar troca de senha e Google Authenticator

### Tarefa adicional 6 — Configurar TOTP

1. No realm `poc5`, abra `Authentication`.
2. Abra `Policies`.
3. Abra `OTP policy`.
4. Selecione `Time based`.
5. Para máxima compatibilidade com Google Authenticator, use:
   - algoritmo `SHA1`;
   - 6 dígitos;
   - período de 30 segundos;
   - janela de tolerância 1.
6. Salve.

O SHA1 aqui participa de HMAC-TOTP; ele não é usado para armazenar a senha. Não altere os valores aleatoriamente: o Keycloak e o aplicativo autenticador precisam usar a mesma política.

### Tarefa adicional 7 — Configurar ações obrigatórias e sua ordem

1. Abra `Authentication`.
2. Abra `Required actions`.
3. Confirme que `Update Password` está habilitada.
4. Ligue `Set as default action` para `Update Password`.
5. Confirme que `Configure OTP` está habilitada.
6. Ligue `Set as default action` para `Configure OTP`.
7. Use os controles de prioridade da tela para colocar `Update Password` **antes** de `Configure OTP`.
8. Salve quando a interface solicitar.

A prioridade importa. A configuração padrão costuma colocar `Configure OTP` antes de `Update Password`. O objetivo desta POC exige a ordem contrária:

```text
senha inicial -> Update Password -> Configure OTP -> aplicação
```

As ações padrão são atribuídas a usuários novos. Crie o usuário de teste somente depois de terminar esta configuração. Para um usuário já existente, abra `Users`, selecione-o e adicione manualmente as duas ações em `Required user actions`.

### Tarefa adicional 8 — Entender os dois caminhos de criação

Autorregistro:

1. A pessoa clica em `Register`.
2. Ela informa perfil e uma senha inicial.
3. Como `Update Password` é uma ação padrão, o Keycloak exige uma nova senha no primeiro fluxo de acesso.
4. Em seguida, `Configure OTP` mostra o QR Code.

Criação administrativa com senha temporária:

1. O administrador abre `Users` e `Add user`.
2. Preenche username, email, nome e sobrenome.
3. Salva.
4. Abre `Credentials`.
5. Define uma senha inicial e mantém `Temporary` ligado.
6. Confirma que `Update Password` e `Configure OTP` aparecem em `Required user actions`.

O segundo caminho é a prova mais explícita de “senha temporária no primeiro acesso”. Os dois caminhos devem terminar com OTP configurado.

## Parte 8 — Registrar o Blazor como cliente OIDC

### Tarefa adicional 9 — Criar o client

No realm `poc5`:

1. Abra `Clients`.
2. Clique em `Create client`.
3. Mantenha `Client type` como `OpenID Connect`.
4. Informe `Client ID`: `user-registration-blazor`.
5. Clique em `Next`.
6. Ligue `Client authentication`.
7. Ligue somente `Standard flow`.
8. Desligue `Direct access grants`.
9. Desligue `Implicit flow`, `Service accounts` e `Authorization` se aparecerem.
10. Salve.

`Client authentication` cria um cliente confidencial: além do usuário, o próprio Blazor prova sua identidade com um segredo. O segredo fica no servidor Blazor, nunca no navegador.

`Standard flow` usa Authorization Code. O navegador recebe um código curto, e o servidor troca esse código por tokens usando um canal de servidor.

### Tarefa adicional 10 — Configurar URLs exatas

Na aba `Settings` do client, informe:

```text
Root URL: https://localhost:7188
Home URL: https://localhost:7188/
Valid redirect URIs: https://localhost:7188/signin-oidc
Valid post logout redirect URIs: https://localhost:7188/signout-callback-oidc
Web origins: https://localhost:7188
```

Salve.

Não use `*`. Redirect URI é um destino permitido depois do login. Um curinga amplo permite redirecionamentos que você não pretendia autorizar.

### Tarefa adicional 11 — Obter o segredo sem publicá-lo

1. Abra a aba `Credentials` do client.
2. Localize `Client secret`.
3. Copie o valor apenas quando chegar à etapa de `user-secrets`.
4. Não cole o segredo em `POC5.md`, `.env.example`, `appsettings.json`, issue, commit ou captura de tela.

## Parte 9 — Criar o Blazor Web App

Esta etapa usa Blazor com interatividade Server. O código executa no servidor ASP.NET Core, o que permite manter o segredo OIDC fora do navegador.

### Tarefa adicional 12 — Criar o projeto

Na raiz:

```bash
dotnet new blazor \
  --name UserRegistration.Web \
  --output src/UserRegistration.Web \
  --interactivity Server \
  --auth None

dotnet sln csharp-user-registration-poc.slnx add \
  src/UserRegistration.Web/UserRegistration.Web.csproj

dotnet add src/UserRegistration.Web/UserRegistration.Web.csproj \
  package Microsoft.AspNetCore.Authentication.OpenIdConnect \
  --version 10.0.11
```

`--auth None` é intencional. Não queremos ASP.NET Core Identity e Keycloak competindo pelo mesmo login.

### Tarefa adicional 13 — Fixar as portas locais

Em `src/UserRegistration.Web/Properties/launchSettings.json`, configure:

```json
"applicationUrl": "https://localhost:7188;http://localhost:5188"
```

Essa porta precisa corresponder às URLs cadastradas no client do Keycloak.

### Tarefa adicional 14 — Configurar dados públicos do OIDC

Em `src/UserRegistration.Web/appsettings.json`, adicione antes de `AllowedHosts`:

```json
"Authentication": {
  "Keycloak": {
    "Authority": "http://localhost:8080/realms/poc5",
    "ClientId": "user-registration-blazor"
  }
},
```

Authority é o endereço do emissor OIDC. O ASP.NET Core acrescenta `/.well-known/openid-configuration` para descobrir endpoints e chaves públicas.

### Tarefa adicional 15 — Guardar o client secret

Inicialize o armazenamento local de segredos:

```bash
dotnet user-secrets init \
  --project src/UserRegistration.Web/UserRegistration.Web.csproj
```

Substitua `[REDACTED_SECRET]` pelo valor copiado da aba `Credentials` e execute localmente:

```bash
dotnet user-secrets set \
  "Authentication:Keycloak:ClientSecret" \
  "[REDACTED_SECRET]" \
  --project src/UserRegistration.Web/UserRegistration.Web.csproj
```

User Secrets guarda configuração de desenvolvimento fora do repositório. Ele não é um cofre de produção, mas evita commit acidental durante a POC.

### Tarefa adicional 16 — Configurar autenticação no `Program.cs`

Substitua `src/UserRegistration.Web/Program.cs` pelo conteúdo abaixo:

```csharp
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using UserRegistration.Web.Components;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddAuthorization();

var keycloak = builder.Configuration.GetSection("Authentication:Keycloak");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    })
    .AddCookie()
    .AddOpenIdConnect(options =>
    {
        options.Authority = keycloak["Authority"];
        options.ClientId = keycloak["ClientId"];
        options.ClientSecret = keycloak["ClientSecret"];
        options.ResponseType = OpenIdConnectResponseType.Code;
        options.SaveTokens = true;
        options.GetClaimsFromUserInfoEndpoint = true;
        options.MapInboundClaims = false;
        options.Scope.Add("email");
        options.TokenValidationParameters.NameClaimType = "preferred_username";
        options.RequireHttpsMetadata = !builder.Environment.IsDevelopment();
    });

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute(
    "/not-found",
    createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapGet("/login", () => TypedResults.Challenge(
    new AuthenticationProperties { RedirectUri = "/conta" },
    [OpenIdConnectDefaults.AuthenticationScheme]));

app.MapPost("/logout", () => TypedResults.SignOut(
        new AuthenticationProperties { RedirectUri = "/" },
        [
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme
        ]))
    .RequireAuthorization()
    .WithMetadata(new RequireAntiforgeryTokenAttribute(true));

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

Pontos importantes:

- cookie mantém a sessão da aplicação;
- OIDC realiza desafio de login e logout no Keycloak;
- `Code` seleciona Authorization Code Flow;
- `MapInboundClaims = false` mantém os nomes originais das claims;
- HTTP para o Authority só é aceito em Development;
- logout usa `POST` e token antifalsificação para evitar requisição forjada.

### Tarefa adicional 17 — Disponibilizar os componentes de autorização

Adicione a `src/UserRegistration.Web/Components/_Imports.razor`:

```razor
@using Microsoft.AspNetCore.Authorization
@using Microsoft.AspNetCore.Components.Authorization
```

### Tarefa adicional 18 — Usar roteamento consciente de autorização

Substitua `src/UserRegistration.Web/Components/Routes.razor` por:

```razor
<Router AppAssembly="typeof(Program).Assembly"
        NotFoundPage="typeof(Pages.NotFound)">
    <Found Context="routeData">
        <AuthorizeRouteView RouteData="routeData"
                            DefaultLayout="typeof(Layout.MainLayout)">
            <NotAuthorized>
                <p>Esta página exige autenticação.</p>
                <a href="/login">Entrar com Keycloak</a>
            </NotAuthorized>
        </AuthorizeRouteView>
        <FocusOnNavigate RouteData="routeData" Selector="h1" />
    </Found>
</Router>
```

`AuthorizeRouteView` verifica os atributos de autorização da página antes de exibi-la.

### Tarefa adicional 19 — Criar os controles de login e logout

Crie `src/UserRegistration.Web/Components/Layout/LoginDisplay.razor`:

```razor
<AuthorizeView>
    <Authorized Context="authenticationState">
        <a href="/conta">@authenticationState.User.Identity?.Name</a>
        <form method="post" action="/logout">
            <AntiforgeryToken />
            <button class="btn btn-link" type="submit">Sair</button>
        </form>
    </Authorized>
    <NotAuthorized>
        <a href="/login">Entrar</a>
    </NotAuthorized>
</AuthorizeView>
```

Em `MainLayout.razor`, substitua o link `About` do bloco `top-row` por:

```razor
<LoginDisplay />
```

O botão Entrar navega para o endpoint `/login`, que inicia o desafio OIDC. O botão Sair encerra o cookie local e a sessão OIDC do Keycloak.

### Tarefa adicional 20 — Criar a área interna protegida

Crie `src/UserRegistration.Web/Components/Pages/Account.razor`:

```razor
@page "/conta"
@attribute [Authorize]

<PageTitle>Minha conta</PageTitle>

<h1>Minha conta</h1>

<AuthorizeView>
    <Authorized Context="authenticationState">
        <p>
            <strong>Usuário:</strong>
            @authenticationState.User.Identity?.Name
        </p>
        <p>
            <strong>Autenticado:</strong>
            @authenticationState.User.Identity?.IsAuthenticated
        </p>

        <h2>Dados recebidos do Keycloak</h2>
        <dl>
            @foreach (var claim in authenticationState.User.Claims
                .OrderBy(claim => claim.Type))
            {
                <dt>@claim.Type</dt>
                <dd>@claim.Value</dd>
            }
        </dl>
    </Authorized>
</AuthorizeView>
```

`[Authorize]` transforma `/conta` em área interna. Um visitante anônimo não recebe o conteúdo protegido.

Mostrar todas as claims é útil nesta POC para estudo. Em uma aplicação real, mostre somente os campos necessários e evite expor tokens completos.

Adicione também a `NavMenu.razor`:

```razor
<div class="nav-item px-3">
    <NavLink class="nav-link" href="conta">
        Minha conta
    </NavLink>
</div>
```

### Tarefa adicional 21 — Compilar antes de executar

```bash
dotnet build csharp-user-registration-poc.slnx
```

Aceite:

- [ ] Build termina com zero erros.
- [ ] Build termina com zero avisos.
- [ ] O client secret não aparece em `git diff`.
- [ ] O projeto Blazor não referencia ASP.NET Core Identity.

## Parte 10 — Executar o fluxo completo

### Preparar HTTPS local

```bash
dotnet dev-certs https --trust
```

Se o sistema não instala confiança automaticamente, siga a orientação exibida pelo próprio comando. Não troque para HTTP no Blazor sem também revisar cookies e redirect URIs.

### Iniciar na ordem correta

Terminal 1:

```bash
docker compose --env-file .env \
  -f keycloak.compose.yaml up -d
```

Confirme o discovery:

```bash
curl -fsS \
  http://localhost:8080/realms/poc5/.well-known/openid-configuration
```

Terminal 2:

```bash
dotnet run \
  --project src/UserRegistration.Web/UserRegistration.Web.csproj \
  --launch-profile https
```

Abra <https://localhost:7188>.

### Prova A — Área protegida

1. Sem login, abra <https://localhost:7188/conta>.
2. Confirme que os dados da conta não aparecem.
3. Use o fluxo de entrada.
4. Confirme que o navegador vai para `http://localhost:8080`.

Se aparecer erro `IDX20803` ou `connection refused`, o Blazor não alcança o discovery do Keycloak. Volte à verificação dos containers e do realm.

### Prova B — Autorregistro, troca de senha e OTP

1. Na página do Keycloak, clique em `Register`.
2. Preencha username, email, nome e sobrenome.
3. Crie a senha inicial conforme a política.
4. Conclua o registro.
5. Na ação `Update Password`, informe uma nova senha diferente da inicial.
6. O Keycloak abre `Configure OTP`.
7. Instale o Google Authenticator pela loja oficial do celular.
8. No aplicativo, escolha adicionar conta por QR Code.
9. Escaneie o QR Code mostrado pelo Keycloak.
10. O celular passa a mostrar um código de seis dígitos.
11. Informe o código atual no Keycloak.
12. Dê um nome reconhecível ao dispositivo se a tela pedir.
13. Confirme.
14. Verifique que o navegador retorna para `/conta`.
15. Confirme que a página mostra `preferred_username`, `email` e `sub`.

Se o código for rejeitado repetidamente, ative data e hora automáticas no celular e sincronize o relógio do computador. TOTP depende do tempo.

### Prova C — Próximo login exige dois fatores

1. Clique em `Sair`.
2. Confirme que `/conta` deixa de mostrar os dados.
3. Clique em `Entrar`.
4. Informe username e a senha nova.
5. Confirme que o Keycloak pede o código OTP.
6. Abra o Google Authenticator e informe o código atual.
7. Confirme o retorno a `/conta`.

O primeiro login cadastra o segundo fator. Os logins posteriores validam esse fator.

### Prova D — Usuário administrativo com senha temporária

Repita o teste com um usuário criado em `Users` pelo administrador e uma senha marcada como `Temporary`.

O resultado obrigatório é:

```text
login + senha temporária
        -> troca de senha
        -> cadastro do Google Authenticator
        -> primeiro código OTP
        -> /conta
```

Se o cadastro do OTP aparecer antes da troca de senha, corrija a prioridade em `Authentication > Required actions` e crie um novo usuário para repetir o teste.

## Parte 11 — Parar de manter duas fontes de identidade

Depois que o fluxo Keycloak + Blazor passar, não mantenha o cadastro de senha antigo como um segundo mecanismo ativo.

### Tarefa adicional 22 — Separar identidade de perfil de negócio

O Keycloak passa a ser dono de:

- username;
- email de autenticação;
- senha;
- troca e recuperação de senha;
- OTP;
- sessão;
- bloqueios e políticas de login.

A API pode continuar dona de:

- endereço;
- telefone de contato de negócio;
- preferências;
- outros dados específicos da aplicação.

Use a claim `sub` como chave externa do perfil local. Não copie senha, segredo OTP ou QR Code para a tabela da aplicação.

### Tarefa adicional 23 — Remover o caminho inseguro antigo

Depois de criar uma migração segura para os dados que precisam permanecer:

- [ ] remova `Password` de `User`;
- [ ] remova `Password` de `UserInput`;
- [ ] remova `Password` de `CreateUserRequest` e `UpdateUserRequest`;
- [ ] remova `ChangePassword` do domínio;
- [ ] remova a coluna `Password` por migration;
- [ ] pare de usar `POST /users` como cadastro de credencial;
- [ ] identifique o perfil local pelo `sub` do Keycloak;
- [ ] proteja endpoints de perfil antes de expô-los ao Blazor.

Não execute a remoção da coluna antes de decidir se algum dado legítimo precisa ser migrado. Senhas em texto aberto não devem ser importadas para o Keycloak.

Proteger a Web API com access token é a próxima etapa natural, mas não é necessária para provar o objetivo desta POC: login/logout no Blazor, área protegida, dados da conta, troca inicial de senha e OTP.

## Checklist final da POC5

### Correções herdadas

- [ ] API e PostgreSQL sobem pelo Compose.
- [ ] Variáveis ausentes produzem erro claro.
- [ ] Migration é aplicada.
- [ ] Resíduos SQLite são removidos.
- [ ] Contratos HTTP são revalidados.

### Keycloak

- [ ] Keycloak 26.7.2 e PostgreSQL sobem pelo Compose.
- [ ] Realm `poc5` existe.
- [ ] Autorregistro está habilitado.
- [ ] Client `user-registration-blazor` é confidencial.
- [ ] Redirect URIs são exatas.
- [ ] `Update Password` é ação padrão.
- [ ] `Configure OTP` é ação padrão.
- [ ] `Update Password` possui prioridade anterior a `Configure OTP`.

### Blazor

- [ ] Blazor compila sem avisos ou erros.
- [ ] O segredo está em User Secrets.
- [ ] Existe controle Entrar.
- [ ] Existe logout por POST.
- [ ] `/conta` possui `[Authorize]`.
- [ ] Usuário anônimo não vê os dados internos.
- [ ] Usuário autenticado vê nome e claims da conta.

### Jornada de segurança

- [ ] Novo usuário troca a senha no primeiro acesso.
- [ ] Depois da troca, cadastra o Google Authenticator.
- [ ] O primeiro código OTP é aceito.
- [ ] O usuário entra na área protegida.
- [ ] Logout remove o acesso à área protegida.
- [ ] O login seguinte pede senha e OTP.
- [ ] O Blazor nunca recebe ou armazena a senha.

## Diagnóstico rápido

### `Missing 'ConnectionStrings:Default'`

Você iniciou a API fora do Compose sem fornecer connection string. Use o Compose reparado ou configure `ConnectionStrings__Default` de maneira segura para execução local.

### Variáveis ficam vazias no Compose

Use `--env-file .env` e confirme que substituiu todos os marcadores `[REDACTED_SECRET]`.

### `relation "Users" does not exist`

O PostgreSQL está acessível, mas a migration não foi aplicada. Confira `db.Database.Migrate()` e os logs da API.

### Keycloak volta ao realm `master`

Selecione `poc5` antes de editar usuários, clients ou autenticação.

### `Invalid parameter: redirect_uri`

A URL enviada pelo Blazor não corresponde exatamente a `Valid redirect URIs`. Confira protocolo, porta, caminho e maiúsculas/minúsculas.

### `IDX20803` ou falha ao obter configuração

Abra o discovery do realm no navegador. Confirme container, porta 8080, nome `poc5` e `Authority`.

### O Keycloak não pede OTP

Confirme:

- `Configure OTP` está habilitada e é ação padrão;
- o usuário novo recebeu a ação;
- a credencial OTP aparece em `Users > usuário > Credentials`;
- o Browser flow mantém o subfluxo condicional de 2FA e o `OTP Form`.

### OTP aparece antes da troca de senha

Mova `Update Password` acima de `Configure OTP` em `Authentication > Required actions`. A ordem das ações segue a prioridade configurada.

### Código do Google Authenticator é inválido

Ative horário automático no celular e no computador, aguarde um código novo e tente uma vez. Não reutilize código expirado.

### O client secret vazou em commit

Gere um novo segredo na aba `Credentials`, atualize User Secrets e invalide o anterior. Apenas apagar o texto do último commit não torna um segredo publicado seguro novamente.

## Como parar o ambiente

```bash
docker compose --env-file .env \
  -f keycloak.compose.yaml down

docker compose --env-file .env \
  -f src/UserRegistration.Api/compose.yaml down
```

`down` preserva os volumes nomeados e, portanto, os dados.

`down -v` apaga os volumes e todos os usuários, realms e configurações locais. Use apenas quando deseja reiniciar a POC do zero e aceita perder esses dados.

## Referências oficiais

- [Keycloak em Docker](https://www.keycloak.org/getting-started/getting-started-docker)
- [Execução do Keycloak em container](https://www.keycloak.org/server/containers)
- [Guia de administração do Keycloak](https://www.keycloak.org/docs/latest/server_admin/)
- [Autenticação OIDC em aplicações web ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/authentication/configure-oidc-web-authentication?view=aspnetcore-10.0)
- [Blazor Web App com OpenID Connect](https://learn.microsoft.com/aspnet/core/blazor/security/blazor-web-app-with-oidc?view=aspnetcore-10.0)
- [Aplicação de migrations do EF Core](https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying)
- [Interpolação de variáveis no Docker Compose](https://docs.docker.com/compose/how-tos/environment-variables/variable-interpolation/)

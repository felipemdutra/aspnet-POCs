# POC3 - Correção das lacunas da POC2

A POC3 não adiciona uma tecnologia nova. Ela existe para fechar itens que já estavam pedidos na POC2 e ficaram incompletos.

O objetivo é formativo: quando uma tarefa pede fronteira arquitetural, não basta mover arquivos para projetos separados. O fluxo também precisa respeitar essa fronteira.

## Estado herdado da POC2

A POC2 entregou vários pontos importantes:

- rotas e códigos de status HTTP consistentes;
- DTOs de requisição e resposta;
- senha fora das respostas;
- validação com DataAnnotations;
- contato e endereço no cadastro;
- OpenAPI e Scalar;
- ProblemDetails;
- `User` rica no projeto `Domain`;
- projetos `Api`, `Application`, `Domain` e `Infrastructure`.

Mas ficaram quatro lacunas:

- o `.http` não cobre telefone inválido;
- `IUserStore` não tem operação explícita de atualização;
- `Application` quase não orquestra casos de uso;
- endpoints ainda assumem criação e atualização de `User`.

## Feedback ao desenvolvedor júnior

Você acertou boa parte da POC2. O contrato HTTP melhorou, a senha saiu das respostas, a validação entrou, a documentação da API existe, os erros ficaram mais previsíveis e o domínio foi separado da API.

O ponto que faltou fechar foi a arquitetura de fluxo. A POC2 não pedia apenas projetos separados. Ela também pedia que a API deixasse de concentrar decisões de criação e atualização. Quando o endpoint chama `User.Create(...)` e `ChangeEmail(...)`, ele ainda sabe demais sobre o caso de uso. Isso torna a camada `Application` quase decorativa.

Por isso, esses itens voltam agora. Não é repetição por capricho. É retrabalho necessário porque a POC2 já pedia essas fronteiras, e elas não foram completadas.

## Evidência após rollback

Depois de reverter as alterações de código feitas indevidamente, o build foi executado novamente:

```bash
dotnet build csharp-user-registration-poc.slnx
```

Resultado em 30/06/2026:

```text
Build succeeded.
0 Warning(s)
0 Error(s)
```

Portanto, as pendências abaixo não são erro de compilação neste momento. Elas são pendências didáticas e arquiteturais que devem ser corrigidas pelo aluno na POC3.

## Auditoria de camadas

Esta auditoria encontrou violação arquitetural de fluxo, não de referência básica entre projetos.

### Sem violação detectada

- `Domain` não possui referências de projeto e não possui pacotes externos.
- `Application` referencia apenas `Domain`.
- `Infrastructure` referencia `Application` e `Domain`, implementando contrato definido pela aplicação.
- `Api` referencia `Infrastructure` apenas para composição no `Program.cs`. Nesta POC, isso é aceitável enquanto o uso concreto ficar restrito ao registro de DI.

### Violações detectadas

- `src/UserRegistration.Api/Endpoints/UserEndpoints.cs` injeta `IUserStore` diretamente nos handlers.
- `POST /users` chama `User.Create(...)` diretamente no endpoint.
- `PUT /users/{id}` chama `ChangeEmail(...)`, `ChangePassword(...)`, `ChangePhone(...)` e `ChangeAddress(...)` diretamente no endpoint.
- `Application` ainda não contém serviço ou caso de uso real para usuários.
- `IUserStore` não possui operação explícita de atualização; por isso, o update depende de mutação por referência do objeto recuperado.

## TAREFA 01 - Completar cobertura manual no `.http`

### Razão

A POC2 pediu validação de telefone inválido. Sem esse exemplo no `.http`, o aluno não consegue provar manualmente que o contrato rejeita esse campo.

### Checklist de execução

- [ ] Abrir `csharp-user-registration-poc.http`.
- [ ] Adicionar um `POST /users` com `phone` inválido.
- [ ] Manter os casos existentes de sucesso, email inválido, senha curta, CEP inválido e estado inválido.
- [ ] Executar o caso novo com a API rodando.

### Checklist de saída

- [ ] O `.http` possui caso de telefone inválido.
- [ ] Telefone inválido retorna `400 Bad Request`.
- [ ] A resposta de erro vem em formato ProblemDetails.

## TAREFA 02 - Dar atualização explícita ao `IUserStore`

### Razão

Na POC2, `IUserStore` deveria representar listagem, busca, criação, atualização e remoção. Sem operação de atualização, o `PUT` depende de buscar a entidade e mutar o objeto em outro lugar. Isso funciona em memória, mas ensina o limite errado para a próxima troca por banco.

### Checklist de execução

- [ ] Abrir `src/UserRegistration.Application/Abstractions/IUserStore.cs`.
- [ ] Adicionar uma operação explícita de atualização.
- [ ] Implementar a operação em `src/UserRegistration.Infrastructure/Users/InMemoryUserStore.cs`.
- [ ] Retornar `false` quando o usuário não existir.
- [ ] Retornar `true` quando a atualização for aplicada.

### Checklist de saída

- [ ] `IUserStore` contém listagem, busca, criação, atualização e remoção.
- [ ] `InMemoryUserStore` implementa a atualização.
- [ ] `PUT /users/{id}` não depende mais de mutação solta no endpoint.

## TAREFA 03 - Criar orquestração real na Application

### Razão

A camada `Application` deve coordenar casos de uso. Ela não substitui o domínio: o domínio continua protegendo invariantes. Mas ela decide o fluxo: criar usuário, salvar usuário, buscar usuário e atualizar usuário.

### Checklist de execução

- [ ] Criar um tipo de entrada da Application para dados de usuário.
- [ ] Criar um serviço concreto de Application para usuários.
- [ ] Fazer o serviço criar usuário chamando `User.Create(...)`.
- [ ] Fazer o serviço atualizar usuário chamando `ChangeEmail(...)`, `ChangePassword(...)`, `ChangePhone(...)` e `ChangeAddress(...)`.
- [ ] Fazer o serviço chamar `IUserStore.Add(...)` na criação.
- [ ] Fazer o serviço chamar `IUserStore.Update(...)` na atualização.
- [ ] Registrar o serviço no DI.

### Checklist de saída

- [ ] A camada `Application` possui orquestração real de criação.
- [ ] A camada `Application` possui orquestração real de atualização.
- [ ] A camada `Domain` continua dona das invariantes.
- [ ] A camada `Api` não conhece detalhes internos de criação e alteração da entidade.

## TAREFA 04 - Afinar responsabilidade dos endpoints

### Razão

Endpoint deve traduzir HTTP. Ele recebe a requisição, chama a camada `Application`, decide o código de status e mapeia a resposta. Regra de criação e atualização deve sair dele.

### Checklist de execução

- [ ] Abrir `src/UserRegistration.Api/Endpoints/UserEndpoints.cs`.
- [ ] Fazer `POST /users` chamar o serviço da camada `Application`.
- [ ] Fazer `PUT /users/{id}` chamar o serviço da camada `Application`.
- [ ] Remover chamadas diretas de `User.Create(...)` dos endpoints.
- [ ] Remover chamadas diretas de `ChangeEmail(...)`, `ChangePassword(...)`, `ChangePhone(...)` e `ChangeAddress(...)` dos endpoints.
- [ ] Manter DTOs HTTP na camada `Api`.
- [ ] Manter mapeamento para `UserResponse` na camada `Api`.

### Checklist de saída

- [ ] `UserEndpoints` não cria `User` diretamente.
- [ ] `UserEndpoints` não chama `Change*` diretamente.
- [ ] `UserEndpoints` continua responsável por rotas, códigos de status e resposta HTTP.
- [ ] O contrato HTTP atual não muda.

## TAREFA 04.1 - Fechar violação de arquitetura de camadas

### Razão

As tarefas 02, 03 e 04 corrigem partes da mesma violação: a API ainda está executando fluxo de aplicação. Esta tarefa existe como checklist de arquitetura para confirmar que a correção realmente respeitou as camadas, sem duplicar o passo a passo das tarefas anteriores.

### Checklist de execução

- [ ] Executar a TAREFA 02 para dar atualização explícita ao `IUserStore`.
- [ ] Executar a TAREFA 03 para criar orquestração real na `Application`.
- [ ] Executar a TAREFA 04 para afinar a responsabilidade dos endpoints.
- [ ] Rodar os comandos de verificação arquitetural listados na seção de validação.

### Checklist de saída

- [ ] Confirmar que `UserEndpoints` não injeta `IUserStore`.
- [ ] Confirmar que `UserEndpoints` não chama `User.Create(...)`.
- [ ] Confirmar que `UserEndpoints` não chama métodos `Change*`.
- [ ] Confirmar que `Application` possui serviço/caso de uso para criação, atualização, busca, listagem e remoção.
- [ ] Confirmar que `IUserStore` possui atualização explícita.
- [ ] Confirmar que `Application` não referencia `Infrastructure`.
- [ ] Confirmar que `Domain` segue sem ASP.NET Core, EF Core, DTOs HTTP, `Results`, `ProblemDetails` ou Scalar.

## TAREFA 05 - Validar comportamento

### Razão

Refatoração sem validação é chute. A POC3 altera o fluxo interno, então precisa provar que o contrato externo continuou igual.

### Checklist de execução

- [ ] Rodar `dotnet build csharp-user-registration-poc.slnx`.
- [ ] Subir a API em Development.
- [ ] Validar `GET /users`.
- [ ] Validar `POST /users` válido.
- [ ] Validar `PUT /users/{id}` válido.
- [ ] Validar `DELETE /users/{id}` válido.
- [ ] Validar `GET /users/999`.
- [ ] Validar `POST /users` com telefone inválido.
- [ ] Validar `/openapi/v1.json`.
- [ ] Validar `/scalar`.
- [ ] Conferir que nenhuma resposta contém `password`.

### Checklist de saída

- [ ] A compilação passa sem avisos e erros.
- [ ] `POST /users` retorna `201 Created`.
- [ ] `PUT /users/{id}` retorna `200 OK`.
- [ ] `DELETE /users/{id}` retorna `204 No Content`.
- [ ] Usuário inexistente retorna `404 Not Found`.
- [ ] Payload inválido retorna `400 Bad Request`.
- [ ] OpenAPI continua respondendo.
- [ ] Scalar continua abrindo.

## Fora de escopo

- Não criar testes automatizados nesta POC.
- Não adicionar EF Core.
- Não adicionar banco de dados.
- Não implementar hash de senha.
- Não adicionar autenticação.
- Não adicionar Docker.

## Comandos de validação

```bash
dotnet build csharp-user-registration-poc.slnx
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/UserRegistration.Api/UserRegistration.Api.csproj
```

Verificar arquitetura:

```bash
for project in src/UserRegistration.*/*.csproj; do dotnet list "$project" reference; done
dotnet list src/UserRegistration.Domain/UserRegistration.Domain.csproj package
rg "User\\.Create|ChangeEmail|ChangePassword|ChangePhone|ChangeAddress|IUserStore" src/UserRegistration.Api/Endpoints/UserEndpoints.cs
rg "UserRegistration.Infrastructure|Microsoft.AspNetCore|Scalar|ProblemDetails|Results" src/UserRegistration.Application src/UserRegistration.Domain
```

Verificar manualmente com `csharp-user-registration-poc.http`:

- [ ] `GET /users`
- [ ] `POST /users`
- [ ] `PUT /users/{id}`
- [ ] `DELETE /users/{id}`
- [ ] `GET /users/999`
- [ ] `POST /users` com telefone inválido
- [ ] `GET /openapi/v1.json`
- [ ] `GET /scalar`

## Critérios de aceite

- [ ] `IUserStore` tem operação de atualização clara.
- [ ] `UserEndpoints` não cria `User` diretamente.
- [ ] `UserEndpoints` não chama `Change*` diretamente.
- [ ] A camada `Application` contém orquestração real de criação e atualização.
- [ ] A auditoria de camadas não aponta mais violação de fluxo entre `Api` e `Application`.
- [ ] `.http` cobre telefone inválido.
- [ ] O contrato HTTP atual não muda.
- [ ] A compilação passa sem avisos e erros.
- [ ] O smoke test confirma `/users`, `/openapi/v1.json` e `/scalar`.

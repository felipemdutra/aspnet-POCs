# POC4 - Correções da auditoria da POC2 e POC3

## Como usar este tutorial

Esta POC não entrega correções prontas. Você altera o código, executa os testes e registra o resultado.

Antes de executar qualquer comando, escolha seu sistema operacional:

- Linux/macOS: execute apenas blocos marcados `bash`.
- Windows PowerShell: execute apenas blocos marcados `powershell`.
- Não misture comandos Bash e PowerShell na mesma etapa.

Comandos abaixo são equivalentes. Muda apenas a sintaxe do terminal. Windows com Git Bash ou WSL pode seguir os blocos Bash; Windows PowerShell deve seguir os blocos PowerShell.

## Objetivo

Fechar lacunas da [POC2](</home/darthlinuxer/aspnet-POCs/POC2.md>)/[POC3](</home/darthlinuxer/aspnet-POCs/POC3.md>) sem entregar a implementação pronta. Esta POC é roteiro para o aluno modificar o código, entender a causa da falha e provar a correção.

## Vocabulário antes da prática

Leia esta seção antes das tarefas. Os nomes aparecem no restante do tutorial.

### API

API é um programa que recebe pedidos de outro programa e devolve respostas. Nesta POC, um cliente envia JSON para a API e recebe JSON com dados de usuários.

### HTTP, rota e endpoint

HTTP é o protocolo usado para conversar com a API.

Uma rota é o endereço, por exemplo `/users/2`.

Um método HTTP diz qual ação queremos:

- `GET`: consultar;
- `POST`: criar;
- `PUT`: atualizar;
- `DELETE`: remover.

Endpoint é a combinação do método com a rota. `GET /users` e `POST /users` usam a mesma parte do endereço, mas são endpoints diferentes porque executam ações diferentes.

### Request, response e DTO

Request é o pedido enviado para a API. Response é a resposta devolvida pela API.

JSON é o formato dos dados enviados no corpo do pedido. DTO significa “objeto usado para transportar dados”. Nesta POC, [`CreateUserRequest`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Api/Requests/CreateUserRequest.cs>) é DTO de entrada e [`UserResponse`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Api/Responses/UserResponse.cs>) é DTO de saída.

O DTO de entrada pode receber `password`; o DTO de saída não deve devolver esse campo. Assim, o contrato público não expõe segredo.

### Status code

Status code é o número que resume o resultado HTTP:

- `200`: operação terminou com sucesso;
- `201`: recurso foi criado;
- `204`: operação terminou sem corpo de resposta;
- `400`: dados enviados são inválidos;
- `404`: recurso não foi encontrado.

### Domínio e regra válida

Domínio é o assunto que o programa representa. Aqui, o assunto é cadastro de usuário.

Uma regra válida é uma condição que precisa ser respeitada. Exemplos: email não pode estar vazio, senha precisa ter tamanho mínimo e CEP precisa ter formato correto.

No código, [`User`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Domain/Models/User.cs>) protege essas regras porque seus dados só podem ser criados ou alterados por métodos que validam os valores. Neste tutorial, chamamos essas regras de “regras do domínio”. O termo “invariante” significa apenas uma regra que deve continuar verdadeira; você pode entendê-lo como “regra que nunca pode ser quebrada”.

### Camada e responsabilidade

Camada é uma parte do programa com uma responsabilidade principal. Responsabilidade significa “qual trabalho esta parte deve fazer”.

Arquitetura é o desenho dessas partes e das regras que dizem quem pode conversar com quem. Uma arquitetura boa reduz confusão: cada parte faz seu trabalho e depende apenas do necessário.

- `Api`: conversa com o cliente HTTP;
- `Application`: organiza uma operação completa;
- `Domain`: representa usuário e suas regras;
- `Infrastructure`: conversa com armazenamento.

### Caso de uso

Caso de uso é uma operação que uma pessoa ou sistema quer realizar. Exemplos desta POC: “criar usuário”, “buscar usuário”, “atualizar usuário” e “remover usuário”.

Application organiza o passo a passo do caso de uso. Por exemplo, para criar usuário: receber dados, pedir ao domínio para criar um usuário válido e mandar armazená-lo.

### Contrato e implementação

Contrato é uma promessa sobre operações disponíveis. [`IUserStore`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Application/Abstractions/IUserStore.cs>) promete listar, buscar, adicionar, atualizar e remover usuários.

Implementação é o código que cumpre a promessa. [`InMemoryUserStore`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Infrastructure/Users/InMemoryUserStore.cs>) é uma implementação que guarda usuários numa lista em memória.

### Persistência

Persistir significa guardar dados para que continuem disponíveis depois da operação. Nesta POC, a lista em memória é uma persistência temporária: os dados desaparecem quando o programa encerra. Um banco seria uma persistência durável.

CRUD é o nome curto para as quatro ações básicas sobre dados: criar, consultar, atualizar e remover.

SQL é a linguagem normalmente usada para pedir essas ações a um banco relacional. Você não precisa escrever SQL nesta POC; o termo aparece apenas para comparar a lista em memória com um banco futuro.

### Orquestrar

Orquestrar significa colocar ações na ordem correta. [`UserApplicationService`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Application/Users/UserApplicationService.cs>) deve orquestrar o caso de uso: buscar usuário, pedir alterações ao domínio e mandar salvar no store. O arquivo ainda será criado pelo aluno.

### DI e registrar serviço

DI significa injeção de dependência. Em vez de uma classe criar tudo sozinha, o programa entrega a ela os objetos de que precisa.

Registrar um serviço no DI significa informar ao programa qual classe deve entregar quando alguém pedir determinado tipo. Isso é feito em [`Program.cs`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Api/Program.cs>).

### ProblemDetails e aviso de dependência

ProblemDetails é um formato padronizado para explicar erro HTTP em JSON.

Dependência é uma biblioteca usada por outra biblioteca ou pelo projeto. Dependência transitiva é uma biblioteca que chega indiretamente: o projeto usa A, e A usa B.

Um aviso `NU1903` informa que uma dependência possui vulnerabilidade conhecida. Warning significa aviso; não é o mesmo que erro de compilação, mas precisa ser investigado. Consulte o [aviso de segurança do Microsoft.OpenApi](https://github.com/advisories/GHSA-v5pm-xwqc-g5wc).

### OpenAPI e Scalar

OpenAPI é um documento que descreve as rotas, entradas e respostas da API. Ele ajuda ferramentas e pessoas a entenderem o contrato sem ler todo o código. Consulte a [especificação oficial OpenAPI](https://spec.openapis.org/oas/latest.html).

Scalar é uma página web que lê o documento OpenAPI e mostra uma interface para consultar a documentação da API. Consulte a [documentação oficial do Scalar](https://scalar.com/).

Swagger não é sinônimo de “produto pago”. O [Swagger UI](https://swagger.io/tools/swagger-ui/) é um projeto open source para visualizar e testar APIs. O [SwaggerHub](https://swagger.io/tools/swaggerhub/) é um produto hospedado com recursos adicionais, planos e colaboração. Nesta POC usamos Scalar localmente, sem SwaggerHub.

## Evidência encontrada

Auditoria executada em 11/07/2026, com o código original restaurado:

- Build passou, mas apresentou 2 avisos `NU1903` sobre vulnerabilidade conhecida em `Microsoft.OpenApi 2.0.0`.
- App iniciou em `http://localhost:5291`.
- `GET /users`: `200`.
- `GET /users/999`: `404` + ProblemDetails.
- `POST /users` válido: `201` + `Location: /users/2`.
- Resposta de criação não expôs `password`.
- Telefone inválido: `400`; validação funciona, mas não existe cenário no `.http`.
- `PUT /users/2`: `200`.
- `PUT /users/999`: `404`.
- `DELETE /users/2`: `204`.
- `/openapi/v1.json`: `200`.
- `/scalar`: `302`, redirecionando para a UI.

O comportamento HTTP está funcional. A incompletude é arquitetural/documental, não falha geral de execução.

## Verificação da página Scalar — já existente

Esta etapa não exige alteração de código. A auditoria confirmou que o projeto já possui:

- [`Scalar.AspNetCore`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Api/UserRegistration.Api.csproj>) no arquivo do projeto;
- [`AddOpenApi()` e `MapOpenApi()`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Api/Program.cs>) para gerar o documento OpenAPI;
- [`MapScalarApiReference()`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Api/Program.cs>) para criar a página visual;
- proteção por `app.Environment.IsDevelopment()`, portanto a página aparece somente em modo Development.

Se você esperava encontrar uma tarefa para instalar Scalar, ela já foi concluída na POC2. Não instale outro pacote nem duplique o registro.

### Como subir a API

Escolha o bloco do seu sistema operacional. Execute somente um.

Linux/macOS:

```bash
dotnet build csharp-user-registration-poc.slnx
ASPNETCORE_ENVIRONMENT=Development dotnet run \
  --project src/UserRegistration.Api/UserRegistration.Api.csproj
```

Windows PowerShell:

```powershell
dotnet build csharp-user-registration-poc.slnx
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src/UserRegistration.Api/UserRegistration.Api.csproj
```

Deixe o terminal aberto. A mensagem `Now listening on: http://localhost:5291` confirma que a API está ouvindo nessa porta.

### Como abrir no navegador

Com a API rodando, abra:

- [Página Scalar](http://localhost:5291/scalar)
- [Documento OpenAPI bruto](http://localhost:5291/openapi/v1.json)

Se `/scalar` redirecionar, isso é esperado: o navegador deve seguir o `302` até a página visual.

### Como testar endpoints dentro do Scalar

1. Abra [http://localhost:5291/scalar](http://localhost:5291/scalar).
2. Localize a seção de usuários.
3. Abra `GET /users` e clique em `Try it`/`Execute`.
4. Confirme resposta `200` e uma lista JSON.
5. Abra `GET /users/{id}`, informe `999` e execute.
6. Confirme `404` e corpo ProblemDetails.
7. Abra `POST /users`, escolha `Try it`/`Execute` e informe um JSON válido.
8. Confirme `201 Created`, cabeçalho `Location` e ausência de `password` na resposta.
9. Execute `POST /users` com `phone` igual a `bad`.
10. Confirme `400 Bad Request` e erro de validação.
11. Execute `PUT /users/2` com dados válidos e confirme `200`.
12. Execute `DELETE /users/2` e confirme `204`.

Os nomes dos botões podem variar conforme a versão do Scalar. A ação é sempre a mesma: abrir operação, preencher parâmetros/corpo, executar e conferir status code e resposta.

## Conceito central: projeto separado não basta

Separar `Api`, `Application`, `Domain` e `Infrastructure` cria fronteiras físicas. A arquitetura só está correta quando o fluxo respeita essas fronteiras:

```text
Api -> Application -> Domain
                  -> contrato IUserStore -> Infrastructure
```

`Api` traduz HTTP. `Application` coordena casos de uso. `Domain` protege invariantes. `Infrastructure` implementa persistência. Portanto, endpoint não deve criar [`User`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Domain/Models/User.cs>), chamar `Change*` ou conhecer [`IUserStore`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Application/Abstractions/IUserStore.cs>).

Antes de editar, leia [`UserEndpoints.cs`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Api/Endpoints/UserEndpoints.cs>), [`IUserStore.cs`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Application/Abstractions/IUserStore.cs>), [`InMemoryUserStore.cs`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Infrastructure/Users/InMemoryUserStore.cs>) e [`User.cs`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Domain/Models/User.cs>). Responda: quem recebe request, quem cria usuário, quem altera usuário e quem persiste usuário? Essa resposta é seu diagnóstico inicial.

## Tarefa 01 — Adicionar telefone inválido ao `.http`

### Falha

[`CreateUserRequest`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Api/Requests/CreateUserRequest.cs>) possui `[Phone]` e o domínio valida telefone, mas [`csharp-user-registration-poc.http`](</home/darthlinuxer/aspnet-POCs/csharp-user-registration-poc.http>) não contém caso manual para esse requisito.

### Por que importa

Regra existente sem exemplo executável não permite ao aluno repetir a comprovação do contrato.

### O que o aluno deve fazer

O `.http` é documentação executável: o código implementa a regra; o exemplo permite que outra pessoa reproduza a regra.

Adicionar `POST /users` com payload válido, exceto `phone`, por exemplo `"bad"`. Executar com API ligada.

### Aceite

- Caso aparece no `.http`.
- Resposta é `400 Bad Request`.
- Corpo é erro de validação/ProblemDetails.

### Defesa

Request com payload válido, exceto `phone`, já existe, e já foi testado, resultando em 400 Bad Request.

## Tarefa 02 — Adicionar atualização explícita ao armazenamento

### Falha

[`IUserStore`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Application/Abstractions/IUserStore.cs>) possui listagem, busca, criação e remoção, mas não possui `Update`. O `PUT` busca [`User`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Domain/Models/User.cs>) e altera a mesma instância em memória.

### Por que importa

Mutação por referência funciona acidentalmente com `List<User>`, mas não representa operação de persistência. Ao trocar memória por banco, esse fluxo não define como salvar alteração.

### O que o aluno deve fazer

[`IUserStore`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Application/Abstractions/IUserStore.cs>) é contrato, não implementação. A aplicação precisa declarar “atualizar usuário”; o store em memória decide como fazer isso. Um banco futuro poderá traduzir a mesma operação para SQL.

1. Abrir [`IUserStore.cs`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Application/Abstractions/IUserStore.cs>).
2. Adicionar `bool Update(User user)`.
3. Em [`InMemoryUserStore`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Infrastructure/Users/InMemoryUserStore.cs>), localizar índice pelo `Id`.
4. Retornar `false` quando não encontrar.
5. Substituir o item e retornar `true` quando encontrar.

### Aceite

[`IUserStore`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Application/Abstractions/IUserStore.cs>) declara operações CRUD completas; `PUT` usa atualização explícita; usuário inexistente continua resultando em `404`.

### Defesa

Método `bool Update(User user)` já existe dentro de IUserStore, e retorna `false` quando não encontra,
e substitui o item e retorna `true` quando encontra.

## Tarefa 03 — Criar orquestração na Application

### Falha

`UserRegistration.Application` contém apenas [`IUserStore`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Application/Abstractions/IUserStore.cs>). Não existe serviço de caso de uso. Logo, a camada API executa fluxo de criação e atualização.

### Por que importa

Separar projetos não basta. A camada Application deve coordenar o caso de uso; Domain deve proteger invariantes; Infrastructure deve persistir.

### O que o aluno deve fazer

Application coordena o caso de uso; não duplica validações do domínio. Ela não deve receber DTO HTTP nem retornar `Results`, `IResult` ou `ProblemDetails`.

1. Criar um tipo [`UserInput`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Application/Users/UserInput.cs>) na Application, sem referência a HTTP.
2. Criar [`UserApplicationService`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Application/Users/UserApplicationService.cs>).
3. Implementar listagem, busca, criação, atualização e remoção.
4. Na criação, chamar [`User`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Domain/Models/User.cs>).Create(...) e depois [`IUserStore`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Application/Abstractions/IUserStore.cs>).Add(...).
5. Na atualização, buscar por id, chamar métodos `Change*` e depois [`IUserStore`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Application/Abstractions/IUserStore.cs>).Update(...).
6. Registrar o serviço no DI em [`Program.cs`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Api/Program.cs>).

### Aceite

Application orquestra criação/atualização; Domain continua dono das invariantes; Application não referencia Infrastructure.

### Defesa

- Tipo UserInput que não faz referência ao HTTP já existe
- Serviço de user (UserService) existe, e implementa listagem, busca, criação, atualização e remoção.
- Ele já utiliza `User.Create` e IUserStore.
- Ele corretamente chama Change* e depois IUserStore.
- Serviço já está registrado.

## Tarefa 04 — Afinar endpoints

### Falha

[`UserEndpoints.cs`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Api/Endpoints/UserEndpoints.cs>) injeta [`IUserStore`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Application/Abstractions/IUserStore.cs>), chama [`User`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Domain/Models/User.cs>).Create(...) e chama `ChangeEmail`, `ChangePassword`, `ChangePhone` e `ChangeAddress`.

### Por que importa

Endpoint deve traduzir HTTP. Quando conhece o fluxo interno, API fica acoplada ao modelo e ao mecanismo de armazenamento.

### O que o aluno deve fazer

Endpoint é adapter HTTP. Ele recebe request, chama Application, escolhe status code e mapeia response. Ele não deve conter regra de criação ou alteração.

- Trocar [`IUserStore`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Application/Abstractions/IUserStore.cs>) pelo serviço da Application nos handlers.
- Converter [`CreateUserRequest`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Api/Requests/CreateUserRequest.cs>)/[`UpdateUserRequest`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Api/Requests/UpdateUserRequest.cs>) para `UserInput`.
- Remover chamadas diretas a [`User`](</home/darthlinuxer/aspnet-POCs/src/UserRegistration.Domain/Models/User.cs>).Create(...) e `Change*`.
- Manter DTOs, mapeamento, rotas e status codes na API.

### Aceite

Escolha um bloco conforme seu sistema operacional. Não execute os dois.

Linux/macOS:

```bash
rg "User\.Create|ChangeEmail|ChangePassword|ChangePhone|ChangeAddress|IUserStore" \
  src/UserRegistration.Api/Endpoints/UserEndpoints.cs
```

Windows PowerShell:

```powershell
Select-String -Path src/UserRegistration.Api/Endpoints/UserEndpoints.cs `
  -Pattern 'User\.Create|ChangeEmail|ChangePassword|ChangePhone|ChangeAddress|IUserStore'
```

Antes da correção, ocorrências são esperadas. Depois, nenhum bloco deve retornar ocorrência.

### Defesa

- UserEndpoints.cs não injeta IUserStore, mas IUserService.
- UserEndpoints.cs utiliza UserInput como argumento no serviço.
- Endpoints não chamam `Change*` e `User.Create` diretamente.

## Tarefa 05 — Corrigir aviso de dependência

### Falha

Build emitiu `NU1903`: `Microsoft.OpenApi 2.0.0` possui vulnerabilidade conhecida.

### O que o aluno deve fazer

Warning de segurança não impede compilação, mas não deve ser escondido. Descubra a dependência que introduziu o pacote e atualize versão compatível.

Investigar a dependência transitiva e atualizar o pacote compatível, sem mascarar o aviso com supressão. Reexecutar restore/build e registrar versão escolhida.

### Aceite

Build sem `NU1903`, sem quebrar OpenAPI ou Scalar.

## Validação final do aluno

Linux/macOS:

```bash
dotnet build csharp-user-registration-poc.slnx
ASPNETCORE_ENVIRONMENT=Development dotnet run \
  --project src/UserRegistration.Api/UserRegistration.Api.csproj
```

Windows PowerShell:

```powershell
dotnet build csharp-user-registration-poc.slnx
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run --project src/UserRegistration.Api/UserRegistration.Api.csproj
```

Testar: `GET /users` 200; `GET /users/999` 404; POST válido 201 com Location; telefone inválido 400; PUT válido 200; PUT inexistente 404; DELETE válido 204; ausência de `password`; OpenAPI 200; Scalar disponível via redirecionamento.

Verificar camadas no Linux/macOS:

```bash
for project in src/UserRegistration.*/*.csproj; do
  dotnet list "$project" reference
done

rg "UserRegistration.Infrastructure|Microsoft.AspNetCore|ProblemDetails|Results|Scalar" \
  src/UserRegistration.Application src/UserRegistration.Domain
```

Verificar camadas no Windows PowerShell:

```powershell
Get-ChildItem -Path src -Filter *.csproj -Recurse | ForEach-Object {
    dotnet list $_.FullName reference
}

Get-ChildItem src/UserRegistration.Application,src/UserRegistration.Domain -Filter *.cs -Recurse |
    Select-String -Pattern 'UserRegistration.Infrastructure|Microsoft.AspNetCore|ProblemDetails|Results|Scalar'
```

Verificar que os endpoints não chamam diretamente armazenamento ou domínio.

Linux/macOS:

```bash
rg "User\.Create|ChangeEmail|ChangePassword|ChangePhone|ChangeAddress|IUserStore" \
  src/UserRegistration.Api/Endpoints/UserEndpoints.cs
```

Windows PowerShell:

```powershell
Select-String -Path src/UserRegistration.Api/Endpoints/UserEndpoints.cs `
  -Pattern 'User\.Create|ChangeEmail|ChangePassword|ChangePhone|ChangeAddress|IUserStore'
```

Resultado esperado: nenhum comando retorna ocorrência após a correção.

## Smoke test — escolha seu sistema operacional

Execute apenas um dos blocos abaixo.

### Linux/macOS

Com a API executando em outro terminal:

```bash
base=http://localhost:5291
curl -i "$base/users"
curl -i "$base/users/999"
curl -i -H 'Content-Type: application/json' \
  -d '{"email":"new@example.com","password":"longpassword123","phone":"+5511999997777","addressLine":"A","city":"Sao Paulo","state":"SP","zipCode":"01310-100"}' \
  "$base/users"
curl -i -H 'Content-Type: application/json' \
  -d '{"email":"badphone@example.com","password":"longpassword123","phone":"bad","addressLine":"A","city":"Sao Paulo","state":"SP","zipCode":"01310-100"}' \
  "$base/users"
curl -i -X PUT -H 'Content-Type: application/json' \
  -d '{"email":"updated@example.com","password":"longpassword123","phone":"+5511999997777","addressLine":"A","city":"Sao Paulo","state":"SP","zipCode":"01310-100"}' \
  "$base/users/2"
curl -i -X DELETE "$base/users/2"
curl -i "$base/openapi/v1.json"
curl -i "$base/scalar"
```

### Windows PowerShell

Com a API executando em outro terminal:

```powershell
$base = "http://localhost:5291"

Invoke-WebRequest "$base/users" -Method Get
Invoke-WebRequest "$base/users/999" -Method Get

$validUser = @{
    email = "new@example.com"
    password = "longpassword123"
    phone = "+5511999997777"
    addressLine = "A"
    city = "Sao Paulo"
    state = "SP"
    zipCode = "01310-100"
} | ConvertTo-Json

Invoke-WebRequest "$base/users" -Method Post `
  -ContentType "application/json" -Body $validUser

$invalidPhone = $validUser | ConvertFrom-Json
$invalidPhone.phone = "bad"
$invalidPhone = $invalidPhone | ConvertTo-Json

Invoke-WebRequest "$base/users" -Method Post `
  -ContentType "application/json" -Body $invalidPhone

Invoke-WebRequest "$base/users/2" -Method Put `
  -ContentType "application/json" -Body $validUser

Invoke-WebRequest "$base/users/2" -Method Delete
Invoke-WebRequest "$base/openapi/v1.json" -Method Get
Invoke-WebRequest "$base/scalar" -Method Get -MaximumRedirection 0
```

Para observar status e corpo de erro:

```powershell
try {
    Invoke-WebRequest "$base/users/999" -Method Get
} catch {
    $_.Exception.Response.StatusCode.value__
    $reader = New-Object IO.StreamReader($_.Exception.Response.GetResponseStream())
    $reader.ReadToEnd()
    $reader.Dispose()
}
```

O `-MaximumRedirection 0` permite observar o `302` do Scalar. Para seguir até a UI:

```powershell
Invoke-WebRequest "$base/scalar/" -Method Get
```

## Checklist de conclusão

- [x] Scalar já está instalado e configurado no projeto.
- [x] API sobe em modo Development e página Scalar abre no navegador.
- [x] Endpoints foram testados pela página Scalar.
- [ ] Telefone inválido documentado no `.http`.
- [ ] `IUserStore.Update` implementado.
- [ ] Application possui orquestração real.
- [ ] Endpoints não conhecem store nem métodos internos de `User`.
- [ ] Build não emite `NU1903`.
- [ ] HTTP mantém contrato original.

Itens marcados `[x]` refletem o estado atual auditado. Itens `[ ]` continuam sendo trabalho do aluno.

## Fora de escopo

EF Core, banco, autenticação, hash de senha, Docker e testes automatizados.

## Referências para estudo

Leia somente depois de entender o exemplo local:

- [Minimal APIs no ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis)
- [Injeção de dependência no .NET](https://learn.microsoft.com/dotnet/core/extensions/dependency-injection)
- [ProblemDetails no ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/error-handling-api)
- [Auditoria de pacotes com `dotnet list package --vulnerable`](https://learn.microsoft.com/dotnet/core/tools/dotnet-list-package)

# POC2 - Roteiro de Tarefas

A trilha sai da Minimal API inicial em memória e chega até uma solução modular com fronteiras arquiteturais claras.

Cada tarefa primeiro entende a limitação da etapa anterior, depois introduz o menor conceito novo que resolve essa limitação.

## Glossário mínimo para começar

- `HTTP`: protocolo usado pelo navegador, frontend ou arquivo `.http` para conversar com a API.
- Método HTTP, também chamado de verbo HTTP: ação da requisição. Exemplos: `GET` busca dados, `POST` cria dados, `PUT` atualiza dados, `DELETE` remove dados.
- Rota: endereço do endpoint, por exemplo `/users` ou `/users/{id}`.
- Endpoint: combinação de método HTTP com rota. Exemplo: `GET /users`.
- Request: pedido enviado para a API.
- Response: resposta enviada pela API.
- Payload: corpo da requisição, normalmente em JSON.
- Status code: número HTTP que resume o resultado. Exemplos: `200 OK`, `201 Created`, `400 Bad Request`, `404 Not Found`, `204 No Content`.
- Contrato da API: combinação de rotas, métodos HTTP, payloads aceitos, respostas devolvidas e status codes.
- Cabeçalho HTTP: informação extra enviada junto da request ou response, fora do corpo JSON.
- Recurso: algo que a API manipula. Nesta POC, usuário é o principal recurso.
- Build: compilação do projeto para verificar se o código está válido.
- NuGet: gerenciador de pacotes do .NET.

## Avaliação do roteiro proposto

Esta análise verifica se o roteiro ensina por causa e efeito, não por acúmulo de tecnologias. Cada tarefa precisa deixar claro qual problema foi descoberto na etapa anterior, qual conceito resolve esse problema e qual entrega concreta prova que o aluno entendeu.

O roteiro está coerente para um iniciante porque começa pela leitura do sistema antes de pedir qualquer refatoração. Depois evolui o contrato HTTP, separa entrada e saída com DTOs (Data Transfer Objects), adiciona validação, aprofunda o cadastro com dados de contato e endereço, documenta o contrato com OpenAPI/Scalar, padroniza erros, organiza responsabilidades dentro de um projeto único e só então cria projetos separados.

A ordem também evita abstração precoce. DTOs entram antes de validação porque primeiro é preciso proteger o contrato público. Dados de contato e endereço entram depois da validação básica porque o aluno já entende o mecanismo antes de lidar com mais campos. OpenAPI entra depois disso porque a documentação deve refletir um contrato mais realista. Clean Code entra antes da solução modular porque pastas e responsabilidades claras são um passo menor que vários projetos.

Pontos reforçados nesta revisão:

- DTOs explicam segurança, over-posting, desacoplamento e diferença entre request e response.
- A validação mostra primeiro email e senha, depois usa telefone, endereço, cidade, estado e CEP para praticar validadores prontos do .NET 10.
- OpenAPI diferencia o documento bruto `/openapi/v1.json` da UI navegável do Scalar em `/scalar`.
- Clean Code descreve arquivos, pastas, responsabilidades e exemplos de estrutura.

Quando a referência oficial é ampla demais para um aluno iniciante, a tarefa inclui um exemplo orientativo curto. O exemplo mostra forma e intenção, mas não entrega a implementação completa.

## TAREFA 01 - Entender a POC atual

### Apresentação

Esta tarefa não muda código. Ela serve para o aluno reconhecer os arquivos principais, entender onde a aplicação começa e identificar quais endpoints já existem.

### Dica para iniciante

- `csharp-user-registration-poc.csproj` é o arquivo de configuração do projeto C#.
- `TargetFramework` mostra a versão do .NET usada pelo projeto.
- `Nullable` ajuda o compilador a avisar sobre possíveis valores nulos.
- `ImplicitUsings` ativa imports automáticos comuns do C#.
- `Users.cs` contém o código atual da API.
- `WebApplication.CreateBuilder` prepara serviços e configuração da aplicação.
- `app.Build()` monta a aplicação antes dela começar a responder HTTP.
- `MapGet`, `MapPost`, `MapPut` e `MapDelete` criam endpoints.
- `MapGet` corresponde ao método `GET`; `MapPost` corresponde a `POST`; `MapPut` corresponde a `PUT`; `MapDelete` corresponde a `DELETE`.
- `.http` é um arquivo usado para chamar a API manualmente sem criar frontend.

### Exemplo de anotação esperada

Ao ler este código:

```csharp
app.MapGet("/users", () => Results.Ok(users));
```

Anote assim:

```text
GET /users - lista usuários
```

### Checklist de execução

- [ ] Abrir `csharp-user-registration-poc.csproj`.
- [ ] Identificar `TargetFramework`, `Nullable`, `ImplicitUsings` e pacotes NuGet, usando a dica acima.
- [ ] Abrir `Users.cs`.
- [ ] Localizar `WebApplication.CreateBuilder`.
- [ ] Localizar `app.Build()`.
- [ ] Localizar `MapGet`, `MapPost`, `MapPut` e `MapDelete`.
- [ ] Listar cada endpoint existente no formato `MÉTODO rota`, por exemplo `GET /users`.
- [ ] Identificar o método HTTP de cada endpoint: `GET`, `POST`, `PUT` ou `DELETE`.
- [ ] Identificar onde a lista `users` é criada.
- [ ] Identificar que os dados estão em memória.
- [ ] Identificar que a senha faz parte do modelo atual.
- [ ] Abrir `csharp-user-registration-poc.http`.
- [ ] Comparar cada chamada `.http` com as rotas reais do código.
- [ ] Rodar o build antes de qualquer alteração.

### Checklist de saída

- [ ] As rotas e métodos HTTP atuais foram anotados.
- [ ] O ponto de entrada da aplicação foi identificado.
- [ ] O armazenamento em memória foi identificado.
- [ ] O campo `Password` no modelo atual foi identificado.
- [ ] Nenhum código foi alterado nesta tarefa.

### Perguntas de autoavaliação

- O que é uma Minimal API?
- Onde a aplicação inicia?
- Quais endpoints existem hoje?
- Por que dados em memória somem ao reiniciar a aplicação?
- Por que expor senha no contrato HTTP é um problema?

### Validação

```bash
dotnet --info
dotnet build csharp-user-registration-poc.slnx
dotnet run --project csharp-user-registration-poc.csproj
```

### Referências para estudo

- [Minimal APIs - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-10.0)
- [Route handlers em Minimal APIs - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/route-handlers?view=aspnetcore-10.0)

## TAREFA 02 - Corrigir semântica HTTP e status codes

### Apresentação

Depois de entender a API, a primeira evolução é tornar o contrato HTTP previsível. A tarefa anterior mostrou endpoints funcionando, mas ainda há inconsistências como rotas sem barra inicial, `DELETE` retornando texto e `POST` com `Location` incompleto.

### Crítica da etapa anterior

A API responde, mas ainda não ensina bem a diferença entre executar uma ação e comunicar corretamente o resultado dessa ação pelo protocolo HTTP.

### Conceito novo

Semântica HTTP: usar métodos HTTP, rotas, status codes e cabeçalhos para expressar intenção de forma previsível.

### Dica para iniciante

- `GET` deve buscar dados.
- `POST` deve criar dados.
- `PUT` deve atualizar dados.
- `DELETE` deve remover dados.
- Cabeçalho `Location` informa onde o recurso criado pode ser encontrado.
- Prefixo `/users` significa que todas as rotas desta tarefa começam pelo mesmo caminho.
- Status code não é detalhe visual: ele informa ao cliente se a operação deu certo, criou algo, falhou por entrada inválida ou não encontrou recurso.
- Para consultar a lista completa de status codes HTTP, use o registro oficial da IANA nas referências desta tarefa.

### Exemplo orientativo

Associe intenção com status code antes de alterar todos os endpoints:

```csharp
return user is null
    ? Results.NotFound()
    : Results.Ok(response);

return Results.Created($"/users/{created.Id}", response);
return Results.NoContent();
```

O exemplo não resolve a tarefa inteira. Ele só mostra o tipo de resposta esperado para busca, criação e remoção.

### Checklist de execução

- [ ] Padronizar todas as rotas com o prefixo `/users`.
- [ ] Garantir que `GET /users` retorna `200 OK`.
- [ ] Garantir que `GET /users/{id}` retorna `200 OK` quando encontra usuário.
- [ ] Garantir que `GET /users/{id}` retorna `404 Not Found` quando não encontra usuário.
- [ ] Garantir que `POST /users` retorna `201 Created`.
- [ ] Garantir que o `Location` do `POST` aponta para `/users/{id}`.
- [ ] Garantir que `PUT /users/{id}` retorna `200 OK` quando atualiza usuário.
- [ ] Garantir que `PUT /users/{id}` retorna `404 Not Found` quando não encontra usuário.
- [ ] Garantir que `DELETE /users/{id}` retorna `204 No Content` quando remove usuário.
- [ ] Garantir que `DELETE /users/{id}` retorna `404 Not Found` quando não encontra usuário.
- [ ] Atualizar `csharp-user-registration-poc.http` com casos de sucesso e erro.

### Checklist de saída

- [ ] Todas as rotas começam com `/users`.
- [ ] O endpoint de criação retorna `201 Created`.
- [ ] O endpoint de criação retorna uma URL de recurso criado.
- [ ] O endpoint de remoção bem-sucedida retorna `204 No Content`.
- [ ] Usuário inexistente retorna `404 Not Found`.
- [ ] O `.http` permite testar o caminho feliz e pelo menos um erro.

### Perguntas de autoavaliação

- Por que `POST /users` deve retornar `201 Created`?
- Para que serve o cabeçalho `Location` depois de criar um recurso?
- Por que `DELETE` bem-sucedido pode retornar `204 No Content`?
- Por que usuário inexistente deve retornar `404 Not Found`?

### Validação

```bash
dotnet build csharp-user-registration-poc.slnx
dotnet run --project csharp-user-registration-poc.csproj
```

Verificar manualmente:

- [ ] `GET /users`
- [ ] `POST /users`
- [ ] `GET /users/{id}`
- [ ] `PUT /users/{id}`
- [ ] `DELETE /users/{id}`
- [ ] `GET /users/999`

### Referências para estudo

- [HTTP Status Code Registry - IANA](https://www.iana.org/assignments/http-status-codes/http-status-codes.xhtml)
- [HTTP Semantics, Status Codes - RFC 9110](https://www.rfc-editor.org/rfc/rfc9110.html#name-status-codes)
- [Route handlers em Minimal APIs - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis/route-handlers?view=aspnetcore-10.0)
- [Routing no ASP.NET Core - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/routing?view=aspnetcore-10.0)

## TAREFA 03 - Criar DTOs e não expor senha

### Apresentação

Com HTTP mais consistente, a próxima limitação fica evidente: a API ainda usa o mesmo tipo para entrada, modelo interno e resposta. Isso acopla o contrato público ao código interno e expõe senha.

### Crítica da etapa anterior

A tarefa anterior melhorou o protocolo, mas ainda permite que detalhes internos vazem para o cliente. O problema mais grave é a senha aparecer no modelo usado pela API.

### Conceito novo

DTO, ou Data Transfer Object, é um objeto desenhado para transportar dados entre processos ou camadas. Em uma Web API, DTOs definem o que entra e o que sai pelo HTTP.

DTOs são necessários porque:

- Permitem esconder campos sensíveis, como senha e futuramente `PasswordHash`.
- Evitam over-posting, quando o cliente envia campos que não deveria controlar.
- Reduzem acoplamento entre contrato HTTP e modelo interno.
- Permitem que request e response tenham formatos diferentes.
- Permitem evoluir o domínio sem quebrar imediatamente clientes externos.
- Tornam o contrato da API mais fácil de documentar e testar.

### Dica para iniciante

- Modelo interno é o objeto usado dentro da aplicação.
- DTO de request é o objeto recebido pela API.
- DTO de response é o objeto devolvido pela API.
- Mapeamento é a conversão entre modelo interno e DTO.
- Campo sensível é dado que não deve sair na resposta, como senha.
- Over-posting acontece quando o cliente envia campos extras tentando controlar algo que deveria ser decidido pela aplicação.

### Exemplo orientativo

Compare o contrato de entrada com o contrato de saída. Entrada pode receber senha; saída nunca deve devolver senha.

```csharp
public sealed record CreateUserRequest(string Email, string Password);

public sealed record UserResponse(long Id, string Email);
```

O aluno ainda precisa decidir onde usar cada DTO nos endpoints e onde fazer o mapeamento para o modelo interno.

### Checklist de execução

- [ ] Criar `CreateUserRequest`.
- [ ] Adicionar em `CreateUserRequest` apenas os campos que o cliente pode enviar na criação: `Email` e `Password`.
- [ ] Criar `UpdateUserRequest`.
- [ ] Adicionar em `UpdateUserRequest` apenas os campos que o cliente pode alterar nesta etapa: `Email` e `Password`.
- [ ] Criar `UserResponse`.
- [ ] Adicionar em `UserResponse` apenas os campos seguros para resposta: `Id` e `Email`.
- [ ] Alterar `POST /users` para receber `CreateUserRequest`.
- [ ] Alterar `PUT /users/{id}` para receber `UpdateUserRequest`.
- [ ] Alterar `GET /users` para retornar lista de `UserResponse`.
- [ ] Alterar `GET /users/{id}` para retornar `UserResponse`.
- [ ] Criar um mapeamento explícito de `User` para `UserResponse`.
- [ ] Garantir que `Password` nunca aparece em response.
- [ ] Atualizar `.http` para conferir que a resposta de criação não contém `password`.

### Checklist de saída

- [ ] Existe um DTO específico para criação.
- [ ] Existe um DTO específico para atualização.
- [ ] Existe um DTO específico para resposta.
- [ ] Nenhuma resposta HTTP contém `password`.
- [ ] O modelo interno pode mudar sem mudar automaticamente o contrato HTTP.

### Perguntas de autoavaliação

- Por que request DTO e response DTO podem ser diferentes?
- Qual risco aparece quando a API recebe diretamente o modelo interno?
- Como DTO reduz vazamento de campos sensíveis?
- O que é over-posting?

### Validação

```bash
dotnet build csharp-user-registration-poc.slnx
dotnet run --project csharp-user-registration-poc.csproj
```

Verificar manualmente:

- [ ] `POST /users` aceita email e senha.
- [ ] A resposta de `POST /users` contém `id` e `email`.
- [ ] A resposta de `POST /users` não contém `password`.
- [ ] `GET /users` não contém `password`.
- [ ] `GET /users/{id}` não contém `password`.

### Referências para estudo

- [Create Data Transfer Objects (DTOs) - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/web-api/overview/data/using-web-api-with-entity-framework/part-5)
- [Model validation no ASP.NET Core - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-10.0)

## TAREFA 04 - Adicionar validação com DataAnnotations e validação manual

### Apresentação

DTOs impedem vazamento de senha, mas a API ainda pode aceitar qualquer conteúdo. A nova evolução é rejeitar dados inválidos no limite da aplicação, antes que eles cheguem a regras, armazenamento ou banco futuro. A tarefa mostra DataAnnotations como caminho principal e validação manual como alternativa didática sem biblioteca externa.

### Crítica da etapa anterior

DTOs definem formato, mas formato não é regra suficiente. Um `CreateUserRequest` ainda pode chegar com email vazio, email inválido ou senha curta.

### Conceito novo

Validação de entrada: declarar regras mínimas que todo request precisa cumprir antes de executar o caso de uso.

Também entra uma distinção importante para DDD:

- Validação de DTO protege o boundary HTTP: formato, campos obrigatórios e tamanho.
- Validação manual mostra explicitamente o fluxo `receber request -> validar -> retornar erro`.
- Invariante de domínio protege o modelo interno: a entidade não pode existir em estado inválido, mesmo que outro caminho crie ou altere o objeto.

### Dica para iniciante

- DataAnnotations são atributos C# colocados em propriedades para declarar regras simples.
- `[Required]` exige valor.
- `[EmailAddress]` exige formato de email.
- `[Phone]` valida formato de telefone.
- `[CreditCard]` valida número de cartão de crédito.
- `[Url]` valida URL.
- `[RegularExpression]` valida texto usando regex.
- `[CustomValidation]` permite apontar para um método de validação customizado.
- `[StringLength]` limita tamanho mínimo ou máximo.
- Não existe `CpfAttribute` nativo no .NET 10. Para CPF, use validação manual ou customizada, porque CPF tem dígitos verificadores; regex sozinho valida formato, mas não garante CPF válido.
- `400 Bad Request` significa que o cliente enviou dados inválidos.
- Validação no backend é obrigatória mesmo que futuramente exista validação no Blazor.
- EF Core Fluent API é gratuita, mas serve para mapear modelo no banco, não para validar request HTTP.
- FluentValidation é uma biblioteca externa com licença Apache-2.0, mas não entra nesta tarefa para reduzir carga cognitiva.
- Validação manual simples ajuda o estagiário a enxergar a lógica antes de usar bibliotecas.

### Exemplo orientativo

Use DataAnnotations para declarar regras no DTO. O objetivo é tornar a regra visível perto do contrato de entrada.

```csharp
public sealed class CreateUserRequest
{
    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;
}
```

O exemplo orienta o formato. O aluno ainda precisa habilitar a validação em Minimal APIs e confirmar o `400 Bad Request`.

Se uma POC futura adicionar telefone, o DTO pode usar `[Phone]`. Não adicione telefone nesta tarefa se ele ainda não existir no contrato:

```csharp
[Phone]
public string? Phone { get; init; }
```

Para CPF, prefira validação manual ou customizada. Este exemplo mostra a forma, mas não implementa o algoritmo completo de CPF:

```csharp
if (!CpfValidator.IsValid(request.Cpf))
{
    errors["cpf"] = ["CPF inválido."];
}
```

Como alternativa didática, a mesma ideia pode aparecer em código manual. Esse exemplo não substitui a implementação principal com DataAnnotations nesta tarefa; ele mostra como a API poderia montar erros sem biblioteca externa:

```csharp
var errors = UserRequestValidator.Validate(request);

if (errors.Count > 0)
{
    return Results.ValidationProblem(errors);
}
```

O validador manual teria uma forma parecida com esta:

```csharp
public static Dictionary<string, string[]> Validate(CreateUserRequest request)
{
    var errors = new Dictionary<string, string[]>();

    if (string.IsNullOrWhiteSpace(request.Email))
    {
        errors["email"] = ["Email é obrigatório."];
    }

    if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
    {
        errors["password"] = ["Senha deve ter pelo menos 8 caracteres."];
    }

    return errors;
}
```

### Checklist de execução

- [ ] Adicionar DataAnnotations em `CreateUserRequest`.
- [ ] Adicionar `using System.ComponentModel.DataAnnotations;` nos arquivos de request.
- [ ] Marcar `Email` como obrigatório.
- [ ] Validar `Email` com formato de email.
- [ ] Definir tamanho máximo para `Email`.
- [ ] Marcar `Password` como obrigatório.
- [ ] Definir tamanho mínimo para `Password`.
- [ ] Definir tamanho máximo para `Password`.
- [ ] Registrar no roteiro que .NET 10 tem atributos prontos como `[EmailAddress]`, `[Phone]`, `[CreditCard]`, `[Url]` e `[RegularExpression]`.
- [ ] Registrar que CPF não possui atributo nativo no .NET 10 e exige validação manual/customizada.
- [ ] Repetir as regras necessárias em `UpdateUserRequest`.
- [ ] Habilitar validação para Minimal APIs em .NET 10.
- [ ] Ler o exemplo de validação manual e comparar com DataAnnotations.
- [ ] Registrar no próprio roteiro que a implementação principal desta tarefa usa DataAnnotations.
- [ ] Não adicionar FluentValidation nesta tarefa.
- [ ] Atualizar `.http` com payload sem email.
- [ ] Atualizar `.http` com email inválido.
- [ ] Atualizar `.http` com senha curta.
- [ ] Confirmar que payload inválido retorna `400 Bad Request`.

### Checklist de saída

- [ ] Requests inválidos são rejeitados antes de alterar a lista em memória.
- [ ] Requests válidos continuam funcionando.
- [ ] As regras estão visíveis nos DTOs.
- [ ] A tarefa explica quais validadores prontos do .NET podem ser usados em campos futuros.
- [ ] A tarefa deixa claro que CPF não é validador nativo do .NET.
- [ ] A alternativa de validação manual está documentada como recurso didático.
- [ ] Nenhuma biblioteca externa de validação foi adicionada.
- [ ] O `.http` tem exemplos de erro de validação.

### Perguntas de autoavaliação

- Por que validação no frontend não substitui validação no backend?
- Por que rejeitar entrada inválida no limite da aplicação?
- Qual diferença entre formato do DTO e regra de validação?
- Qual diferença entre validação de DTO e invariante de domínio?
- Quando usar um atributo pronto como `[EmailAddress]` ou `[Phone]`?
- Por que CPF precisa de validação própria e não apenas regex?
- Por que não usar FluentValidation antes de entender validação manual?

### Validação

```bash
dotnet build csharp-user-registration-poc.slnx
dotnet run --project csharp-user-registration-poc.csproj
```

Verificar manualmente:

- [ ] Email vazio retorna `400 Bad Request`.
- [ ] Email inválido retorna `400 Bad Request`.
- [ ] Senha curta retorna `400 Bad Request`.
- [ ] Request válido retorna sucesso.

### Referências para estudo

- [Validation support in Minimal APIs - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-10.0)
- [Model validation - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation?view=aspnetcore-10.0)
- [System.ComponentModel.DataAnnotations - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations?view=net-10.0)
- [EmailAddressAttribute - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.emailaddressattribute?view=net-10.0)
- [PhoneAttribute - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.phoneattribute?view=net-10.0)
- [CustomValidationAttribute - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.customvalidationattribute?view=net-10.0)
- [FluentValidation Minimal APIs - leitura opcional](https://github.com/fluentvalidation/fluentvalidation/blob/main/docs/aspnet.md)
- [FluentValidation License - leitura opcional](https://github.com/FluentValidation/FluentValidation/blob/main/License.txt)

## TAREFA 04.1 - Aprofundar User com contato e endereço

### Apresentação

A tarefa anterior ensinou validação com poucos campos: email e senha. Agora o cadastro fica mais próximo de um caso real, incluindo telefone e endereço. O objetivo não é criar uma ficha cadastral enorme; é praticar campos obrigatórios, campos opcionais, tamanho máximo, regex simples e validadores prontos do .NET 10.

### Crítica da etapa anterior

Validar só email e senha ensina o mecanismo, mas ainda esconde situações comuns: telefone pode ser opcional, endereço pode ter campos obrigatórios, CEP precisa de formato específico e response não deve vazar senha mesmo quando o modelo interno cresce.

### Conceito novo

Modelagem incremental do contrato: aumentar o objeto `User` aos poucos, mantendo DTOs, validação e response seguros.

Nesta tarefa, use DataAnnotations para praticar:

- `[Required]` em campos obrigatórios.
- `[StringLength]` em textos como cidade, estado, endereço e complemento.
- `[Phone]` em telefone.
- `[RegularExpression]` em CEP.
- Tipos nullable, como `string?`, para campos opcionais.

### Dica para iniciante

- Telefone pode ser opcional; por isso pode ser `string? Phone`.
- `[Phone]` valida formato geral de telefone, mas não garante todas as regras de telefone brasileiro com DDD.
- CEP não tem um atributo pronto específico no .NET. Para esta POC, use `[RegularExpression]` para validar formato, por exemplo `12345-678` ou `12345678`.
- Estado brasileiro pode começar como texto com 2 caracteres, por exemplo `SP`, `MG` ou `RJ`.
- Complemento de endereço normalmente é opcional.
- Não crie `Address` como Value Object ainda. Primeiro o aluno precisa entender DTO, validação e mapeamento simples.
- Não coloque DataAnnotations em `Domain` quando a solução modular chegar. A validação por atributo pertence ao contrato de entrada HTTP.

### Exemplo orientativo

O exemplo mostra a forma esperada dos campos. Ele não resolve todos os endpoints nem todos os mapeamentos.

```csharp
public sealed class CreateUserRequest
{
    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 8)]
    public string Password { get; init; } = string.Empty;

    [Phone]
    [StringLength(30)]
    public string? Phone { get; init; }

    [Required]
    [StringLength(200)]
    public string AddressLine { get; init; } = string.Empty;

    [StringLength(100)]
    public string? AddressComplement { get; init; }

    [Required]
    [StringLength(100)]
    public string City { get; init; } = string.Empty;

    [Required]
    [StringLength(2, MinimumLength = 2)]
    public string State { get; init; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{5}-?\d{3}$")]
    public string ZipCode { get; init; } = string.Empty;
}
```

O `UserResponse` deve devolver somente dados seguros:

```csharp
public sealed record UserResponse(
    long Id,
    string Email,
    string? Phone,
    string AddressLine,
    string? AddressComplement,
    string City,
    string State,
    string ZipCode);
```

### Checklist de execução

- [ ] Adicionar `Phone` ao DTO de criação.
- [ ] Adicionar `AddressLine` ao DTO de criação.
- [ ] Adicionar `AddressComplement` ao DTO de criação como campo opcional.
- [ ] Adicionar `City` ao DTO de criação.
- [ ] Adicionar `State` ao DTO de criação.
- [ ] Adicionar `ZipCode` ao DTO de criação.
- [ ] Repetir os campos necessários no DTO de atualização.
- [ ] Adicionar os mesmos dados ao modelo interno `User`.
- [ ] Atualizar `UserResponse` com os dados seguros de contato e endereço.
- [ ] Garantir que `UserResponse` continua sem `Password`.
- [ ] Usar `[Phone]` no telefone.
- [ ] Usar `[Required]` nos campos obrigatórios.
- [ ] Usar `[StringLength]` nos campos de texto.
- [ ] Usar `[RegularExpression]` para o formato de CEP.
- [ ] Atualizar o mapeamento de `User` para `UserResponse`.
- [ ] Atualizar o `POST /users` para preencher os novos campos.
- [ ] Atualizar o `PUT /users/{id}` para alterar os novos campos.
- [ ] Atualizar `.http` com um payload válido contendo telefone e endereço.
- [ ] Atualizar `.http` com CEP inválido.
- [ ] Atualizar `.http` com telefone inválido.
- [ ] Não adicionar biblioteca externa.
- [ ] Não criar Value Object ainda.

### Checklist de saída

- [ ] `User` possui dados de contato e endereço.
- [ ] Os DTOs de request validam email, senha, telefone, endereço, cidade, estado e CEP.
- [ ] Campos opcionais usam tipo nullable.
- [ ] Campo `Password` continua fora das responses.
- [ ] Payload válido cria usuário com os novos campos.
- [ ] Payload inválido retorna `400 Bad Request`.
- [ ] O aluno consegue explicar por que `[Phone]` e regex de CEP validam formato, mas não substituem regras de negócio completas.

### Perguntas de autoavaliação

- Por que telefone pode ser opcional?
- Por que `AddressComplement` deve ser opcional?
- Por que CEP pode usar regex nesta etapa?
- Por que regex de CEP não prova que o endereço existe?
- Por que ainda não criar um Value Object `Address`?
- Por que os novos campos precisam aparecer no DTO de response, mas a senha não?

### Validação

```bash
dotnet build csharp-user-registration-poc.slnx
dotnet run --project csharp-user-registration-poc.csproj
```

Verificar manualmente:

- [ ] `POST /users` com email, senha, telefone e endereço válidos retorna sucesso.
- [ ] `GET /users/{id}` mostra os dados de contato e endereço.
- [ ] `POST /users` com CEP inválido retorna `400 Bad Request`.
- [ ] `POST /users` com estado maior que 2 caracteres retorna `400 Bad Request`.
- [ ] Nenhuma resposta contém `password`.

### Referências para estudo

- [Validation support in Minimal APIs - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-10.0)
- [System.ComponentModel.DataAnnotations - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations?view=net-10.0)
- [PhoneAttribute - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.phoneattribute?view=net-10.0)
- [RegularExpressionAttribute - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.regularexpressionattribute?view=net-10.0)
- [StringLengthAttribute - Microsoft Learn](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.stringlengthattribute?view=net-10.0)

## TAREFA 05 - Adicionar OpenAPI e Scalar

### Apresentação

A API agora tem DTOs, validação e um cadastro com mais campos, mas seu contrato ainda precisa ser descoberto lendo código ou executando o `.http`. Esta tarefa gera o documento OpenAPI e adiciona Scalar para visualizar os endpoints no navegador.

### Crítica da etapa anterior

Validação melhora a API, mas não ajuda outro desenvolvedor a descobrir rapidamente quais endpoints existem, quais payloads aceitam e quais respostas retornam.

### Conceito novo

OpenAPI descreve a API em um documento padronizado. Scalar usa esse documento para criar uma interface visual e interativa no navegador.

### Dica para iniciante

- OpenAPI é a documentação técnica da API em formato JSON.
- `/openapi/v1.json` é o endpoint que mostra esse JSON bruto.
- Scalar é uma página web que lê o OpenAPI e mostra endpoints de forma navegável.
- Schema é a descrição dos campos esperados no request ou response.
- `WithSummary` e `WithDescription` ajudam humanos a entender cada endpoint na UI.
- Ambiente de desenvolvimento é o modo local usado para programar e testar, normalmente antes de produção.

### Exemplo orientativo

`MapOpenApi` expõe o JSON do contrato. Ele não cria, sozinho, uma página navegável para o aluno explorar endpoints. Scalar usa esse JSON e cria a UI no browser.

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
```

Depois de subir a API, acesse `/openapi/v1.json` para ver o documento bruto e `/scalar` para ver a página visual.

### Checklist de execução

- [ ] Confirmar que o projeto tem `Microsoft.AspNetCore.OpenApi`.
- [ ] Registrar `builder.Services.AddOpenApi();`.
- [ ] Mapear `app.MapOpenApi();` apenas em ambiente de desenvolvimento.
- [ ] Instalar Scalar:

```bash
dotnet add package Scalar.AspNetCore
```

- [ ] Adicionar `using Scalar.AspNetCore;` no arquivo principal.
- [ ] Mapear `app.MapScalarApiReference();` apenas em ambiente de desenvolvimento.
- [ ] Adicionar `WithName` nos endpoints.
- [ ] Adicionar `WithSummary` nos endpoints.
- [ ] Adicionar `WithDescription` nos endpoints.
- [ ] Declarar respostas com `Produces`.
- [ ] Declarar erros com `ProducesProblem` ou `ProducesValidationProblem` quando aplicável.
- [ ] Atualizar `.http` com chamada para `/openapi/v1.json`.
- [ ] Documentar que `/openapi/v1.json` é o documento bruto.
- [ ] Documentar que `/scalar` é a página visual para estudar e testar a API no browser.

### Checklist de saída

- [ ] `/openapi/v1.json` responde em desenvolvimento.
- [ ] `http://localhost:5291/scalar` abre a UI do Scalar no navegador.
- [ ] A UI mostra os endpoints de usuários.
- [ ] A UI mostra schemas de request e response.
- [ ] A UI mostra os principais status codes.

### Perguntas de autoavaliação

- Qual diferença entre o documento `/openapi/v1.json` e a página `/scalar`?
- Por que OpenAPI ajuda frontend, testes e outros consumidores?
- Por que documentar status codes também faz parte do contrato?

### Validação

```bash
dotnet build csharp-user-registration-poc.slnx
dotnet run --project csharp-user-registration-poc.csproj
```

Abrir no navegador:

- [ ] `http://localhost:5291/openapi/v1.json`
- [ ] `http://localhost:5291/scalar`

### Referências para estudo

- [Generate OpenAPI documents - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-10.0)
- [Use generated OpenAPI documents and Scalar - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/using-openapi-documents?view=aspnetcore-10.0)
- [Scalar ASP.NET Core integration](https://scalar.com/products/api-references/integrations/aspnetcore/integration)

## TAREFA 06 - Padronizar erros com ProblemDetails

### Apresentação

O contrato de sucesso já está mais claro, mas erro também faz parte da API. Esta tarefa padroniza respostas de falha para que clientes não precisem tratar formatos imprevisíveis.

### Crítica da etapa anterior

OpenAPI torna endpoints visíveis, mas se os erros não forem previsíveis, o frontend ainda terá dificuldade para exibir mensagens e tratar falhas.

### Conceito novo

ProblemDetails é um formato padronizado para representar erros HTTP em APIs.

### Dica para iniciante

- Erro esperado é uma falha prevista, como usuário não encontrado.
- Exceção inesperada é uma falha não planejada, como bug ou indisponibilidade.
- `ProblemDetails` transforma erro em JSON previsível.
- Não exponha `exception.Message` em produção porque pode revelar detalhes internos.
- Formato previsível ajuda frontend e testes a tratarem erros sem adivinhação.
- Status code pages ajudam a produzir respostas padronizadas para status codes sem corpo detalhado.

### Exemplo orientativo

O aluno deve procurar um formato previsível, parecido com este, em vez de texto solto:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
  "title": "Not Found",
  "status": 404
}
```

O conteúdo exato pode variar conforme configuração, mas o formato deve continuar estruturado e previsível.

### Checklist de execução

- [ ] Registrar `builder.Services.AddProblemDetails();`.
- [ ] Configurar exception handling global.
- [ ] Configurar status code pages quando fizer sentido.
- [ ] Garantir que `404` retorna formato previsível.
- [ ] Garantir que validação retorna formato previsível.
- [ ] Evitar retornar `exception.Message` em produção.
- [ ] Evitar `try/catch` repetitivo em cada endpoint.
- [ ] Atualizar OpenAPI com respostas de erro quando aplicável.
- [ ] Atualizar `.http` com `GET /users/999`.
- [ ] Atualizar `.http` com `POST /users` inválido.

### Checklist de saída

- [ ] Erros comuns retornam formato previsível.
- [ ] Mensagens internas não vazam ao cliente.
- [ ] Endpoints continuam focados no fluxo principal.
- [ ] O `.http` demonstra pelo menos um erro de validação e um `404`.

### Perguntas de autoavaliação

- Qual diferença entre erro esperado e exceção inesperada?
- Por que erro também faz parte do contrato público da API?
- Por que não retornar `exception.Message` em produção?

### Validação

```bash
dotnet build csharp-user-registration-poc.slnx
dotnet run --project csharp-user-registration-poc.csproj
```

Verificar manualmente:

- [ ] `GET /users/999`
- [ ] `POST /users` com payload inválido

### Referências para estudo

- [Handle errors in ASP.NET Core APIs - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling-api?view=aspnetcore-10.0)
- [OpenAPI metadata for ProblemDetails - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/include-metadata?view=aspnetcore-10.0)

## TAREFA 07 - Refatorar com Clean Code e rich domain model no projeto único

### Apresentação

A API já tem um contrato melhor, mas o código ainda pode concentrar endpoint, modelo, mapeamento, armazenamento e configuração em poucos lugares. Além disso, `User` ainda pode ser uma classe anêmica: guarda dados de identidade, contato e endereço, mas não protege suas próprias regras. Esta tarefa organiza responsabilidades sem criar vários projetos ainda e começa a transformar `User` em rich class.

### Crítica da etapa anterior

Ter contratos e erros melhores não resolve o problema de manutenção se tudo continuar em um arquivo grande. Também não resolve se endpoints ou stores alteram `Email`, `Password`, `Phone`, `AddressLine`, `City`, `State` e `ZipCode` diretamente. Um iniciante precisa saber exatamente para onde cada responsabilidade deve ir e por que regra de negócio deve morar perto do dado que ela protege.

### Conceito novo

Clean Code aplicado incrementalmente: separar responsabilidades por arquivos e pastas, mantendo o mesmo projeto e preservando o comportamento.

Também entra o primeiro passo de rich domain model:

- Modelo anêmico: classe com dados públicos e pouca ou nenhuma regra.
- Rich class: classe com dados e comportamento, protegendo suas próprias invariantes.
- Invariante: regra que precisa ser sempre verdadeira para o objeto ser válido.
- Factory method: método como `User.Create(...)` usado para criar objeto válido desde o início.
- Guard clause: `if` curto que rejeita valor inválido cedo.

### Dica para iniciante

- Responsabilidade é aquilo que um arquivo ou classe deve cuidar.
- Endpoint cuida de HTTP: rota, status code, request e response.
- Handler é o código executado quando um endpoint recebe uma request.
- Service ou store cuida de operação de dados.
- Interface define o contrato que outra classe precisa implementar.
- DI, ou Dependency Injection, entrega dependências prontas para o endpoint usar.
- Middleware é uma etapa do pipeline HTTP executada antes ou depois dos endpoints.
- Método de extensão permite chamar `app.MapUserEndpoints()` como se fosse método do próprio `app`.
- Refatorar significa melhorar organização sem mudar comportamento externo.
- `private set` permite leitura pública, mas impede alteração direta fora da classe.
- `ChangeEmail`, `ChangePassword`, `ChangePhone` e `ChangeAddress` expressam intenção melhor que alterar propriedades diretamente.
- `ChangeAddress` ainda pode receber campos simples. Um Value Object `Address` pode entrar depois, quando o aluno já entender entidade rica.
- Senha ainda fica em texto nesta etapa por didática; hash entra apenas na POC 15.
- O contador `nextId` continua temporário enquanto a POC usa memória; ele não é uma regra de negócio.

### Exemplo orientativo

`Program.cs` deve perder conhecimento sobre detalhes de usuários. Ele chama uma extensão, e a extensão concentra rotas de usuário.

```csharp
public static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/users");

        // group.MapGet(...);
        // group.MapPost(...);

        return app;
    }
}
```

No `Program.cs`, a intenção deve ficar evidente:

```csharp
builder.Services.AddSingleton<IUserStore, InMemoryUserStore>();

app.MapUserEndpoints();
```

O exemplo mostra o desenho. O aluno ainda precisa mover cada endpoint, preservar status codes e testar o comportamento.

`User` também deve começar a proteger regras próprias. O exemplo abaixo mostra a intenção, sem entregar a classe final. O `nextId` aparece apenas porque esta POC ainda usa memória; quando o banco entrar, a estratégia de identidade será revisada.

```csharp
public sealed class User
{
    private static long nextId;

    public long Id { get; }
    public string Email { get; private set; }
    public string Password { get; private set; }
    public string? Phone { get; private set; }
    public string AddressLine { get; private set; }
    public string? AddressComplement { get; private set; }
    public string City { get; private set; }
    public string State { get; private set; }
    public string ZipCode { get; private set; }

    private User(
        long id,
        string email,
        string password,
        string? phone,
        string addressLine,
        string? addressComplement,
        string city,
        string state,
        string zipCode)
    {
        Id = id;
        Email = email;
        Password = password;
        Phone = phone;
        AddressLine = addressLine;
        AddressComplement = addressComplement;
        City = city;
        State = state;
        ZipCode = zipCode;
    }

    public static User Create(
        string email,
        string password,
        string? phone,
        string addressLine,
        string? addressComplement,
        string city,
        string state,
        string zipCode)
    {
        // validar email, senha, telefone e endereco antes de criar
        return new User(
            nextId++,
            email,
            password,
            phone,
            addressLine,
            addressComplement,
            city,
            state,
            zipCode);
    }

    public void ChangeEmail(string email)
    {
        // validar email antes de alterar
        Email = email;
    }

    public void ChangePassword(string password)
    {
        // validar senha antes de alterar
        Password = password;
    }

    public void ChangePhone(string? phone)
    {
        // validar formato quando telefone for informado
        Phone = phone;
    }

    public void ChangeAddress(string addressLine, string? complement, string city, string state, string zipCode)
    {
        // validar endereco, cidade, estado e CEP antes de alterar
        AddressLine = addressLine;
        AddressComplement = complement;
        City = city;
        State = state;
        ZipCode = zipCode;
    }
}
```

O endpoint ou store não deve mais fazer `user.Email = request.Email` nem `user.City = request.City`. Ele deve chamar comportamentos explícitos, como `user.ChangeEmail(request.Email)` e `user.ChangeAddress(...)`.

### Checklist de execução

- [ ] Criar a pasta `Endpoints`.
- [ ] Criar `Endpoints/UserEndpoints.cs`.
- [ ] Mover o mapeamento de rotas de usuário para `UserEndpoints`.
- [ ] Criar um método de extensão `MapUserEndpoints(this WebApplication app)` ou `MapUserEndpoints(this IEndpointRouteBuilder app)`.
- [ ] Fazer `Program.cs` chamar apenas `app.MapUserEndpoints();` para rotas de usuário.
- [ ] Criar a pasta `Models`.
- [ ] Criar `Models/User.cs`.
- [ ] Mover o modelo interno `User` para `Models/User.cs`.
- [ ] Transformar `User` em rich class simples.
- [ ] Trocar setters públicos por `private set`.
- [ ] Criar factory `User.Create(...)` recebendo email, senha, telefone e endereço.
- [ ] Criar método `ChangeEmail(email)`.
- [ ] Criar método `ChangePassword(password)`.
- [ ] Criar método `ChangePhone(phone)`.
- [ ] Criar método `ChangeAddress(addressLine, complement, city, state, zipCode)`.
- [ ] Adicionar guard clauses internas para impedir email vazio, email inválido, senha curta, endereço vazio, cidade vazia, estado inválido e CEP inválido.
- [ ] Fazer endpoints ou store chamarem `User.Create`, `ChangeEmail`, `ChangePassword`, `ChangePhone` e `ChangeAddress`.
- [ ] Remover alterações diretas como `user.Email = ...`, `user.Password = ...` e `user.City = ...` fora da classe `User`.
- [ ] Manter `Password` apenas no modelo interno nesta etapa; hash entra na POC 15.
- [ ] Criar a pasta `Requests`.
- [ ] Criar `Requests/CreateUserRequest.cs`.
- [ ] Criar `Requests/UpdateUserRequest.cs`.
- [ ] Criar a pasta `Responses`.
- [ ] Criar `Responses/UserResponse.cs`.
- [ ] Criar a pasta `Mappings`.
- [ ] Criar `Mappings/UserMappings.cs`.
- [ ] Implementar método de mapeamento de `User` para `UserResponse`.
- [ ] Criar a pasta `Services`.
- [ ] Criar `Services/IUserStore.cs`.
- [ ] Definir em `IUserStore` operações de listagem, busca por id, criação, atualização e remoção.
- [ ] Criar `Services/InMemoryUserStore.cs`.
- [ ] Mover a lista em memória para `InMemoryUserStore`.
- [ ] Registrar `IUserStore` no DI em `Program.cs`.
- [ ] Injetar `IUserStore` nos handlers dos endpoints.
- [ ] Remover a lista global de usuários do arquivo principal.
- [ ] Deixar `Program.cs` somente com builder, registro de services, middleware, OpenAPI/Scalar, ProblemDetails e `app.MapUserEndpoints();`.
- [ ] Preservar rotas, status codes, DTOs, validação, OpenAPI e ProblemDetails já implementados.

### Estrutura esperada

```text
Endpoints/
  UserEndpoints.cs
Mappings/
  UserMappings.cs
Models/
  User.cs
Requests/
  CreateUserRequest.cs
  UpdateUserRequest.cs
Responses/
  UserResponse.cs
Services/
  IUserStore.cs
  InMemoryUserStore.cs
Program.cs
```

### Responsabilidades esperadas

- [ ] `UserEndpoints.cs` conhece HTTP e chama o store.
- [ ] `User.cs` representa o modelo interno em memória e protege regras básicas de usuário.
- [ ] `CreateUserRequest.cs` representa entrada de criação.
- [ ] `UpdateUserRequest.cs` representa entrada de atualização.
- [ ] `UserResponse.cs` representa saída segura.
- [ ] `UserMappings.cs` converte modelo interno para DTO de resposta.
- [ ] `IUserStore.cs` define o contrato de armazenamento.
- [ ] `InMemoryUserStore.cs` contém a lista em memória e suas operações.
- [ ] `Program.cs` configura a aplicação, mas não contém regra de usuário.

### Checklist de saída

- [ ] `Program.cs` ficou pequeno e focado em configuração.
- [ ] Nenhuma pasta mistura request, response, modelo e endpoint no mesmo arquivo.
- [ ] A lista em memória não está mais no arquivo principal.
- [ ] Endpoints continuam retornando os mesmos status codes.
- [ ] DTOs continuam protegendo senha.
- [ ] `User` não tem setters públicos para `Email`, `Password`, `Phone`, `AddressLine`, `AddressComplement`, `City`, `State` e `ZipCode`.
- [ ] Criação de usuário passa por `User.Create(...)`.
- [ ] Alteração de email, senha, telefone e endereço passa por métodos da própria classe `User`.
- [ ] Nenhuma abstração foi criada sem uso real.

### Perguntas de autoavaliação

- Qual responsabilidade de cada arquivo criado nesta tarefa?
- Por que `Program.cs` deve ficar focado em configuração?
- Por que `IUserStore` existe antes de EF Core?
- Que abstração seria exagerada nesta etapa?
- O que torna uma classe anêmica?
- Por que `User` deve proteger suas próprias regras?
- Qual diferença entre validar request e proteger invariante da entidade?

### Validação

```bash
dotnet build csharp-user-registration-poc.slnx
dotnet run --project csharp-user-registration-poc.csproj
```

Verificar manualmente:

- [ ] `GET /users`
- [ ] `POST /users`
- [ ] `GET /users/{id}`
- [ ] `PUT /users/{id}`
- [ ] `DELETE /users/{id}`
- [ ] `GET /openapi/v1.json`
- [ ] `GET /scalar` no navegador

### Referências para estudo

- [Dependency injection in ASP.NET Core - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-10.0)
- [Minimal APIs quick reference - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-10.0)

## TAREFA 08 - Migrar para solução modular

### Apresentação

Pastas melhoram organização, mas não impedem dependências erradas. Esta tarefa transforma a POC em uma solução modular, criando fronteiras reais entre domínio, aplicação, infraestrutura e API. A `User` rica criada na tarefa anterior passa a morar no projeto `Domain`.

### Crítica da etapa anterior

No projeto único, qualquer arquivo ainda pode referenciar qualquer outro. Isso facilita atalhos que quebram separação de responsabilidades conforme o sistema cresce. Uma `User` rica perde valor se começar a depender de HTTP, DTO, ProblemDetails, EF Core ou atributos de validação de entrada.

### Conceito novo

Solução modular: separar o sistema em projetos com direção de dependência controlada.

Também entra reforço de DDD tático:

- `Domain` contém a entidade rica e suas invariantes.
- `Application` orquestra o caso de uso, mas não decide regra interna de `User`.
- `Infrastructure` implementa detalhes técnicos, como armazenamento.
- `Api` traduz HTTP para comandos ou chamadas de aplicação.

### Dica para iniciante

- Solution é o agrupamento de projetos .NET.
- Projeto é uma unidade compilável com seu próprio `.csproj`.
- Referência entre projetos permite um projeto usar código de outro.
- `Domain` contém regras e conceitos centrais.
- `Application` coordena casos de uso e contratos.
- `Infrastructure` contém detalhes técnicos, como armazenamento.
- `Api` contém HTTP, endpoints, OpenAPI e configuração web.
- Template `webapi` cria exemplo inicial; nesta tarefa ele deve ser removido ou substituído pelo código da POC.
- `Domain` não deve usar DataAnnotations, DTOs HTTP, `Results`, `ProblemDetails`, Scalar, ASP.NET Core ou EF Core.
- A entidade `User` deve continuar sendo criada por `User.Create(...)` e alterada por métodos como `ChangeEmail(...)`, `ChangePhone(...)` e `ChangeAddress(...)`.

### Exemplo orientativo

A direção de dependência deve apontar para dentro, não para detalhes técnicos:

```text
UserRegistration.Api -> UserRegistration.Application
UserRegistration.Api -> UserRegistration.Infrastructure
UserRegistration.Infrastructure -> UserRegistration.Application
UserRegistration.Application -> UserRegistration.Domain
UserRegistration.Domain -> nenhuma camada da aplicação
```

Comandos típicos para criar a estrutura, sem ainda mover todo o código:

```bash
dotnet new classlib -n UserRegistration.Domain -o src/UserRegistration.Domain
dotnet new classlib -n UserRegistration.Application -o src/UserRegistration.Application
dotnet new classlib -n UserRegistration.Infrastructure -o src/UserRegistration.Infrastructure
dotnet new webapi -n UserRegistration.Api -o src/UserRegistration.Api
dotnet sln csharp-user-registration-poc.slnx add src/UserRegistration.Domain/UserRegistration.Domain.csproj
dotnet sln csharp-user-registration-poc.slnx add src/UserRegistration.Application/UserRegistration.Application.csproj
dotnet sln csharp-user-registration-poc.slnx add src/UserRegistration.Infrastructure/UserRegistration.Infrastructure.csproj
dotnet sln csharp-user-registration-poc.slnx add src/UserRegistration.Api/UserRegistration.Api.csproj
```

O aluno ainda precisa adicionar referências entre projetos, mover os arquivos certos e validar que o domínio não conhece HTTP, banco, Scalar, ASP.NET Core, EF Core, DTOs ou ProblemDetails.

### Checklist de execução

- [ ] Criar `src/UserRegistration.Domain`.
- [ ] Criar `src/UserRegistration.Application`.
- [ ] Criar `src/UserRegistration.Infrastructure`.
- [ ] Criar `src/UserRegistration.Api`.
- [ ] Mover a `User` rica para `UserRegistration.Domain`.
- [ ] Garantir que `UserRegistration.Domain` não possui referência a ASP.NET Core, EF Core, Scalar ou pacotes de validação HTTP.
- [ ] Garantir que `User` não usa DataAnnotations.
- [ ] Mover contratos de casos de uso e interfaces necessárias para `UserRegistration.Application`.
- [ ] Mover `IUserStore` para `UserRegistration.Application`, pois a aplicação deve depender de contrato e não da implementação em memória.
- [ ] Mover `InMemoryUserStore` para `UserRegistration.Infrastructure`.
- [ ] Mover endpoints e configuração HTTP para `UserRegistration.Api`.
- [ ] Remover ou substituir endpoints de exemplo criados pelo template `webapi`.
- [ ] Manter DTOs HTTP de request e response na camada `Api`, a menos que uma tarefa futura crie contratos próprios de aplicação.
- [ ] Configurar referências: `Application` referencia `Domain`.
- [ ] Configurar referências: `Infrastructure` referencia `Application` e `Domain` quando necessário.
- [ ] Configurar referências: `Api` referencia `Application` e `Infrastructure`.
- [ ] Garantir que `Domain` não referencia ASP.NET Core.
- [ ] Garantir que `Domain` não referencia EF Core.
- [ ] Garantir que `Domain` não referencia DTOs, `Results` ou `ProblemDetails`.
- [ ] Garantir que `Application` não referencia `Infrastructure`.
- [ ] Atualizar a solution.
- [ ] Remover o projeto antigo apenas depois que a nova API compilar e responder.

### Checklist de saída

- [ ] A solução possui projetos separados em `src/`.
- [ ] `Domain` contém a `User` rica e não conhece HTTP.
- [ ] `Application` contém contratos e orquestração de casos de uso.
- [ ] `Infrastructure` contém implementação técnica do armazenamento em memória.
- [ ] `Api` contém endpoints, OpenAPI, Scalar, ProblemDetails e configuração HTTP.
- [ ] A direção das dependências impede que domínio dependa de infraestrutura.
- [ ] `User` continua protegendo suas invariantes dentro de `Domain`.
- [ ] Os endpoints existentes continuam funcionando.

### Perguntas de autoavaliação

- Como fica o fluxo de execução: HTTP -> Api -> Application -> contrato -> Infrastructure -> Domain?
- Por que `Domain` não deve referenciar ASP.NET Core?
- Por que `Application` não deve referenciar `Infrastructure`?
- Qual problema projetos separados resolvem que pastas não resolvem?
- Por que DataAnnotations pertencem ao DTO HTTP e não à entidade de domínio?
- Por que Application orquestra, mas `User` decide suas próprias regras?

### Validação

```bash
dotnet build
dotnet run --project src/UserRegistration.Api/UserRegistration.Api.csproj
```

Verificar manualmente:

- [ ] API sobe pelo novo projeto `UserRegistration.Api`.
- [ ] `GET /users` continua funcionando.
- [ ] `POST /users` continua funcionando.
- [ ] `GET /openapi/v1.json` continua funcionando.
- [ ] `GET /scalar` continua funcionando no navegador.

### Referências para estudo

- [Dependency injection in ASP.NET Core - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-10.0)
- [ASP.NET Core fundamentals overview - Microsoft Learn](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/?view=aspnetcore-10.0)

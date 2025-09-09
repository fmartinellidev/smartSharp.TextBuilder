# TextBuilder — Manipulação de Texto de Alta Performance em C#

## Visão Geral

O **TextBuilder** é uma biblioteca desenvolvida em **C# 13** sobre o ambiente **.NET 9**, voltada para **busca, edição e análise de texto** com foco em **alta performance**, **baixo consumo de memória** e **API intuitiva**.  
Seu diferencial está na capacidade de realizar operações complexas — como busca com curingas, substituições posicionais, manipulação de snippets HTML — sem depender de bibliotecas externas.

## 📦 Dependências e Segurança

- ✅ Zero dependências externas  
- ✅ Utiliza apenas 4 bibliotecas nativas do C#  
- ✅ Segurança na manutenção de versões  
- ✅ Escalabilidade nativa  
- ✅ Sem conflitos com pacotes de terceiros  

## Arquitetura Interna

Inspirado no padrão **MVC**, adaptado para operações de texto:

| Camada       | Função                                                                 |
|--------------|------------------------------------------------------------------------|
| Constructor  | Recebe parâmetros em alto nível (`string`) e converte para `Span`      |
| Controller   | Interpreta o tipo de operação e organiza os parâmetros                 |
| Model        | Executa a busca real e retorna coordenadas ou trechos encontrados      |

## 🚀 Benchmark Comparativo

### 🔍 Busca com OR

| Método            | Tempo Médio | Memória | GC Gen0 |
|------------------|-------------|---------|---------|
| TextBuilder       | 489.9 ns    | 40 B    | 0.0029  |
| Regex             | 385.7 ns    | 208 B   | 0.0162  |

### 🔡 Busca com IgnoreCase

| Método            | Tempo Médio | Memória | GC Gen0 |
|------------------|-------------|---------|---------|
| TextBuilder       | 480.2 ns    | 40 B    | 0.0029  |
| Regex             | 772.5 ns    | 208 B   | 0.0162  |

### 🧩 Wildcard Match

| Método            | Tempo Médio | Memória |
|------------------|-------------|---------|
| TextBuilder       | 253.3 ns    | 3.456 B |
| Regex             | 1.473.463 ns| 248 B   |

### 🔢 Padrões Compostos

| Método            | Tempo Médio | Memória |
|------------------|-------------|---------|
| TextBuilder       | 251.8 ns    | 120 B   |
| Regex             | 764.6 ns    | 416 B   |

---

## 🧬 Opções de Sintaxe

O TextBuilder oferece múltiplas formas de uso, adaptando-se ao estilo e à necessidade de cada desenvolvedor:

### 🔹 Instância direta do `TextMatcher`

```csharp
TextMatcher builder = new TextMatcher(text);
builder.CaseSensitive = false;
builder.EnableIgnoreCharsInQuotes();
StringAndPosition firstMatch = builder.Match("john doe|marie doe");
```

---

### 🔹 Bloco `IDisposable` com configuração interna

```csharp
StringAndPosition firstMatch;
using (var builder = new TextMatcher(text))
{
    builder.DisableCaseSensitive();
    builder.EnableIgnoreCharsInQuotes();
    builder.EnableIgnoreCharsInDoubleQuotes();
    firstMatch = builder.Match("John Doe|Marie Doe");
}
```

---

### 🔹 Instância inline com propriedades configuradas

```csharp
StringAndPosition firstMatch = new TextMatcher(text)
{
    CaseSensitive = false
}.Match("john doe|marie doe");
```

---

### 🔹 Uso direto via classe estática `TextBuilder`

```csharp
StringAndPosition firstMatch = TextBuilder.Match(text, "Marie Doe|Jane Doe|Jack|John Doe", TextOpt.MatchWholeWordOnly);
```

---

## ⚙️ Parâmetros de Configuração (`TextOpt`)

Estes parâmetros podem ser usados para configurar o comportamento das buscas e operações do TextBuilder:

| Parâmetro                        | Descrição                                                                 |
|----------------------------------|---------------------------------------------------------------------------|
| `CaseSensitive`                 | Considera diferenciação entre maiúsculas e minúsculas                     |
| `IgnoreCharsInQuotes`          | Ignora conteúdo entre aspas simples (`'...'`) durante o parsing           |
| `IgnoreCharsInDoubleQuotes`    | Ignora conteúdo entre aspas duplas (`"..."`) durante o parsing            |
| `IgnoreDynamicChars`           | Identifica e ignora caracteres dinâmicos no padrão e no texto             |
| `MatchGreedyOccurences`        | Não força busca pela ocorrência mais curta; permite busca gulosa         |
| `MatchWholeWordOnly`           | Retorna apenas ocorrências que sejam palavras inteiras                    |

---

Esses exemplos mostram como o TextBuilder pode ser adaptado para diferentes cenários — desde buscas simples até parsing avançado com múltiplas regras.

---

## 🧪 Exemplos de Uso

### 🔍 Match

```csharp
TextBuilder.Match("Marie Doe|Jane Doe|Jack|John Doe");
TextBuilder.Match("*residential");
TextBuilder.Match("Name:*cidade de *.");
TextBuilder.Match("email*@hotmail.com|@gmail.com|@yahoo.com");
```

### ✍️ Inserção

Perfeito, Fernando! Aqui está a tabela organizada para a função **Insert**, com cada variação explicada de forma clara e objetiva. Essa estrutura é ideal para incluir no `README.md` ou na documentação técnica:

---

## ✍️ Tabela de Funções — Inserção de Palavras

| Método                        | Descrição                                                                                   | Exemplo de Uso                                                                 |
|------------------------------|----------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------|
| `Insert(text, value, index)` | Insere o conteúdo `value` diretamente na posição `index` do texto                          | `Insert(text, "the client ", 75)`                                              |
| `InsertBeforeFirst(text, value, pattern)` | Insere `value` antes da **primeira ocorrência** do `pattern` no texto         | `InsertBeforeFirst(text, "the client ", "Marie")`                              |
| `InsertAfterFirst(text, value, pattern)`  | Insere `value` após a **primeira ocorrência** do `pattern` no texto           | `InsertAfterFirst(text, "Marie", " the client")`                               |
| `InsertBefore(text, pattern, value)`      | Insere `value` **antes de todas** as ocorrências do `pattern` no texto        | `InsertBefore(text, "<o>", ",")`                                               |
| `InsertAfter(text, pattern, value)`       | Insere `value` **após todas** as ocorrências do `pattern` no texto            | `InsertAfter(text, "<o>", ",")`                                                |

```csharp
TextBuilder.Insert(text, "the client ", 75);
TextBuilder.InsertBeforeFirst(text, "the client ", "Marie");
TextBuilder.InsertBefore(text, "<o>", ",");
TextBuilder.InsertAfter(text, "<o>", ",");
TextBuilder.InsertAfterFirst(text, "the client ", "Marie");
```

### 📌 Observações Técnicas

- Todos os métodos têm tempo médio de execução de **1ms** e alocação de memória de aproximadamente **6.216 bytes**.
- Suporte a padrões com curingas (`*`) e múltiplas ocorrências.
- Preservam a integridade do texto original, realizando inserções precisas.

---

### 🧹 Remoção

```csharp
TextBuilder.RemoveFirst(text, "Marie Doe Towner ");
TextBuilder.Remove(text, ",");
```

### 🔁 Substituição

```csharp
TextBuilder.ReplaceFirst(text, "Marie Doe Towner", "Jene Doe Sanders");
TextBuilder.Replace(text, ",", "<o>");
```

### 🔄 Tradução Posicional

```csharp
TextBuilder.TranslateFirst(text, "Doe;married;,", "Silva;Divorced;<o>");
TextBuilder.Translate(text, "Doe;married;,", "Silva;Divorced;<o>");
```

### 🔎 Verificação

```csharp
TextBuilder.Contains(text, "John "); // True
TextBuilder.Contains(text, "kkkkkkkbua"); // False
TextBuilder.Contains(text, "Mar*ner"); // True
```

### 🔢 Contagem

```csharp
TextBuilder.Cont(text, "act"); // 6
TextBuilder.Cont(text, "r*act"); // 4
TextBuilder.Cont(text, "r*act", TextOpt.MatchWholeWordOnly); // 3
```

## 🧱 Snippets in Text

### 🔍 Match de Blocos

```csharp
TextBuilder.Snippet(html, "<div", "</div>");
TextBuilder.Snippet(html, "<div*id='divTemp'", "</div>");
```

### ✍️ Inserção em Blocos

```csharp
TextBuilder.InsertSnippet(html, "<input ... />", 346);
TextBuilder.InsertSnippetBefore(html, "<span", "/span>", "<input ... />");
TextBuilder.InsertSnippetAfter(html, "<div*divUnitPopup_group", "/div>", "<input ... />");
```

### 🧹 Remoção de Blocos

```csharp
TextBuilder.RemoveSnippetFirst(html, "<div*divUnitPopup_group", "/div>");
TextBuilder.RemoveSnippet(html, "<span", "/span>*\r\n");
```

### 🔁 Substituição de Blocos

```csharp
TextBuilder.ReplaceSnippetFirst(html, "<div*divUnitPopup_group", "/div>", "<article ... />");
TextBuilder.ReplaceSnippet(html, "<span", "/span>", "<article ... />");
```

### 🔎 Verificação de Blocos

```csharp
TextBuilder.ContainsSnippet(html, "<div*divUnitPopup_group", "/div>"); // True
TextBuilder.ContainsSnippet(html, "<article", "/article>"); // False
```

### 🔢 Contagem de Blocos

```csharp
TextBuilder.ContSnippets(html, "<div*divUnitPopup_group", "/div>"); // 1
TextBuilder.ContSnippets(html, "<span", "/span>"); // 3
```

Excelente observação, Fernando! Esse comportamento é um dos diferenciais mais inteligentes do TextBuilder, e merece destaque na documentação. Aqui está a seção que você pode adicionar ao `README.md` para explicar isso com clareza:

---

## 🧠 Reconhecimento Inteligente de Tags de Abertura (`Snippet`)

O método `Snippet` do TextBuilder possui um mecanismo avançado de reconhecimento de **tags de abertura**, mesmo quando o padrão de busca contém curingas (`*`) ou atributos adicionais.

### 🔍 Como funciona

Ao buscar um trecho com padrão como:

```csharp
TextBuilder.Snippet(html, "<div*id='divTest'", "</div>");
```

O TextBuilder realiza os seguintes passos:

1. **Identifica a primeira ocorrência** que corresponde ao padrão com curinga (`<div*id='divTest'`).
2. **Remove o curinga e o conteúdo após ele**, passando a considerar apenas `"<div"` como a **tag de abertura principal**.
3. A partir disso, ele reconhece corretamente:
   - A **hierarquia de trechos filhos** contidos dentro da tag principal.
   - O **fechamento correto** com `</div>`, mesmo em estruturas aninhadas.

### ✅ Benefícios

- Permite buscar trechos complexos com atributos sem quebrar a estrutura.
- Garante que o trecho retornado seja **completo e bem formado**, mesmo com múltiplos níveis de aninhamento.
- Evita erros comuns de Regex, como capturas incompletas ou quebras de DOM.

### 📌 Exemplo prático

```csharp
StringAndPosition snippetMatch = TextBuilder.Snippet(html, "<div*id='divTemp'", "</div>");
Console.WriteLine(snippetMatch.Text);
```

**Resultado**: Retorna o bloco completo `<div id='divTemp'>...</div>`, incluindo todos os elementos filhos corretamente.

---

## 📌 Conclusão

O **TextBuilder** entrega uma solução robusta, leve e escalável para manipulação textual em C#.  
Com benchmarks sólidos, API intuitiva e suporte a padrões avançados, ele se posiciona como uma alternativa moderna e segura ao uso de Regex em aplicações reais.

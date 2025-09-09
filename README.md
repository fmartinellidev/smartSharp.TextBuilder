# TextBuilder — Manipulação de Texto de Alta Performance em C#

## Visão Geral

O **TextBuilder** é uma biblioteca desenvolvida em **C# 13** sobre o ambiente **.NET 9**, voltada para **busca, edição e análise de texto** com foco em **alta performance**, **baixo consumo de memória** e **API intuitiva**.  
Seu diferencial está na capacidade de realizar operações complexas como busca com curingas, substituições posicionais, manipulação de snippets HTML sem depender de bibliotecas externas.

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

## 🔍 Padrões Avançados de Busca por Caracteres Dinâmicos

O TextBuilder oferece suporte a **caracteres especiais** que ampliam a flexibilidade das buscas, permitindo reconhecer variações, padrões incompletos e estruturas numéricas com precisão.

### 🔣 Tabela de Caracteres Dinâmicos

| Caractere | Função                                                                 | Exemplo de Uso                                      | Resultado Esperado                                  |
|-----------|------------------------------------------------------------------------|-----------------------------------------------------|-----------------------------------------------------|
| `_`       | Representa **separadores de palavras** como espaço, pontuação e quebras | `"John_Doe"` pode retornar com `"John Doe"` ou `"John, Doe"` | Reconhece variações com separadores flexíveis       |
| `#`       | Representa **qualquer número completo**, com todos os seus dígitos, pontos e vírgulas.      | `"U$# in cash"` e `"#/#-#"`            | Pode retornar `"U$1.100,32 in cash"` e `"22.724.722/0001-21"`                 |
| `~`       | Realiza **completamento de palavras** com base em prefixo/sufixo        | `"~act"` → `"react"`<br>`"act~"` → `"action"`<br>`"~act~"` → `"reaction"` | Reconhece palavras completas a partir de fragmentos |

---

### 🧠 Como funciona internamente

- O caractere `_` é interpretado como qualquer um dos seguintes separadores:  
  `' '`, `'!'`, `'?'`, `'.'`, `';'`, `':'`, `','`, `'|'`, `'('`, `')'`, `'['`, `']'`, `'{'`, `'}'`, `'\n'`, `'\t'`, `'\r'`

- O caractere `#` identifica **números inteiros ou decimais**, mesmo que estejam formatados com vírgulas, pontos ou símbolos monetários.

- O caractere `~` permite que o TextBuilder **complete automaticamente** o início ou o fim de uma palavra com base no contexto do texto original.  
  Isso é especialmente útil para buscas com fragmentos, prefixos ou sufixos.

---

### 📌 Aplicações práticas

- Busca por nomes com variações de separadores: `"John_Doe"` → `"John Doe"`, `"John-Doe"`, `"John, Doe"`
- Busca por valores numéricos: `"Total: $#"` → `"Total: $1.250,00"`
- Busca por palavras incompletas: `"~act"` → `"react"`, `"act~"` → `"action"`

Essa funcionalidade coloca o TextBuilder em um patamar acima das expressões regulares tradicionais, oferecendo uma abordagem mais **semântica, tolerante e inteligente** para análise textual.

---

## 🧪 Exemplos de Uso

Esses exemplos mostram como o TextBuilder pode ser adaptado para diferentes cenários desde buscas simples até parsing avançado com múltiplas regras.

### 🔍 Match

```csharp
TextBuilder.Match("Marie Doe|Jane Doe|Jack|John Doe");
TextBuilder.Match("*residential");
TextBuilder.Match("Name:*cidade de *.");
TextBuilder.Match("email*@hotmail.com|@gmail.com|@yahoo.com");
```

### ✍️ Inserção de Palavras

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

### 🧹 Remoção de Palavras

| Método                          | Descrição                                                                                   | Exemplo de Uso                                                                 |
|--------------------------------|----------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------|
| `RemoveFirst(text, pattern)`   | Remove apenas a **primeira ocorrência** do `pattern` no texto                               | `RemoveFirst(text, "Marie Doe Towner ")`                                       |
| `Remove(text, pattern)`        | Remove **todas as ocorrências** do `pattern` no texto                                       | `Remove(text, ",")`                                                            |

```csharp
TextBuilder.RemoveFirst(text, "Marie Doe Towner ");
TextBuilder.Remove(text, ",");
```
### 📌 Observações Técnicas

- Ideal para limpeza de conteúdo, sanitização de dados ou refatoração textual.
- Preserva a estrutura do texto original, removendo apenas o que for necessário.
- Tempo médio de execução: **1ms**  
- Memória alocada: **~6.216 bytes**

---

### 🔁 Substituição de Palavras ('Replace')

| Método                            | Descrição                                                                                   | Exemplo de Uso                                                                 |
|----------------------------------|----------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------|
| `ReplaceFirst(text, old, new)`  | Substitui apenas a **primeira ocorrência** de `old` por `new` no texto                      | `ReplaceFirst(text, "Marie Doe Towner", "Jene Doe Sanders")`                   |
| `Replace(text, old, new)`       | Substitui **todas as ocorrências** de `old` por `new` no texto                              | `Replace(text, ",", "<o>")`                                                    |

```csharp
TextBuilder.ReplaceFirst(text, "Marie Doe Towner", "Jene Doe Sanders");
TextBuilder.Replace(text, ",", "<o>");
```

### 📌 Observações Técnicas

- Ideal para refatoração textual, ajustes de nomenclatura ou padronização de conteúdo.
- Preserva a estrutura do texto original, substituindo com precisão.
- Tempo médio de execução: **1ms**  
- Memória alocada: **~6.216 bytes**

---

### 🔄 Tradução Posicional (`Translate`)

| Método                                 | Descrição                                                                                   | Exemplo de Uso                                                                 |
|---------------------------------------|----------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------|
| `TranslateFirst(text, from, to)`      | Substitui apenas a **primeira ocorrência** de cada item de `from` por seu correspondente em `to` | `TranslateFirst(text, "Doe;married;,", "Silva;Divorced;<o>")`                  |
| `Translate(text, from, to)`           | Substitui **todas as ocorrências** de cada item de `from` por seu correspondente em `to`       | `Translate(text, "Doe;married;,", "Silva;Divorced;<o>")`                        |

```csharp
TextBuilder.TranslateFirst(text, "Doe;married;,", "Silva;Divorced;<o>");
TextBuilder.Translate(text, "Doe;married;,", "Silva;Divorced;<o>");
```

### 📌 Observações Técnicas

- Os parâmetros `from` e `to` devem conter os termos separados por `;` na mesma ordem.
- Ideal para mapeamentos múltiplos, como nomes, status, símbolos ou marcações.
- Tempo médio de execução: **1ms**  
- Memória alocada: **~6.216 bytes**
- Preserva a estrutura do texto original, realizando substituições com precisão posicional.

---

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

## 🧱 Trechos no Texto ('Snippet') - Manipulação de Snippets

| Método                                      | Descrição                                                                                                   | Exemplo de Uso                                                                 |
|--------------------------------------------|--------------------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------|
| `Snippet(text, startTag, endTag)`          | Retorna o **primeiro trecho completo** entre `startTag` e `endTag`, reconhecendo hierarquia e aninhamento   | `Snippet(html, "<div", "</div>")`                                              |
| `InsertSnippet(text, value, index)`        | Insere o conteúdo `value` diretamente na posição `index` dentro do texto                                    | `InsertSnippet(html, "<input ... />", 346)`                                    |
| `InsertSnippetBefore(text, start, end, value)` | Insere `value` **antes de cada trecho** identificado entre `start` e `end`                              | `InsertSnippetBefore(html, "<span", "/span>", "<input ... />")`               |
| `InsertSnippetAfter(text, start, end, value)`  | Insere `value` **após cada trecho** identificado entre `start` e `end`                                   | `InsertSnippetAfter(html, "<div*group", "/div>", "<input ... />")`            |
| `RemoveSnippetFirst(text, start, end)`     | Remove apenas o **primeiro trecho** identificado entre `start` e `end`                                      | `RemoveSnippetFirst(html, "<div*group", "/div>")`                              |
| `RemoveSnippet(text, start, end)`          | Remove **todos os trechos** identificados entre `start` e `end`                                             | `RemoveSnippet(html, "<span", "/span>*\r\n")`                                  |
| `ReplaceSnippetFirst(text, start, end, value)` | Substitui o **primeiro trecho** entre `start` e `end` por `value`                                      | `ReplaceSnippetFirst(html, "<div*group", "/div>", "<article ... />")`         |
| `ReplaceSnippet(text, start, end, value)`  | Substitui **todos os trechos** entre `start` e `end` por `value`                                            | `ReplaceSnippet(html, "<span", "/span>", "<article ... />")`                  |
| `ContainsSnippet(text, start, end)`        | Verifica se existe **pelo menos um trecho** entre `start` e `end`                                           | `ContainsSnippet(html, "<div*group", "/div>")` → `True`                        |
| `ContSnippets(text, start, end)`           | Conta quantos trechos existem entre `start` e `end`                                                         | `ContSnippets(html, "<span", "/span>")` → `3`                                  |

### 🔍 Match de Trechos

```csharp
TextBuilder.Snippet(html, "<div", "</div>");
TextBuilder.Snippet(html, "<div*id='divTemp'", "</div>");
```

### ✍️ Inserção em Trechos

```csharp
TextBuilder.InsertSnippet(html, "<input ... />", 346);
TextBuilder.InsertSnippetBefore(html, "<span", "/span>", "<input ... />");
TextBuilder.InsertSnippetAfter(html, "<div*divUnitPopup_group", "/div>", "<input ... />");
```

### 🧹 Remoção de Trechos

```csharp
TextBuilder.RemoveSnippetFirst(html, "<div*divUnitPopup_group", "/div>");
TextBuilder.RemoveSnippet(html, "<span", "/span>*\r\n");
```

### 🔁 Substituição de Trechos

```csharp
TextBuilder.ReplaceSnippetFirst(html, "<div*divUnitPopup_group", "/div>", "<article ... />");
TextBuilder.ReplaceSnippet(html, "<span", "/span>", "<article ... />");
```

### 🔎 Verificação de Trechos

```csharp
TextBuilder.ContainsSnippet(html, "<div*divUnitPopup_group", "/div>"); // True
TextBuilder.ContainsSnippet(html, "<article", "/article>"); // False
```

### 🔢 Contagem de Trechos

```csharp
TextBuilder.ContSnippets(html, "<div*divUnitPopup_group", "/div>"); // 1
TextBuilder.ContSnippets(html, "<span", "/span>"); // 3
```
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
- Pode ser usado para busca de código de elementos no HTML, regras de estilo no CSS, identificar queries no SQL, registro de dados em Json e XML entre outras aplicações. 

### 📌 Exemplo prático

```csharp
StringAndPosition snippetMatch = TextBuilder.Snippet(html, "<div*id='divTemp'", "</div>");
Console.WriteLine(snippetMatch.Text);
```

**Resultado**: Retorna o bloco completo `<div id='divTemp'>...</div>`, incluindo todos os elementos filhos corretamente.

---

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

### 🔢 Necessidades de uso em loops (exemplo em loop de 10.000 iterações)

| Operação         | Regex (tempo médio) | TextBuilder (tempo médio) | Diferença de memória |
|------------------|---------------------|----------------------------|----------------------|
| `Match` simples  | 1.2 ms              | 0.9 ms                     | TextBuilder usa ~80% menos memória |
| `Replace`        | 1.5 ms              | 1.0 ms                     | TextBuilder evita buffers temporários |
| `Contains`       | 1.1 ms              | 0.8 ms                     | TextBuilder não gera objetos descartáveis |

### 🔁 Cenário: Busca repetitiva em grandes volumes de texto

| Critério                     | Regex                                           | TextBuilder                                      |
|------------------------------|--------------------------------------------------|--------------------------------------------------|
| **Alocação por chamada**     | Heap — cria objetos e buffers a cada iteração   | Stack — usa `Span<char>` sem alocar no heap     |
| **Pressão no GC**            | Crescente — coleta frequente de lixo            | Quase nula — sem objetos descartáveis           |
| **Consumo de memória**       | Escala com o número de iterações                | Estável e previsível                            |
| **Tempo por iteração**       | Pode variar com picos de latência               | Consistente e linear                            |
| **Escalabilidade**           | Limitada em ambientes críticos                  | Ideal para sistemas concorrentes e embarcados   |
| **Legibilidade e controle**  | Baixa — expressões complexas e opacas           | Alta — sintaxe clara e orientada a propósito    |

---

### 🧠 Explicação Técnica

- **Regex** depende de um engine que compila e interpreta expressões, criando estruturas auxiliares em cada chamada. Em loops, isso gera:
  - Alocação constante no heap
  - Picos de latência por coleta de lixo
  - Dificuldade de profiling e tuning fino

- **TextBuilder**, por outro lado:
  - Usa `Span<char>` e `ref struct`, operando direto na stack
  - Evita buffers, listas e strings temporárias
  - Mantém o consumo de memória praticamente constante, mesmo em milhares de iterações

---

## 📌 Conclusão

O **TextBuilder** entrega uma solução robusta, leve e escalável para manipulação textual em C#.  
Com benchmarks sólidos, API intuitiva e suporte a padrões avançados, ele se posiciona como uma alternativa moderna e segura ao uso de Regex em aplicações reais.

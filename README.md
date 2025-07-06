
# 💻 TextBuilder – Engine de Manipulação e Busca Inteligente de Texto (.NET 9 / C# 13)

TextBuilder é uma ferramenta estática desenvolvida para .NET 9 Core com C# 13, projetada para facilitar a análise, busca e extração de informações em textos. Ela combina performance, flexibilidade e simplicidade para resolver tarefas que normalmente exigiriam expressões regulares complexas ou múltiplas manipulações de string.

## 🎯 Funcionalidades Principais

### 🔍 Busca de Texto com Padrões

- ✅ Busca literal simples
- 🌟 Curinga (`*`) para trechos variáveis
- 🔗 Condições múltiplas (`|`) para lógica OR
- 🔠 Ignorar maiúsculas/minúsculas (`TextOptions.IgnoreCase`)
- 🧾 Ignorar conteúdo entre aspas simples (`TextOptions.IgnoreInSingleQuotes`)

### 🧠 Padrões Dinâmicos

| Símbolo | Significado                        |
|---------|------------------------------------|
| `@`     | Qualquer letra                     |
| `#`     | Qualquer número                    |
| `_`     | Qualquer separador de palavra      |
| `\`     | Início ou fim de palavra           |

> Exemplo: O padrão `\@#_` pode localizar uma palavra que começa com uma letra, seguida de um número e depois um separador.

### ✂️ Extração de Trechos (Snippets)

- Extração de blocos de texto entre marcações (ex: `<div>...</div>`)
- Suporte a identificadores internos (ex: `id='divTemp'`)
- Alternativa mais leve e legível que Regex

---

## 🧪 Exemplos de Uso

### 🔹 Busca Literal

```csharp
TextBuilder.Match(text, "Marie Doe|John Doe");
```

🔸 Retorna: `John Doe - 842`

---

### 🔹 Ignorando Aspas Simples

```csharp
TextBuilder.Match(text, "John Doe|Marie Doe", TextOptions.IgnoreInSingleQuotes);
```

🔸 Retorna: `Marie Doe - 861`

---

### 🔹 Ignorando Case

```csharp
TextBuilder.Match(text, "john doe|marie doe", TextOptions.IgnoreCase);
```

🔸 Retorna: `John Doe - 842`

---

### 🔹 Padrão com Curinga

```csharp
TextBuilder.Match(text, "Name*Jard*.");
```

🔸 Retorna: `Name: Jardim Barcelona, cidade de Araraquara/SP.`

---

### 🔹 Condição OR com Curinga

```csharp
TextBuilder.Match(text, "married*Marie|John|Jack");
```

🔸 Retorna: trecho contendo `infrastructure`

---

### 🔹 Padrão Dinâmico

```csharp
TextBuilder.MatchDynamic(text, @"\cture");
```

🔸 Retorna: `infrastructure`

---

### 🔹 Extração de Primeiro Trecho

```csharp
TextBuilder.ExtractFirstSnippet("<div*</div>");
```

🔸 Retorna: primeiro bloco `<div>...</div>`

---

### 🔹 Extração com Identificador

```csharp
TextBuilder.ExtractFirstSnippet("<div *</div>", "id='divTemp'");
```

🔸 Retorna: bloco `<div>` com `id='divTemp'`

---

### 🔹 Todos os Trechos

```csharp
TextBuilder.ExtractSnippets("<div *</div>");
```

🔸 Retorna: array com todos os blocos `<div>...</div>`

---

## ⚡ Benchmark de Desempenho

| Cenário                          | Ferramenta     | Tempo Médio | Memória Média | Observações |
|----------------------------------|----------------|-------------|----------------|-------------|
| Busca literal simples            | TextBuilder    | 1 ms        | 352 bytes      | Alta performance |
| Busca com ignorar aspas         | TextBuilder    | 1 ms        | 456 bytes      | Regex não cobre |
| Busca com ignorar case          | TextBuilder    | 2 ms        | 400 bytes      | Mais leve que Regex |
| Padrão com curinga (`*`)        | TextBuilder    | 1 ms        | 688 bytes      | Regex exige expressão complexa |
| Condição OR (`|`)               | TextBuilder    | 2 ms        | 456 bytes      | Regex requer agrupamento |
| Padrão dinâmico (`@`, `#`, `\`) | TextBuilder    | 2 ms        | 504 bytes      | Regex não cobre todos os casos |
| Extração de trecho `<div>`      | TextBuilder    | 1 ms        | 7032 bytes     | Regex: 7 ms / 72 KB |

---

## 📊 Comparativo de Uso por Situação

| Situação                                          | Melhor Ferramenta | Justificativa |
|--------------------------------------------------|--------------------|----------------|
| Busca literal simples                            | `TextBuilder`      | Mais rápido e direto |
| Busca com múltiplas opções (`|`)                 | `TextBuilder`      | Sintaxe simples e eficiente |
| Ignorar maiúsculas/minúsculas                    | `TextBuilder`      | Suporte nativo via `TextOptions` |
| Ignorar conteúdo entre aspas                     | `TextBuilder`      | Regex não cobre esse caso |
| Padrões com curinga (`*`)                        | `TextBuilder`      | Regex exige expressões complexas |
| Padrões dinâmicos (`@`, `#`, `_`, `\`)           | `TextBuilder`      | Regex não cobre todos os símbolos |
| Extração de blocos HTML/XML                      | `TextBuilder`      | Mais leve e legível que Regex |
| Validação de padrões fixos (ex: e-mail, CPF)     | `Regex`            | Regex é mais adequado para validação formal |
| Manipulação de strings simples (Split, Replace)  | `C# Nativo`        | Métodos como `.Split()`, `.Replace()` são mais diretos |

---

## ✅ Conclusão

O **TextBuilder** é a escolha ideal para desenvolvedores que precisam:

- 🔍 Realizar buscas textuais com lógica avançada
- ✂️ Extrair trechos com marcações personalizadas
- 🧠 Interpretar padrões dinâmicos com símbolos especiais
- ⚡ Obter alta performance com baixo consumo de memória

---

## 📄 Licença

Este projeto é de uso livre para fins educacionais e comerciais. Contribuições são bem-vindas!

---

# 📘 Documentação Técnica – Matches Models (.NET 9 / C# 13)

## 🧩 Visão Geral

A solução **Matches Models** é um mecanismo de correspondência de padrões textuais altamente flexível e extensível, projetado para localizar trechos específicos em uma string de origem. Ela suporta:

- Correspondência literal e dinâmica
- Padrões compostos com curingas (`*`) e múltiplas opções (`|`)
- Interpretação de símbolos especiais (`@`, `#`, `_`, `\`)
- Opções configuráveis como ignorar case e ignorar trechos entre aspas simples

---

## 🧱 Estrutura da Solução

### 🔹 Métodos Públicos

| Método         | Descrição |
|----------------|-----------|
| `Match`        | Realiza a busca do primeiro padrão literal ou composto em um texto. |
| `MatchDynamic` | Variante que ativa a lógica de padrões dinâmicos (com `@`, `#`, etc.). |

---

### 🔹 Métodos Privados Auxiliares

| Método               | Função |
|----------------------|--------|
| `matchPattern`       | Núcleo da lógica de correspondência. Interpreta curingas e múltiplas opções. |
| `matchText`          | Realiza comparação literal entre padrão e texto. |
| `matchTextDynamics`  | Executa correspondência com padrões dinâmicos. |
| `isMatchedPattern`   | Interpreta símbolos especiais e verifica se há correspondência no texto. |
| `IsSeparator`        | Determina se um caractere é separador de palavras. |
| `isDynamicPattern`   | Verifica se um padrão contém símbolos dinâmicos. |

---

## ⚙️ Detalhamento dos Componentes

### 🔸 Match / MatchDynamic

```csharp
public static StringAndPosition Match(string sourceText, string stringsToMatch, TextOptions options = default)
public static StringAndPosition MatchDynamic(string sourceText, string stringsToMatch, TextOptions options = default)
```

- Entrada: texto de origem, padrões separados por `"|"`, e opções de busca.
- Saída: objeto `StringAndPosition` com o trecho encontrado e sua posição.
- Ambos delegam para `matchPattern`, com `dynamicChars = true` ou `false`.

---

### 🔸 matchPattern

```csharp
private static StringAndPosition matchPattern(ReadOnlySpan<char> sourceTextSpan, ReadOnlySpan<char> matchSpanText, int startIndex, bool dynamicChars, TextOptions options)
```

- Interpreta padrões com `*` (curingas) e `|` (opções).
- Decide entre `matchText` e `matchTextDynamics` com base em `dynamicChars` e `isDynamicPattern`.
- Retorna a primeira ocorrência encontrada.

---

### 🔸 matchText

```csharp
private static WordAndPosition matchText(ReadOnlySpan<char> sourceTextSpan, ReadOnlySpan<char> pattern, int startIndex, TextOptions options)
```

- Realiza comparação literal.
- Suporta `IgnoreCase` e `IgnoreInSingleQuotes`.

---

### 🔸 matchTextDynamics

```csharp
private static WordAndPosition matchTextDynamics(ReadOnlySpan<char> sourceTextSpan, Span<char> pattern, int startIndex, int occurIni, int litteralPatLen, TextOptions options)
```

- Executa correspondência com padrões contendo símbolos especiais.
- Chama `isMatchedPattern` para validar a sequência.

---

### 🔸 isMatchedPattern

```csharp
private static int isMatchedPattern(ReadOnlySpan<char> sourceTextSpan, ReadOnlySpan<char> pattern, int pos, ref int occurIni, int litteralPatLen, TextOptions options)
```

- Interpreta símbolos:
  - `@`: letra
  - `#`: número
  - `_`: separador
  - `\`: início/fim de palavra
- Retorna o índice final da correspondência.

---

### 🔸 IsSeparator

```csharp
private static bool IsSeparator(char c)
```

- Define quais caracteres são considerados separadores de palavras (espaço, pontuação, etc.).

---

### 🔸 isDynamicPattern

```csharp
private static bool isDynamicPattern(ReadOnlySpan<char> pattern)
```

- Verifica se o padrão contém símbolos especiais que exigem lógica dinâmica.

---

## 🔄 Fluxo de Execução

```plaintext
Match / MatchDynamic
        ↓
  matchPattern
    ├── Se padrão é dinâmico → matchTextDynamics → isMatchedPattern
    └── Se padrão é literal  → matchText
```

---

## 🧪 Exemplo de Uso

```csharp
var result = Match("The quick brown fox", "quick|slow", TextOptions.IgnoreCase);
Console.WriteLine(result.Word); // "quick"
```

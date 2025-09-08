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

Perfeito, Fernando! Aqui está a tabela dos parâmetros que você mencionou, formatada para inclusão direta no `README.md` em Markdown:

---

Claro, Fernando! Aqui está a tabela dos parâmetros do `TextOpt` sem a coluna de valor, formatada para inclusão direta no `README.md` em Markdown:

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

## 🧪 Exemplos de Uso

### 🔍 Match

```csharp
TextBuilder.Match("Marie Doe|Jane Doe|Jack|John Doe");
TextBuilder.Match("*residential");
TextBuilder.Match("Name:*cidade de *.");
TextBuilder.Match("email*@hotmail.com|@gmail.com|@yahoo.com");
```

### ✍️ Inserção

```csharp
TextBuilder.Insert(text, "the client ", 75);
TextBuilder.InsertBeforeFirst(text, "the client ", "Marie");
TextBuilder.InsertAfter(text, "<o>", ",");
```

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

## 📌 Conclusão

O **TextBuilder** entrega uma solução robusta, leve e escalável para manipulação textual em C#.  
Com benchmarks sólidos, API intuitiva e suporte a padrões avançados, ele se posiciona como uma alternativa moderna e segura ao uso de Regex em aplicações reais.

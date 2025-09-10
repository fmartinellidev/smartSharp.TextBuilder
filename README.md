Claro, Fernando! Aqui está a versão traduzida para o inglês da sua documentação reorganizada do GitHub, com todos os termos técnicos e nuances preservados para manter clareza e profissionalismo:

---

# TextBuilder — High-Performance Text Manipulation in C#

## Overview

**TextBuilder** is a library developed in **C# 13** on the **.NET 9** platform, designed for **searching, editing, and analyzing text** with a focus on **high performance**, **low memory usage**, and an **intuitive API**.  
Its key differentiator is the ability to perform complex operations such as wildcard searches, positional replacements, and HTML snippet manipulation — all without relying on external libraries.

---

## 📦 Dependencies & Safety

- Zero external dependencies  
- Uses only 4 native C# libraries  
- Version-safe maintenance  
- Native scalability  
- No conflicts with third-party packages  

---

## ► Internal Architecture

Inspired by the **MVC pattern**, adapted for text operations:

| Layer       | Function                                                                 |
|-------------|--------------------------------------------------------------------------|
| Constructor | Receives high-level parameters (`string`) and converts them to `Span`    |
| Controller  | Interprets the operation type and organizes parameters                   |
| Model       | Executes the actual search and returns coordinates or matched segments   |

---

## ► Syntax Options

TextBuilder offers multiple usage styles, adapting to each developer’s preferences and needs:

### 🔹 Direct `TextMatcher` instance

```csharp
TextMatcher builder = new TextMatcher(text);
builder.CaseSensitive = false;
builder.EnableIgnoreCharsInQuotes();
StringAndPosition firstMatch = builder.Match("john doe|marie doe");
```

---

### 🔹 `IDisposable` block with internal configuration

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

### 🔹 Inline instance with configured properties

```csharp
StringAndPosition firstMatch = new TextMatcher(text)
{
    CaseSensitive = false
}.Match("john doe|marie doe");
```

---

### 🔹 Static class usage via `TextBuilder`

```csharp
StringAndPosition firstMatch = TextBuilder.Match(text, "Marie Doe|Jane Doe|Jack|John Doe", TextOpt.MatchWholeWordOnly);
```

---

### Why does TextBuilder return results using `StringAndPosition`?

To preserve **freedom, performance, and memory control**, TextBuilder avoids certain abstractions that, while convenient, would compromise efficiency — especially in repetitive or simple tasks.

The `StringAndPosition` structure serves two key purposes:

- **Returns the matched segment** (`Text`) with precision  
- **Provides the exact position** (`Position`) of the match within the original text

This approach offers major advantages:

- Avoids `IEnumerable` or intermediate collections, reducing heap allocation and GC pressure  
- Enables loop execution with stable performance using stack-based `Span<char>`  
- Facilitates chained or positional searches with precise control  

Rather than returning just a `string` or a list, TextBuilder delivers **contextual and positional information** — essential for systems requiring precision and efficiency.

---

## ⚙️ Configuration Parameters (`TextOpt`)

These parameters control the behavior of TextBuilder’s search and manipulation operations:

| Parameter                     | Description                                                                 |
|-------------------------------|-----------------------------------------------------------------------------|
| `CaseSensitive`              | Enables case sensitivity                                                    |
| `IgnoreCharsInQuotes`        | Ignores content between single quotes (`'...'`) during parsing              |
| `IgnoreCharsInDoubleQuotes`  | Ignores content between double quotes (`"..."`) during parsing              |
| `IgnoreDynamicChars`         | Detects and ignores dynamic characters in both pattern and text             |
| `MatchGreedyOccurences`      | Allows greedy matching instead of shortest match                            |
| `MatchWholeWordOnly`         | Matches only full words                                                     |

---

## ► Advanced Pattern Matching with Dynamic Characters

TextBuilder supports **special characters** that enhance search flexibility, enabling recognition of variations, incomplete patterns, and numeric structures.

| Character | Function                                                                 | Example Usage                                      | Expected Result                                  |
|-----------|--------------------------------------------------------------------------|---------------------------------------------------|--------------------------------------------------|
| `_`       | Represents **word separators** like spaces, punctuation, and breaks      | `"John_Doe"` matches `"John Doe"` or `"John, Doe"`| Recognizes flexible separators                   |
| `#`       | Represents **any complete number**, including digits, commas, and dots   | `"U$# in cash"` or `"#/#-#"`                      | Matches `"U$1,100.32 in cash"` or `"22.724.722/0001-21"` |
| `~`       | Performs **word completion** based on prefix/suffix                      | `"~act"` → `"react"`<br>`"act~"` → `"action"`<br>`"~act~"` → `"reaction"` | Matches full words from fragments               |

---

### ► Internal Mechanics

- `_` is interpreted as any of the following separators:  
  `' '`, `'!'`, `'?'`, `'.'`, `';'`, `':'`, `','`, `'|'`, `'('`, `')'`, `'['`, `']'`, `'{'`, `'}'`, `'\n'`, `'\t'`, `'\r'`

- `#` identifies **integers or decimals**, even if formatted with symbols or punctuation

- `~` allows TextBuilder to **auto-complete** the start or end of a word based on context — ideal for prefix/suffix searches

---

### ► Practical Applications

- Name searches with separator variations: `"John_Doe"` → `"John Doe"`, `"John-Doe"`, `"John, Doe"`  
- Numeric value searches: `"Total: $#"` → `"Total: $1,250.00"`  
- Incomplete word searches: `"~act"` → `"react"`, `"act~"` → `"action"`

This functionality elevates TextBuilder beyond traditional regex, offering a more **semantic, tolerant, and intelligent** approach to text analysis.

---

## ► Use Examples

These examples show how TextBuilder can be adapted for different scenarios from simple searches to advanced parsing with multiple rules.

### 🔍 Match

```csharp
TextBuilder.Match("Marie Doe|Jane Doe|Jack|John Doe");
TextBuilder.Match("*residential");
TextBuilder.Match("Name:*cidade de *.");
TextBuilder.Match("email*@hotmail.com|@gmail.com|@yahoo.com");
```

## ✍️ Function Table — Word Insertion

| Method                                | Description                                                                 | Example Usage                                      |
|---------------------------------------|-----------------------------------------------------------------------------|---------------------------------------------------|
| `Insert(text, value, index)`          | Inserts `value` directly at the specified `index`                          | `Insert(text, "the client ", 75)`                 |
| `InsertBeforeFirst(text, value, pattern)` | Inserts `value` before the **first occurrence** of `pattern`           | `InsertBeforeFirst(text, "the client ", "Marie")` |
| `InsertAfterFirst(text, value, pattern)`  | Inserts `value` after the **first occurrence** of `pattern`            | `InsertAfterFirst(text, "Marie", " the client")`  |
| `InsertBefore(text, pattern, value)`      | Inserts `value` **before all occurrences** of `pattern`                 | `InsertBefore(text, "<o>", ",")`                  |
| `InsertAfter(text, pattern, value)`       | Inserts `value` **after all occurrences** of `pattern`                  | `InsertAfter(text, "<o>", ",")`                   |

```csharp
TextBuilder.InsertSnippet(html, "<input ... />", 346);
TextBuilder.InsertSnippetBefore(html, "<span", "/span>", "<input ... />");
TextBuilder.InsertSnippetAfter(html, "<div*divUnitPopup_group", "/div>", "<input ... />");
```

---

## 🧹 Function Table — Word Removal

| Method                          | Description                                                                 | Example Usage                                      |
|---------------------------------|-----------------------------------------------------------------------------|---------------------------------------------------|
| `RemoveFirst(text, pattern)`    | Removes only the **first occurrence** of `pattern`                         | `RemoveFirst(text, "Marie Doe Towner ")`          |
| `Remove(text, pattern)`         | Removes **all occurrences** of `pattern`                                   | `Remove(text, ",")`                               |

## 🔁 Function Table — Word Replacement

| Method                          | Description                                                                 | Example Usage                                      |
|---------------------------------|-----------------------------------------------------------------------------|---------------------------------------------------|
| `ReplaceFirst(text, old, new)`  | Replaces only the **first occurrence** of `old` with `new`                 | `ReplaceFirst(text, "Marie Doe Towner", "Jene Doe Sanders")` |
| `Replace(text, old, new)`       | Replaces **all occurrences** of `old` with `new`                           | `Replace(text, ",", "<o>")`                       |

```csharp
TextBuilder.ReplaceSnippetFirst(html, "<div*divUnitPopup_group", "/div>", "<article ... />");
TextBuilder.ReplaceSnippet(html, "<span", "/span>", "<article ... />");
```

---

## 🔄 Function Table — Positional Translation (`Translate`)

| Method                                 | Description                                                                 | Example Usage                                      |
|----------------------------------------|-----------------------------------------------------------------------------|---------------------------------------------------|
| `TranslateFirst(text, from, to)`       | Replaces only the **first occurrence** of each item in `from` with its counterpart in `to` | `TranslateFirst(text, "Doe;married;,", "Silva;Divorced;<o>")` |
| `Translate(text, from, to)`            | Replaces **all occurrences** of each item in `from` with its counterpart in `to` | `Translate(text, "Doe;married;,", "Silva;Divorced;<o>")` |

```csharp
TextBuilder.TranslateFirst(testText, "Doe;married;,", "Silva;Divorced;<o>");
TextBuilder.Translate(testText, "Doe;married;,", "Silva;Divorced;<o>");
```

### 🔎 Contains word

```csharp
TextBuilder.Contains(text, "John "); // True
TextBuilder.Contains(text, "kkkkkkkbua"); // False
TextBuilder.Contains(text, "Mar*ner"); // True
```

### 🔢 Cont words

```csharp
TextBuilder.Cont(text, "act"); // 6
TextBuilder.Cont(text, "r*act"); // 4
TextBuilder.Cont(text, "r*act", TextOpt.MatchWholeWordOnly); // 3
```

---

## 🧱 Function Table — Snippet Manipulation

| Method                                      | Description                                                                 | Example Usage                                      |
|---------------------------------------------|------------------------------------------------------------------------------|---------------------------------------------------|
| `Snippet(text, startTag, endTag)`           | Returns the **first complete segment** between `startTag` and `endTag`, recognizing hierarchy | `Snippet(html, "<div", "</div>")`                 |
| `InsertSnippet(text, value, index)`         | Inserts `value` directly at the specified `index` within the text           | `InsertSnippet(html, "<input ... />", 346)`       |
| `InsertSnippetBefore(text, start, end, value)` | Inserts `value` **before each segment** identified between `start` and `end` | `InsertSnippetBefore(html, "<span", "/span>", "<input ... />")` |
| `InsertSnippetAfter(text, start, end, value)`  | Inserts `value` **after each segment** identified between `start` and `end` | `InsertSnippetAfter(html, "<div*group", "/div>", "<input ... />")` |
| `RemoveSnippetFirst(text, start, end)`      | Removes only the **first segment** identified between `start` and `end`     | `RemoveSnippetFirst(html, "<div*group", "/div>")` |
| `RemoveSnippet(text, start, end)`           | Removes **all segments** identified between `start` and `end`               | `RemoveSnippet(html, "<span", "/span>*\r\n")`     |
| `ReplaceSnippetFirst(text, start, end, value)` | Replaces the **first segment** between `start` and `end` with `value`       | `ReplaceSnippetFirst(html, "<div*group", "/div>", "<article ... />")` |
| `ReplaceSnippet(text, start, end, value)`   | Replaces **all segments** between `start` and `end` with `value`            | `ReplaceSnippet(html, "<span", "/span>", "<article ... />")` |
| `ContainsSnippet(text, start, end)`         | Checks if there is **at least one segment** between `start` and `end`       | `ContainsSnippet(html, "<div*group", "/div>")` → `True` |
| `ContSnippets(text, start, end)`            | Counts how many segments exist between `start` and `end`                    | `ContSnippets(html, "<span", "/span>")` → `3`     |


---

### 🧠 Intelligent Tag Recognition

When the `startTag` parameter contains a wildcard (`*`), such as:

```csharp
"<div*id='divTemp'"
```

TextBuilder:

1. Locates the **first occurrence** that matches the pattern  
2. **Removes the wildcard and everything after it**, treating only `"<div"` as the **main opening tag**  
3. Correctly identifies **nested child segments** and **closing tags**, ensuring that the returned snippet is **complete and well-formed**

---

### Benefits

- Allows you to search for complex snippets with attributes without breaking the structure.
- Ensures that the returned snippet is **complete and well-formed**, even with multiple levels of nesting.
- Avoids common Regex errors, such as incomplete captures or DOM breaks.
- Can be used to search for element code in HTML, style rules in CSS, identify queries in SQL, record data in JSON and XML, and other applications.

### Practial Example

```csharp
StringAndPosition snippetMatch = TextBuilder.Snippet(html, "<div*id='divTemp'", "</div>");
Console.WriteLine(snippetMatch.Text);
```

**Result**: Return the complete snippet `<div id='divTemp'>...</div>`, including all child elements correctly.

---

## ► Benchmarks — Regex vs TextBuilder

### Scenario: Repetitive Search in Loops

| Metric                  | Regex                          | TextBuilder                     |
|-------------------------|--------------------------------|----------------------------------|
| Allocation per call     | Heap (high)                    | Stack (minimal)                 |
| GC pressure             | Increasing                     | Near zero                       |
| Memory consumption      | Scales with iterations         | Stable and predictable          |
| Iteration time          | May spike due to GC            | Consistent and linear           |
| Scalability             | Limited under heavy load       | Ideal for concurrent systems    |

---

### Technical Explanation

- **Regex** relies on an engine that compiles and interprets expressions, creating auxiliary structures on each call. In loops, this leads to:
  - Constant heap allocation  
  - Latency spikes due to garbage collection  
  - Difficulty in profiling and fine-tuning

- **TextBuilder**, on the other hand:
  - Uses `Span<char>` and `ref struct`, operating directly on the stack  
  - Avoids temporary buffers, lists, and strings  
  - Keeps memory usage nearly constant, even across thousands of iterations

---

### Real Benchmark (10,000 iterations)

| Operation     | Regex (avg time) | TextBuilder (avg time) | Memory Difference            |
|---------------|------------------|-------------------------|------------------------------|
| `Match`       | 1.2 ms           | 0.9 ms                  | ~80% less memory usage       |
| `Replace`     | 1.5 ms           | 1.0 ms                  | No temporary buffer creation |
| `Contains`    | 1.1 ms           | 0.8 ms                  | No disposable objects        |

---

## ► Final Thoughts

TextBuilder is not a generic replacement for Regex, it’s a **smarter**, **faster**, and **more readable** alternative for developers who need precision and control.

Built with modern C# features like `Span<char>`, it offers:

- Low-level performance  
- High-level clarity  
- Scalable architecture  
- Context-aware search and manipulation

Whether you're building parsers, editors, analyzers, or just need clean and efficient text handling — TextBuilder is a powerful tool that reflects thoughtful engineering.

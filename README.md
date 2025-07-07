
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
- Reconhecimento de trechos semelhantes filho, ou seja, ele reconhece um trecho filho com as marcações semelhantes dentro de um trecho pai
  (ex: `<div>esse é o trecho pai <div>esse é o trecho filho</div></div>`). Apesar da marcação final do filho estar antes e ser igual a do pai, a ferramenta reconhece e retorna `<div>esse é o trecho pai <div>esse é o trecho filho</div></div>`. 

---
### Textos Usado para os Exemplos e Testes

```csharp

    html = @"<head>
                                <meta charset='UTF-8'>
                                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                                <title></title>
                                <link href='../fonts.css' rel=\'stylesheet'/>
                                <link href='../styles/vertical/popup_master.css?v=4' rel='stylesheet'/>
                                <link href='../styles/vertical/popup_visitor.css?v=6' rel='stylesheet'/>
                                <script src='../scripts/vertical/scriptCheckbutton.js'></script>
                           </head>
                           <body style='width:1920px; height:1080px'>
                           <section id='popup'>
                                <div id='divPopupVertical' class='divPopupVertical popupColorBack_light popupClosed'>teste
                                    <div id='divUnitPopup' class='divPopupVertical popupColorBack_mid popupOpened' data-unidade=''>                
                                        <div id='divTemp' class='divPopupVertical popupColorBack_mid popupClosed' data-info='</div>header'>
                                            <input type='button' class='btnPopupClose popupColorBack_dark' value='x' onclick='subPopup_vertical_OpenClose()'/>
                                            <span id='divUnitPopup_block_caption' data-info='label_bloco' class='lblTorre_label popupColorBack_mid'>TORRE:</span>
                                            <div id='divUnitPopup_block' data-info='bloco' class='lblTorre_value popupColorBack_mid'></div>
                                            <span id='divUnitPopup_group_caption' data-info='label_grupo' class='lblAndar_label popupColorBack_mid'>ANDAR:</span>
                                            <div id='divUnitPopup_group' data-info='quadra' class='lblAndar_value popupColorBack_mid'></div>
                                            <span id='divUnitPopup_unit_caption' data-info='label_unidade' class='lblUnidade_label popupColorBack_mid'>APTO:</span>
                                            <div id='divUnitPopup_unit' data-info='lote' class='lblUnidade_value popupColorBack_mid'></div>
                                        </div>
                                    </div>
                                divPopupVertical</div>
                                <div class='divPlant popupColorBack_mid'>
                                    <div class='divMetragem'>
                                       <span data-info='metragem' data-form='popup' data-field='metragem' id='lblMetragem_value'>0</span><br>m²
                                    </div>
                                    <div style='height:20px; top:-2px; left:26px;' class='divPlantVerticalLine'></div>
                                    <div style='width:26px; top:40px; left:-2px;' class='divPlantHorizontalLine'></div>
                                    <div style='height:12px; top:40px; left:24px;' class='divPlantVerticalLine'></div>
                                    <div style='height:16px; top:74px; left:24px;' class='divPlantVerticalLine'></div>
                                    <div style='height:8px; top:-2px; left:84px;' class='divPlantVerticalLine'></div>
                                    <div id='divValorFinal' style='width:32px; top:44px; left:84px;' class='divPlantHorizontalLine'></div>
                                </div>
                            </section>";

    text = @"PRIVATE INSTRUMENT OF PROMISE OF PURCHASE AND SALE OF PROPERTY
              
              SUBJECT OF SUBDIVISION
              
              Summary Table

              A. PARTIES
              A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private law company, duly registered 
                   with the CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                   Batista, nº 2281, room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                   represented in this act in accordance with its Articles of Incorporation, in the capacity 
                   of Owner and Developer, hereinafter simply referred to as SELLER.
              A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                   CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                   Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                   providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                   email marie@gmail.com; married to John Doe Silva, registered under CPF number 012.869.980-93, 
                   RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                   hereinafter simply referred to as BUYER.

              B. THE SUBDIVISION:

              The type of property is a residential subdivision, named LOTEAMENTO RESIDENCIAL BARCELONA, located 
              in Araçatuba, State of São Paulo, registered under property record nº 100.314 at the Real Estate 
              Registry Office of the District of Araçatuba – State of São Paulo.

              B.1. Subdivision Description:
                   The LOTEAMENTO RESIDENCIAL BARCELONA will be developed according to the specifications 
                   contained in the Descriptive Report, copies of which will be archived along with the 
                   registration process of the development.
                   The BUYER is fully and unequivocally aware that the information contained in the Conceptual 
                   Plan is merely illustrative and may be modified according to the needs of the project and 
                   the interest of the SELLER and/or determinations from the competent public authorities.

              B.2. Basic Characteristics of the Subdivision:

              - Name: Jardim Barcelona, cidade de Araraquara/SP.
              - Total Area: 315,467.00 m².
              - Area allocated for Residential Lots: 136,705.37 m².
              - Area allocated for Commercial Lots: 3,031.19 m².
              - Leisure System: 27,381.80 m².
              - Area allocated for the Entrance Gate: 704.85 m².
              - Green Areas: 40,860.29 m².
              - Total Road System: 91,579.16 m².
              - Institutional Areas: 15,909.19 m².
              - Total Number of Lots: 609.
              - Residential Lots: 608.
              - Commercial Lots: 01.

              B.3. Deadline for completion of infrastructure works:
                   24 (twenty-four) months, counted from the date of the public launch of the subdivision, 
                   which was carried out on December 5, 2015, and may be anticipated at any time or extended 
                   for a period granted by the Municipality of Araçatuba-SP, under the terms of this instrument.";

```

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
---

## 📄 Licença

Este projeto é de uso livre para fins educacionais e comerciais. Contribuições são bem-vindas!
Vale lembrar que ainda estou desenvolvendo a ferramenta e novas funcionalidades e atualizações serão feitas.

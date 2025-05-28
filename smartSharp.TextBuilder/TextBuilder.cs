using System;
using System.Buffers;
using System.Text;

namespace SmartSharp.TextBuilder
{
    #region ▼ TextOptions

    [Flags]
    public enum TextOptions
    {
        /// <summary>
        /// <para>In match not returns duplicate occurs.</para>
        /// <para>Only removes first occurs of duplicate text.</para>
        /// </summary>
        Distinct = 10,

        /// <summary>
        /// Don't ignore content in single quotes('').
        /// <para>Return if find text in single quotes</para>
        /// </summary>
        IgnoreInSingleQuotes = 20,

        /// <summary>
        /// Return snippets without open and close tags.
        /// <para>Ex: "Extract a snippet of ->text<-", result="text"</para>
        /// </summary>
        RemoveOpenCloseTags = 30,
                
        /// <summary>
        /// The list of matches return sorted by position in text
        /// </summary>
        OrderByPosition = 40,

        /// <summary>
        /// The list of matches return sorted by position in text
        /// </summary>
        IgnoreCase = 50

    }

    #endregion

    #region ▼ StringAndPosition

    public class StringAndPosition
    {
        public int Position { get; set; }
        public char[] Chars { get; set; }

        public string Text { get { return new string(Chars); } }
        public bool Empty { get { return Chars.Length == 0; } }

        public StringAndPosition(ReadOnlySpan<char> text, int position)
        {
            Position = position;
            Chars = text.ToArray();
        }

        public StringAndPosition()
        {
            Position = -1;
            Chars = new char[0];
        }

        public void Set(ReadOnlySpan<char> text, int position)
        {
            Position = position;
            Chars = text.ToArray();
        }
    }

    #endregion

    #region ▼ WordAndPosition

    public ref struct WordAndPosition
    {
        public bool Empty { get { return Word.Length == 0; } }
        public int Position { get; set; }
        public ReadOnlySpan<char> Word { get; set; }

        public WordAndPosition(ReadOnlySpan<char> text, int position)
        {
            Position = position;
            Word = text;
        }

        public WordAndPosition()
        {
            Position = -1;
            Word = new char[0];
        }

        public void Set(ReadOnlySpan<char> text, int position)
        {
            Position = position;
            Word = text;
        }
    }

    #endregion
        
    #region ▼ TextBuilder

    public static class TextBuilder
    {
        #region ▼ Properties
                
        #region • Success

        private static bool success = false;
        public static bool Success
        {
            get { return success; }
        }

        #endregion

        #region ▼ Params

        private static bool textOption = false;
        private static bool ignoreApostrophes = false;
        private static bool distinct = false;
        private static bool removeTags = false;
        private static bool orderByPosition = false;
        private static bool onlyPatternParts = false;
        private static bool ignoreCase = false;

        #endregion

        #region • EnableDinamicsChars

        private static bool EnableDinamicChars = false;

        #endregion

        private static bool startWord = false;
        private static bool endWord = false;

        #endregion

        #region ▼ Matches Controllers

        #region ► Match

        /// <summary>
        /// Match first text in string. Not case sentive.
        /// </summary>
        /// <param name="stringsToMatch">Texts to search in source string.
        /// <para>Use "|" to split various texts. Ex:"john|marie|Jack"</para></param>
        /// <param name="options">Parameters of the search in 'Options' internal class</param>
        /// <returns>Return string of first occurs without case sensitive.</returns>
        public static StringAndPosition Match( string sourceText, string stringsToMatch, TextOptions option = default)
        {
            ReadOnlySpan<char> sourceTextSpan = sourceText.AsSpan();
            setParams(option);
                        
            return matchPattern(sourceText, stringsToMatch, 0);
        }

        #endregion

        #region ► MatchDynamic

        /// <summary>
        /// Match first text in string. Not case sentive.
        /// </summary>
        /// <param name="stringsToMatch">Texts to search in source string.
        /// <para>Use "|" to split various texts. Ex:"john|marie|Jack"</para></param>
        /// <param name="options">Parameters of the search in 'Options' internal class</param>
        /// <returns>Return string of first occurs without case sensitive.</returns>
        public static StringAndPosition MatchDynamic(string sourceText, string stringsToMatch, TextOptions option = default)
        {
            ReadOnlySpan<char> sourceTextSpan = sourceText.AsSpan();
            setParams(option);

            EnableDinamicChars = true;

            return matchPattern(sourceText, stringsToMatch, 0);
        }

        #endregion

        #endregion

        #region ▼ Matches Models

        #region » SetParams

        private static  void setParams(TextOptions option)
        {
            textOption = false;

            ignoreApostrophes = false;
            distinct = false;
            orderByPosition = false;
            EnableDinamicChars = false;
            ignoreCase = false;

            if (option.HasFlag(TextOptions.IgnoreInSingleQuotes))
            { ignoreApostrophes = true; textOption = true; }
            else { ignoreApostrophes = false; }
                       
            if (option.HasFlag(TextOptions.Distinct))
            { distinct = true; textOption = true; }
            else { distinct = false; }

            if (option.HasFlag(TextOptions.OrderByPosition))
            { orderByPosition = true; textOption = true; }
            else { orderByPosition = false; }

            if (option.HasFlag(TextOptions.IgnoreCase))
            { ignoreCase = true; textOption = true; }
            else { ignoreCase = false; }
        }

        #endregion

        #region » matchPattern

        private static StringAndPosition matchPattern(ReadOnlySpan<char> sourceTextSpan, ReadOnlySpan<char> matchSpanText, int startIndex)
        {
            int patIni = 0;
            int patLen = 0;
            int patStart = 0;

            int occurIni = -1;
            int occurLen = 0;

            int orOccurIni = 0;
            int orOccurLen = 0;           

            if (matchSpanText[0] == '*') { occurIni = 0; patStart = 1; patIni = 1; }

            for (int patIndex = patStart; patIndex < matchSpanText.Length; patIndex++)
            {
                patLen++;

                #region + By wildcard

                if ((matchSpanText[patIndex] == '*' || patIndex == matchSpanText.Length - 1)
                    && matchSpanText[patIndex] != '|')
                {
                    #region + Litteral pattern

                    if (matchSpanText[patIni] == '*') { patIni++; }
                    if (matchSpanText[patIni + (patLen - 1)] == '*') { patLen--; }

                    WordAndPosition occur;

                    if (!EnableDinamicChars)
                    { occur = matchLitteral(sourceTextSpan, matchSpanText.Slice(patIni, patLen), startIndex); }
                    else
                    { occur = matchLitteralDynamics(sourceTextSpan, matchSpanText.Slice(patIni, patLen).ToArray(), startIndex); }

                    if(occur.Position == -1 || occur.Word=="")
                    {
                        #region + Not found any pattern, so returns a empty occurrence
                        return new StringAndPosition("", 0);
                        #endregion
                    }

                    #endregion

                    if ((occur.Position < orOccurIni || orOccurLen == 0) && !occur.Empty)
                    {
                        #region + Is a regular pattern

                        if (occurIni == -1 || (occurIni == 0 && textOption)) { occurIni = occur.Position; occurLen = occur.Word.Length; }
                        else { occurLen = (occur.Position - occurIni) + occur.Word.Length; }

                        orOccurIni = 0;
                        orOccurLen = 0;

                        #endregion
                    }
                    else if (occur.Position >= orOccurIni || orOccurLen != 0)
                    {
                        #region + Is a orCondition pattern

                        if (occurIni == -1) { occurIni = orOccurIni; occurLen = orOccurLen; }
                        else { occurLen = (orOccurIni - occurIni) + orOccurLen; }

                        orOccurIni = 0;
                        orOccurLen = 0;

                        #endregion
                    }
                    
                    patLen = 0;
                    patIni = patIndex + 1;
                    startIndex = occurIni + patLen;
                }

                #endregion

                #region + By orCondition

                else if (matchSpanText[patIndex] == '|' && patIndex > 0)
                {
                    if (matchSpanText[patIni] == '|') { patIni++; }
                    if (matchSpanText[patIni + (patLen - 1)] == '|') { patLen--; }

                    WordAndPosition occur = matchLitteral(sourceTextSpan, matchSpanText.Slice(patIni, patLen), startIndex);

                    if (occur.Position < orOccurIni || orOccurIni == 0)
                    {
                        orOccurIni = occur.Position;
                        orOccurLen = occur.Word.Length;
                    }

                    patLen = 0;
                    patIni = patIndex + 1;
                    //startIndex = orOccurIni;
                }

                #endregion
            }

            if (matchSpanText[matchSpanText.Length - 1] == '*') { occurLen = sourceTextSpan.Length - occurIni; }

            return new StringAndPosition(sourceTextSpan.Slice(occurIni, occurLen), occurIni);
        }
                
        #endregion

        #region » matchLitteral
        private static WordAndPosition matchLitteral(ReadOnlySpan<char> sourceTextSpan, ReadOnlySpan<char> pattern, int startIndex)
        {
            bool ignoreIt = false;

            for (int pos = startIndex; pos < sourceTextSpan.Length; ++pos)
            {
                
                #region + Lengths of source text and pattern

                if (pos + pattern.Length > sourceTextSpan.Length) { break; }

                #endregion

                #region + Ignore apostrophes

                if (ignoreApostrophes)
                {
                    if (sourceTextSpan[pos] == '\'')
                    { ignoreIt = !ignoreIt; continue; }
                    if (ignoreIt) continue;
                }

                #endregion
                                
                if (ignoreCase)
                {
                    #region + Ignore case

                    Span<char> currentText = new char[pattern.Length];
                    Span<char> currentPattern = new char[pattern.Length];
                    sourceTextSpan.Slice(pos, pattern.Length).ToUpperInvariant(currentText);
                    pattern.ToUpperInvariant(currentPattern);

                    if (currentText.SequenceEqual(currentPattern))
                    { WordAndPosition occur = new(pattern, pos); return occur; }

                    #endregion
                }
                else 
                {
                    #region + Regular match

                    if (sourceTextSpan.Slice(pos, pattern.Length).SequenceEqual(pattern))
                    { WordAndPosition occur = new(pattern, pos); return occur; }

                    #endregion
                }
            }

            return new(pattern, -1);
        }

        #endregion

        #region » matchLitteralDynamics
        private static WordAndPosition matchLitteralDynamics(ReadOnlySpan<char> sourceTextSpan, Span<char> pattern, int startIndex)
        {
            bool ignoreIt = false;
            int pos = 0;
            int patLen = 0;
            int iniOccur = -1;

            WordAndPosition occur;

            #region + Open and close word command

            if (pattern.Length > 1)
            {
                if (pattern[0] == 92)
                { startWord = true; pattern = pattern.Slice(1); }
                else if (pattern[pattern.Length - 1] == 92)
                { endWord = true; pattern = pattern.Slice(0, pattern.Length - 2); }
            }

            #endregion

            for (pos = startIndex; pos < sourceTextSpan.Length; ++pos)
            {
                #region + Lengths of source text and pattern

                if ( (pos + pattern.Length) > sourceTextSpan.Length - 1) { break; }

                #endregion

                #region + Ignore apostrophes

                if (ignoreApostrophes)
                {
                    if (sourceTextSpan[pos] == '\'')
                    { ignoreIt = !ignoreIt; continue; }
                    if (ignoreIt) continue;
                }

                #endregion

                #region + Open word

                if (startWord)
                {
                    if (pos > 0 && IsSeparator(sourceTextSpan[pos]))
                    {
                        pos++;
                        iniOccur = pos;
                    }
                    else if(iniOccur == -1)
                    { continue; }
                }

                #endregion

                #region + Close word

                if (endWord && iniOccur != -1)
                {
                    if (IsSeparator(sourceTextSpan[pos]))
                    { patLen = (pos - iniOccur); break; }
                    else { patLen++; continue; }
                }

                #endregion

                patLen = isMatchedPattern(sourceTextSpan, pattern, ref pos, ref iniOccur);

                if (patLen > 0)
                {                    
                    if (!endWord) { break; } // If not end word, break to return occur
                }
                else if (!startWord) { iniOccur = -1; patLen = 0; }
            }

            if (iniOccur == -1 || patLen == 0)
            {
                #region + Not found any pattern, so returns a empty occurrence
                return new WordAndPosition("", -1);
                #endregion
            }
            else { occur = new(sourceTextSpan.Slice(iniOccur, patLen), iniOccur); }

            return occur;
        }

        private static int isMatchedPattern(ReadOnlySpan<char> sourceTextSpan, Span<char> pattern, ref int pos, ref int iniOccur)
        {
            int patPos = 0;
            int patLen = 0;
            bool isNum = false;
            bool isLetter = false;
            bool isSeparator = false;

            for (int txtPos = pos; txtPos < sourceTextSpan.Length; txtPos++)
            {
                #region ? Ignore word separators

                if (pattern[patPos] == '_' && iniOccur !=-1)
                {
                    if (IsSeparator(sourceTextSpan[txtPos])) 
                    { isSeparator = true; continue; }
                    else if (isSeparator)
                    { isSeparator = false; patPos++; }
                }
                else if (isSeparator)
                {  isSeparator = false; patPos++; txtPos--; }
                #endregion

                #region ? Insert number in matches if '#' is in pattern

                if (Char.IsDigit(sourceTextSpan[txtPos]) && pattern[patPos] == '#')
                {
                    isNum = true;
                    if (iniOccur == -1) { iniOccur = txtPos; }
                }
                else if (isNum) 
                { 
                    if (patPos == pattern.Length - 1) { return txtPos - iniOccur; }
                    isNum = false; patPos++; txtPos--;
                }
                #endregion

                #region ? Insert letter in matches if '@'

                else if (Char.IsLetter(sourceTextSpan[txtPos]) && pattern[patPos] == '@')
                {
                    isLetter = true;
                    if (iniOccur == -1) { iniOccur = txtPos; }
                }
                else if (isLetter) 
                { 
                    if (patPos == pattern.Length - 1) { return txtPos - iniOccur; }
                    isLetter = false; patPos++; txtPos--;
                }
                #endregion

                #region ? Insert char if is in pattern

                else if (sourceTextSpan[txtPos] == pattern[patPos])
                {
                    if (iniOccur == -1) { iniOccur = txtPos; }
                    if (patPos == pattern.Length - 1) { txtPos++; return txtPos - iniOccur; }
                    else { patPos++; }
                }

                #endregion

                #region ! Not match pattern in word mode

                else if (startWord) { return 0; }

                #endregion

                #region ! Not match pattern

                else { iniOccur = -1; return 0; }

                #endregion
            }

            return patLen;
        }

        #endregion
                
        #region » IsSeparator
        private static bool IsSeparator(char c) => c == ' ' || c == '!' || c == '?' || c == '.' || c == ';' ||
                                    c == ':' || c == ',' || c == '|' || c == '(' || c == ')' || c == '[' || c == ']'
                                    || c == '{' || c == '}' || c=='\n' || c=='\t' || c=='\r';
        #endregion

        #endregion

        #region ▼ Snippets

        #region ► First occurs

        /// <summary>
        /// Capture first founded snippet of text using open and close patterns tags 
        /// </summary>
        /// <param name="snippetTags">Patterns tags to identify open and close snippet.
        /// <para>The tags must be splited by wildcard("*"). Ex:"ini:*:end","[*]"</para>
        /// <para>In exemple, open tag is "ini:" or "[" and close tag is ":end" or "]".</para></param>
        /// <param name="options">Options parameters by "TextOptions" static class.
        /// <para>•IgnoreContentInApostrophes - Ignore opening/closing tags enclosed in single quotes</para>
        /// <para>•RemoveOpenCloseTags- Return without the "open" tag at the beginning of the snippet and without the "close" tag at the end of the snippet</para></param>
        /// <returns>Return string with first founded snippet of text</returns>
        public static string ExtractFirstSnippet(string snippetTags, TextOptions options = default)
        {
            string[] _occurs;

            _occurs = snippets("", snippetTags, 1, options);

            if (_occurs.Count() == 0) { return null; }

            return _occurs[0];
        }

        /// <summary>
        /// Capture first founded snippet of text using open and close patterns tags.
        /// <para>If not informed by parameter, the open/close tag in single quote will not be considered.</para>
        /// </summary>
        /// <param name="snippetTag">Patterns tags to identify open and close snippet.
        /// <para>The tags must be splited by wildcard("*"). Ex:"ini:*:end","[*]", "div */div"</para>
        /// <para>In exemple, open tag is "ini:" or "[" and close tag is ":end" or "]".</para></param>
        /// <param name="snippetID">Expression to identify especific snippet in text.
        /// <para>This will be concatened with "open" tag to find escific snippet. Inside founded snippet, the snippetID is not used.</para>
        /// <para>Thus only "open" tag without snippetID is consider to identify child snippets inside snippet father.</para>
        /// <para>Ex:SnippetTag="div */div", SnippetID="id='divTest'". Search snippet starting with "div id='divText'"</para>
        /// <para>Inside this snippet only "div " is considered like any child snippet "open" tag.</para></param>
        /// <param name="options">Options parameters by "TextOptions" static class.
        /// <para>•ConsiderSingleQuotes - The opening/closing tags will be considered even if it in enclosed in single quotes</para>
        /// <para>•RemoveOpenCloseTags- Return without the "open" tag at the beginning of the snippet and without the "close" tag at the end of the snippet</para></param>
        /// <returns>Return string with first founded snippet of text</returns>
        public static string ExtractFirstSnippet(string snippetTag, string snippetID, TextOptions options = default)
        {
            string[] _occurs;

            _occurs = snippets(snippetID, snippetTag, 1, options);

            if (_occurs.Count() == 0) { return null; }

            return _occurs[0];
        }

        #endregion

        #region ► Snippets
        /// <summary>
        /// Capture all founded snippets of text using open and close patterns tags.
        /// <para>If not informed by parameter, the open/close tag in single quote will not be considered.</para>
        /// </summary>
        /// <param name="snippetTag">Patterns tags to identify open and close snippet.
        /// <para>The tags must be splited by wildcard("*"). Ex:"ini:*:end","[*]"</para>
        /// <para>In exemple, open tag is "ini:" or "[" and close tag is "end:" or "]".</para></param>
        /// <param name="options">Options parameters by "TextOptions" static class.
        /// <para>•Distinct- Not return duplcate snippet</para>
        /// <para>•IgnoreContentInApostrophes- Ignore opening/closing tags enclosed in single quotes</para>
        /// <para>•RemoveOpenCloseTags- Return without the "open" tag at the beginning of the snippet and without the "close" tag at the end of the snippet</para></param>
        /// <returns></returns>
        public static string[] ExtractSnippets(string snippetTag, TextOptions options = default)
        {
            string[] _occurs;

            _occurs = snippets("", snippetTag, 0, options);

            return _occurs;
        }

        #endregion

        #region » Get Sinppets

        private static string[] snippets(string snippet, string openCloseTags = "", int count = 0, TextOptions options = default)
        {
            if (snippet == "" && openCloseTags == "") { return default; }

            #region + Snippet open and close tags

            string[] _openCloseTags = openCloseTags.Split('*');
            string closeTag = _openCloseTags[_openCloseTags.Length - 1];
            int openedTextCount = 0;

            #endregion

            #region + Ignore text open and close

            bool ignoreIt = false;
            bool inOccur = false;

            #endregion

            string openTag = _openCloseTags[0] + snippet;
            #region + Params           

            setParams(options);

            #endregion

            #region + Occurs list

            List<string> occurs = new List<string>();
            HashSet<string> distinctOccurs = new HashSet<string>();
            StringBuilder occur = new StringBuilder();

            #endregion

            ReadOnlySpan<char> text = ""; // SourceText;

            for (int pos = 0; pos < text.Length; ++pos)
            {
                if (pos + openTag.Length > text.Length || pos + closeTag.Length > text.Length) { break; }

                //occur.Append(text[pos]);

                #region + Ignore in apostrophes

                if (ignoreApostrophes)
                {
                    if (!ignoreIt)
                    {
                        if (text.Slice(pos).StartsWith("'"))
                        {
                            ignoreIt = true;
                            if (inOccur) { occur.Append("'"); }
                            continue;
                        }
                    }
                    else if (text.Slice(pos).StartsWith("'"))
                    {
                        ignoreIt = false;
                        if (inOccur) { occur.Append("'"); }
                        continue;
                    }
                    else
                    {
                        if (inOccur) { occur.Append(text[pos]); continue; }
                    }
                }

                #endregion

                #region + Opened snippet

                if (text.Slice(pos, openTag.Length).SequenceEqual(openTag))
                {
                    openedTextCount++;

                    if (!inOccur)
                    {
                        pos += openTag.Length - 1;

                        if (!removeTags) { occur.Append(openTag); }

                        openTag = _openCloseTags[0];
                        inOccur = true;
                    }
                    else
                    { occur.Append(text[pos]); }
                    continue;
                }

                #endregion

                #region + Verify close snippet

                else if (text.Slice(pos, closeTag.Length).SequenceEqual(closeTag))
                {
                    openedTextCount--;

                    if (openedTextCount == 0)
                    {
                        #region + Close snippet

                        inOccur = false;

                        #region + Remove open/close tags

                        if (!removeTags) { occur.Append(closeTag); }

                        #endregion

                        string currentOccur = occur.ToString();
                        if (distinct) { distinctOccurs.Add(currentOccur); }
                        else { occurs.Add(currentOccur); }
                        occur.Clear();

                        if (count > 0)
                        {
                            if (occurs.Count == count)
                            {
                                if (distinct) { return distinctOccurs.ToArray(); }
                                else { return occurs.ToArray(); }
                            }
                        }

                        openTag = _openCloseTags[0] + snippet;

                        continue;

                        #endregion
                    }
                    else
                    { occur.Append(text[pos]); }
                }

                #endregion

                else if (inOccur)
                {
                    occur.Append(text[pos]);
                }

            }

            if (distinct) { return distinctOccurs.ToArray(); }
            else { return occurs.ToArray(); }
        }

        #endregion

        #endregion

        #region ► RestoreIdentifiedMatches

        public static string RestoreIdentifiedMatches(string text, string pattern, string[] removed)
        {
            StringBuilder _text = new StringBuilder(text);

            int _index = 0;
            foreach (string _occur in removed)
            {
                string _pattern = pattern.Replace("{n}", (_index).ToString());
                _text.Replace(_pattern, _occur);
                _index++;
            }

            return _text.ToString();
        }
        #endregion

    }

    #endregion
}
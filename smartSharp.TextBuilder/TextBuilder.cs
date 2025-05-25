using System;
using System.Buffers;
using System.ComponentModel.Design;
using System.Net;
using System.Reflection;
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
        /// The list of matches return sorted by word alphabetical order
        /// </summary>
        OrderByWord = 50,

        /// <summary>
        /// Return occurrence with only parts building by pattern(wildcard). Remove litteral text.
        /// <para>Return occurrences only pattern parts</para>
        /// </summary>
        OnlyCharsInPattern = 60,

        /// <summary>
        /// <para>Init occur with start word symbol.</para>
        /// </summary>
        ByStartWord = 70,

        /// <summary>
        /// <para>End occur with end word symbol.</para>
        /// </summary>
        ByEndWord = 80
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

    public class TextBuilder
    {
        #region ▼ Properties

        #region • SourceText

        private ReadOnlyMemory<char> sourceText;

        #endregion

        #region • Success

        private bool success = false;
        public bool Success
        {
            get { return success; }
        }

        #endregion

        #region ▼ Params

        private bool textOption { get; set; }
        private bool ignoreApostrophes { get; set; }
        private bool byStartWord { get; set; }
        private bool byEndWord { get; set; }
        private bool distinct { get; set; }
        private bool removeTags { get; set; }
        private bool orderByPosition { get; set; }
        private bool orderByWord { get; set; }
        private bool onlyPatternParts { get; set; }

        #endregion

        #region • StartIndex

        private int startIndex = 0;
        public int StartIndex { get { return startIndex; } }

        #endregion

        #region ► Separator of words

        private static bool IsSeparator(char c) => c == '\'' || c == ' ' || c == '!' || c == '?' || c == '.' || c == ';' ||
                                    c == ':' || c == ',' || c == '|';

        private static bool IgnoreSeparator(char c, char lastChar, char nextChar)
        {
            if (Char.IsNumber(lastChar) && (Char.IsNumber(nextChar) && nextChar != ' '))
            {
                return c != '\'' && c != ' ' & c != '!' && c != '?' && c != ';' &&
                                    c != ':' && c != '|';
            }
            else
            {
                return c != '\'' && c != ' ' && c != '!' && c != '?' && c != '.' && c != ';' &&
                                    c != ':' && c != ',' && c != '|';
            }
        }

        #endregion
                
        #endregion

        #region ► Instance

        private void setParams(string text)
        {
            sourceText = text.AsMemory();
            byStartWord = false;
            byEndWord = false;
            ignoreApostrophes = false;
            distinct = false;
            removeTags = false;
            startIndex = 0;
            orderByPosition = false;
            orderByWord = false;
        }

        public TextBuilder(string text)
        {
            setParams(text);
        }

        public TextBuilder(string text, TextOptions options)
        {
            setParams(text);

            if (options.HasFlag(TextOptions.OrderByPosition)) { orderByPosition = true; }
            else { orderByWord = true; }
        }

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
        public StringAndPosition MatchFirst(string stringsToMatch, TextOptions option = default)
        {
            setParams(option);
            return matchPattern(sourceText.Span, stringsToMatch, 0);
           //return executeMatch(sourceText.Span, stringsToMatch, 0, 1, option);
        }

        /// <summary>
        /// Match first text in string. Not case sentive.
        /// </summary>
        /// <param name="stringsToMatch">Texts to search in source string.
        /// <para>Use "|" to split various texts. Ex:"john|marie|Jack"</para></param>
        /// <param name="options">Parameters of the search in 'Options' internal class</param>
        /// <returns>Return string of first occurs without case sensitive.</returns>
        //private MatchEntry executeMatch(ReadOnlySpan<char> sourceText, string stringsToMatch, int startIndex, int count, TextOptions option)
        //{
        //    if (stringsToMatch == "") { return default; }

        //    #region + Params           

        //    setParams(option);

        //    #endregion

        //    #region + List words to matches

        //    MatchEntry listToMatch = new ToMatchStrings(stringsToMatch);

        //    #endregion

        //    #region + Matches collection

        //    MatchEntry occurs;
        //    if (count == 1)
        //    { occurs = new MatchString(); }
        //    else { occurs = new MatchesList(); }

        //    #endregion      

        //    for (int i = 0; i < listToMatch.Count; i++)
        //    {
        //        #region + First match

        //        if (count == 1)
        //        {
        //            if (!listToMatch.GetIsPattern(i))// If this word not a pattern match
        //            {
        //                #region ? Simple match

        //                StringAndPosition _occur = matchFirst(sourceText, listToMatch[i], startIndex, ignoreApostrophes);
        //                if (_occur == null) { return null; }
        //                if (_occur.Position < occurs.Position || occurs.Position == -1)
        //                { occurs.Set(_occur.Text, _occur.Position); }

        //                #endregion
        //            }
        //            else
        //            {
        //                #region ! Pattern first match

        //                int posMatch = 0;
        //                ReadOnlySpan<char> patterns = stringsToMatch.AsSpan();
        //                StringAndPosition _occur = matchPattern(sourceText, patterns, startIndex,
        //                                                              ignoreApostrophes);

        //                if (_occur == null) { success = false; return null; } else { success = true; }
        //                if (_occur.Position < occurs.Position || occurs.Position == -1)
        //                { occurs.Set(_occur.Text, _occur.Position); }


        //                #endregion
        //            }
        //        }
        //        #endregion

        //        #region + More than one match

        //        else
        //        {
        //            if (!listToMatch.GetIsPattern(i))
        //            {
        //                #region ? Simple matches

        //                match(sourceText, listToMatch[i], ref occurs, startIndex, count,
        //                       ignoreApostrophes, byWord, distinct, crossMatches, removeTags);

        //                #endregion
        //            }
        //            else
        //            {
        //                #region ! Pattern matches

        //                #endregion
        //            }
        //        }

        //        #endregion
        //    }

        //    if (occurs.Count == 0) { return default; }
        //    return occurs;
        //}

        //#endregion

        #region ► Matches

        /// <summary>
        /// Match first text in string. Not case sentive.
        /// </summary>
        /// <param name="textsToMatch">Texts to search in source string.
        /// <para>Use "|" to split various texts. Ex:"john|marie|Jack"</para></param>
        /// <param name="options">Parameters of the search in 'Options' internal class</param>
        /// <returns>Return string of first occurs without case sensitive.</returns>
        //public WordsMatches Matches(string textsToMatch, TextOptions option = default)
        //{
        //    if (textsToMatch == "") { return default; }

        //    #region + Params           

        //    setParams(option);
        //    occurs.OrderByPosition = orderByPosition;
        //    occurs.OrderByWord = orderByWord;

        //    #endregion

        //    startIndex = 0;

        //    StringBuffer _texts = new(textsToMatch);

        //    for (int i = 0; i < _texts.Count; i++)
        //    {
        //        if (_texts.IsPattern) {  matchPattern(_texts[i]);  }
        //        else {  //match( _texts[i] ); }
        //    }

        //    occurs.OrderByPosition = orderByPosition;
        //    occurs.OrderByWord = orderByWord;

        //    if (occurs.Count == 0) { return default; }

        //    return occurs;
        //}

        /// <summary>
        /// Match first text in string. Not case sentive.
        /// </summary>
        /// <param name="texts">Texts to search in source string.
        /// <para>Use "|" to split various texts. Ex:"john|marie|Jack"</para></param>
        /// <param name="startIndex">Start search from this position in source text</param>
        /// <param name="count">Number of characters to search</param>
        /// <param name="options">Parameters of the search in 'Options' internal class</param>
        /// <returns>Return string of first occurs without case sensitive.</returns>
        //public WordsMatches Matches(string texts, int startIndex, int count, TextOptions option = default)
        //{
        //    if (texts == "") { return default; }

        //    #region + Params           

        //    setParams(option);
        //    occurs.OrderByPosition = orderByPosition;
        //    occurs.OrderByWord = orderByWord;

        //    #endregion

        //    count = 1;

        //    StringBuffer _texts = new(texts);

        //    for (int i = 0; i < _texts.Count; i++)
        //    {
        //        if (_texts.IsPattern) { matchPattern(_texts[i]); }
        //        else { match(_texts[i]); }
        //    }

        //    occurs.OrderByPosition = orderByPosition;
        //    occurs.OrderByWord = orderByWord;

        //    if (occurs.Count == 0) { return default; }
        //    return occurs;
        //}

        #endregion

        #endregion

        #endregion

        #region ▼ Matches Models

        #region » SetParams

        private void setParams(TextOptions option)
        {
            textOption = false;

            if (option.HasFlag(TextOptions.IgnoreInSingleQuotes))
            { ignoreApostrophes = true; textOption = true; }
            else { ignoreApostrophes = false; }

            if (option.HasFlag(TextOptions.ByStartWord))
            { byStartWord = true; textOption = true; }
            else { byStartWord = false; }

            if (option.HasFlag(TextOptions.ByEndWord))
            { byEndWord = true; textOption = true; }
            else { byEndWord = false; }

            if (option.HasFlag(TextOptions.Distinct))
            { distinct = true; textOption = true; }
            else { distinct = false; }

            if (option.HasFlag(TextOptions.OrderByPosition))
            { orderByPosition = true; textOption = true; }
            else { orderByPosition = false; }

            //if (option.HasFlag(TextOptions.OrderByWord))
            //{ orderByWord = true; }
            //else { orderByWord = false; }
                        
            if (option.HasFlag(TextOptions.OnlyCharsInPattern))
            { onlyPatternParts = true; textOption = true; }
            else { onlyPatternParts = false; }
        }

        #endregion

        #region » MatchPattern

        private StringAndPosition matchPattern(ReadOnlySpan<char> sourceTextSpan, ReadOnlySpan<char> matchSpanText, int startIndex)
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

                if ((matchSpanText[patIndex] == '*' || patIndex == matchSpanText.Length -1) 
                    && matchSpanText[patIndex] != '|')
                {
                    if(matchSpanText[patIni] == '*') { patIni++; }
                    if(matchSpanText[patIni + (patLen - 1)] == '*') { patLen--; }
                                              
                    WordAndPosition occur = matchLitteral(sourceTextSpan, matchSpanText.Slice(patIni, patLen), startIndex);

                    if ( (occur.Position < orOccurIni || orOccurLen == 0) && !occur.Empty)
                    {
                        #region + Is a regular pattern

                        if (occurIni == -1 || (occurIni ==0 && textOption) ) { occurIni = occur.Position; occurLen = occur.Word.Length; }
                        else { occurLen = (occur.Position - occurIni) + occur.Word.Length; }

                        orOccurIni = 0;
                        orOccurLen = 0;

                        #endregion
                    }
                    else if(occur.Position >= orOccurIni || orOccurLen != 0)
                    {
                        #region + Is a orCondition pattern

                        if (occurIni == -1) { occurIni = orOccurIni; occurLen = orOccurLen; }
                        else { occurLen = (orOccurIni - occurIni) + orOccurLen; }

                        orOccurIni = 0;
                        orOccurLen = 0;

                        #endregion
                    }
                    else if(occur.Empty)
                    {
                        #region + Don't found any pattern, so returns a empty occurrence

                        return new StringAndPosition("", 0);

                        #endregion
                    }

                    patLen = 0;
                    patIni = patIndex + 1;
                    startIndex = occurIni;
                }

                #endregion

                #region + By orCondition

                else if (matchSpanText[patIndex] == '|' && patIndex >0)
                {
                    if (matchSpanText[patIni] == '|') { patIni++; }
                    if (matchSpanText[patIni + (patLen - 1)] == '|') { patLen--; }

                    WordAndPosition occur = matchLitteral(sourceTextSpan, matchSpanText.Slice(patIni, patLen), startIndex);

                    if (occur.Position < orOccurIni || orOccurIni ==0 )
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

            if (matchSpanText[matchSpanText.Length -1] == '*') { occurLen = sourceTextSpan.Length - occurIni; }

            return new StringAndPosition(sourceTextSpan.Slice(occurIni, occurLen), occurIni);
        }

        #endregion

        #region » match
        private WordAndPosition matchLitteral(ReadOnlySpan<char> sourceTextSpan, ReadOnlySpan<char> pattern, int startIndex)
        {
            bool ignoreIt = false;
            int commandIndex = -1;

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

                #region + By command

                if(byStartWord)
                {
                    if (IsSeparator( sourceTextSpan[pos]))
                    { commandIndex = pos; pos++;}
                }

                if (byEndWord && commandIndex > -1)
                {
                    if (IsSeparator(sourceTextSpan[pos]))
                    {
                        return new(sourceTextSpan.Slice(++commandIndex, (pos - commandIndex) + pattern.Length), commandIndex);
                    }
                }

                #endregion

                if (sourceTextSpan.Slice(pos, pattern.Length).SequenceEqual(pattern))
                {
                    if(byEndWord)
                    { commandIndex = pos; continue; }

                    WordAndPosition occur;

                    if (commandIndex > -1)
                    {
                        occur = new(sourceTextSpan.Slice(++commandIndex, (pos - commandIndex) + pattern.Length), commandIndex);
                    }
                    else { occur = new(pattern, pos); }

                    return occur;
                }
            }

            return default;
        }

        #endregion

        #region » matchFirst
        private static StringAndPosition matchFirst(ReadOnlySpan<char> sourceTextSpan, ReadOnlySpan<char> matchSpanText,
                                                   int startIndex, bool ignoreApostrophes)
        {
            bool ignoreIt = false;

            for (int pos = startIndex; pos < sourceTextSpan.Length; ++pos)
            {
                if (pos + matchSpanText.Length > sourceTextSpan.Length) { break; }

                if (ignoreApostrophes)
                {
                    if (sourceTextSpan[pos] == '\'')
                    { ignoreIt = !ignoreIt; continue; }
                    if (ignoreIt) continue;
                }

                if (sourceTextSpan.Slice(pos, matchSpanText.Length).SequenceEqual(matchSpanText))
                {
                    StringAndPosition occur = new(matchSpanText.ToString(), pos);
                    return occur;
                }
            }

            return null;
        }

        #endregion

        #region » matchFirstPattern

        //private static StringAndPosition matchFirstPattern(ReadOnlySpan<char> sourceTextSpan, ReadOnlySpan<char> matchSpanText,
        //                                           int startIndex, bool ignoreApostrophes, bool onlyPatternParts, ref int posMatch)
        //{
        //    PatternBuffer pattern = new PatternBuffer(matchSpanText);
                        
        //    #region + Flags

        //    bool ignoreIt = false;

        //    int initOccur = -1;
        //    //if (pattern.CaptureInStart) { initOccur = 0; }

        //    int length = 0;

        //    #endregion

        //    int sourceTextLength = sourceTextSpan.Length;

        //    //Patterns itens
        //    for (int _iten =0; _iten < patPos.Count; _iten++)
        //    {
        //        int patLength = patLens[_iten];
        //        ReadOnlySpan<char> thisPattern = patPos.Slice(positions[_iten], patLength).ToArray();

        //        if (pos + patLength > sourceTextLength) { break; }

        //        //Source text characters
        //        for (int pos = startIndex; pos < sourceTextSpan.Length; ++pos)
        //        {
        //            #region + Lengths of source text and pattern

                    

        //            #endregion

        //            #region + Ignore apostrophes

        //            if (ignoreApostrophes)
        //            {
        //                if (sourceTextSpan[pos] == '\'') { ignoreIt = !ignoreIt; continue; }
        //                if (ignoreIt) continue;
        //            }

        //            #endregion
                                                            
        //            int equalSequenceLen = pattern.SequenceEqual(sourceTextSpan.Slice(pos, patLength));

        //            if (equalSequenceLen != 0)
        //            {
        //                #region + Start position of match
        //                if (pattern.IsFirst && pattern.CaptureInStart)
        //                {
        //                    initOccur = 0;
        //                }
        //                #endregion

        //                #region + Not first pattern
        //                if ( initOccur ==-1) 
        //                {
        //                    initOccur = pos;
        //                    length = equalSequenceLen;
        //                }
        //                #endregion

        //                else
        //                {
        //                    #region + Length of match

        //                    length = (pos - initOccur ) + equalSequenceLen;

        //                    #endregion
        //                }

        //                pattern.Captured();

        //                #region + Is end of match and there is wildcard in end of pattern

        //                if (pattern.CaptureInEnd && pattern.IsEnd)
        //                {
        //                    length = (sourceTextSpan.Length - initOccur);
        //                    break;
        //                }

        //                #endregion

        //                #region + End of match

        //                startIndex = initOccur + length;
        //                break;

        //                #endregion

        //            }

        //        }
        //    }

        //    if (pattern.CapturedAll)
        //    {
        //        return new(sourceTextSpan.Slice(initOccur, length ), initOccur);
        //    }
        //    else
        //    {
        //        return null;
        //    }
        //}

        #endregion

        #region » match
        //private static void match(ReadOnlySpan<char> sourceTextSpan, ReadOnlySpan<char> matchSpanText,
        //                          ref MatchEntry occurs, int startIndex, int count, bool ignoreApostrophes,
        //                          bool byWord, bool distinct, bool crossMatches, bool removeTags)
        //{
        //    bool ignoreIt = false;
        //    int posIndex = 0;

        //    for (int pos = startIndex; pos < sourceTextSpan.Length; ++pos)
        //    {
        //        if (pos + matchSpanText.Length > sourceTextSpan.Length) { break; }

        //        if (ignoreApostrophes)
        //        {
        //            if (sourceTextSpan[pos] == '\'') { ignoreIt = !ignoreIt; continue; }
        //            if (ignoreIt) continue;
        //        }

        //        if (sourceTextSpan.Slice(pos, matchSpanText.Length).SequenceEqual(matchSpanText))
        //        {
        //            //resultMatches[posIndex++] =
        //            occurs.Add(matchSpanText.ToString(), pos);

        //            if (distinct || count == 1) { break; }

        //            posIndex = pos;
        //            pos += matchSpanText.Length;
        //        }
        //    }

        //    return;
        //}

        #endregion
               
        #region » matchPattern
        //private void matchPattern(string matchText)
        //{
        //    ReadOnlySpan<char> sourceTextSpan = SourceText.AsSpan();
        //    PatternBuffer pattern = new PatternBuffer(matchText);

        //    #region + Flags

        //    bool ignoreIt = false;
        //    int initOccur = -1;
        //    #endregion

        //    if (pattern.InCapture && !byWord) { initOccur = 0; }

        //    int length = 0;

        //    for (int pos = startIndex; pos < sourceTextSpan.Length; ++pos)
        //    {
        //        int segmentLength = pattern.Text.Length;
        //        if (pos + segmentLength > sourceTextSpan.Length) { break; }

        //        if (ignoreApostrophes)
        //{
        //    if (sourceTextSpan[pos] == '\'') { ignoreIt = !ignoreIt; continue; }
        //    if (ignoreIt) continue;
        //}

        //        #region + By word

        //        if (pattern.InCapture && byWord)
        //        {
        //            char cmd = breakOrContinueByWord(ref sourceTextSpan, ref pattern, pos, ref initOccur);
        //            if (cmd =='b') { break; } else if (cmd =='c') { continue; }                   
        //        }

        //        #endregion

        //        if (sourceTextSpan.Slice(pos, segmentLength).SequenceEqual(pattern.Text) && pattern.Text!="")
        //        {
        //            if (initOccur == -1) { initOccur = pos; }

        //            pattern.Next();

        //            if (pattern.IsEnd && pattern.InCapture && byWord)
        //            { continue; }
        //            else if (pattern.IsEnd)
        //            {
        //                if (endOfMatch(ref sourceTextSpan, ref pattern, pos, length, initOccur))
        //                { break; }
        //            }
        //        }
        //    }

        //    #region + Early end of capture

        //    endOfPatternWildcard(ref sourceTextSpan, pattern.InCapture, length, initOccur);

        //    #endregion

        //    return;
        //}

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
        public string ExtractFirstSnippet(string snippetTags, TextOptions options = default)
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
        public string ExtractFirstSnippet(string snippetTag, string snippetID, TextOptions options = default)
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
        public string[] ExtractSnippets(string snippetTag, TextOptions options = default)
        {
            string[] _occurs;

            _occurs = snippets("", snippetTag, 0, options);

            return _occurs;
        }

        #endregion

        #region » Get Sinppets

        private string[] snippets(string snippet, string openCloseTags = "", int count = 0, TextOptions options = default)
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
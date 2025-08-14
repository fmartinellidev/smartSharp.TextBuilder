using System.Buffers;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using static System.Net.Mime.MediaTypeNames;

namespace SmartSharp.TextBuilder
{
    #region ▼ StringAndPosition

    public class StringAndPosition
    {
        public bool Empty
        {
            get
            {
                if (Text.Length == 0 || Position < 1)
                { return true; }
                else { return false; }
            }
        }
        public int Position { get; set; }
        public string Text { get; set; }

        public StringAndPosition(string text, int position)
        {
            Position = position;
            Text = text;
        }

        public StringAndPosition()
        {
            Position = -1;
            Text = "";
        }

        public void Set(string text, int position)
        {
            Position = position;
            Text = text;
        }
    }

    #endregion

    public static class TextBuilder
    {
        #region ▼ ListPatterns
        public ref struct PatternsToMatch
        {
            #region Properties

            private ReadOnlySpan<char> source;
            private ReadOnlySpan<int> positions;
            private ReadOnlySpan<int> lengths;
            private ReadOnlySpan<int> dynamicChar;
            public int DynamicCharPosition(int index)
            {
                if ((uint)index >= (uint)count) // fast bounds check
                { throw new IndexOutOfRangeException(); }
                return dynamicChar[index] - positions[index];
            }

            private int count;
            public bool WildcardInFirstChar { get; }
            public bool WildcardInLastChar { get; }
            public int Count => count;
            public bool Empty => count == 0;

            public bool ContainsWildcards(int index)
            {
                int pos = this[index].IndexOf('*');

                if(  pos == -1 || 
                    (pos ==0 && index==0) || 
                     pos == this[index].Length && index == positions.Length) 
                {  return false; }

                return true;
            }

            public bool ContainsCompleteWord(int index)
            {
                return this[index].IndexOf('~') != -1;
            }

            #endregion

            #region Constructor

            public PatternsToMatch(ReadOnlySpan<char> text, char splitChar = '\0', char ignoreChar = '\0')
            {
                source = text;
                Span<int> _positions = stackalloc int[160];
                Span<int> _lengths = stackalloc int[160];
                Span<int> _dynamic = stackalloc int[160];
                _dynamic.Fill(-1);

                if (source.Length == 0) { count = 0; return; }

                #region ** Exception : Invlaid pattern

                if (splitChar != '*')
                {
                    if (text[0] == splitChar || text[text.Length - 1] == splitChar)
                    { throw new InvalidOperationException("Can't use splitChar to start or end pattern!"); }
                }
                else if (splitChar == '\0')
                {
                    if (text[0] == '|' || text[text.Length - 1] == '|')
                    { throw new InvalidOperationException("Can't use 'OR' char to start or end pattern!"); }
                }

                #endregion

                int pos = 0, idx = 0;
                bool ignoreMode = false;
                int start = 0;

                for (; pos < source.Length; pos++)
                {
                    char c = source[pos];

                    if (c == ignoreChar) ignoreMode = !ignoreMode;
                    else if (!ignoreMode)
                    {
                        #region ? Is a double 'OR' split
                        if (splitChar == '\0' && c == '|' && pos + 1 < source.Length)
                        { if (source[pos + 1] == '|') c = splitChar; }
                        #endregion

                        if (c == splitChar && pos != 0 && pos != source.Length - 1)
                        {
                            #region + Update positions and lengths
                            _positions[idx] = start;
                            _lengths[idx++] = pos - start;
                            #endregion

                            #region ? Update 'pos' if is double 'OR' split
                            if (c == '\0' && splitChar == '\0') pos++;
                            #endregion

                            start = pos + 1;

                            #region ** Exception : Too many pattern parts
                            if (idx == 161)
                            { throw new IndexOutOfRangeException("TextBuilder only accept max 160 pattern parts ('|', '*', '||' and '~')!"); }
                            #endregion
                        }

                        #region + Dynamic char position
                        if (c == '_' || c == '#' || c == '~')
                        {
                            if (_dynamic[idx] == -1) { _dynamic[idx] = pos; }
                        }
                        #endregion
                    }
                }

                #region ? Found only one pattern and not split
                _positions[idx] = start;
                _lengths[idx++] = pos - start;
                if (start == 0) { count = 1; } else { count = idx; }
                #endregion

                #region + Start or end with wildcard

                if (text[0] == '*')
                { WildcardInFirstChar = true; _positions[0] = 1; _lengths[0] = _lengths[0] - 1; }

                if (text[text.Length - 1] == '*')
                { WildcardInLastChar = true; _lengths[count - 1] = _lengths[count - 1] - 1; }

                #endregion

                positions = _positions.Slice(0, count).ToArray();
                lengths = _lengths.Slice(0, count).ToArray();
                dynamicChar = _dynamic.Slice(0, count).ToArray();

                _positions = null;
                _lengths = null;
            }

            #endregion

            #region Controller

            public ReadOnlySpan<char> this[int index]
            {
                get
                {
                    if ((uint)index >= (uint)count) // fast bounds check
                    { throw new IndexOutOfRangeException(); }
                    return source.Slice(positions[index], lengths[index]);
                }
            }

            #endregion
        }

        #endregion
                
        #region ▼ Parameters

        /// <summary>
        /// Ignore if upper or down case.
        /// </summary>
        public static byte ParamsIgnoreCaseSensitive { get; } = 1;

        /// <summary>
        /// Ignore content in single quotes when parsing text.
        /// </summary>
        public static byte ParamsIgnoreInQuotes { get; } = 2;

        /// <summary>
        /// Ignore content in single quotes when parsing text.
        /// </summary>
        public static byte ParamsIgnoreInDoubleQuotes { get; } = 3;

        /// <summary>
        /// Return only captured chars in text and ignore chars in pattern.
        /// </summary>
        public static byte ReturnCapturedSegment { get; } = 4;

        /// <summary>
        /// Identify dynamic chars of pattern in text and in pattern.
        /// </summary>
        public static byte ParamsDynamicChars { get; } = 5;

        /// <summary>
        /// Do not force search in text by nearest words to return the shortest occurrence.
        /// </summary>
        public static byte ParamsGreedyOccurence { get; } = 6;

        #endregion
                
        #region ▼ Match Words

        #region ▼ Constructors

        public static StringAndPosition Match(string text, string SequenceToMatch, params byte[] options)
        {
            return Match(text, SequenceToMatch, 0,0, 0, options);
        }

        public static StringAndPosition Match(string text, string SequenceToMatch, int startIndex, params byte[] options)
        {
            return Match(text, SequenceToMatch, startIndex, 0, 0, options);
        }

        public static StringAndPosition Match(string text, string SequenceToMatch, int startIndex, int startIndexReturn, params byte[] options)
        {
            return Match(text, SequenceToMatch, startIndex, startIndexReturn, 0, options);
        }

        public static StringAndPosition Match(string text, string SequenceToMatch, int startIndex, int startIndexReturn, int endCutLenReturn, params byte[] options)
        {
            if(text.Length >=256)
            {
                Span<char> textStack = stackalloc char[text.Length];
                text.CopyTo(textStack);

                return match(textStack, SequenceToMatch.AsSpan().ToArray(), startIndex, startIndexReturn, endCutLenReturn, options);
            }
            else
            {
                char[] textSpan = text.ToArray();

                StringAndPosition result = match(textSpan, SequenceToMatch.AsSpan().ToArray(), startIndex, startIndexReturn, endCutLenReturn, options);
                if (textSpan != null){ Array.Clear(textSpan, 0, textSpan.Length); }

                return result;
            }
        }

        private static ReadOnlySpan<char> adjustByParameters(Span<char> text, params byte[] options)
        {
            #region ? Ignore in quote

            if (options.Contains(ParamsIgnoreInQuotes))
            { text = fillCore(text, ReadOnlySpan<char>.Empty, ReadOnlySpan<char>.Empty, "\'", "\'"); }

            #endregion

            #region ? Ignore in double quote

            if (options.Contains(ParamsIgnoreInQuotes))
            { text = fillCore(text, ReadOnlySpan<Char>.Empty, ReadOnlySpan<Char>.Empty, "\'", "\'"); }

            #endregion           

            return text;
        }

        #endregion

        #region ► Controller

        private static StringAndPosition match(Span<char> text, ReadOnlySpan<char> sequenceToMatch, int startIndex, int startIndexReturn, int endCutLenReturn, params byte[] options)
        {
            if (text.Length == 0) { return new StringAndPosition(); }
            if (sequenceToMatch.Length == 0) { return new StringAndPosition(); }

            #region ++ Flags and variable

            int returnPos = -1;
            int returnLen = -1;

            #endregion

            ReadOnlySpan<char> textToSearch = adjustByParameters(text, options);           
            PatternsToMatch toMatch = new PatternsToMatch(sequenceToMatch);

            for (int i = 0; i < toMatch.Count; i++)
            {
                int pos = startIndex;
                int len = 0;

                #region + Match Pattern

                if (toMatch.ContainsWildcards(i))
                {
                    for (; pos < textToSearch.Length; pos++)
                    {
                        #region + Match
                        (pos, len) = matchPattern(textToSearch, toMatch[i], pos, options);
                        #endregion

                        if (pos == -1) { break; }

                        #region ? There is not wildcard in start and end of pattern and is first occurrence params
                        if (toMatch.WildcardInFirstChar || toMatch.WildcardInLastChar ||
                             options.Contains(ParamsGreedyOccurence))
                        { break; }
                        #endregion

                        #region + Update if this occurence pos is before that previous ( to the OR condition )
                        //If not used OR condition, the occurrence automatically will be the first

                        if ((uint)returnLen > (uint)len)
                        {
                            returnPos = pos;
                            returnLen = len;
                        }
                        #endregion
                    }
                }
                #endregion

                #region + Match Litteral
                else
                {
                    #region + Match

                    (pos, len) = matchLitteral(textToSearch, toMatch[i], pos, options);
                    if (pos == -1) { continue; }

                    #endregion
                }
                #endregion

                #region ? The occurrence is first position in text?

                if ( (uint)pos < (uint)returnPos )
                { returnPos = pos; returnLen = len; }
                else { pos = startIndex; }

                #endregion
            }

            if (returnPos == -1 ) { return new StringAndPosition(); }

            #region + Wildcard of start char in pattern

            if (toMatch.WildcardInFirstChar)
            {   returnLen = returnPos + returnLen; returnPos = 0; }

            #endregion

            #region + Wildcard of last char in pattern

            if (toMatch.WildcardInLastChar)
            { returnLen = text.Length - returnPos; }

            #endregion

            if (startIndexReturn != 0) { returnPos += startIndexReturn; returnLen--; }
            if(endCutLenReturn != 0) { returnLen -= endCutLenReturn; }

            return new StringAndPosition(text.Slice(returnPos, returnLen).ToString(), returnPos);
        }

        #endregion

        #region ▼ Private Auxiliar Methods

        #region » IsSeparator
        private static bool IsSeparator(char c) => c == ' ' || c == '!' || c == '?' || c == '.' || c == ';' ||
                                    c == ':' || c == ',' || c == '|' || c == '(' || c == ')' || c == '[' || c == ']'
                                    || c == '{' || c == '}' || c == '\n' || c == '\t' || c == '\r';
        #endregion

        #region » DynamicCharPosition
        private static int getDynamicCharPosition(ReadOnlySpan<char> pattern)
        {
            for (int i = 0; i < pattern.Length; i++)
            {
                if (pattern[i] == '_' || pattern[i] == '#' || pattern[i] == '~')
                { return i; }
            }

            return -1;
        }

        #endregion

        #endregion

        #region ▼ Models

        #region ► matchLitteral

        private static (int, int) matchLitteral(ReadOnlySpan<char> text, ReadOnlySpan<char> patterns, int startIndex, params byte[] options)
        {
            int pos = 0;
            int returnPos = -1;
            int returnLen = 0;

            #region ** Exception : Invlaid pattern
            if (patterns[0] == '|' || patterns[patterns.Length -1] =='|')
            {
                throw new InvalidFilterCriteriaException(
                "Invalid use of 'OR' char('|')! Not use it to start, end pattern and after wildcard '*'."
                );
            }
            #endregion

            #region + Or split to or pattern

            PatternsToMatch toMatch = new PatternsToMatch(patterns, '|');

            #endregion

            int count = toMatch.Count;
            for (int i = 0; i < count; i++)
            {
                //Reuse pattern to new resize length if inWord char is found
                int len = 0;

                #region + Litteral pattern

                bool caseSensitive = options.Contains(ParamsIgnoreCaseSensitive);
                (pos, len) = indexOf(text, toMatch[i], startIndex, options);

                if (pos == -1) { continue; }

                #endregion

                #region ? The occurrence is first position in text?

                if ((uint)returnPos > (uint)pos) 
                { returnPos = pos; returnLen = len; }
               
                #endregion
            }

            #region + Not found match
            if (returnPos == -1)
            { return (-1, 0); }
            #endregion
                        
            return (returnPos, returnLen);
        }

        #endregion

        #region ► matchPattern

        private static (int, int) matchPattern(ReadOnlySpan<char> text, ReadOnlySpan<char> patterns, int startIndex, byte[] options = null)
        {
            int returnPos = -1;
            int returnLen = -1;
            int pos = 0;
            int len = 0;

            PatternsToMatch toMatch = new PatternsToMatch(patterns, '*');
            int count = toMatch.Count;

            for (int i = 0; i < count; i++)
            {
                (pos, len) = matchLitteral(text, toMatch[i], startIndex, options);
                
                #region ? Not matched pattern
                if (pos == -1)
                {  return (-1, 0); }
                #endregion

                #region ! Matched pattern
                if (returnPos == -1)
                { returnPos = pos; startIndex = pos + 1; returnLen = len; }
                else { returnLen = (pos - returnPos) + len; }
                #endregion
            }

            return (returnPos, returnLen);
        }

        #endregion

        #region ► matchWord

        private static (int, int) matchWord(ReadOnlySpan<char> text, ReadOnlySpan<char> patterns, int startIndex, params byte[] options)
        {
            bool inStart = patterns[0] == '~'; 
            bool inLast = patterns[patterns.Length -1]=='~';

            int wordStart = 0;
            int wordPos = 0;
            int wordLen = patterns.Length;

            #region + Fix up pattern

            Span<char> pattern = stackalloc char[patterns.Length + 2];

            int _start = 0;
            int _len = 0;

            if(inStart) { _start = 1; }
            if(inLast) { _len = (patterns.Length - _start) - 1; }
            else { _len = (patterns.Length - _start); }

            patterns.Slice(_start, _len).CopyTo(pattern);
            pattern = pattern.TrimEnd('\0');

            int inMiddle = pattern.IndexOf('~');

            #endregion

            #region + Search pattern

            wordLen = pattern.Length;
            while (wordPos != -1)
            {
                bool ignoreCase = options.Contains(ParamsIgnoreCaseSensitive);
                ( wordPos, wordLen) = indexOf(text, pattern, startIndex, options);
                wordStart = wordPos;

                #region ? Matched pattern in text already is a complete word, so re-search another
                if (wordStart != -1 && wordStart < text.Length - 1)
                {
                    if (
                          (IsSeparator(text[wordPos - 1]) && inStart) ||
                          (!IsSeparator(text[wordPos - 1]) && !inStart) ||
                          (IsSeparator(text[wordPos + pattern.Length]) && inLast) ||
                          (!IsSeparator(text[wordPos + pattern.Length]) && !inLast)
                        )
                    { startIndex = wordPos + 1; }
                    else
                    { wordPos = -1; }
                }
                #endregion
            }

            #endregion

            #region + Build start word

            wordPos = wordStart;

            if (inStart)
            {
                for (; wordStart >= 0; wordStart--)
                {
                    if (IsSeparator(text[wordStart]))
                    { wordStart++; break; }
                }
            }
            #endregion

            wordLen = (wordPos + pattern.Length) - wordStart;

            #region + Build end of word

            if (inLast)
            {
                for (; wordLen < text[wordPos..].Length; wordLen++)
                {
                    if (IsSeparator(text[wordLen])) { wordLen++; break; }
                }
            }

            #endregion
                        
            return (wordStart, wordLen);
        }

        #endregion

        #endregion

        #region ▼ Methods Utils

        #region ▼ IndexOf

        #region ► IndexOf

        public static int IndexOf(string text, string SequenceToMatch, params byte[] options)
        {
            StringAndPosition matchReturn = Match(text, SequenceToMatch, options);

            return matchReturn.Position;
        }

        public static int IndexOf(string text, string SequenceToMatch, int startIndex, params byte[] options)
        {
            StringAndPosition matchReturn = Match(text, SequenceToMatch, startIndex, options);

            return matchReturn.Position;
        }

        #endregion

        #region ► IndexOfAll

        public static int[] IndexOfAll(string text, string SequenceToMatch, params byte[] options)
        {
            return IndexOfAll(text, SequenceToMatch, 0, options);
        }

        public static int[] IndexOfAll(string text, string SequenceToMatch, int startIndex, params byte[] options)
        {
            if (text.Length >= 256)
            {
                Span<char> textStack = stackalloc char[text.Length];
                text.CopyTo(textStack);

                return indexOfAllCore(textStack, SequenceToMatch, startIndex, options); 
            }
            else
            {
                char[] textSpan = text.ToArray();

                int[] result = indexOfAllCore(textSpan, SequenceToMatch, startIndex, options);
                if (textSpan != null) { Array.Clear(textSpan, 0, textSpan.Length); }

                return result;
            }
        }

        /// <summary>
        /// Index position of start all occurrences in text 
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="SequenceToMatch">Various words splited by coma to match positions.
        /// <para>Each string must be beetween in apostrophes.</para>
        /// <para>All the coma out of apostrophes, will be used to split it.</para></param>
        /// <param name="startIndex">Start search in this position</param>
        /// <param name="options">Parameters</param>
        /// <returns>Array of int with all occurrence index positions</returns>
        private static int[] indexOfAllCore(Span<char> text, ReadOnlySpan<char> SequenceToMatch, int startIndex, params byte[] options)
        {
            //ReadOnlySpan<char> toMatch = SequenceToMatch;
            PatternsToMatch toMatch = new PatternsToMatch(SequenceToMatch.ToArray(), ',', '\'');
            List<int> result = new List<int>();

            int count = toMatch.Count;
            for (int i = 0; i < count; i++)
            {
                StringAndPosition matchReturn = match(text, toMatch[i], 0,0, 0, options);
                result.Add(matchReturn.Position);
            }

            return result.ToArray();
        }

        #endregion

        #region ► Model
        private static (int position, int additionalLen) indexOf(ReadOnlySpan<char> text, ReadOnlySpan<char> pattern, int startIndex, params byte[] options)
        {
            if (pattern.Length == 0) { return (-1, 0); }
            if (text.Length == 0) { return (-1, 0); }
            int occurPos = 0;
            int occurLen = pattern.Length;
            int patPos = 0;
            int len = 0;
            int pos = startIndex;
            bool dynamicChar = options.Contains(ParamsDynamicChars);
            bool ignoreCase = options.Contains(ParamsIgnoreCaseSensitive);
            bool ignoreInQuotes = options.Contains(ParamsIgnoreInQuotes);
            bool ignoreInDoubleQuotes = options.Contains(ParamsIgnoreInDoubleQuotes);
            bool completeWord = false;
            bool ignore = false;

            #region + Literal pattern
            if (!dynamicChar && !ignoreCase && !ignoreInQuotes && !ignoreInDoubleQuotes)
            {
                if (getDynamicCharPosition(pattern) > 0)
                {
                    occurPos = text[occurPos..].IndexOf(pattern);
                    if (occurPos == -1) { return (-1, 0); }
                    else { return (occurPos, occurLen); }
                }
            }
            #endregion

            for (; pos < text.Length; pos++)
            {
                #region + Char in text
                char c = text[pos];
                #endregion

                #region + Ignore in quotes
                                
                if ((ignoreInQuotes && c == '\'') ||
                   (ignoreInDoubleQuotes && c == '\"'))
                {
                    ignore = !ignore;
                    continue;
                }

                if (ignore) { continue; }

                #endregion

                #region + Pattern char
                if ( patPos == pattern.Length ) { break; }
                if( patPos == 0 && completeWord ) { patPos++; }
                char p = (char)pattern[patPos];
                #endregion

                #region + Match char
                if (c == p) 
                { 
                    if (occurPos == -1) { occurPos = pos; }
                    occurLen++; patPos++; continue;
                }
                #endregion

                #region + Word separator char
                if (p == '_' && IsSeparator(c))
                {
                    if (occurPos == -1) { occurPos = pos; }
                    occurLen++; patPos++; continue;
                }
                #endregion

                #region + Number char

                if (Char.IsDigit(c) && p == '#' )
                {
                    len = DynamicNumber(text, pos);
                    if (occurPos == -1) { occurPos = pos; }
                    pos += (len -1); occurLen += len; patPos++; continue;
                }

                #endregion

                #region + Word complete char

                if (p == '~')
                {
                    int _pos=-1;

                    if (patPos ==0) 
                    {
                        if ( pos > 0 )
                        {
                            if (!IsSeparator(text[pos - 1]))
                            { completeWord = true; pos--; patPos++; continue; }
                        }
                    }
                    else 
                    { (_pos, len) = completeWordEnd(text, pos); }

                    if(_pos != -1)
                    {
                        if (occurPos == -1) { occurPos = _pos; }
                        occurLen += len + 1; patPos++; continue;
                    }
                }

                #endregion

                #region + Ignore sensitive case char

                if (ignoreCase)
                {
                    if (toLower(c) == toLower(p))
                    {
                        if (occurPos == -1) { occurPos = pos; }
                        occurLen++; patPos++; continue;
                    }
                }
 
                #endregion

                occurPos = -1; // Reset occurrence position
                occurLen = 0; // Reset occurrence length
                patPos = 0; // Reset pattern position

            }

            if( completeWord)
            { (occurPos, len) = completeWordStart(text, occurPos); occurLen += len; }

            return (occurPos, occurLen);
        }

        #endregion
                
        #region ► dynamicNumber
        private static int DynamicNumber(ReadOnlySpan<char> text, int pos)
        {
            int len = 0;
            for(; pos < text.Length; pos++)
            {
                char c = text[pos];
                if (c < '0' || c > '9')
                { break;}

                len++;
            }

            return len;
        }

        #endregion

        #region ► completeWord

        private static (int newPos, int newCharsLen) completeWordStart(ReadOnlySpan<char> text, int pos)
        {
            int len = 0;
            for (; pos > -1; pos--)
            {
                char c = text[pos];
                if (IsSeparator(c))
                { break; }
                len++;
            }

            return ( pos +1, len - 1 );
        }

        public static (int, int) completeWordEnd(ReadOnlySpan<char> text, int pos)
        {
            int len = 0;
            int start = pos;

            for (; pos < text.Length; pos++)
            {
                char c = text[pos];
                if (IsSeparator(c))
                {
                    if(pos == start)
                    { len = 0; pos = -1; } // Skip first char if not separator

                    break;
                }
                len++;
            }

            return (pos - 1, len - 1);
        }

        #endregion

        #endregion

        #region ▼ ToLower

        #region ► ToLower Ignore in snippet
        public static string ToLowerIgnoreInSnippet(string text, (string open, string close) ignoreBetween)
        {
            if (text.Length >= 256)
            {
                Span<char> textStack = stackalloc char[text.Length];
                text.CopyTo(textStack);

                ReadOnlySpan<char> result = toLowerTextIgnore(textStack, ignoreBetween.open, ignoreBetween.close);
                    
                return result.ToString();
            }
            else
            {
                char[] textSpan = text.ToArray();

                ReadOnlySpan<char> result = toLowerTextIgnore(textSpan, ignoreBetween.open, ignoreBetween.close);
                if (textSpan != null) { Array.Clear(textSpan, 0, textSpan.Length); }

                return result.ToString();
            }
        }
        #endregion

        #region ► ToLOwer Only in snippet
        public static string ToLowerOnlyInSnippet(string text, (string open, string close) onlyBetween)
        {
            if (text.Length >= 256)
            {
                Span<char> textStack = stackalloc char[text.Length];
                text.CopyTo(textStack);

                ReadOnlySpan<char> result = toLowerTextBetween(textStack, onlyBetween.open, onlyBetween.close);

                return result.ToString();
            }
            else
            {
                char[] textSpan = text.ToArray();

                ReadOnlySpan<char> result = toLowerTextIgnore(textSpan, onlyBetween.open, onlyBetween.close);
                if (textSpan != null) { Array.Clear(textSpan, 0, textSpan.Length); }

                return result.ToString();
            }
        }
        #endregion

        #region ► ToLower char
        public static string ToLowerChar(string text, char character)
        {
            Span<char> _char = stackalloc char[1] {character};

            if (text.Length >= 256)
            {
                Span<char> textStack = stackalloc char[text.Length];
                text.CopyTo(textStack);

                ReadOnlySpan<char> result = toLowerChar(textStack, _char);

                return result.ToString();
            }
            else
            {
                char[] textSpan = text.ToArray();

                ReadOnlySpan<char> result = toLowerChar(textSpan, _char);
                if (textSpan != null) { Array.Clear(textSpan, 0, textSpan.Length); }

                return result.ToString();
            }
        }
        #endregion

        #region ► ToLower match
        public static string ToLowerMatch(string text, string sequenceToMatch, int startIndex, params byte[] options)
        {
            ReadOnlySpan<char> _text = text;
            ReadOnlySpan<char> _sequenceToMatch = sequenceToMatch;

            StringAndPosition matchReturn = Match(text, sequenceToMatch, startIndex, options);

            matchReturn.Text = matchReturn.Text.ToLower();

            ReadOnlySpan<char> result = insert(text, matchReturn.Text, matchReturn.Position, matchReturn.Text.Length, true);

            return result.ToString();
        }
        #endregion

        #region ▼ Models

        private static ReadOnlySpan<char> toLowerChar(Span<char> text, Span<char> character)
        {
            // Tabelas estáticas (não alocam em cada chamada)
            static ReadOnlySpan<char> UpperChars() => "ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞß";
            static ReadOnlySpan<char> LowerChars() => "àáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþß";
                        
            for (int i = 0; i < text.Length; i++)
            {
                ref char c = ref text[i];
                // ASCII A-Z

                if (c >= 'A' && c <= 'Z' && c == character[0])
                {
                    c = (char)(c + 32);
                }
                else
                {
                    // Busca em tabela de acentuados
                    int index = UpperChars().IndexOf(c);
                    if (index != -1 && c == character[0])
                    {
                        c = LowerChars()[index];
                    }
                    // Se quiser, adicione fallback:
                    // else
                    // {
                    //     c = char.ToLowerInvariant(c);
                    // }
                }
            }

            return text;
        }

        private static ReadOnlySpan<char> toLowerTextIgnore(Span<char> text, ReadOnlySpan<char> openTag, ReadOnlySpan<char> closeTag)
        {
            // Tabelas estáticas (não alocam em cada chamada)
            static ReadOnlySpan<char> UpperChars() => "ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞß";
            static ReadOnlySpan<char> LowerChars() => "àáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþß";
            bool ignore = false;

            for (int i = 0; i < text.Length; i++)
            {
                ref char c = ref text[i];
                // ASCII A-Z

                if (text.Slice(i, openTag.Length).SequenceEqual(openTag))
                { ignore = true; i += openTag.Length; }
                else if (text.Slice(i, closeTag.Length).SequenceEqual(closeTag))
                { ignore = false; i += closeTag.Length; }

                if (ignore) { continue; }

                if (c >= 'A' && c <= 'Z')
                {
                    c = (char)(c + 32);
                }
                else
                {
                    // Busca em tabela de acentuados
                    int index = UpperChars().IndexOf(c);
                    if (index != -1)
                    {
                        c = LowerChars()[index];
                    }
                    // Se quiser, adicione fallback:
                    // else
                    // {
                    //     c = char.ToLowerInvariant(c);
                    // }
                }
            }

            return text;
        }

        private static ReadOnlySpan<char> toLowerTextBetween(Span<char> text, ReadOnlySpan<char> openTag, ReadOnlySpan<char> closeTag)
        {
            // Tabelas estáticas (não alocam em cada chamada)
            static ReadOnlySpan<char> UpperChars() => "ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞß";
            static ReadOnlySpan<char> LowerChars() => "àáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþß";
            bool accept = false;

            for (int i = 0; i < text.Length; i++)
            {
                ref char c = ref text[i];
                // ASCII A-Z

                if (text.Slice(i, openTag.Length).SequenceEqual(openTag))
                { accept = true; i += openTag.Length; }
                else if (text.Slice(i, closeTag.Length).SequenceEqual(closeTag))
                { accept = false; i += closeTag.Length; }

                if (!accept) { continue; }

                if (c >= 'A' && c <= 'Z')
                {
                    c = (char)(c + 32);
                }
                else
                {
                    // Busca em tabela de acentuados
                    int index = UpperChars().IndexOf(c);
                    if (index != -1)
                    {
                        c = LowerChars()[index];
                    }
                    // Se quiser, adicione fallback:
                    // else
                    // {
                    //     c = char.ToLowerInvariant(c);
                    // }
                }
            }

            return text;
        }

        private static char toLower(char character)
        {
            if (character >= 'a' && character <= 'z') { return character; }

            // Tabelas estáticas (não alocam em cada chamada)
            static ReadOnlySpan<char> UpperChars() => "ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞß";
            static ReadOnlySpan<char> LowerChars() => "àáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþß";

            // ASCII A-Z
            if (character >= 'A' && character <= 'Z')
            {
                character = (char)(character + 32);
            }
            else
            {
                // Busca em tabela de acentuados
                int index = UpperChars().IndexOf(character);
                if (index != -1)
                {
                    character = LowerChars()[index];
                }
                else
                {
                    character = char.ToLowerInvariant(character);
                }
            }

            return character;
        }

        #endregion

        #endregion

        #region ▼ ToUpper

        #region ► ToUpper Ignore in snippet
        public static string ToUpperIgnoreInSnippet(string text, (string open, string close) ignoreBetween)
        {
            if (text.Length >= 256)
            {
                Span<char> textStack = stackalloc char[text.Length];
                text.CopyTo(textStack);

                ReadOnlySpan<char> result = toUpperTextIgnore(textStack, ignoreBetween.open, ignoreBetween.close);

                return result.ToString();
            }
            else
            {
                char[] textSpan = text.ToArray();

                ReadOnlySpan<char> result = toUpperTextIgnore(textSpan, ignoreBetween.open, ignoreBetween.close);
                if (textSpan != null) { Array.Clear(textSpan, 0, textSpan.Length); }

                return result.ToString();
            }
        }
        #endregion

        #region ► ToUpper Only in snippet
        public static string ToUpperOnlyInSnippet(string text, (string open, string close) onlyBetween)
        {
            if (text.Length >= 256)
            {
                Span<char> textStack = stackalloc char[text.Length];
                text.CopyTo(textStack);

                ReadOnlySpan<char> result = toUpperTextBetween(textStack, onlyBetween.open, onlyBetween.close);

                return result.ToString();
            }
            else
            {
                char[] textSpan = text.ToArray();

                ReadOnlySpan<char> result = toUpperTextBetween(textSpan, onlyBetween.open, onlyBetween.close);
                if (textSpan != null) { Array.Clear(textSpan, 0, textSpan.Length); }

                return result.ToString();
            }
        }
        #endregion

        #region ► ToUpper char
        public static string ToUpperChar(string text, char character)
        {
            Span<char> _char = stackalloc char[1] { character };

            if (text.Length >= 256)
            {
                Span<char> textStack = stackalloc char[text.Length];
                text.CopyTo(textStack);

                ReadOnlySpan<char> result = toUpperChar(textStack, _char);

                return result.ToString();
            }
            else
            {
                char[] textSpan = text.ToArray();

                ReadOnlySpan<char> result = toUpperChar(textSpan, _char);
                if (textSpan != null) { Array.Clear(textSpan, 0, textSpan.Length); }

                return result.ToString();
            }
        }
        #endregion

        #region ► ToUpper match
        public static string ToUpperMatch(string text, string sequenceToMatch, int startIndex, params byte[] options)
        {
            ReadOnlySpan<char> _text = text;
            ReadOnlySpan<char> _sequenceToMatch = sequenceToMatch;

            StringAndPosition matchReturn = Match(text, sequenceToMatch, startIndex, options);

            matchReturn.Text = matchReturn.Text.ToLower();

            ReadOnlySpan<char> result = insert(text, matchReturn.Text, matchReturn.Position, matchReturn.Text.Length, true);

            return result.ToString();
        }
        #endregion

        #region ▼ Models

        private static ReadOnlySpan<char> toUpperChar(Span<char> text, Span<char> character)
        {
            // Tabelas estáticas (não alocam em cada chamada)
            static ReadOnlySpan<char> UpperChars() => "ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞß";
            static ReadOnlySpan<char> LowerChars() => "àáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþß";

            for (int i = 0; i < text.Length; i++)
            {
                ref char c = ref text[i];
                // ASCII A-Z

                if (c >= 'a' && c <= 'z' && c == character[0])
                {
                    c = (char)(c - 32);
                }
                else
                {
                    // Busca em tabela de acentuados
                    int index = LowerChars().IndexOf(c);
                    if (index != -1 && c == character[0])
                    {
                        c = UpperChars()[index];
                    }
                    // Se quiser, adicione fallback:
                    // else
                    // {
                    //     c = char.ToLowerInvariant(c);
                    // }
                }
            }

            return text;
        }

        private static ReadOnlySpan<char> toUpperTextIgnore(Span<char> text, ReadOnlySpan<char> openTag, ReadOnlySpan<char> closeTag)
        {
            // Tabelas estáticas (não alocam em cada chamada)
            static ReadOnlySpan<char> UpperChars() => "ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞß";
            static ReadOnlySpan<char> LowerChars() => "àáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþß";
            bool ignore = false;

            for (int i = 0; i < text.Length; i++)
            {
                ref char c = ref text[i];
                // ASCII A-Z

                if (openTag[0] == c)
                { ignore = true; }
                else if (closeTag[0] == c)
                { ignore = false; }

                if (ignore) { continue; }

                if (c >= 'a' && c <= 'z')
                {
                    c = (char)(c - 32);
                }
                else
                {
                    // Busca em tabela de acentuados
                    int index = LowerChars().IndexOf(c);
                    if (index != -1)
                    {
                        c = UpperChars()[index];
                    }
                    // Se quiser, adicione fallback:
                    // else
                    // {
                    //     c = char.ToLowerInvariant(c);
                    // }
                }
            }

            return text;
        }

        private static ReadOnlySpan<char> toUpperTextBetween(Span<char> text, ReadOnlySpan<char> openTag, ReadOnlySpan<char> closeTag)
        {
            // Tabelas estáticas (não alocam em cada chamada)
            static ReadOnlySpan<char> UpperChars() => "ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞß";
            static ReadOnlySpan<char> LowerChars() => "àáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþß";
            bool accept = false;

            for (int i = 0; i < text.Length; i++)
            {
                ref char c = ref text[i];
                // ASCII a-z

                if (openTag[0] == c)
                { accept = true; }
                else if (closeTag[0] == c)
                { accept = false; }

                if (!accept) { continue; }

                if (c >= 'a' && c <= 'z')
                {
                    c = (char)(c - 32);
                }
                else
                {
                    // Busca em tabela de acentuados
                    int index = LowerChars().IndexOf(c);
                    if (index != -1)
                    {
                        c = UpperChars()[index];
                    }
                    // Se quiser, adicione fallback:
                    // else
                    // {
                    //     c = char.ToLowerInvariant(c);
                    // }
                }
            }

            return text;
        }

        private static char toUpper(char charactere)
        {
            // Tabelas estáticas (não alocam em cada chamada)
            static ReadOnlySpan<char> UpperChars() => "ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞß";
            static ReadOnlySpan<char> LowerChars() => "àáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþß";

            // ASCII A-Z
            if (charactere >= 'a' && charactere <= 'z')
            {
                charactere = (char)(charactere - 32);
            }
            else
            {
                // Busca em tabela de acentuados
                int index = LowerChars().IndexOf(charactere);
                if (index != -1)
                {
                    charactere = UpperChars()[index];
                }
                else
                {
                    charactere = char.ToUpperInvariant(charactere);
                }
            }

            return charactere;
        }

        #endregion

        #endregion

        #region ▼ Fill

        #region ▼ Fill

        public static string Fill(string text, char character)
        {
            return Fill(text, character, '\0', default);
        }

        public static string Fill(string text, char character, char onlyCharacter)
        {
            return Fill(text, character, onlyCharacter, default);
        }

        public static string Fill(
            string text,
            char character,
            char onlyCharacter,
            (string open, string close) between = default)
        {
            // Básica validation
            if (string.IsNullOrEmpty(text)) return text;

            #region + Cast to spans

            ReadOnlySpan<char> inputSpan = text.AsSpan();
            ReadOnlySpan<char> openSpan = !string.IsNullOrEmpty(between.open) ? between.open.AsSpan() : ReadOnlySpan<char>.Empty;
            ReadOnlySpan<char> closeSpan = !string.IsNullOrEmpty(between.close) ? between.close.AsSpan() : ReadOnlySpan<char>.Empty;

            Span<char> charSpan = stackalloc[] { character };
            Span<char> onlySpan = onlyCharacter != '\0' ? stackalloc[] { onlyCharacter } : Span<char>.Empty;

            #endregion

            if (inputSpan.Length <= 256) // Limite seguro para stack
            {
                #region + Little text in stack

                Span<char> stackBuffer = stackalloc char[inputSpan.Length];
                inputSpan.CopyTo(stackBuffer);

                var resultSpan = fillCore(stackBuffer, charSpan, onlySpan, openSpan, closeSpan);
                return resultSpan.ToString();

                #endregion
            }
            else
            {
                #region + Big text in array

                char[] arrayToDispose = inputSpan.ToArray(); // Apenas se grande
                                
                var resultSpan = fillCore(arrayToDispose, charSpan, onlySpan, openSpan, closeSpan);
                string result = resultSpan.ToString();

                if (arrayToDispose != null)
                {
                    Array.Clear(arrayToDispose, 0, arrayToDispose.Length); // opcional: segurança
                }

                return result;

                #endregion
            }
        }

        #endregion

        #region ▼ Models      
        
        private static Span<char> fillCore(Span<char> text, ReadOnlySpan<char> charactere, ReadOnlySpan<char> onlyCharacter, ReadOnlySpan<char> openSnippet, ReadOnlySpan<char> closeSnippet)
        {
            #region ? Just fill all characters

            if (onlyCharacter == ReadOnlySpan<char>.Empty && openSnippet == ReadOnlySpan<char>.Empty)
            {
                text.Fill(charactere[0]); 
            }
            
            #endregion

            #region ? Only fill respective character of 'onlyCharacter' parameter
            else if (openSnippet == ReadOnlySpan<char>.Empty)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == onlyCharacter[0]) { text[i] = charactere[0]; }
                }
            }
            #endregion

            #region ! Fill all characters between of 'open and close snippet' chars in parameter
            else
            {
                #region ** Exception : Inavalid 'between'

                if (openSnippet == ReadOnlySpan<char>.Empty || closeSnippet == ReadOnlySpan<char>.Empty)
                {
                    throw new InvalidOperationException(
                    "Invalid format of 'between' snippet identification! Ex:'[start]*[end]'"
                );
                }

                #endregion

                int pos = 0;

                while (pos != -1 && pos < text.Length)
                {
                    #region + Match start tag of between parameter

                    pos = text[pos..].IndexOf(openSnippet);
                    if (pos == -1) { break; }

                    #endregion

                    pos += openSnippet.Length;

                    for (; pos < text.Length; pos++)
                    {
                        #region ? A close tag of between parameter

                        if (text.Slice(pos, closeSnippet.Length).SequenceEqual(closeSnippet))
                        { pos += closeSnippet.Length; break; }

                        #endregion

                        #region ? if OnlyCharacter paraneter informed, Just fill respective charactere
                        if (onlyCharacter != ReadOnlySpan<char>.Empty)
                        {
                            if (text[pos] == onlyCharacter[0]) { text[pos] = charactere[0]; }
                        }
                        #endregion

                        else if (charactere == ReadOnlySpan<char>.Empty)
                        { text[pos] = '\0'; }
                        else
                        { text[pos] = charactere[0]; }
                    }
                }
            }
            #endregion

            return text;
        }

        #endregion

        #endregion

        #region ▼ Replace

        #region ► ReplaceFirst

        public static string ReplaceFirst(string text, string sequenceToMatch, string toRepalce, params byte[] options)
        {
            return ReplaceFirst(text, sequenceToMatch, toRepalce, 0, options);
        }

        public static string ReplaceFirst(string text, string sequenceToMatch, string toRepalce, int startIndex, params byte[] options)
        {
            ReadOnlySpan<char> _text = text;
            ReadOnlySpan<char> _sequenceToMatch = sequenceToMatch;
            ReadOnlySpan<char> _toReplace = toRepalce;

            StringAndPosition matchReturn = Match(text, sequenceToMatch, startIndex, options);
            ReadOnlySpan<char> result = insert(text, toRepalce, matchReturn.Position, matchReturn.Text.Length, true);

            return result.ToString();
        }

        #endregion

        #region ► ReplaceLast

        public static string ReplaceLast(string text, string sequenceToMatch, string toRepalce, params byte[] options)
        {
            return ReplaceLast(text, sequenceToMatch, toRepalce, 0, options);
        }

        public static string ReplaceLast(string text, string sequenceToMatch, string toRepalce, int startIndex, params byte[] options)
        {
            ReadOnlySpan<char> _text = text;
            ReadOnlySpan<char> _sequenceToMatch = sequenceToMatch;
            ReadOnlySpan<char> _toReplace = toRepalce;
            ReadOnlySpan<char> result = default;
            int position = 0;
            int len = 0;

            while (position != -1)
            {
                StringAndPosition matchReturn = Match(text, sequenceToMatch, startIndex, options);
                position = matchReturn.Position;

                if (matchReturn.Position != -1)
                { startIndex = matchReturn.Position; len = matchReturn.Text.Length; }
            }

            result = insert(text, toRepalce, startIndex, len, true);

            return result.ToString();
        }

        #endregion
                
        #region ► Replace

        public static string Replace(string text, string sequenceToMatch, string toRepalce, params byte[] options)
        {
            return Replace(text, sequenceToMatch, toRepalce, 0, options);
        }

        public static string Replace(string text, string sequenceToMatch, string toRepalce, int startIndex, params byte[] options)
        {
            return replaceCore(text, sequenceToMatch, toRepalce, startIndex, options);
        }

        #endregion

        #region ► Model

        private static string replaceCore(string text, string sequenceToMatch, string toRepalce, int startIndex, params byte[] options)
        {
            ReadOnlySpan<char> _text = text;
            ReadOnlySpan<char> _sequenceToMatch = sequenceToMatch;
            ReadOnlySpan<char> _toReplace = toRepalce;
            ReadOnlySpan<char> result = default;
            int position = 0;

            while (position != -1)
            {
                StringAndPosition matchReturn = Match(text, sequenceToMatch, startIndex, ParamsGreedyOccurence);
                position = matchReturn.Position;
                result = insert(text, toRepalce, matchReturn.Position, matchReturn.Text.Length, true);
            }

            return result.ToString();
        }

        #endregion

        #endregion

        #region ▼ Insert

        #region ► Insert

        public static string Insert(string text, string toInsert, int positionIndex)
        {
            var result = insert(text, toInsert, positionIndex, toInsert.Length, false);

            return result.ToString();
        }

        #endregion

        #region ► InsertBefore

        public static string InsertBefore(string text, string toInsert, string beforeIt, params byte[] options)
        {
            StringAndPosition matchReturn = Match(text, beforeIt, options);
            int position = matchReturn.Position;
            ReadOnlySpan<char> result = insert(text, toInsert, position, toInsert.Length, false).ToString();

            return result.ToString();
        }

        #endregion

        #region ► InsertAfter

        public static string InsertAfter(string text, string toInsert, string afterIt, params byte[] options)
        {
            StringAndPosition matchReturn = Match(text, afterIt, options);
            int position = matchReturn.Position + matchReturn.Text.Length;
            ReadOnlySpan<char> result = insert(text, toInsert, position, toInsert.Length, false);

            return result.ToString();
        }

        #endregion

        #region ► Model
        private static ReadOnlySpan<char> insert(ReadOnlySpan<char> text,
                                                 ReadOnlySpan<char> toRepalce,
                                                 int position,
                                                 int len,
                                                 bool replace)
        {
            int _len = text.Length + toRepalce.Length;

            Span<char> replacement = new char[_len];

            text[..position].CopyTo(replacement);
            toRepalce.CopyTo(replacement[position..]);

            if (replace) { _len = position + len; }
            else { _len = position; }

            position += toRepalce.Length;

            text[_len..].CopyTo(replacement[position..]);

            return replacement.TrimEnd('\0');
        }

        #endregion

        #endregion

        #region ▼ Remove

        #region ► RemoveFirst

        public static string RemoveFirst(string text, string sequenceToMatch, params byte[] options)
        {
            return RemoveFirst(text, sequenceToMatch, 0, options);
        }

        public static string RemoveFirst(string text, string sequenceToMatch, int startIndex, params byte[] options)
        {
            ReadOnlySpan<char> _text = text;
            ReadOnlySpan<char> _sequenceToMatch = sequenceToMatch;

            StringAndPosition matchReturn = Match(text, sequenceToMatch, startIndex, options);
            ReadOnlySpan<char> result = insert(text, ReadOnlySpan<char>.Empty, matchReturn.Position, matchReturn.Text.Length, true);

            return result.ToString();
        }

        #endregion

        #region ► RemoveLast

        public static string RemoveLast(string text, string sequenceToMatch, params byte[] options)
        {
            return RemoveLast(text, sequenceToMatch, 0, options);
        }

        public static string RemoveLast(string text, string sequenceToMatch, int startIndex, params byte[] options)
        {
            ReadOnlySpan<char> _text = text;
            ReadOnlySpan<char> _sequenceToMatch = sequenceToMatch;
            ReadOnlySpan<char> result = default;
            int position = 0;
            int len = 0;

            while (position != -1)
            {
                StringAndPosition matchReturn = Match(text, sequenceToMatch, startIndex, options);
                position = matchReturn.Position;

                if (matchReturn.Position != -1)
                { startIndex = matchReturn.Position; len = matchReturn.Text.Length; }
            }

            result = insert(text, ReadOnlySpan<char>.Empty, startIndex, len, true);

            return result.ToString();
        }

        #endregion

        #region ► Remove

        public static string Remove(string text, string sequenceToMatch, params byte[] options)
        {
            return Remove(text, sequenceToMatch, 0, options);
        }

        public static string Remove(string text, string sequenceToMatch, int startIndex, params byte[] options)
        {
            ReadOnlySpan<char> _text = text;
            ReadOnlySpan<char> _sequenceToMatch = sequenceToMatch;
            ReadOnlySpan<char> result = default;
            int position = 0;

            while (position != -1)
            {
                StringAndPosition matchReturn = Match(text, sequenceToMatch, startIndex, ParamsGreedyOccurence);
                position = matchReturn.Position;
                result = insert(text, ReadOnlySpan<char>.Empty, matchReturn.Position, matchReturn.Text.Length, true);
            }

            return result.ToString();
        }

        #endregion

        #endregion

        #region ▼ Split

        public static string[] Split(string text, char separator, (char open, char close)ignoreChar)
        {
            return split(text, separator,0, ignoreChar);
        }

        public static string[] Split(string text, char separator, int startIndex, (char open, char close) ignoreChar)
        {
            return split(text, separator, startIndex, ignoreChar);
        }

        private static string[] split(ReadOnlySpan<char> text, char separator, int startIndex, (char open, char close)ignoreChar)
        {
            int pos = 0;
            bool ignoreMode = false;
            List<string> splited = new List<string>();

            while (pos < text[startIndex..].Length)
            {
                int start = pos;
                while (pos < text[startIndex..].Length)
                {
                    char c = text[startIndex..][pos];

                    if (c == ignoreChar.open) ignoreMode = true;
                    if (c == ignoreChar.close) ignoreMode = false;

                    if (!ignoreMode)
                    {
                        if (c == separator) break;
                    }
                    pos++;
                }

                if (pos > start)
                {
                    splited.Add(text.Slice(start, pos - start).ToString());
                }

                if (pos < text.Length) pos++;
            }

            return splited.ToArray();
        }

        #endregion

        #region ▼ Contains

        public static bool Contains(string text, string SequenceToMatch, params byte[] options)
        {
            StringAndPosition matchReturn = Match(text, SequenceToMatch, options);

            return matchReturn.Position !=-1;
        }

        public static bool Contains(string text, string SequenceToMatch, int startIndex, params byte[] options)
        {
            StringAndPosition matchReturn = Match(text, SequenceToMatch, startIndex, options);

            return matchReturn.Position !=-1;
        }

        #endregion

        #endregion

        #endregion

        #region ▼ Match Snippets

        #region ► Constructors

        public static StringAndPosition Snippet(string sourceText, string openAndCloseTags, params byte[] options)
        {
            return Snippet(sourceText, openAndCloseTags, string.Empty, 0, options);
        }

        public static StringAndPosition Snippet(string sourceText, string openAndCloseTags, int startIndex, params byte[] options)
        {
            return Snippet(sourceText, openAndCloseTags, string.Empty, startIndex, options);
        }

        public static StringAndPosition Snippet(string sourceText, string openAndCloseTags, string idOfSnippet, params byte[] options)
        {
            return Snippet(sourceText, openAndCloseTags, idOfSnippet, 0, options);
        }

        public static StringAndPosition Snippet(string sourceText, string openAndCloseTags, string idOfSnippet, int startIndex, params byte[] options)
        {
            var resultSpan = snippet(sourceText, openAndCloseTags, idOfSnippet, startIndex, options);

            return resultSpan;
        }

        #endregion

        #region ► Controller
        private static StringAndPosition snippet(ReadOnlySpan<char> text, ReadOnlySpan<char> snippetTags, ReadOnlySpan<char> snippetID, int startIndex, params byte[] options)
        {
            if (snippetTags.Length == 0 && text == Span<char>.Empty) { return default; }

            (int position, int length) = snippetCore(text, snippetTags.ToArray(), snippetID, startIndex, options);

            if (position == -1 || length == 0)
            { return new StringAndPosition(); }

            return new StringAndPosition(text.Slice(position, length).ToString(), position);
        }

        #endregion

        #region ► Model

        private static (int, int) snippetCore(ReadOnlySpan<char> text, ReadOnlySpan<char> snippet, ReadOnlySpan<char> snippetID, int startIndex, params byte[] options)
        {
            #region ** Exception : The snippet not contain a '*' character to split open and close tag.
            int splitPos = snippet.IndexOf('*');

            if (splitPos == -1)
            { throw new Exception("!Invalid snippet pattern!"); }

            if(options.Contains(ParamsDynamicChars))
            { throw new Exception("!Snippet no accept dynamic char!"); }

            #endregion
                        
            #region ++ Flags e variables

            int openTagCount = 0;
            int closeTagCount = 0;
            int occurPos = -1;
            int occurLen = 0;
            int returnPos = -1;
            int snippetIDPos = -1;
            PatternsToMatch toMatch = new PatternsToMatch(snippet, '*');
            ReadOnlySpan<char> openTag = toMatch[0];
            ReadOnlySpan<char> closeTag = toMatch[1];

            if (toMatch.Count != 2)
            { throw new InvalidCastException("!Snippet pattern mismatch! Just inform open*close tag in pattern."); }

            #endregion
            
            #region + If snippetID is not empty, search for snippetID in open tag.

            if (snippetID.Length > 0)
            {
                // If found, set snippetIDPos to current position.
                //snippetIDPos = text.IndexOf(snippetID);
                ( snippetIDPos, occurLen ) = matchLitteral(text, snippetID, startIndex, options);
                occurLen = 0;

                if (snippetIDPos == -1)
                {
                    // If not found, set snippetIDPos to -1.
                    return (-1, 0);
                }
            }
            #endregion

            for (int pos = startIndex; pos < text.Length; pos++)
            {
                #region + Search for open and close tag in text and return only the first occurrence of first tag. 

                int tagIndex = 0; // 0 = open tag, 1 = close tag

                #region + Search for open or close tag in text

                (int openPos, int openLen) = matchLitteral(text, openTag, startIndex, options);
                (int closePos, int closeLen) = matchLitteral(text, closeTag, startIndex, options);

                #endregion

                #region ** Exception : If not found open or close tag, return -1 and 0.

                if (closePos == -1) { break; }

                #endregion

                #endregion

                #region ? Snippet ID informed in parameter and after open tag and before close tag

                if (snippetIDPos != -1)
                {
                    if ((openPos < snippetIDPos || closePos < snippetIDPos) && openTagCount == 1)
                    {
                        occurPos = -1; occurLen = 0; returnPos = -1; openTagCount = 0; closeTagCount = 0;
                    }
                }

                #endregion

                if ((uint)openPos < (uint)closePos)
                {
                    #region + Increment open tag counter
                    openTagCount++;
                    occurPos = openPos; occurLen = openLen; startIndex = openPos + openLen; tagIndex = 0;
                    #endregion
                }
                else
                {
                    #region + Increment close tag counter
                    closeTagCount++;
                    occurPos = closePos; occurLen = closeLen; startIndex = closePos + closeLen; tagIndex = 1;
                    #endregion
                }
                                
                #region + Mark the position of first open tag and calculate the length of snippet when all tags are closed.
                if (returnPos == -1 && tagIndex ==0)
                { returnPos = occurPos; }
                #endregion
                                
                if (openTagCount != -1 && openTagCount == closeTagCount)
                { return (returnPos, (occurPos - returnPos) + occurLen); }
            }

            // If not found open tag, return -1 and 0.
            return (-1, 0);
        }

        #endregion

        #region ▼ Replace

        #region ► ReplaceFirst

        public static string SinippetReplaceFirst(string text, string openAndCloseTags, string toRepalce, params byte[] options)
        {
            return SnippetReplaceFirst(text, openAndCloseTags, toRepalce, "", 0, options);
        }

        public static string SinippetReplaceFirst(string text, string openAndCloseTags, string toRepalce, int startIndex, params byte[] options)
        {
            return SnippetReplaceFirst(text, openAndCloseTags, toRepalce, "", startIndex, options);
        }

        public static string SinippetReplaceFirst(string text, string openAndCloseTags, string snippetID, string toRepalce, params byte[] options)
        {
            return SnippetReplaceFirst(text, openAndCloseTags, toRepalce, snippetID, 0, options);
        }

        public static string SnippetReplaceFirst(string text, string openAndCloseTags, string snippetID, string toRepalce, int startIndex, params byte[] options)
        {
            StringAndPosition matchReturn = Snippet(text, openAndCloseTags, snippetID, startIndex, options);
            ReadOnlySpan<char> result = insert(text, toRepalce, matchReturn.Position, matchReturn.Text.Length, true);

            return result.ToString();
        }

        #endregion

        #region ► ReplaceLast

        public static string ReplaceSnippetLast(string text, string openAndCloseTags, string toRepalce, params byte[] options)
        {
            return ReplaceSnippetLast(text, openAndCloseTags, "", toRepalce, 0, options);
        }

        public static string ReplaceSnippetLast(string text, string openAndCloseTags, string snippetID, string toRepalce, int startIndex, params byte[] options)
        {
            ReadOnlySpan<char> result = default;
            int position = 0;
            int len = 0;

            while (position != -1)
            {
                StringAndPosition matchReturn = Snippet(text, openAndCloseTags, snippetID, startIndex, options);
                position = matchReturn.Position;

                if (matchReturn.Position != -1)
                { startIndex = matchReturn.Position; len = matchReturn.Text.Length; }
            }

            result = insert(text, toRepalce, startIndex, len, true);

            return result.ToString();
        }

        #endregion

        #region ► Replace

        public static string ReplaceSnippet(string text, string openAndCloseTags, string toRepalce, params byte[] options)
        {
            return ReplaceSnippet(text, openAndCloseTags, "", toRepalce, 0, options);
        }

        public static string ReplaceSnippet(string text, string openAndCloseTags, string snippetID, string toRepalce, int startIndex, params byte[] options)
        {
            ReadOnlySpan<char> _text = text;
            ReadOnlySpan<char> _openAndCloseTags = openAndCloseTags;
            ReadOnlySpan<char> _toReplace = toRepalce;
            ReadOnlySpan<char> result = default;
            int position = 0;

            while (position != -1)
            {
                StringAndPosition matchReturn = Snippet(text, openAndCloseTags, startIndex, options);
                position = matchReturn.Position;
                result = insert(text, toRepalce, matchReturn.Position, matchReturn.Text.Length, true);
            }

            return result.ToString();
        }

        #endregion

        #endregion

        #region ▼ Insert

        #region ► Insert

        public static string InsertSnippet(string text, string toInsert, int positionIndex)
        {
            var result = insert(text, toInsert, positionIndex, toInsert.Length, false);

            return result.ToString();
        }

        #endregion

        #region ► InsertBefore

        public static string InsertSnippetBefore(string text, string toInsert, string beforeIt, params byte[] options)
        {
            StringAndPosition matchReturn = Match(text, beforeIt, options);
            int position = matchReturn.Position;
            var result = insert(text, toInsert, position, toInsert.Length, false);

            return result.ToString();
        }

        #endregion

        #region ► InsertAfter

        public static string InsertSnippetAfter(string text, string toInsert, string afterIt, params byte[] options)
        {
            StringAndPosition matchReturn = Match(text, afterIt, options);
            int position = matchReturn.Position + matchReturn.Text.Length;
            var result = insert(text, toInsert, position, toInsert.Length, false);

            return result.ToString();
        }

        #endregion

        #endregion

        #region ▼ Remove

        #region ► RemoveFirst

        public static string SnippetRemoveFirst(string text, string openAndCloseTags, params byte[] options)
        {
            return SnippetRemoveFirst(text, openAndCloseTags, "", 0, options);
        }

        public static string SnippetRemoveFirst(string text, string openAndCloseTags, int startIndex, params byte[] options)
        {
            return SnippetRemoveFirst(text, openAndCloseTags, "", startIndex, options);
        }
                
        public static string SnippetRemoveFirst(string text, string openAndCloseTags, string snippetID, int startIndex, params byte[] options)
        {
            StringAndPosition matchReturn = Snippet(text, openAndCloseTags, snippetID, startIndex, options);
            ReadOnlySpan<char> result = insert(text, "", matchReturn.Position, matchReturn.Text.Length, true);

            return result.ToString();
        }

        #endregion

        #region ► RemoveLast

        public static string RemoveSnippetLast(string text, string openAndCloseTags, params byte[] options)
        {
            return RemoveSnippetLast(text, openAndCloseTags, "", 0, options);
        }

        public static string RemoveSnippetLast(string text, string openAndCloseTags, int startIndex, params byte[] options)
        {
            return RemoveSnippetLast(text, openAndCloseTags, "", startIndex, options);
        }

        public static string RemoveSnippetLast(string text, string openAndCloseTags, string snippetID, int startIndex, params byte[] options)
        {
            ReadOnlySpan<char> result = default;
            int position = 0;
            int len = 0;

            while (position != -1)
            {
                StringAndPosition matchReturn = Snippet(text, openAndCloseTags, snippetID, startIndex, options);
                position = matchReturn.Position;

                if (matchReturn.Position != -1)
                { startIndex = matchReturn.Position; len = matchReturn.Text.Length; }
            }

            result = insert(text, "", startIndex, len, true);

            return result.ToString();
        }

        #endregion

        #region ► Remove

        public static string RemoveSnippet(string text, string openAndCloseTags, params byte[] options)
        {
            return RemoveSnippet(text, openAndCloseTags, "", 0, options);
        }

        public static string RemoveSnippet(string text, string openAndCloseTags, string snippetID, int startIndex, params byte[] options)
        {
            ReadOnlySpan<char> _text = text;
            ReadOnlySpan<char> _openAndCloseTags = openAndCloseTags;
            ReadOnlySpan<char> result = default;
            int position = 0;

            while (position != -1)
            {
                StringAndPosition matchReturn = Snippet(text, openAndCloseTags, startIndex, options);
                position = matchReturn.Position;
                result = insert(text, "", matchReturn.Position, matchReturn.Text.Length, true);
            }

            return result.ToString();
        }

        #endregion

        #endregion

        #region ▼ Contains

        public static bool ContainsSnippet(string text, string openAndCloseTags, params byte[] options)
        {
            return ContainsSnippet(text, openAndCloseTags, "", 0, options);
        }

        public static bool ContainsSnippet(string text, string openAndCloseTags, int startIndex, params byte[] options)
        {
            return ContainsSnippet(text, openAndCloseTags, "", startIndex, options);
        }

        public static bool ContainsSnippet(string text, string openAndCloseTags, string snippetID, int startIndex, params byte[] options)
        {                        
            StringAndPosition matchReturn = Snippet(text, openAndCloseTags, startIndex, options);  
            return matchReturn.Position !=-1;
        }

        #endregion

        #endregion
    }
}
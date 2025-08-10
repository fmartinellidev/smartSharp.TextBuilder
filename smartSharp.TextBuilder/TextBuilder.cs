using System.Buffers;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
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

            private Span<char> source;
            private ReadOnlySpan<int> positions;
            private ReadOnlySpan<int> lengths;
            private int count;
            public bool SplitInFirstChar { get; }
            public bool SplitInLastChar { get; }
            public int Count => count;
            public bool Empty => count == 0;

            public bool ContainsWildcards(int index)
            {
                return this[index].IndexOf('*') != -1;
            }

            public bool ContainsCompleteWord(int index)
            {
                return this[index].IndexOf('~') != -1;
            }

            #endregion

            #region Constructor

            public PatternsToMatch(Span<char> text, char splitChar = '\0', char ignoreChar = '\0')
            {
                source = text;
                Span<int> _positions = stackalloc int[160];
                Span<int> _lengths = stackalloc int[160];
                                
                if (source.Length == 0) { count = 0; return; }

                int pos = 0, idx = 0;
                bool ignoreMode = false;
                                
                while (pos < source.Length)
                {
                    int start = pos;
                    while (pos < source.Length)
                    {
                        char c = source[pos];

                        if (c == ignoreChar)
                            ignoreMode = !ignoreMode;
                        else if (!ignoreMode)
                        {
                            if (c == splitChar)
                                break;

                            if ( splitChar == '\0' && c == '|' && pos + 1 < source.Length && source[pos + 1] == '|')
                            {  break; }
                        }
                        pos++;
                    }

                    if (pos > start)
                    {                   
                        _positions[idx] = start;
                        _lengths[idx] = pos - start;
                        idx++;

                        if (splitChar == '\0')
                        {
                            if (pos < source.Length && source[pos] == '|')
                            { pos++; }
                        }

                        if (idx == 160)
                        { throw new IndexOutOfRangeException("TextBuilder only accept max 160 pattern parts ('|', '*', '||' and '~')!"); }
                    }

                    if (pos < source.Length) pos++;
                }

                count = idx;

                if (text[0] == splitChar) 
                { SplitInFirstChar = true; }
                
                if (text[text.Length - 1] == splitChar) 
                { SplitInLastChar = true; _lengths[count - 1] = _lengths[count - 1] - 1; }

                positions = _positions.Slice(0, count).ToArray();
                lengths = _lengths.Slice(0, count).ToArray();

                _positions = null;
                _lengths = null;
            }

            #endregion

            #region Controller

            public Span<char> this[int index]
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
            return match(text.AsSpan().ToArray(), SequenceToMatch.AsSpan().ToArray(), startIndex, startIndexReturn, endCutLenReturn, options);
        }

        private static ReadOnlySpan<char> adjustByParameters(ReadOnlySpan<char> text, params byte[] options)
        {
            Span<char> outText = text.ToArray();

            #region ? Ignore in quote

            if (options.Contains(ParamsIgnoreInQuotes))
            { outText = fill(outText, '\0', "\'*\'"); }

            #endregion

            #region ? Ignore in double quote

            if (options.Contains(ParamsIgnoreInQuotes))
            { outText = fill(outText, '\0', "\"*\""); }

            #endregion           

            return outText.ToArray();
        }

        #endregion

        #region ► Controller

        private static StringAndPosition match(ReadOnlySpan<char> text, Span<char> sequenceToMatch, int startIndex, int startIndexReturn, int endCutLenReturn, params byte[] options)
        {
            if (text.Length == 0) { return new StringAndPosition(); }
            if (sequenceToMatch.Length == 0) { return new StringAndPosition(); }

            #region ++ Flags and variable

            int occurPos = -1;
            int occurLen = 0;
            
            #endregion

            ReadOnlySpan<char> textToSearch = adjustByParameters(text, options);           
            PatternsToMatch toMatch = new PatternsToMatch(sequenceToMatch);

            for (int i = 0; i < toMatch.Count; i++)
            {
                int pos = startIndex;
                int len = 0;

                #region + Match Pattern

                if (toMatch.ContainsWildcards(i) || (i == 0 && toMatch[i][0] == '*'))
                {
                    do
                    {
                        #region + Match
                        (pos, len) = matchPattern(textToSearch, toMatch[i], pos, options);
                        #endregion

                        if (len != -1)
                        {
                            #region ? Not the smaller occcurence lenght (the most accurate word), re-match pattern
                            /*if informed 'ParamsGreedyOccurence' parameter, ignore accurate 
                             * word and return first matches of pattern */

                            if (!options.Contains(ParamsGreedyOccurence) && len !=-1)
                            {
                                if ((len < occurLen && len != 0) || occurLen == 0)
                                {
                                    #region + If wordTemp is more smaller of already storage word
                                    //The smaller occurence is the more accurate result.
                                    occurLen = len;
                                    occurPos = pos;
                                    #endregion
                                }
                                else if (len == 0) { len = -1; }

                                #region + Reset flags

                                pos++;
                                                                
                                #endregion
                            }
                            else { len = -1; }

                            #endregion
                        }

                    } while (len != -1 && pos < textToSearch.Length - 1);
                }
                #endregion

                #region + Match Litteral
                else
                {
                    #region + Match

                    (pos, len) = matchLitteral(textToSearch, toMatch[i], pos, false, options);
                    if (pos == -1) { continue; }

                    #endregion

                    #region ? The occurrence is first position in text?

                    else if (pos < occurPos || occurPos == -1)
                    { occurPos = pos; occurLen = len; }
                    else { pos = startIndex; }

                    #endregion
                }

                #endregion
            }

            if (occurLen == 0) { return new StringAndPosition(); }

            if(startIndexReturn != 0) { occurPos += startIndexReturn; occurLen--; }
            if(endCutLenReturn != 0) { occurLen -= endCutLenReturn; }

            return new StringAndPosition(text.Slice(occurPos, occurLen).ToString(), occurPos);
        }

        #endregion

        #region ▼ Models

        #region ► matchLitteral

        private static (int, int) matchLitteral(ReadOnlySpan<char> text, Span<char> patterns, int startIndex, bool orInSequence = false, params byte[] options)
        {
            int occurIndex = 0;
            int returnPos = -1;
            int returnLen = 0;

            if (patterns[0] == '|' || patterns[patterns.Length -1] =='|')
            {
                throw new InvalidFilterCriteriaException(
                "Invalid use of 'OR' char('|')! Not use it to start, end pattern and after wildcard '*'."
                );
            }

            #region + Or split to or pattern

            PatternsToMatch toMatch = new PatternsToMatch(patterns, '|');

            #endregion

            int count = toMatch.Count;
            for (int i = 0; i < count; i++)
            {
                //Reuse pattern to new resize length if inWord char is found
                ReadOnlySpan<char> pattern = toMatch[i];
                int len = 0;

                if (toMatch.ContainsCompleteWord(i))
                {
                    #region + InWord char in pattern
                    (occurIndex, len) = matchWord(text, pattern, startIndex);
                    #endregion
                }
                else
                {
                    #region + Litteral pattern

                    bool dynamic = options.Contains(ParamsDynamicChars);
                    bool caseSensitive = options.Contains(ParamsIgnoreCaseSensitive);
                    occurIndex = indexOf(text, pattern, startIndex, dynamic);

                    if (occurIndex == -1) { continue; }

                    len = pattern.Length;

                    #endregion
                }

                #region + Update if this occurence pos is more smaller that previous ( to the OR condition )
                //If not used OR condition, the occurrence automatically will be the first

                if ((uint)occurIndex < (uint)returnPos)
                {
                    returnPos = occurIndex;
                    returnLen = len;

                    #region ? Start or end pattern with dynamic char

                    if (options.Contains(ParamsDynamicChars))
                    {
                        if (pattern[0] == (char)1) { returnPos++; returnLen--; }
                        if (pattern[pattern.Length - 1] == (char)1) { returnLen--; }
                    }
                    #endregion
                }

                #endregion
            }

            #region + Not found match
            if (returnPos == -1)
            { return (-1, 0); }
            #endregion

            #region + Pattern starting with wildcard

            if (toMatch.SplitInFirstChar)
            { returnPos = 0; }

            #endregion

            return (returnPos, returnLen);
        }

        #endregion

        #region ► matchPattern

        private static ( int, int) matchPattern(ReadOnlySpan<char> text, Span<char> patterns, int startIndex, byte[] options = null)
        {
            int occurPos = -1;
            int occurLen = 0;
            int occurStart = -1;

            PatternsToMatch toMatch = new PatternsToMatch(patterns, '*');

            int count = toMatch.Count;
            for (int i = 0; i < count; i++)
            {
                (occurPos, occurLen) = matchLitteral(text, toMatch[i], startIndex);

                #region ? Not matched pattern
                if (occurPos == -1)
                { return (default, -1); }
                #endregion

                if (toMatch.SplitInFirstChar)
                {
                    #region ! Pattern start with a wildcard, start occurence with first char of text
                    occurStart = 0;
                    #endregion
                }
                else if (occurStart == -1)
                {
                    #region ? Not start with wildcard storage pos of first occurence
                    occurStart = occurPos;
                    #endregion
                }

                #region + Start new match after this position

                startIndex = occurPos + occurLen;
                occurLen = startIndex - occurStart;

                #endregion
            }

            #region + Wildcard of start char in pattern

            if (toMatch.SplitInFirstChar) { occurStart = 0; }

            #endregion

            #region + Wildcard of last char in pattern

            if (toMatch.SplitInLastChar) { occurLen = text.Length - occurStart; }

            #endregion

            return (occurStart, occurLen);
        }

        #endregion

        #region ► matchWord

        private static (int, int) matchWord(ReadOnlySpan<char> text, ReadOnlySpan<char> patterns, int startIndex)
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

            #endregion
                        
            #region + Search pattern
            
            wordLen = pattern.Length;
            while (wordPos != -1)
            {
                wordPos = indexOf(text, pattern, startIndex, true, true);
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

            #region + Buils end of word

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

        #region ▼ IndexOf

        #region ► IndexOf

        private static int indexOf(ReadOnlySpan<char> text, ReadOnlySpan<char> pattern, int startIndex, bool acceptDynamicChars=false, bool ignoreCaseSensitive = false )
        {
            if (pattern.Length == 0) { return -1; }
            if (text.Length == 0) { return -1; }

            int patPos = 0;
            int occurPos = 0;
            bool isDynamic = false;

            if (!acceptDynamicChars)
            { occurPos = text[startIndex..].IndexOf(pattern); occurPos += startIndex; }
            else
            {
                #region + Get at dynaminc char to start search

                for (; patPos < pattern.Length; patPos++)
                {
                    char c = pattern[patPos];
                    if (c == '~' || c == '_' || c == '#')
                    { isDynamic = true; if (patPos > 0) { patPos--; }; break; }
                }

                #endregion

                #region ? Not found any dynamic char

                if (!isDynamic)
                { occurPos = text[startIndex..].IndexOf(pattern); occurPos += startIndex; }

                #endregion

                #region ! Is a dynamic chars
                else
                {
                    int patStart = patPos;
                    int pos = 0;
                    while (pos != -1)
                    {
                        for (; pos < text.Length; pos++)
                        {
                            #region + IndexOf sequence
                            if (patStart > 0 && patPos == patStart)
                            {
                                patPos++;
                                pos = text[startIndex..].IndexOf(pattern[..patStart]);
                                pos += startIndex;
                                occurPos = pos;
                                pos += pattern[..patStart].Length;
                            }
                            #endregion

                            char c = (char)text[pos];
                            char pat = (char)pattern[patPos];

                            #region ! Match text character with pattern character
                            if (c == pat)
                            { patPos++; if (occurPos == 0) { occurPos = pos; } }
                            #endregion

                            #region ! Pattern char is number  
                            else if (pat == '#' && char.IsDigit(c))
                            { patPos++; if (occurPos == 0) { occurPos = pos; } }
                            #endregion

                            #region ! Pattern char is word separator 
                            else if (pat == '_' && IsSeparator(c))
                            { patPos++; if (occurPos == 0) { occurPos = pos; } }
                            #endregion

                            #region ! Match text char if IgnoreCaseSensitive is on
                            else if (ignoreCaseSensitive)
                            {
                                if (toLower(c) == toLower(pat)) { patPos++; occurPos = pos; }
                                else { break; }
                            }
                            #endregion

                            #region ! Not match found
                            else
                            {
                                startIndex = pos - (pattern[..patPos].Length - 1);
                                patPos = patStart;
                                if (occurPos != 0) { pos = occurPos; occurPos = 0; }
                            }
                            #endregion

                            if (patPos == pattern.Length) { pos = -1; break; }
                        }
                    }
                }
                #endregion
            }

            return occurPos;
        }

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
        public static Dictionary<string, int> IndexOfAll(string text, string SequenceToMatch, params byte[] options)
        {
            //ReadOnlySpan<char> toMatch = SequenceToMatch;
            PatternsToMatch toMatch = new PatternsToMatch(SequenceToMatch.ToArray(), ',', '\'');
            Dictionary<string, int> result = new Dictionary<string, int>();

            int count = toMatch.Count;
            for (int i = 0; i < count; i++)
            {
                StringAndPosition matchReturn = Match(text, toMatch[i].ToString(), 0, options);
                result.Add(matchReturn.ToString(), matchReturn.Position);
            }

            return result;
        }

        public static Dictionary<string, int> IndexOfAll(string text, string SequenceToMatch, int startIndex, params byte[] options)
        {
            return IndexOfAll(text, SequenceToMatch, startIndex, options);
        }

        #endregion

        #endregion

        #region ► ToLower

        private static Span<char> toLowerText(Span<char> text)
        {
            // Tabelas estáticas (não alocam em cada chamada)
            static ReadOnlySpan<char> UpperChars() => "ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞß";
            static ReadOnlySpan<char> LowerChars() => "àáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþß";

            for (int i = 0; i < text.Length; i++)
            {
                ref char c = ref text[i];

                // ASCII A-Z
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

        private static char toLower(char charactere)
        {
            // Tabelas estáticas (não alocam em cada chamada)
            static ReadOnlySpan<char> UpperChars() => "ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞß";
            static ReadOnlySpan<char> LowerChars() => "àáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþß";

            // ASCII A-Z
            if (charactere >= 'A' && charactere <= 'Z')
            {
                charactere = (char)(charactere + 32);
            }
            else
            {
                // Busca em tabela de acentuados
                int index = UpperChars().IndexOf(charactere);
                if (index != -1)
                {
                    charactere = LowerChars()[index];
                }
                else
                {
                    charactere = char.ToLowerInvariant(charactere);
                }
            }

            return charactere;
        }

        #endregion

        #region ► Fill

        #region ▼ Publics

        public static string Fill(string text, char charactere)
        {
            return Fill(text, charactere, '\0');
        }

        public static string Fill(string text, char charactere, char onlyCharacter)
        {
            return Fill(text, charactere, onlyCharacter);
        }

        public static string Fill(string text, char charactere, char onlyCharacter, string between)
        {

            return fill(text.ToArray(), charactere, onlyCharacter, between).ToString();
        }

        #endregion

        #region ▼ Privates

        private static Span<char> fill(Span<char> text, char charactere)
        {
            return fill(text, charactere, '\0', ReadOnlySpan<char>.Empty);
        }

        private static Span<char> fill(Span<char> text, char charactere, ReadOnlySpan <char> between)
        {
            return fill(text, charactere, '\0', between);
        }

        private static Span<char> fill(Span<char> text, char charactere, char onlyCharacter, ReadOnlySpan<char> between)
        {
            #region ? Just fill all characters
            if (onlyCharacter == '\0' && between.IsEmpty)
            { text.Fill(charactere); }
            #endregion

            #region ? Just fill respective character of onlyCharacter
            else if (between.IsEmpty)
            {
                for (int i = 0; i < text.Length; i++)
                {
                    if (text[i] == onlyCharacter) { text[i] = charactere; }
                }
            }
            #endregion

            #region ! Fill characters between pattern in 'between' parameter
            else
            {
                Span<char> _between = stackalloc char[between.Length];
                between.CopyTo(_between);

                PatternsToMatch tagsIn = new PatternsToMatch(_between, '*');
                Span<char> openTag = tagsIn[0];
                Span<char> closeTag = tagsIn[1];

                #region ** Exception : Inavalid 'between'

                if (tagsIn.Count != 2)
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

                    pos = text[pos..].IndexOf(openTag);
                    if (pos == -1) { break; }

                    #endregion

                    pos++;

                    for (; pos < text.Length; pos++)
                    {
                        #region ? A close tag of between parameter

                        if (text.Slice(pos, closeTag.Length).SequenceEqual(closeTag))
                        { pos += closeTag.Length + 1; break; }

                        #endregion

                        #region ? if OnlyCharacter paraneter informed, Just fill respective charactere
                        if (onlyCharacter != '\0')
                        {
                            if (text[pos] == onlyCharacter) { text[pos] = charactere; }
                        }
                        #endregion

                        else
                        { text[pos] = charactere; }
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
            return insert(text, toInsert, positionIndex, toInsert.Length, false).ToString();
        }

        #endregion

        #region ► InsertBefore

        public static string InsertBefore(string text, string toInsert, string beforeIt, params byte[] options)
        {
            StringAndPosition matchReturn = Match(text, beforeIt, options);
            int position = matchReturn.Position;
            return insert(text, toInsert, position, toInsert.Length, false).ToString();
        }

        #endregion

        #region ► InsertAfter

        public static string InsertAfter(string text, string toInsert, string afterIt, params byte[] options)
        {
            StringAndPosition matchReturn = Match(text, afterIt, options);
            int position = matchReturn.Position + matchReturn.Text.Length;
            return insert(text, toInsert, position, toInsert.Length, false).ToString();
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

        #endregion

        #region ▼ Match Snippets

        #region ▼ Snippets

        #region ► Constructors

        public static StringAndPosition Snippet(string sourceText, string openAndCloseTags, params byte[] options)
        {
            return snippet(sourceText, openAndCloseTags, string.Empty, 0, options);
        }

        public static StringAndPosition Snippet(string sourceText, string openAndCloseTags, int startIndex, params byte[] options)
        {
            return snippet(sourceText, openAndCloseTags, string.Empty, startIndex, options);
        }

        public static StringAndPosition Snippet(string sourceText, string openAndCloseTags, string idOfSnippet, params byte[] options)
        {
            return snippet(sourceText, openAndCloseTags, idOfSnippet, 0, options);
        }

        public static StringAndPosition Snippet(string sourceText, string openAndCloseTags, string idOfSnippet, int startIndex, params byte[] options)
        {
            return snippet(sourceText, openAndCloseTags, idOfSnippet, startIndex, options);
        }

        #endregion

        #region ► Controller
        private static StringAndPosition snippet(ReadOnlySpan<char> text, ReadOnlySpan<char> snippetTags, ReadOnlySpan<char> snippetID, int startIndex, params byte[] options)
        {
            if (snippetTags.Length == 0 && text == "") { return default; }

            (int position, int length) = snippetsMatch(text, snippetTags.ToArray(), snippetID, startIndex, options);

            if (position == -1 || length == 0)
            { return new StringAndPosition(); }

            return new StringAndPosition(text.Slice(position, length).ToString(), position);
        }

        #endregion

        #region ► Model

        private static (int, int) snippetsMatch(ReadOnlySpan<char> text, Span<char> snippet, ReadOnlySpan<char> snippetID, int startIndex, params byte[] options)
        {
            #region ** Exception : The snippet not contain a '*' character to split open and close tag.
            int splitPos = snippet.IndexOf('*');

            if (splitPos == -1)
            { throw new Exception("!Invalid snippet pattern!"); }

            #endregion

            #region ++ Flags e variables

            int openTagCount = -1;
            int closeTagCount = 0;
            int occurStart = -1;
            int occurLen = 0;
            int startPos = 0;
            int snippetIDPos = -1;
            PatternsToMatch toMatch = new PatternsToMatch(snippet, '*');

            if (toMatch.Count != 2)
            { throw new InvalidCastException("!Snippet pattern mismatch! Just inform open*close tag in pattern."); }

            ReadOnlySpan<char> textToSearch = text.Slice(startIndex);

            #endregion

            #region ▼ Parameters and Dynamic chars tasks

            #region + Replace the '*' with a '|' character in snippet pattern.
            // This is used to matchLitteral match the open or close tags.
            // Using OR condition between tags, matchLitteral will return the first tag found.
            // So is possible know if after open tag, the next is the respective close.
            snippet[splitPos] = '|';

            #endregion

            #region + If Ignore in quote

            if (options.Contains(ParamsIgnoreInQuotes))
            { textToSearch = markWordsInText(textToSearch.ToArray(), '\''); }

            #endregion

            #region + If Ignore in double quote

            if (options.Contains(ParamsIgnoreInDoubleQuotes))
            { textToSearch = markWordsInText(textToSearch.ToArray(), '\"'); }

            #endregion

            #region ? Dynamics chars

            //if (options.Contains(ParamsDynamicChars))
            //{
            //    if (snippet.IndexOf('#') != -1 || snippetID.IndexOf('#') != -1)
            //    { textToSearch = markNumbersInText(textToSearch.ToArray()); }
            //}

            #endregion

            #region + If case insentive

            //if (options.Contains(ParamsIgnoreCase))
            //{ textToSearch = toLowerText(textToSearch.ToArray()); }

            #endregion

            #endregion

            #region + If snippetID is not empty, search for snippetID in open tag.

            if (snippetID.Length > 0)
            {
                // If found, set snippetIDPos to current position.
                snippetIDPos = textToSearch.IndexOf(snippetID);
            }

            #endregion

            int _position = startIndex;

            while (openTagCount != closeTagCount)
            {
                #region + Search for open or close tag in text

                (_position, int _len) = matchLitteral(textToSearch, snippet, _position, true);

                #endregion

                #region ** Exception : If not found open or close tag, return -1 and 0.

                if (_position == -1)
                {
                    if (openTagCount > 0 || closeTagCount > 0)
                    {
                        if (openTagCount > closeTagCount)
                        { throw new Exception("!Invalid snippet! Exist open tag without respective close tag."); }
                        else if (closeTagCount > openTagCount)
                        { throw new Exception("!Invalid snippet! Exist close tag without respective open tag."); }
                    }
                    return (-1, 0);
                }

                #endregion

                if (textToSearch.Slice(_position, _len).SequenceEqual(toMatch[0]))
                {

                    #region ! If found open tag, increase openTagCount and set occurStart if not set.
                    // openTagCount is used to track how many open tags are found.

                    if (openTagCount == -1) { openTagCount = 1; }
                    else { openTagCount++; }

                    if (occurStart == -1) { occurStart = _position; }

                    #endregion

                    #region + If snippetID is not empty, check if found in open tag.

                    if (snippetIDPos != -1)
                    {
                        if (_position < snippetIDPos && snippetIDPos != -1)
                        {
                            openTagCount = -1;
                            _position++;
                            occurStart = _position;
                            closeTagCount = 0;
                            occurLen = 0;
                            continue;
                        }
                        else { snippetIDPos = -1; openTagCount = 2; }
                    }

                    #endregion

                }
                else if (textToSearch.Slice(_position, _len).SequenceEqual(toMatch[1]))
                {
                    if (snippetIDPos != -1 && startPos < snippetIDPos)
                    {
                        #region ! If snippetID parameter is not empty and close tag position in text is before snippetID

                        _position++;
                        closeTagCount = 0;
                        openTagCount = -1;
                        occurLen = 0;
                        continue;

                        #endregion
                    }
                    else if (openTagCount > 0 && openTagCount > closeTagCount)
                    {
                        #region ! If found close tag, decrease openTagCount and set occurLen if not set.
                        //Need openTagCount = closeTagCount to confirm that is last close tag of first open tag.

                        closeTagCount++;

                        #endregion
                    }
                    else
                    {
                        #region ! If already not found open tag to this snippet

                        _position++;
                        closeTagCount = 0;
                        openTagCount = -1;
                        occurLen = 0;
                        continue;

                        #endregion
                    }
                }

                occurLen = (_position - occurStart) + _len;
                _position++;

                if (textToSearch.Length <= _position)
                {
                    // No more text to search, break the loop.
                    break;
                }
            }

            return (occurStart, occurLen);
        }

        #endregion

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
            ReadOnlySpan<char> _text = text;
            ReadOnlySpan<char> _snippetID = snippetID;
            ReadOnlySpan<char> _sequenceToMatch = toRepalce;
            ReadOnlySpan<char> _toReplace = toRepalce;

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
            ReadOnlySpan<char> _text = text;
            ReadOnlySpan<char> _openAndCloseTags = openAndCloseTags;
            ReadOnlySpan<char> _snippetID = snippetID;
            ReadOnlySpan<char> _toReplace = toRepalce;
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
            return insert(text, toInsert, positionIndex, toInsert.Length, false).ToString();
        }

        #endregion

        #region ► InsertBefore

        public static string InsertSnippetBefore(string text, string toInsert, string beforeIt, params byte[] options)
        {
            StringAndPosition matchReturn = Match(text, beforeIt, options);
            int position = matchReturn.Position;
            return insert(text, toInsert, position, toInsert.Length, false).ToString();
        }

        #endregion

        #region ► InsertAfter

        public static string InsertSnippetAfter(string text, string toInsert, string afterIt, params byte[] options)
        {
            StringAndPosition matchReturn = Match(text, afterIt, options);
            int position = matchReturn.Position + matchReturn.Text.Length;
            return insert(text, toInsert, position, toInsert.Length, false).ToString();
        }

        #endregion

        #endregion

        #endregion

        #region » IsSeparator
        private static bool IsSeparator(char c) => c == ' ' || c == '!' || c == '?' || c == '.' || c == ';' ||
                                    c == ':' || c == ',' || c == '|' || c == '(' || c == ')' || c == '[' || c == ']'
                                    || c == '{' || c == '}' || c == '\n' || c == '\t' || c == '\r';
        #endregion
    }
}
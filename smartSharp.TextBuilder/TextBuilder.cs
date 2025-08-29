using System.Buffers;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace SmartSharp.TextBuilder
{
    #region ▼ StringAndPosition
    /// <summary>
    /// Represents a result of a text search, containing the matched string and its position.
    /// </summary>
    [DebuggerDisplay("[{Position}] \"{Text}\"")]
    public sealed class StringAndPosition
    {
        /// <summary>
        /// Gets the position of the occurrence in the original text.
        /// </summary>
        public int Position { get; }

        /// <summary>
        /// Gets the matched text content.
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// Indicates whether the result is empty or invalid.
        /// </summary>
        public bool Empty => string.IsNullOrEmpty(Text) || Position < 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="StringAndPosition"/> class.
        /// </summary>
        /// <param name="text">The matched text. Cannot be null.</param>
        /// <param name="position">The position of the match. Must be non-negative.</param>
        public StringAndPosition(ReadOnlySpan<char> text, int position)
        {
            Text = text.ToString() ?? throw new ArgumentNullException(nameof(text));
            Position = position >= 0 ? position : throw new ArgumentOutOfRangeException(nameof(position));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringAndPosition"/> class.
        /// </summary>
        public StringAndPosition()
        {
            Text = "";
            Position = -1;
        }
    }

    #endregion

    /// <summary>
    /// Provides a collection of static methods and properties for advanced text manipulation and pattern matching.
    /// </summary>
    /// <remarks>The <see cref="TextBuilder"/> class includes utilities for matching, replacing, splitting,
    /// and transforming text based on complex patterns. It supports operations such as wildcard matching,
    /// case-insensitive comparisons, dynamic character handling, and snippet-based text manipulation. This class is
    /// designed for scenarios where precise and efficient text processing is required, such as parsing, filtering, or
    /// transforming structured text.</remarks>
    public ref struct TextBuilder : IDisposable
    {
        #region ▼ Properties

        /// <summary>
        /// Source text to search pattern.
        /// </summary>
        private ReadOnlySpan<char> text;

        /// <summary>
        /// Source text to search pattern.
        /// </summary>
        public string Text => text.ToString();
        
        public int Lenght { get; }
                                
        /// <summary>
        /// Start index position in text to search pattern.
        /// </summary>
        public int StartIndex { get; set; } = 0;
        public void setStartIndex(int index) => StartIndex = index;

        /// <summary>
        /// Length of char to supress in start of return occurrence.
        /// </summary>
        /// <example>Supress 1 length of " test" return "test",</example>
        public int SupressCharsInStart { get; set; } = 0;
        public void setSupressCharsInStart(int index) => SupressCharsInStart = index;

        /// <summary>
        /// Start index position in text to search pattern.
        /// </summary>
        /// <example>Supress 1 length of "test " return "test",</example>
        public int SupressCharsInEnd { get; set; } = 0;
        public void setSupressCharsInEnd(int index)=> SupressCharsInEnd = index;

        /// <summary>
        /// Length of source text.
        /// </summary>
        public int Length => text.Length;
                
        /// <summary>
        /// Is the source text empty?
        /// </summary>
        public bool IsEmpty => text.IsEmpty;

        /// <summary>
        /// Is the source text null or empty?
        /// </summary>
        public bool IsNullOrEmpty => text.IsEmpty;
        
        #endregion

        #region ▼ Parameters

        /// <summary>
        /// Consider upper or lower case.
        /// </summary>
        public bool CaseSensitive = true;
        public void EnableCaseSensitive() => CaseSensitive = true;
        public void DisableCaseSensitive() => CaseSensitive = false;
        
        /// <summary>
        /// Ignore content in single quotes '' when parsing text.
        /// </summary>
        public bool IgnoreCharsInQuotes = false;
        public void EnableIgnoreCharsInQuotes () => IgnoreCharsInQuotes = true;
        public void DisbleIgnoreCharsInQuotes() => IgnoreCharsInQuotes = false;
 
        /// <summary>
        /// Ignore content in double quotes "" when parsing text.
        /// </summary>
        public bool IgnoreCharsInDoubleQuotes = false;
        public void EnableIgnoreCharsInDblQuotes() => IgnoreCharsInDoubleQuotes = true;
        public void DisableIgnoreCharsInDoubleQuotes() => IgnoreCharsInDoubleQuotes = false;

        /// <summary>
        /// Return only captured chars in text and ignore chars in pattern.
        /// </summary>
        //public bool ReturnOnlyCapturedSegment = false;
        //public void EnableReturnOnlyCapturedSegment() => ReturnOnlyCapturedSegment = true;
        //public void DisableReturnOnlyCapturedSegment() => ReturnOnlyCapturedSegment = false;

        /// <summary>
        /// Identify dynamic chars of pattern in text and in pattern.
        /// </summary>
        public bool IgnoreDynamicChars = false;
        public void EnableIgnoreDynamicChars() => IgnoreDynamicChars = true;
        public void DisableIgnoreDynamicChars() => IgnoreDynamicChars = false;

        /// <summary>
        /// Do not force search in text by nearest words to return the shortest occurrence.
        /// </summary>
        public bool MatchGreedyOccurences = false;
        public void EnableMatchGreedyOccurences() => MatchGreedyOccurences = true;
        public void DisableMatchGreedyOccurences() => MatchGreedyOccurences = false;

        #endregion

        #region ▼ Constructor

        public TextBuilder(string text)
        {
            if (text is null) { throw new ArgumentNullException(nameof(text)); }
            this.text = text.AsSpan();
            this.Lenght = text.Length;
        }

        #endregion

        #region ▼ Match Words

        #region ▼ Constructors

        /// <summary>
        /// Matches a specified sequence within the given text and returns the result along with positional information.
        /// </summary>
        /// <remarks>This method processes the input text differently based on its length to optimize
        /// performance.</remarks>
        /// <param name="toMatch">String of the sequence of characters to match within the text.</param>
        /// <returns>A <see cref="StringAndPosition"/> object containing the matched sequence and its positional information.</returns>
        /// <example>Wildcard simple pattern: "name*.", found "name: John Doe."</example>
        /// <example>Dynamic char Word separation pattern: "John_Marie_", found "John Marie " or "John,Marie " or "John(Marie)" and others</example>
        /// <example>Dynamic char number pattern: "iten #", found "iten 10" or "iten 2000" and more</example>
        /// <example>Dynamic char complete word: "~act", found "react", "act~" found "action", "~act~" found "Characteristics"/example>
        /// <remarks>To method to interpret dynamic char, set propertie paramsDynamicChars to true</remarks>
        public StringAndPosition Match(ReadOnlySpan<char> toMatch)
        {
            (int pos, int len) result = match(ref toMatch);
            if (result.pos == -1) { return new StringAndPosition(); }
            else { return new StringAndPosition(text.Slice(result.pos, result.len), result.pos); }
        }

        #endregion

        #region ► Controller
        /// <summary>
        /// Model match of text with sequence to match.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="sequenceToMatch">Pattern with words and dynamic chars to search in source text</param>
        /// <param name="startIndex">Start index position in text</param>
        /// <param name="startIndexReturn">Remove start char count of returned occurence</param>
        /// <param name="endCutLenReturn">Remove end char count of returned occurence</param>
        /// <param name="options">Options parameters</param>
        /// <returns></returns>
        private (int, int) match( ref ReadOnlySpan<char> toMatch)
        {
            if (toMatch.Length == 0) { return (-1, 0); }

            #region + Wildcard in start pattern

            bool wildcardStart = false;
            bool wildcardEnd = false;
            if (toMatch[0] == '*') { toMatch = toMatch[1..]; wildcardStart = true; }
            if (toMatch[^1] == '*') { toMatch = toMatch[..^1]; wildcardEnd = true; }

            #endregion

            #region ++ Flags and variable                       
            
            int txtPos = StartIndex;
            int returnPos = -1;
            int returnLen = -1;
            bool wildcardMode = false;
            
            #endregion
                        
            for (; txtPos < this.Length; txtPos++)
            {
                int occurPos = -1; int occurLen = 0;
                ReadOnlySpan<char> wildcardMatch = toMatch;
                
                while (!wildcardMatch.IsEmpty)
                {
                    #region + Split wildcard parts

                    //The first occurence is a start pos and other occurences is a len
                    int splitPos = wildcardMatch.IndexOf('*');
                    if (splitPos == -1) { splitPos = wildcardMatch.Length; }
                    ReadOnlySpan<char> orMatch = wildcardMatch[..splitPos];
                    if (orMatch.Length < wildcardMatch.Length) 
                    { splitPos = orMatch.Length + 1; wildcardMatch = wildcardMatch[splitPos..]; }
                    else { wildcardMatch = ReadOnlySpan<char>.Empty; }
                    
                    #endregion

                    #region + Split OR parts

                    int orLen = -1;
                    int orPos = -1;
                    while (!orMatch.IsEmpty)
                    {
                        #region + Split OR parts by '|'

                        splitPos = orMatch.IndexOf('|');
                        if(splitPos == -1) { splitPos = orMatch.Length; }
                        ReadOnlySpan<char> matchWord = orMatch[..splitPos];
                        if(matchWord.Length < orMatch.Length) 
                        { splitPos = matchWord.Length + 1; orMatch = orMatch[splitPos..]; }
                        else { orMatch = ReadOnlySpan<char>.Empty; }
                        
                        #endregion

                        #region + Text IndexOf part
                        (int pos, int len ) = indexOf(matchWord, txtPos);
                        if(pos == -1) { continue; }
                        #endregion
                        
                        #region + Verify if OR occurrence is smaller position in text

                        if ( (uint)pos < (uint)orPos )
                        {  orPos = pos; orLen = len; }

                        #endregion
                    }

                    #endregion

                    #region ? March not found
                    if (orPos == -1) { break; }
                    #endregion

                    #region + Update return data
                    if (occurPos ==-1) { occurPos = orPos; }
                    occurLen = (orPos + orLen) - occurPos;
                    #endregion

                    txtPos = occurPos;
                }
                                
                if ((uint)returnLen > (uint)occurLen)
                {
                    returnPos = occurPos; returnLen = occurLen;

                    if (MatchGreedyOccurences) { break; }
                }

                if (!wildcardMode || wildcardMatch.IsEmpty) { break; }

                txtPos = returnPos;
            }

            #region + Wildcard of start char in pattern

            if (wildcardStart)
            { returnLen += returnPos; returnPos = 0; }

            #endregion

            #region + Wildcard of last char in pattern

            if (wildcardEnd)
            { returnLen = text.Length - returnPos; }

            #endregion

            #region + Supress chars in start/end
            if( SupressCharsInStart != 0 ) { returnPos += SupressCharsInStart; returnLen -= SupressCharsInStart; }
            if( SupressCharsInEnd != 0 ) { returnLen -= SupressCharsInEnd; }
            #endregion

            StartIndex = 0;
            SupressCharsInStart = 0;
            SupressCharsInEnd = 0;
            CaseSensitive = true;
            IgnoreCharsInDoubleQuotes = false;
            IgnoreCharsInQuotes = false;
            IgnoreDynamicChars = false;

            return (returnPos, returnLen);
        }

        #endregion

        #region ▼ Private Auxiliar Methods

        #region » IsSeparator
        /// <summary>
        /// If the char is a separator.
        /// </summary>
        /// <param name="c">Char of text</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsSeparator(char c) => c == ' ' || c == '!' || c == '?' || c == '.' || c == ';' ||
                                            c == ':' || c == ',' || c == '|' || c == '(' || c == ')' || 
                                            c == '[' || c == ']' || c == '{' || c == '}' || c == '\n'|| 
                                            c == '\t' || c == '\r';
        #endregion

        #region » IsDynamicOrSeparator
        /// <summary>
        /// If the char is a separator.
        /// </summary>
        /// <param name="c">Char of text</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isDynamicOrSeparatorChar(char c) => c == '_' || c == '#' || c == '~' || c == ' ' || c == '!' || 
                                          c == '?' || c == '.' || c == ';' || c == ':' || c == ',' || 
                                          c == '|' || c == '(' || c == ')' || c == '[' || c == ']' || 
                                          c == '{' || c == '}' || c == '\n'|| c == '\t' || c == '\r';

        #endregion

        #region » IsDynamicChar
        /// <summary>
        /// If the char is a separator.
        /// </summary>
        /// <param name="c">Char of text</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isDynamicChar(char c) => c == '_' || c == '#' || c == '~';

        #endregion

        #endregion

        #region ▼ Models

        #region ► matchLitteral
        /// <summary>
        /// Litteral match word in text with patterns without wildcard.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="patterns">Litteral text to search</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">Parametes options</param>
        /// <returns>Tulpla with position int and len int of occurence</returns>
        /// <exception cref="InvalidFilterCriteriaException"></exception>
        //private (int, int) matchLitteral( ref ReadOnlySpan<char> patterns)
        //{
        //    int pos = 0;
        //    int returnPos = -1;
        //    int returnLen = 0;

        //    #region ** Exception : Invlaid pattern
        //    if (patterns[0] == '|' || patterns[patterns.Length - 1] == '|')
        //    {
        //        throw new InvalidFilterCriteriaException(
        //        "Invalid use of 'OR' char('|')! Not use it to start, end pattern and after wildcard '*'."
        //        );
        //    }
        //    #endregion

        //    #region + Or split to or pattern

        //    PatternsToMatch toMatch = new PatternsToMatch(patterns, '|');

        //    #endregion

        //    int count = toMatch.Count;
        //    for (int i = 0; i < count; i++)
        //    {
        //        //Reuse pattern to new resize length if inWord char is found
        //        int len = 0;

        //        #region + Litteral pattern

        //        (pos, len) = indexOf(ref text, patterns, 0, true, false, false, false);

        //        if (pos == -1) { continue; }

        //        #endregion

        //        #region ? The occurrence is first position in text?

        //        if ((uint)returnPos > (uint)pos)
        //        { returnPos = pos; returnLen = len; }

        //        #endregion
        //    }

        //    #region + Not found match
        //    if (returnPos == -1)
        //    { return (-1, 0); }
        //    #endregion

        //    return (returnPos, returnLen);
        //}

        #endregion

        #region ► matchPattern
        /// <summary>
        /// Match pattern in text with dynamic chars and wildcards.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="patterns">Patterns with words parts splited by wildcard</param>
        /// <param name="startIndex">Start position of source text</param>
        /// <param name="options">Parameters oprions</param>
        /// <returns></returns>
        //private static (int, int) matchPattern(ReadOnlySpan<char> text, ReadOnlySpan<char> patterns, int startIndex, byte[] options = null)
        //{
        //    int returnPos = -1;
        //    int returnLen = -1;
        //    int pos = 0;
        //    int len = 0;

        //    PatternsToMatch toMatch = new PatternsToMatch(patterns, '*');
        //    int count = toMatch.Count;

        //    for (int i = 0; i < count; i++)
        //    {
        //        (pos, len) = matchLitteral(text, toMatch[i], startIndex, options);

        //        #region ? Not matched pattern
        //        if (pos == -1)
        //        { return (-1, 0); }
        //        #endregion

        //        #region ! Matched pattern
        //        if (returnPos == -1)
        //        { returnPos = pos; startIndex = pos + 1; returnLen = len; }
        //        else { returnLen = (pos - returnPos) + len; }
        //        #endregion
        //    }

        //    return (returnPos, returnLen);
        //}

        #endregion

        #region ► indexOf
        /// <summary>
        /// Model/Source index of first occurrence in text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="pattern">Pattern to match in source text</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Parms options</param>
        /// <returns></returns>
        private (int position, int additionalLen) indexOf( ReadOnlySpan<char> toMatch, int startIndex=-1)
        {
            #region ** Exception : Empty pattern or text
            if (toMatch.Length == 0) { return (-1, 0); }
            if (text.Length == 0) { return (-1, 0); }
            #endregion
                        
            if (startIndex == -1) startIndex = StartIndex;

            #region + Start dynamic char to complete word

            bool startCharCompleteWord = false;
            if (toMatch[0] == '~')
            { toMatch = toMatch.Slice(1); startCharCompleteWord = true; }

            #endregion

            #region + End dynamic char to complete word

            bool endCharCompleteWord = false;
            if (toMatch[^1] == '~')
            { toMatch = toMatch[..^1]; endCharCompleteWord = true; }

            #endregion

            #region + Variable and flags

            int txtLength = text.Length;
            int patLength = toMatch.Length;
            int occurPos = -1;
            int occurLen = patLength;
            int patPos = 0;
            int pos = startIndex;
            bool completeWord = false;
            int previewCharSeparator = -1;
            
            #endregion

            #region + Security start index
            if (StartIndex < 0) { StartIndex = 0; }
            #endregion

            ref char refText = ref MemoryMarshal.GetReference(text);
            ref char refPat = ref MemoryMarshal.GetReference(toMatch);

            while (occurPos == -1 && pos < txtLength)
            {
                for (; pos < txtLength; pos++)
                {
                    #region + Build pattern part and indexOf

                    if (!IgnoreCharsInQuotes && !IgnoreCharsInDoubleQuotes && CaseSensitive && pos == startIndex)
                    {
                        int lastPos = pos;
                        if (subGetByIndexOf(ref toMatch, ref patPos, ref patLength, ref refPat,
                                           ref pos, ref occurPos, ref occurLen))
                        { break; }

                        if (pos == -1) { pos = lastPos; }
                    }

                    #endregion

                    #region + Char in text
                    char c = Unsafe.Add(ref refText, pos);
                    #endregion

                    #region + Ignore in quotes

                    if (IgnoreCharsInQuotes && c == '\'' || IgnoreCharsInQuotes && c == '\"')
                    {
                        pos = subIgnoreQuote(pos, ref c);
                    }

                    #endregion

                    #region + Pattern char
                    if (patPos == patLength) { break; }
                    if (patPos == 0 && completeWord) { patPos++; }
                    char p = Unsafe.Add(ref refPat, patPos);
                    #endregion

                    if (!CaseSensitive)
                    {
                        #region + Ignore sensitive case char
                        if (toCaseChar(c, 0) == toCaseChar(p, 0))
                        {
                            if (occurPos == -1) { occurPos = pos; }
                            occurLen++; patPos++; continue;
                        }
                        #endregion
                    }
                    else if (c == p)
                    {
                        #region + Match char
                        if (occurPos == -1) { occurPos = pos; }
                        occurLen++; patPos++; continue;
                        #endregion
                    }

                    #region + Word separator char
                    if (p == '_' && IsSeparator(c) && !IgnoreDynamicChars)
                    {
                        if (occurPos == -1) { occurPos = pos; }
                        occurLen++; patPos++; previewCharSeparator = pos; continue;
                    }
                    #endregion

                    #region + Number char

                    if (Char.IsDigit(c) && p == '#' && !IgnoreDynamicChars)
                    {
                        if (occurPos == -1) { occurPos = pos; }
                        DynamicNumber(ref pos, ref occurLen); patPos++; continue;
                    }

                    #endregion
                                       
                    #region + Has Partial Match

                    if (occurPos != -1 || occurLen > 0 || patPos > 0)
                    { occurPos = -1; occurLen = 0; patPos = 0; }

                    #endregion
                }

                #region + Word complete char

                if (startCharCompleteWord && !IgnoreDynamicChars && occurPos != -1)
                {
                    subCompleteWordStart(ref occurPos, ref occurLen, ref pos, ref patPos, ref toMatch);
                }

                #endregion

                #region + Word complete char

                if (endCharCompleteWord && !IgnoreDynamicChars && occurPos != -1)
                {
                    subCompleteWordEnd(ref occurPos, ref occurLen, ref pos, ref patPos, ref toMatch);
                }

                #endregion
            }

            return (occurPos, occurLen);
        }

        #endregion

        #region » subGetByIndexOf

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool subGetByIndexOf(ref ReadOnlySpan<char> toMatch,
                                      ref int patPos, ref int patLength, ref char refPat,
                                      ref int pos, ref int occurPos, ref int occurLen)
        {
            int tempPatStart = patPos;

            for (; patPos < patLength; patPos++)
            {
                char part = Unsafe.Add(ref refPat, patPos);

                if (!isDynamicChar(part) || IgnoreDynamicChars) continue;
                else { return false; }
            }

            // Executa busca personalizada
            int txtStart = pos;
            pos = text[txtStart..].IndexOf(toMatch.Slice(tempPatStart, patPos));

            if (pos == -1) { return false; }
            
            pos += txtStart;
            
            if (pos < toMatch.Length) return false;
                        
            if (occurPos == -1)
            {
                occurPos = pos;
                occurLen = patPos;
            }

            return true;
        }
        #endregion

        #region » Ignore in quotes

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int subIgnoreQuote(int pos, ref char c)
        {
            if (IgnoreCharsInQuotes && c == '\'')
            {
                pos++;
                int endQuote = text[pos..].IndexOf('\'');
                if (endQuote < 0) { throw new InvalidOperationException("Non-closed opened apostrophe quote!"); }
                pos += endQuote;
                pos++;
            }

            if (IgnoreCharsInQuotes && c == '\"')
            {
                pos++;
                int endQuote = text[pos..].IndexOf('\"');
                if (endQuote < 0) { throw new InvalidOperationException("Non-closed opened double quote!"); }
                pos += endQuote;
                pos++;
            }

            return pos;
        }

        #endregion

        #region » dynamicNumber
        /// <summary>
        /// Builds the length of a dynamic numbers in text by '#' in pattern.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="pos">Position in source text</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DynamicNumber(ref int pos, ref int occurLen)
        {
            for (; pos < text.Length; pos++)
            {
                char c = text[pos];
                if (c < '0' || c > '9')
                { pos--; break; }

                occurLen++;
            }

            return;
        }

        #endregion

        #region » completeWord
        /// <summary>
        /// Completes the word start in text from the given position when '~' in pattern.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="pos">Position in text of occurence</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void subCompleteWordStart(ref int occurPos, ref int occurLen, ref int pos, ref int patPos, ref ReadOnlySpan<char> toMatch)
        {
            int len = occurLen;
            pos = occurPos;
            int continuePos = pos;
            pos--;
                                    
            ref char refText = ref MemoryMarshal.GetReference(text);

            for (; pos > -1; pos--)
            {
                char c = Unsafe.Add(ref refText, pos);
                if (IsSeparator(c))
                {
                    if (pos == occurPos - 1)
                    { pos = continuePos + 1; occurPos = -1; occurLen = 0; patPos = 0; return; }
                    break; 
                }
                len++;
            }

            occurPos = pos + 1; occurLen = len;
            pos = continuePos;

            return;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void subCompleteWordEnd(ref int occurPos, ref int occurLen, ref int pos, ref int patPos, ref ReadOnlySpan<char> toMatch)
        {
            int continuePos = pos;
            pos += toMatch.Length;
            int firstChar = pos;
            int len = occurLen;

            ref char refText = ref MemoryMarshal.GetReference(text);

            for (; pos < text.Length; pos++)
            {
                char c = Unsafe.Add(ref refText, pos);

                if (IsSeparator(c))
                {
                    if (pos == firstChar)
                    { pos++; occurPos = -1; occurLen = 0; patPos = 0; return; } // Skip first char if not separator

                    break;
                }
                len++;
            }
            
            occurLen = len;

            return;
        }

        #endregion

        #endregion

        #region toCase

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private char toCaseChar(char character, byte toCaseCod)
        {
            if (character >= 'a' && character <= 'z' && toCaseCod == 0) { return character; }
            if (character >= 'A' && character <= 'Z' && toCaseCod == 1) { return character; }

            // Tabelas estáticas (não alocam em cada chamada)
            static ReadOnlySpan<char> UpperChars() => "ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞß";
            static ReadOnlySpan<char> LowerChars() => "àáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþß";

            // ASCII A-Z
            if (character >= 'A' && character <= 'Z' && toCaseCod == 0)
            {
                character = (char)(character + 32);
            }
            else if (character >= 'a' && character <= 'z' && toCaseCod == 1)
            {
                character = (char)(character - 32);
            }
            else if (toCaseCod == 0)
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
            else if (toCaseCod == 1)
            {
                // Busca em tabela de acentuados
                int index = LowerChars().IndexOf(character);
                if (index != -1)
                {
                    character = UpperChars()[index];
                }
                else
                {
                    character = char.ToUpperInvariant(character);
                }
            }

            return character;
        }

        #endregion

        #endregion

        public void Dispose()
        {
            // Implement IDisposable if needed
        }
    }
}
using System.Buffers;
using System.Reflection;

namespace SmartSharp.TextBuilder
{
    #region ▼ StringAndPosition
    /// <summary>
    /// Storage class for return string and position of occurrence in text.
    /// </summary>
    public class StringAndPosition
    {
        /// <summary>
        /// Gets a value indicating whether found occurance in text.
        /// </summary>
        public bool Empty
        {
            get
            {
                if (Text.Length == 0 || Position < 1)
                { return true; }
                else { return false; }
            }
        }

        /// <summary>
        /// Position of occurrence in text.
        /// </summary>
        public int Position { get; set;}

        /// <summary>
        /// Gets or sets the text content associated with this instance.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringAndPosition"/> class with the specified text and
        /// position of occurrece.
        /// </summary>
        /// <param name="text">The text associated with this instance. Cannot be <see langword="null"/>.</param>
        /// <param name="position">The position associated with this instance. Must be a non-negative integer.</param>
        public StringAndPosition(string text, int position)
        {
            Position = position;
            Text = text;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StringAndPosition"/> class with default values.
        /// </summary>
        /// <remarks>The <see cref="Position"/> property is initialized to -1, and the <see cref="Text"/>
        /// property is initialized to an empty string.</remarks>
        public StringAndPosition()
        {
            Position = -1;
            Text = "";
        }

        /// <summary>
        /// Sets the text and position values.
        /// </summary>
        /// <param name="text">The text to set. Cannot be null.</param>
        /// <param name="position">The position to set. Must be a non-negative integer.</param>
        public void Set(string text, int position)
        {
            Position = position;
            Text = text;
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
    public static class TextBuilder
    {
        #region ▼ ListPatterns
        /// <summary>
        /// Represents a collection of patterns to will extract from a source text, allowing for efficient access and analysis
        /// of individual patterns and their characteristics.
        /// </summary>
        /// <remarks>This structure is designed to parse and manage patterns from a given text, with
        /// support for splitting based on a specified character and handling dynamic characters within patterns. It
        /// provides methods to analyze patterns for wildcards, complete words, and other characteristics.</remarks>
        public ref struct PatternsToMatch
        {
            #region Properties

            /// <summary>
            /// Represents the source pattern as a read-only span of characters.
            /// </summary>
            /// <remarks>This property provides access to the underlying character data in a read-only
            /// manner. It is intended for scenarios where high-performance, non-allocating access to the source data is
            /// required.</remarks>
            private ReadOnlySpan<char> source;

            /// <summary>
            /// Represents a read-only span of integers that stores positional data.
            /// </summary>
            /// <remarks>This field is intended for internal use to manage positional information
            /// efficiently without allocating additional memory. It is immutable and cannot be modified after
            /// initialization.</remarks>
            private ReadOnlySpan<int> positions;

            /// <summary>
            /// Represents a read-only span of integers that stores lengths.
            /// </summary>
            /// <remarks>This field is private and is used internally to manage a collection of length
            /// values.</remarks>
            private ReadOnlySpan<int> lengths;
                    
            /// <summary>
            /// Represents the internal count value used by the class.
            /// </summary>
            /// <remarks>This field is private and is not intended for direct access. It is used
            /// internally to track a count value.</remarks>
            private int count;
            public bool WildcardInFirstChar { get; }

            /// <summary>
            /// Gets a value indicating whether the last character in the input contains a wildcard.
            /// </summary>
            public bool WildcardInLastChar { get; }

            /// <summary>
            /// Gets the number of elements in the collection.
            /// </summary>
            public int Count => count;

            /// <summary>
            /// Gets a value indicating whether the collection is empty.
            /// </summary>
            public bool Empty => count == 0;

            /// <summary>
            /// Determines whether the specified index contains a string with wildcard characters.
            /// </summary>
            /// <remarks>A string is considered to contain a valid wildcard if it includes the '*'
            /// character and does not  meet the following conditions: <list type="bullet"> <item>The '*' character is
            /// at the start of the string and the index is 0.</item> <item>The '*' character is at the end of the
            /// string and the index is the last position in the collection.</item> </list></remarks>
            /// <param name="index">The index of the string to check within the collection.</param>
            /// <returns><see langword="true"/> if the string at the specified index contains a wildcard character ('*')  and
            /// meets the conditions for being considered a valid wildcard; otherwise, <see langword="false"/>.</returns>
            public bool ContainsWildcards(int index)
            {
                int pos = this[index].IndexOf('*');

                if(  pos == -1 || 
                    (pos ==0 && index==0) || 
                     pos == this[index].Length && index == positions.Length) 
                {  return false; }

                return true;
            }

            #endregion

            #region Constructor
            /// <summary>
            /// Initializes a new instance of the <see cref="PatternsToMatch"/> class, parsing the input text into
            /// patterns based on the specified split and ignore characters.
            /// </summary>
            /// <remarks>This constructor processes the input text to identify patterns based on the
            /// specified split and ignore characters. Patterns are stored internally, and the class provides access to
            /// their positions and lengths.  Special cases: - If the input text starts or ends with the wildcard
            /// character '*', the corresponding pattern is adjusted to exclude the wildcard. - If no split character is
            /// found, the entire input text is treated as a single pattern.</remarks>
            /// <param name="text">The input text to parse into patterns. Must not be empty.</param>
            /// <param name="splitChar">The character used to split the input text into patterns. If set to <see langword="'\0'"/>, the '||'
            /// sequence is treated as the split character. Cannot be used as the first or last character of the input
            /// text.</param>
            /// <param name="ignoreChar">The character used to toggle ignore mode. Characters enclosed by this character are ignored during
            /// parsing.</param>
            /// <exception cref="InvalidOperationException">Thrown if <paramref name="splitChar"/> is used as the first or last character of the input text, or if
            /// the input text starts or ends with the '||' sequence when <paramref name="splitChar"/> is <see
            /// langword="'\0'"/>.</exception>
            /// <exception cref="IndexOutOfRangeException">Thrown if the input text contains more than 160 pattern parts.</exception>
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
                
                _positions = null;
                _lengths = null;
            }

            #endregion

            #region Controller
            /// <summary>
            /// Gets a read-only span of characters at the specified index.
            /// </summary>
            /// <param name="index">The zero-based index of the span to retrieve. Must be within the range of available spans.</param>
            /// <returns>A <see cref="ReadOnlySpan{T}"/> of characters representing the span at the specified index.</returns>
            /// <exception cref="IndexOutOfRangeException">Thrown if <paramref name="index"/> is less than 0 or greater than or equal to the number of available
            /// spans.</exception>
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
        /// <summary>
        /// Searches for a specified sequence of characters within the given text and returns the result along with its
        /// position.
        /// </summary>
        /// <param name="text">The text in which to search for the sequence. Cannot be <see langword="null"/>.</param>
        /// <param name="SequenceToMatch">The sequence of characters to search for. Cannot be <see langword="null"/>.</param>
        /// <param name="options">Optional parameters that modify the behavior of the search. The interpretation of these options depends on
        /// the implementation.</param>
        /// <returns>A <see cref="StringAndPosition"/> object containing the matched sequence and its position in the text,  or
        /// <see langword="null"/> if the sequence is not found.</returns>
        public static StringAndPosition Match(string text, string SequenceToMatch, params byte[] options)
        {
            return Match(text, SequenceToMatch, 0,0, 0, options);
        }

        /// <summary>
        /// Searches for a specified sequence within the given text starting at a specified index.
        /// </summary>
        /// <param name="text">The text to search within. Cannot be <see langword="null"/>.</param>
        /// <param name="SequenceToMatch">The sequence to search for within the text. Cannot be <see langword="null"/>.</param>
        /// <param name="startIndex">The zero-based index in the text at which to begin the search. Must be greater than or equal to 0.</param>
        /// <param name="options">Optional parameters that modify the behavior of the search. Can be empty.</param>
        /// <returns>A <see cref="StringAndPosition"/> object containing the matched sequence and its position in the text,  or
        /// <see langword="null"/> if the sequence is not found.</returns>
        public static StringAndPosition Match(string text, string SequenceToMatch, int startIndex, params byte[] options)
        {
            return Match(text, SequenceToMatch, startIndex, 0, 0, options);
        }

        /// <summary>
        /// Matches a specified sequence within a given text starting from a specified index.
        /// </summary>
        /// <param name="text">The text in which to search for the sequence.</param>
        /// <param name="SequenceToMatch">The sequence to match within the text.</param>
        /// <param name="startIndex">The zero-based index in the text at which to begin the search.</param>
        /// <param name="startIndexReturn">The index to return as the starting position of the match.</param>
        /// <param name="options">Optional parameters that influence the matching behavior.</param>
        /// <returns>A <see cref="StringAndPosition"/> object containing the matched sequence and its position in the text.</returns>
        public static StringAndPosition Match(string text, string SequenceToMatch, int startIndex, int startIndexReturn, params byte[] options)
        {
            return Match(text, SequenceToMatch, startIndex, startIndexReturn, 0, options);
        }

        /// <summary>
        /// Matches a specified sequence within the given text and returns the result along with positional information.
        /// </summary>
        /// <remarks>This method processes the input text differently based on its length to optimize
        /// performance.  For texts with a length of 256 or more, a stack-allocated buffer is used; otherwise, a
        /// heap-allocated array is used.</remarks>
        /// <param name="text">The input text in which the sequence will be searched.</param>
        /// <param name="SequenceToMatch">The sequence of characters to match within the text.</param>
        /// <param name="startIndex">The zero-based index in the text at which to begin the search.</param>
        /// <param name="startIndexReturn">The starting index of the matched sequence to include in the result.</param>
        /// <param name="endCutLenReturn">The number of characters to exclude from the end of the matched sequence in the result.</param>
        /// <param name="options">Optional parameters that influence the matching behavior. The specific options supported depend on the
        /// implementation.</param>
        /// <returns>A <see cref="StringAndPosition"/> object containing the matched sequence and its positional information.</returns>
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
        private static StringAndPosition match(ReadOnlySpan<char> text, ReadOnlySpan<char> sequenceToMatch, int startIndex, int startIndexReturn, int endCutLenReturn, params byte[] options)
        {
            if (text.Length == 0) { return new StringAndPosition(); }
            if (sequenceToMatch.Length == 0) { return new StringAndPosition(); }

            #region ++ Flags and variable

            int returnPos = -1;
            int returnLen = -1;

            #endregion
          
            PatternsToMatch toMatch = new PatternsToMatch(sequenceToMatch);

            for (int i = 0; i < toMatch.Count; i++)
            {
                int pos = startIndex;
                int len = 0;

                #region + Match Pattern

                if (toMatch.ContainsWildcards(i))
                {
                    for (; pos < text.Length; pos++)
                    {
                        #region + Match
                        (pos, len) = matchPattern(text, toMatch[i], pos, options);
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

                    (pos, len) = matchLitteral(text, toMatch[i], pos, options);
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
        /// <summary>
        /// If the char is a separator.
        /// </summary>
        /// <param name="c">Char of text</param>
        /// <returns></returns>
        private static bool IsSeparator(char c) => c == ' ' || c == '!' || c == '?' || c == '.' || c == ';' ||
                                    c == ':' || c == ',' || c == '|' || c == '(' || c == ')' || c == '[' || c == ']'
                                    || c == '{' || c == '}' || c == '\n' || c == '\t' || c == '\r';
        #endregion

        #region » DynamicCharPosition
        /// <summary>
        /// Get the position of the first dynamic char in pattern.
        /// </summary>
        /// <param name="pattern">Pattern word</param>
        /// <returns></returns>
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
        /// <summary>
        /// Litteral match word in text with patterns without wildcard.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="patterns">Litteral text to search</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">Parametes options</param>
        /// <returns>Tulpla with position int and len int of occurence</returns>
        /// <exception cref="InvalidFilterCriteriaException"></exception>
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
        /// <summary>
        /// Match pattern in text with dynamic chars and wildcards.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="patterns">Patterns with words parts splited by wildcard</param>
        /// <param name="startIndex">Start position of source text</param>
        /// <param name="options">Parameters oprions</param>
        /// <returns></returns>
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

        #endregion

        #region ▼ Methods Utils

        #region ▼ IndexOf

        #region ► IndexOf
        /// <summary>
        /// Index position of start first occurrence in text.
        /// </summary>
        /// <param name="text">Source Text</param>
        /// <param name="SequenceToMatch">Sequence char or pattern with wildcard to search form source text.</param>
        /// <param name="options">TextBuilder Parameters options</param>
        /// <returns>Int with of start first occurrence.</returns>
        public static int IndexOf(string text, string SequenceToMatch, params byte[] options)
        {
            StringAndPosition matchReturn = Match(text, SequenceToMatch, options);

            return matchReturn.Position;
        }

        /// <summary>
        /// Index position of start first occurrence in text.
        /// </summary>
        /// <param name="text">Source Text</param>
        /// <param name="SequenceToMatch">Sequence char or pattern with wildcard to search form source text.</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder Parameters options</param>
        /// <returns>Int with of start first occurrence.</returns>
        public static int IndexOf(string text, string SequenceToMatch, int startIndex, params byte[] options)
        {
            StringAndPosition matchReturn = Match(text, SequenceToMatch, startIndex, options);

            return matchReturn.Position;
        }

        #endregion

        #region ► IndexOfAll
        /// <summary>
        /// Index position of start all occurrences in text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="SequenceToMatch">Sequence char or pattern with wildcard to search form source text.</param>
        /// <param name="options">TextBuilder.Parms options</param>
        /// <returns>Array of int with all positions</returns>
        public static int[] IndexOfAll(string text, string SequenceToMatch, params byte[] options)
        {
            return IndexOfAll(text, SequenceToMatch, 0, options);
        }

        /// <summary>
        /// Index position of start all occurrences in text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="SequenceToMatch">Sequence char or pattern with wildcard to search form source text.</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Parms options</param>
        /// <returns>Array of int with all positions</returns>
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
        /// <summary>
        /// Model/Source index of first occurrence in text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="pattern">Pattern to match in source text</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Parms options</param>
        /// <returns></returns>
        private static (int position, int additionalLen) indexOf(ReadOnlySpan<char> text, ReadOnlySpan<char> pattern, int startIndex, params byte[] options)
        {
            if (pattern.Length == 0) { return (-1, 0); }
            if (text.Length == 0) { return (-1, 0); }
            int occurPos = -1;
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

            if(startIndex < 0 ){ startIndex = 0; }

            #region + Literal pattern
            if (!dynamicChar && !ignoreCase && !ignoreInQuotes && !ignoreInDoubleQuotes)
            {
                if (getDynamicCharPosition(pattern) == -1)
                {
                    occurPos = text[startIndex..].IndexOf(pattern);
                    if (occurPos == -1) { return (-1, 0); }
                    else { return (occurPos + startIndex, occurLen); }
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
                    if (toCaseChar(c,0) == toCaseChar(p,0))
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
        /// <summary>
        /// Builds the length of a dynamic numbers in text by '#' in pattern.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="pos">Position in source text</param>
        /// <returns></returns>
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
        /// <summary>
        /// Completes the word start in text from the given position when '~' in pattern.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="pos">Position in text of occurence</param>
        /// <returns></returns>
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

        /// <summary>
        /// Completes the word end in text from the given position when '~' in pattern.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="pos">Position in text of occurrence</param>
        /// <returns></returns>
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

        #region ► Convert case

        #region ► Controller

        /// <summary>
        /// To convert case of the specified sequence/pattern to match in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="sequenceToMatch">Sequence chars to match and upper case in source text</param>
        /// <param name="startIndex">Start position of source text</param>
        /// <param name="options">TextBuilder.Parmas options</param>
        /// <returns>Source text with matched sequence in new case.</returns>
        public static string ToCaseMatch(string text, string sequenceToMatch, byte toCaseCod, params byte[] options)
        {
            ReadOnlySpan<char> _sequenceToMatch = sequenceToMatch;

            StringAndPosition matchReturn = match(text, sequenceToMatch, 0, 0, 0, options);

            if (matchReturn.Position == -1)
            {
                return text; // No match found, return original text
            }

            int pos = matchReturn.Position;
            int len = matchReturn.Text.Length;

            if (text.Length >= 256)
            {
                Span<char> result = stackalloc char[text.Length];
                text.CopyTo(result);
                result = toCase(result, matchReturn.Text, toCaseCod);
                return result.ToString();
            }
            else
            {
                Span<char> result = text.ToArray();
                result = toCase(result, matchReturn.Text, toCaseCod);
                return result.ToString();
            }

        }

        #endregion

        #region ► Model

        private static Span<char> toCase(Span<char> text, ReadOnlySpan<char>sequenceChars, byte toCaseCod, params byte[] options )// 0 tolower, 1 upper
        {
            int pos = 0;
            int len = sequenceChars.Length;
            bool casingChars = false;

            for (; pos < text.Length; pos++)
            {
                if (!casingChars)
                {
                    ( pos, len ) = indexOf( text, sequenceChars, pos, options);
                    if (pos == -1) { return text; }
                    pos--;  casingChars = true;
                }
                else
                {
                    text[pos] = toCaseChar(text[pos], toCaseCod);
                    len--;
                    if (len == 0) { len = sequenceChars.Length; casingChars = false; }
                }
            }

            return text;
        }

        /// <summary>
        /// Model to convert case of the specified character in the text.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="character"></param>
        /// <returns></returns>
        private static ReadOnlySpan<char> toCaseAllChars(Span<char> text, Span<char> character, byte toCaseCod)
        { 
            for (int i = 0; i < text.Length; i++)
            {
                ref char c = ref text[i];
                // ASCII A-Z
                c = toCaseChar(c, toCaseCod);
            }

            return text;
        }

        private static ReadOnlySpan<char> toCaseTextIgnore(Span<char> text, ReadOnlySpan<char> openTag, ReadOnlySpan<char> closeTag, byte toCaseCod)
        {
            bool ignore = false;

            for (int i = 0; i < text.Length; i++)
            {
                ref char c = ref text[i];
                // ASCII A-Z

                if (i + openTag.Length < text.Length && i + closeTag.Length < text.Length)
                {
                    if (text.Slice(i, openTag.Length).SequenceEqual(openTag))
                    { ignore = true; i += openTag.Length - 1; continue; }
                    else if (text.Slice(i, closeTag.Length).SequenceEqual(closeTag))
                    { ignore = false; i += closeTag.Length - 1; continue; }
                }

                if (ignore) { continue; }

                text[i] = toCaseChar(text[i], toCaseCod);
            }

            return text;
        }

        private static ReadOnlySpan<char> toCaseTextBetween(Span<char> text, ReadOnlySpan<char> openTag, ReadOnlySpan<char> closeTag, byte toCaseCod)
        {
            bool accept = false;

            for (int i = 0; i < text.Length; i++)
            {
                ref char c = ref text[i];

                if (i + openTag.Length < text.Length && i + closeTag.Length < text.Length)
                {
                    if (text.Slice(i, openTag.Length).SequenceEqual(openTag))
                    {
                        accept = true;
                        //i += openTag.Length - 1; 
                        //continue; 
                    }
                    else if (text.Slice(i, closeTag.Length).SequenceEqual(closeTag))
                    {
                        accept = false;
                        text = toCase(text, closeTag, toCaseCod);
                        i += closeTag.Length - 1;
                        //continue;

                    }
                }

                if (!accept) { continue; }

                text[i] = toCaseChar(text[i], toCaseCod); // Convert to upper case
            }

            return text;
        }

        private static char toCaseChar(char character, byte toCaseCod)
        {
            if (character >= 'a' && character <= 'z' && toCaseCod == 0) { return character; }
            if (character >= 'A' && character <= 'Z' && toCaseCod == 1) { return character; }

            // Tabelas estáticas (não alocam em cada chamada)
            static ReadOnlySpan<char> UpperChars() => "ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞß";
            static ReadOnlySpan<char> LowerChars() => "àáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþß";

            // ASCII A-Z
            if (character >= 'A' && character <= 'Z' && toCaseCod ==0)
            {
                character = (char)(character + 32);
            }
            else if (character >= 'a' && character <= 'z' && toCaseCod == 1)
            {
                character = (char)(character - 32);
            }
            else if(toCaseCod == 0)
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

        #region ► ToLower

        #region ► ToLower ignore snippet
        /// <summary>
        /// Converts the text to lowercase ignoring and not change between specified open and close tags.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="onlyIgnore">Open and close snippet to ignore lower case.
        /// <para>Use wildcard to separate open of close tag.</para></param>
        /// <returns>Source text with snippet in lower case</returns>
        public static string ToLowerIgnoreInSnippet(string text, (string open, string close) onlyIgnore)
        {
            if (text.Length >= 256)
            {
                Span<char> textStack = stackalloc char[text.Length];
                text.CopyTo(textStack);

                ReadOnlySpan<char> result = toCaseTextIgnore(textStack, onlyIgnore.open, onlyIgnore.close, 0);

                return result.ToString();
            }
            else
            {
                char[] textSpan = text.ToArray();

                ReadOnlySpan<char> result = toCaseTextIgnore(textSpan, onlyIgnore.open, onlyIgnore.close, 0);
                if (textSpan != null) { Array.Clear(textSpan, 0, textSpan.Length); }

                return result.ToString();
            }
        }
        #endregion

        #region ► ToLower Only in snippet
        /// <summary>
        /// Converts the text to lowercase only between specified open and close tags.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="onlyBetween">Open and close snippet.
        /// <para>Use wildcard to separate open of close tag.</para></param>
        /// <returns>Source text with snippet in lower case</returns>
        public static string ToLowerOnlyInSnippet(string text, (string open, string close) onlyBetween)
        {
            if (text.Length >= 256)
            {
                Span<char> textStack = stackalloc char[text.Length];
                text.CopyTo(textStack);

                ReadOnlySpan<char> result = toCaseTextBetween(textStack, onlyBetween.open, onlyBetween.close, 0);

                return result.ToString();
            }
            else
            {
                char[] textSpan = text.ToArray();

                ReadOnlySpan<char> result = toCaseTextBetween(textSpan, onlyBetween.open, onlyBetween.close, 0);
                if (textSpan != null) { Array.Clear(textSpan, 0, textSpan.Length); }

                return result.ToString();
            }
        }
        #endregion

        #region ► ToLower char
        /// <summary>
        /// To lower case of the specified character in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="character">Character to lower in text</param>
        /// <returns></returns>
        public static string ToLowerChar(string text, char character)
        {
            Span<char> _char = stackalloc char[1] {character};

            if (text.Length >= 256)
            {
                Span<char> textStack = stackalloc char[text.Length];
                text.CopyTo(textStack);

                ReadOnlySpan<char> result = toCase(textStack, _char, 0);

                return result.ToString();
            }
            else
            {
                char[] textSpan = text.ToArray();

                ReadOnlySpan<char> result = toCase(textSpan, _char, 0);
                if (textSpan != null) { Array.Clear(textSpan, 0, textSpan.Length); }

                return result.ToString();
            }
        }
        #endregion

        #region ► ToLower match
        /// <summary>
        /// To lower case of the specified sequence/pattern to match in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="sequenceToMatch">sequence/pattern to match</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Parms options</param>
        /// <returns></returns>
        public static string ToLowerMatch(string text, string sequenceToMatch, params byte[] options)
        {
            return ToCaseMatch(text, sequenceToMatch, 0, options);
        }
        #endregion
                
        #endregion

        #region ▼ ToUpper

        #region ► ToUpper Ignore in snippet
        /// <summary>
        /// To upper case of the specified text ignoring and not change between specified open and close tags.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="ignoreBetween">Open and close to ignore snippet.
        /// <para>Use wildcard to separate open of close tag.</para></param>
        /// <returns>Source text with snippet in lower case</returns>
        public static string ToUpperIgnoreInSnippet(string text, (string open, string close) ignoreBetween)
        {
            if (text.Length >= 256)
            {
                Span<char> textStack = stackalloc char[text.Length];
                text.CopyTo(textStack);

                ReadOnlySpan<char> result = toCaseTextIgnore(textStack, ignoreBetween.open, ignoreBetween.close, 1);

                return result.ToString();
            }
            else
            {
                char[] textSpan = text.ToArray();

                ReadOnlySpan<char> result = toCaseTextIgnore(textSpan, ignoreBetween.open, ignoreBetween.close, 1);
                if (textSpan != null) { Array.Clear(textSpan, 0, textSpan.Length); }

                return result.ToString();
            }
        }
        #endregion

        #region ► ToUpper Only in snippet
        /// <summary>
        /// To upper case of the specified text only between specified open and close tags.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="onlyBetween">Open and close snippet.
        /// <para>Use wildcard to separate open of close tag.</para></param>
        /// <returns>Source text with snippet in upper case</returns>
        public static string ToUpperOnlyInSnippet(string text, (string open, string close) onlyBetween)
        {
            if (text.Length >= 256)
            {
                Span<char> textStack = stackalloc char[text.Length];
                text.CopyTo(textStack);

                ReadOnlySpan<char> result = toCaseTextBetween(textStack, onlyBetween.open, onlyBetween.close, 1);

                return result.ToString();
            }
            else
            {
                char[] textSpan = text.ToArray();

                ReadOnlySpan<char> result = toCaseTextBetween(textSpan, onlyBetween.open, onlyBetween.close, 1);
                if (textSpan != null) { Array.Clear(textSpan, 0, textSpan.Length); }

                return result.ToString();
            }
        }
        #endregion

        #region ► ToUpper char
        /// <summary>
        /// To upper case of the specified character in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="character">Character to lower in source text</param>
        /// <returns>Source text with especific char in lower case</returns>
        public static string ToUpperChar(string text, char character)
        {
            Span<char> _char = stackalloc char[1] { character };

            if (text.Length >= 256)
            {
                Span<char> textStack = stackalloc char[text.Length];
                text.CopyTo(textStack);

                ReadOnlySpan<char> result = toCase(textStack, _char, 1);

                return result.ToString();
            }
            else
            {
                char[] textSpan = text.ToArray();

                ReadOnlySpan<char> result = toCase(textSpan, _char,1);
                if (textSpan != null) { Array.Clear(textSpan, 0, textSpan.Length); }

                return result.ToString();
            }
        }
        #endregion

        #region ► ToUpper match
        /// <summary>
        /// To upper case of the specified sequence/pattern to match in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="sequenceToMatch">Sequence chars to match and upper case in source text</param>
        /// <param name="startIndex">Start position of source text</param>
        /// <param name="options">TextBuilder.Parmas options</param>
        /// <returns>Source text with matched sequence in upper case.</returns>
        public static string ToUpperMatch(string text, string sequenceToMatch, params byte[] options)
        {
            return ToCaseMatch(text, sequenceToMatch,1, options);
        }
        #endregion
                
        #endregion

        #region ▼ Fill

        #region ▼ Fill
        /// <summary>
        /// Fills all characters in the text with the specified character.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="character">Character to fill all source text</param>
        /// <returns>Source text length with only respectuive character.</returns>
        public static string Fill(string text, char character)
        {
            return Fill(text, character, '\0', default);
        }

        /// <summary>
        /// Fills all characters equal in onlyCharacter in the text with the specified character,
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="character">character to fill source text</param>
        /// <param name="onlyCharacter">Replace only this character in source text.</param>
        /// <returns></returns>
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
        /// <summary>
        /// Model to fill all characters in the text with the specified character.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="charactere">Character to fill source text</param>
        /// <param name="onlyCharacter">Only replace this character to respective character.</param>
        /// <param name="openSnippet">Only replace character to respective character inside this snippet started by it's</param>
        /// <param name="closeSnippet">Only replace character to respective character inside this snippet ended by it's</param>
        /// <returns>Source text with sinppet, character or all replaced to character.</returns>
        /// <exception cref="InvalidOperationException"></exception>
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
        /// <summary>
        /// Replaces the first occurrence of the specified sequence/pattern to match in the text with the specified toReplace string.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="sequenceToMatch">Sequence of charcters to match in text</param>
        /// <param name="toRepalce">Replace macthed to this sequence characters</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns></returns>
        public static string ReplaceFirst(string text, string sequenceToMatch, string toRepalce, params byte[] options)
        {
            return ReplaceFirst(text, sequenceToMatch, toRepalce, 0, options);
        }

        /// <summary>
        /// Replaces the first occurrence of the specified sequence/pattern to match in the text with the specified toReplace string.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="sequenceToMatch">Sequence of characater to match in source text</param>
        /// <param name="toRepalce">Replace macthed to this sequence characters</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Parmas of options</param>
        /// <returns></returns>
        public static string ReplaceFirst(string text, string sequenceToMatch, string toRepalce, int startIndex, params byte[] options)
        {
            ReadOnlySpan<char> _text = text;
            ReadOnlySpan<char> _sequenceToMatch = sequenceToMatch;
            ReadOnlySpan<char> _toReplace = toRepalce;

            StringAndPosition matchReturn = Match(text, sequenceToMatch, startIndex, options);

            if( matchReturn.Position ==-1 )
            {
                return text; // No match found, return original text
            }

            ReadOnlySpan<char> result = insert(text, toRepalce, matchReturn.Position, matchReturn.Text.Length, true);

            return result.ToString();
        }

        #endregion

        #region ► ReplaceLast
        /// <summary>
        /// Replaces the last occurrence of the specified sequence/pattern to match in the text with the specified toReplace string.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="sequenceToMatch">Sequence characters to match in source text</param>
        /// <param name="toRepalce">Replace to this sequence characters</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Source text with match replaced</returns>
        public static string ReplaceLast(string text, string sequenceToMatch, string toRepalce, params byte[] options)
        {
            return ReplaceLast(text, sequenceToMatch, toRepalce, 0, options);
        }

        /// <summary>
        /// Replaces the last occurrence of the specified sequence/pattern to match in the text with the specified toReplace string.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="sequenceToMatch">Sequence of characters to match in source text.</param>
        /// <param name="toRepalce">Replace to this sequence characters</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Source text with match replaced</returns>
        public static string ReplaceLast(string text, string sequenceToMatch, string toRepalce, int startIndex, params byte[] options)
        {
            ReadOnlySpan<char> _text = text;
            ReadOnlySpan<char> _sequenceToMatch = sequenceToMatch;
            ReadOnlySpan<char> _toReplace = toRepalce;
            ReadOnlySpan<char> result = text;
            int position = 0;
            int len = 0;
            int occurPos = -1;

            while (position != -1)
            {
                StringAndPosition matchReturn = match( result, sequenceToMatch, startIndex, 0, 0, options);

                if (matchReturn.Position == -1)
                {
                    break; // No match found, return original text
                }

                position = matchReturn.Position;
                
                if (matchReturn.Position != -1)
                { len = matchReturn.Text.Length; startIndex = matchReturn.Position + len; occurPos = position; }
            }

            result = insert(text, toRepalce, occurPos, len, true);

            return result.ToString();
        }

        #endregion

        #region ► Replace
        /// <summary>
        /// Replaces all occurrences of the specified sequence/pattern to match in the text with the specified toReplace string.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="sequenceToMatch">Sequence characters to match in source text</param>
        /// <param name="toRepalce">Replace to this sequence characters</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Source text with match replaced</returns>
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
        /// <summary>
        /// Model to replace all occurrences of the specified sequence/pattern to match in the text with the specified toReplace string.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="sequenceToMatch">Sequence to match in source text</param>
        /// <param name="toRepalce">Replace to this sequence characters</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Source text with match replaced</returns>
        private static string replaceCore(string text, string sequenceToMatch, string toRepalce, int startIndex, params byte[] options)
        {
            ReadOnlySpan<char> _text = text;
            ReadOnlySpan<char> _sequenceToMatch = sequenceToMatch;
            ReadOnlySpan<char> _toReplace = toRepalce;
            ReadOnlySpan<char> result = text;
            int position = 0;

            while (position != -1)
            {
                StringAndPosition matchReturn = match(result, sequenceToMatch, startIndex,0,0);
                position = matchReturn.Position;

                if(position ==-1) { break; }
                
                result = insert(result, toRepalce, matchReturn.Position, matchReturn.Text.Length, true);
            }

            return result.ToString();
        }

        #endregion

        #endregion

        #region ► Translate

        /// <summary>
        /// Replaces all occurrences of the specified sequence/pattern to match in the text with the specified toReplace string.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="sequenceToMatch">Sequence characters to match in source text</param>
        /// <param name="toRepalce">Replace to this sequence characters</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Source text with match replaced</returns>
        public static string Translate(string text, string sequencesToMatch, string sequencesToRepalce, params byte[] options)
        {
            return Translate(text, sequencesToMatch, sequencesToRepalce, 0, options);
        }

        public static string Translate(string text, string sequencesToMatch, string sequencesToReplace, int startIndex, params byte[] options)
        {
            string[] oldSequences = Split(sequencesToMatch, ',', startIndex,('\'', '\''), true);
            string[] newSequences = Split(sequencesToReplace, ',', startIndex, ('\'', '\''), true);

            if (oldSequences.Count() != newSequences.Count())
            {
                throw new InvalidOperationException("The sequences to match and replace must have the same length.");
            }

            for (int i = 0; i < oldSequences.Count(); i++)
            {
                text = replaceCore(text, oldSequences[i], newSequences[i], startIndex, options);
            }

            return text;
        }

        #endregion

        #region ▼ Insert

        #region ► Insert
        /// <summary>
        /// Inserts the specified string at the specified position index in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="toInsert">Sequence characters to insert in source text</param>
        /// <param name="positionIndex">Position in source text to insert sequence character</param>
        /// <returns>Source text with sequence character inserted</returns>
        public static string Insert(string text, string toInsert, int positionIndex)
        {
            var result = insert(text, toInsert, positionIndex, toInsert.Length, false);

            return result.ToString();
        }

        #endregion

        #region ► InsertBeforeFirst
        /// <summary>
        /// Inserts the specified string before the first occurrence of the specified sequence/pattern to match in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="toInsert">Sequence characters to insert in source text</param>
        /// <param name="beforeIt">Before this sequence character</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Source text with sequence character inserted</returns>
        public static string InsertBeforeFirst(string text, string toInsert, string beforeIt, params byte[] options)
        {
            int position = 0;
            ReadOnlySpan<char> textSpan = text;

            StringAndPosition matchReturn = match(textSpan, beforeIt, 0, 0, 0, options);

            position = matchReturn.Position - 1;

            if (position == -1)
            { return textSpan.ToString(); }

            textSpan = insert(textSpan, toInsert, position, toInsert.Length, false).ToString();

            return textSpan.ToString();
        }

        #endregion

        #region ► InsertBefore
        /// <summary>
        /// Inserts the specified string before the first occurrence of the specified sequence/pattern to match in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="toInsert">Sequence characters to insert in source text</param>
        /// <param name="beforeIt">Before this sequence character</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Source text with sequence character inserted</returns>
        public static string InsertBefore(string text, string toInsert, string beforeIt, params byte[] options)
        {
            ReadOnlySpan<char> textStack = text;
            int pos = 0;

            for (; pos < textStack.Length; pos++)
            {
                StringAndPosition matchReturn = match(textStack, beforeIt, pos, 0, 0, options);
                pos = matchReturn.Position;

                if (pos == -1)
                { return textStack.ToString(); }

                textStack = insert(textStack, toInsert, pos, toInsert.Length, false).ToString();
                pos += toInsert.Length + beforeIt.Length;
            }

            return textStack.ToString();
        }

        #endregion

        #region ► InsertAfterFirst
        /// <summary>
        /// Inserts the specified string after the first occurrence of the specified sequence/pattern to match in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="toInsert">Sequence characters to insert in source text</param>
        /// <param name="afterIt">After this sequence character</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Source text with sequence character inserted</returns>
        public static string InsertAfterFirst(string text, string toInsert, string afterIt, params byte[] options)
        {
            StringAndPosition matchReturn = Match(text, afterIt, options);

            if (matchReturn.Position == -1)
            { return text; }

            int position = matchReturn.Position + matchReturn.Text.Length;
            ReadOnlySpan<char> result = insert(text, toInsert, position, toInsert.Length, false);

            return result.ToString();
        }

        #endregion

        #region ► InsertAfter
        /// <summary>
        /// Inserts the specified string after the first occurrence of the specified sequence/pattern to match in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="toInsert">Sequence characters to insert in source text</param>
        /// <param name="afterIt">After this sequence character</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Source text with sequence character inserted</returns>
        public static string InsertAfter(string text, string toInsert, string afterIt, params byte[] options)
        {
            ReadOnlySpan<char> textStack = text;
            int pos = 0;

            for (; pos < textStack.Length; pos++)
            {
                StringAndPosition matchReturn = match(textStack, afterIt, pos, 0, 0, options);
                pos = matchReturn.Position + matchReturn.Text.Length;

                if (pos == -1)
                { return textStack.ToString(); }

                textStack = insert(textStack, toInsert, pos, toInsert.Length, false).ToString();
                pos += toInsert.Length;
            }

            return textStack.ToString();
        }

        #endregion

        #region ► Model
        /// <summary>
        /// Model to insert the specified string at the specified position index in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="toInsert">insert sequence character in source text</param>
        /// <param name="position">Position where insert sequence character</param>
        /// <param name="len">Len of characters to remove staring of position in source text</param>
        /// <param name="replace">Replace to this sequence of characters</param>
        /// <returns></returns>
        private static ReadOnlySpan<char> insert(ReadOnlySpan<char> text,
                                                 ReadOnlySpan<char> toInsert,
                                                 int position,
                                                 int len,
                                                 bool replace)
        {
            int _len = ( text.Length + toInsert.Length ) + 2;

            Span<char> replacement = new char[_len];

            text[..position].CopyTo(replacement);
            toInsert.CopyTo(replacement[position..]);

            if (replace) { _len = position + len; }
            else { _len = position; }

            position += toInsert.Length;

            text[_len..].CopyTo(replacement[position..]);

            return replacement.TrimEnd('\0');
        }

        #endregion

        #endregion

        #region ▼ Remove

        #region ► RemoveFirst
        /// <summary>
        /// Removes the first occurrence of the specified sequence/pattern to match in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="sequenceToMatch">Sequence of characters to match in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Source text without sequence characters matched</returns>
        public static string RemoveFirst(string text, string sequenceToMatch, params byte[] options)
        {
            return RemoveFirst(text, sequenceToMatch, 0, options);
        }

        /// <summary>
        /// Removes the first occurrence of the specified sequence/pattern to match in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="sequenceToMatch">Sequence of characters to match in source text</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Source text without sequence characters matched</returns>
        public static string RemoveFirst(string text, string sequenceToMatch, int startIndex, params byte[] options)
        {
            ReadOnlySpan<char> _text = text;
            ReadOnlySpan<char> _sequenceToMatch = sequenceToMatch;
            ReadOnlySpan<char> result = text;

            StringAndPosition matchReturn = match(result, sequenceToMatch, startIndex,0, 0, options);

            if(matchReturn.Position == -1)
            {
                return text; // No match found, return original text
            }

            result = insert(text, ReadOnlySpan<char>.Empty, matchReturn.Position, matchReturn.Text.Length, true);

            return result.ToString();
        }

        #endregion

        #region ► RemoveLast
        /// <summary>
        /// Removes the last occurrence of the specified sequence/pattern to match in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="sequenceToMatch">Sequence of characters to match in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Source text without sequence characters matched</returns>
        public static string RemoveLast(string text, string sequenceToMatch, params byte[] options)
        {
            return RemoveLast(text, sequenceToMatch, 0, options);
        }

        /// <summary>
        /// Removes the last occurrence of the specified sequence/pattern to match in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="sequenceToMatch">Sequence of characters to match in source text</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Source text without sequence characters matched</returns>
        public static string RemoveLast(string text, string sequenceToMatch, int startIndex, params byte[] options)
        {
            ReadOnlySpan<char> _text = text;
            ReadOnlySpan<char> _sequenceToMatch = sequenceToMatch;
            ReadOnlySpan<char> result = text;
            int position = 0;
            int occurPos = -1;
            int len = 0;

            while (position != -1)
            {
                StringAndPosition matchReturn = match(result, sequenceToMatch, startIndex, 0, 0, options);

                if (matchReturn.Position == -1 && occurPos ==-1) { return text; }
                                
                if (matchReturn.Position == -1) { break; }

                occurPos = matchReturn.Position;
                len = matchReturn.Text.Length;
                startIndex = occurPos + 1;
            }

            result = insert(text, ReadOnlySpan<char>.Empty, occurPos, len, true);

            return result.ToString();
        }

        #endregion

        #region ► Remove
        /// <summary>
        /// Removes all occurrences of the specified sequence/pattern to match in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="sequenceToMatch">Sequence of characters to match in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Source text without sequence characters matched</returns>
        public static string Remove(string text, string sequenceToMatch, params byte[] options)
        {
            return Remove(text, sequenceToMatch, 0, options);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="sequenceToMatch">Sequence of characters to match in source text</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Source text without sequence characters matched</returns>
        public static string Remove(string text, string sequenceToMatch, int startIndex, params byte[] options)
        {
            ReadOnlySpan<char> _text = text;
            ReadOnlySpan<char> _sequenceToMatch = sequenceToMatch;
            ReadOnlySpan<char> result = text;

            for(; startIndex< result.Length; startIndex++ )
            {
                StringAndPosition matchReturn = match(result, sequenceToMatch, startIndex,0,0, options);

                if (matchReturn.Position == -1) { break; }

                result = insert(result, ReadOnlySpan<char>.Empty, matchReturn.Position, matchReturn.Text.Length, true);
                startIndex = matchReturn.Position + 1; // Move start index to next position after the removed text
            }

            return result.ToString();
        }

        #endregion

        #endregion

        #region ▼ Split

        /// <summary>
        /// Splits the text into an array of substrings at the positions defined by the specified separator character,
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="separator">Separator character</param>
        /// <param name="ignoreChar">Tupla with open tag and close tag to ignore snippet of text</param>
        /// <returns>Array with splited parts of text</returns>
        public static string[] Split(string text, char separator)
        {
            return split(text, separator, 0, ('\0', '\0'));
        }

        /// <summary>
        /// Splits the text into an array of substrings at the positions defined by the specified separator character,
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="separator">Separator character</param>
        /// <param name="ignoreChar">Tupla with open tag and close tag to ignore snippet of text</param>
        /// <returns>Array with splited parts of text</returns>
        public static string[] Split(string text, char separator, (char open, char close)ignoreChar, bool removeIgnoreChar = false)
        {
            return split(text, separator,0, ignoreChar);
        }

        /// <summary>
        /// Splits the text into an array of substrings at the positions defined by the specified separator character,
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="separator">Separator character</param>
        /// <param name="startIndex">Start position in text</param>
        /// <param name="ignoreChar">Tupla with open tag and close tag to ignore snippet of text</param>
        /// <returns>Array with splited parts of text</returns>
        public static string[] Split(string text, char separator, int startIndex, (char open, char close) ignoreChar, bool removeIgnoreChar = false)
        {
            return split(text, separator, startIndex, ignoreChar, removeIgnoreChar);
        }

        /// <summary>
        /// Splits the text into an array of substrings at the positions defined by the specified separator character,
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="separator">Separator character</param>
        /// <param name="startIndex">Start position in text</param>
        /// <param name="ignoreChar">Tupla with open tag and close tag to ignore snippet of text</param>
        /// <returns>Array with splited parts of text</returns>
        private static string[] split(ReadOnlySpan<char> text, char separator, int startIndex, (char open, char close)ignoreChar, bool removeIgnoreChar = false)
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
                    if( removeIgnoreChar && ignoreChar.open !='\0')
                    {
                        int len = pos - start;
                        if (text[start] == ignoreChar.open && removeIgnoreChar)
                        { start++; len = len - 2; }
                        
                        splited.Add(text.Slice(start, len).ToString().Replace(ignoreChar.open, '\0'));
                    }
                    else
                    {
                        // Add the substring from start to pos
                        splited.Add(text.Slice(start, pos - start).ToString());
                    }
                }

                if (pos < text.Length) pos++;
            }

            return splited.ToArray();
        }

        #endregion

        #region ▼ Contains
        /// <summary>
        /// Contains the specified sequence/pattern to match in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="SequenceToMatch">Sequence to match in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>True or false if found or not sequence of character</returns>
        public static bool Contains(string text, string SequenceToMatch, params byte[] options)
        {
            StringAndPosition matchReturn = Match(text, SequenceToMatch, options);

            return matchReturn.Position !=-1;
        }

        /// <summary>
        /// Contains the specified sequence/pattern to match in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="SequenceToMatch">Sequence to match in source text</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>True or false if found or not sequence of character</returns>
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

        /// <summary>
        /// Match a snippet in the source text with open and close tags.
        /// </summary>
        /// <param name="sourceText">Source text</param>
        /// <param name="snippetTags">Open snippet and close snippet sequence characters.
        /// <param name="options">TextBuilde.Params option</param>
        /// <returns>Matched snippet of characters</returns>
        public static StringAndPosition Snippet(string sourceText, (string open, string close) snippetTags, params byte[] options)
        {
            return Snippet(sourceText, snippetTags, 0, options);
        }

        /// <summary>
        /// Match a snippet in the source text with open and close tags.
        /// </summary>
        /// <param name="sourceText">Source text</param>
        /// <param name="snippetTags">Open snippet and close snippet sequence characters.
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilde.Params option</param>
        /// <returns>Matched snippet of characters</returns>
        public static StringAndPosition Snippet(string sourceText, (string open, string close) snippetTags, int startIndex, params byte[] options)
        {
            return snippet(sourceText, snippetTags.open, snippetTags.close, startIndex, options);
        }

        #endregion

        #region ► Controller
        /// <summary>
        /// Match a snippet in the source text with open and close tags.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="snippetOpen">Open snippet tag sequence character</param>
        /// <param name="snippetClose">Close snippet tag sequence character</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Matched snippet of characters</returns>
        private static StringAndPosition snippet(ReadOnlySpan<char> text, ReadOnlySpan<char> snippetOpen, ReadOnlySpan<char> snippetClose, int startIndex, params byte[] options)
        {
            if ( (snippetOpen.Length == 0 || snippetClose.Length==0) && text == Span<char>.Empty) { return default; }

            (int position, int length) = snippetCore(text, snippetOpen, snippetClose, startIndex, options);

            if (position == -1 || length == 0)
            { return new StringAndPosition(); }

            return new StringAndPosition(text.Slice(position, length).ToString(), position);
        }

        #endregion

        #region ► Model
        /// <summary>
        /// Match a snippet in the source text with open and close tags.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="openTag">Snippet open tag</param>
        /// <param name="closeTag">Snippet close tag</param>
        /// <param name="snippetID">Snippet ID to identify snippet</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Matched snippet of characters</returns>
        /// <exception cref="Exception">Invalid snippet pattern, if not found wildcard in snippet tags</exception>
        /// <exception cref="InvalidCastException"></exception>
        private static (int, int) snippetCore(ReadOnlySpan<char> text, ReadOnlySpan<char> openTag, ReadOnlySpan<char> closeTag, int startIndex, params byte[] options)
        {
            #region ** Exception : The snippet not contain a '*' character to split open and close tag.

            if (openTag.Length == 0 || closeTag.Length ==0)
            { throw new Exception("!Open or/and close snippet empty!"); }

            #endregion

            #region ++ Flags e variables

            int openPos = -1;
            int openLen = 0;
            int closePos = -1;
            int closeLen = 0;
            int openStartIndex = startIndex;
            int closeStartIndex = startIndex;
            bool onlyOpenTag = false;

            int openTagCount = 0;
            int closeTagCount = 0;
            int returnPos = -1;
            int returnLen = 0;

            #endregion

            for (int pos = startIndex; pos < text.Length; pos++)
            {
                #region + Search for open and close tag in text and return only the first occurrence of first tag. 

                #region + Search for open or close tag in text

                StringAndPosition returnOpen = match(text, openTag, openStartIndex, 0, 0, options);
                openPos = returnOpen.Position;

                if ( openPos == -1 && openTagCount ==0 ) { break; }

                openLen = returnOpen.Text.Length;
                if (openTagCount == 0) { closeStartIndex = openPos + openLen; }

                #region + If open tag contains a wildcard, remove it from the tag.

                if (!onlyOpenTag)
                {
                    int wildcardIndex = openTag.IndexOf('*');
                    if (wildcardIndex != -1)
                    {
                        ReadOnlySpan<char> openTagWithoutWildcard = openTag.Slice(0, wildcardIndex);
                        openTag = openTagWithoutWildcard; onlyOpenTag = true;
                    }
                }

                #endregion

                StringAndPosition returnClose = match(text, closeTag, closeStartIndex, 0, 0, options);
                closePos = returnClose.Position;

                if (closePos == -1 && closeTagCount == 0) { break; }

                closeLen = returnClose.Text.Length;
                
                #endregion

                #region ** Exception : If not found open or close tag, return -1 and 0.

                if (openPos == -1 && closePos == -1)
                { break; }

                #endregion

                #endregion

                if ((uint)openPos < (uint)closePos)
                {
                    #region + Increment open tag counter

                    openTagCount++;

                    if (returnPos == -1) { returnPos = openPos; }
                    openStartIndex = openPos + openLen;
                                        
                    #endregion
                }
                else
                {
                    #region + Increment close tag counter

                    closeTagCount++;
                    returnLen = (closePos - returnPos) + closeLen;
                    closeStartIndex = closePos + closeLen;

                    #endregion
                }

                #region + Mark the position of first open tag and calculate the length of snippet when all tags are closed.
                if ((openPos == -1 && closePos == -1) || openTagCount == closeTagCount)
                { break; }
                #endregion
            }

            // If not found open tag, return -1 and 0.
            return (returnPos, returnLen);
        }

        #endregion

        #region ▼ Replace

        #region ► ReplaceFirst

        /// <summary>
        /// Replaces the first specified snippet in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="snippetTags">Open snippet and close snippet sequence characters.
        /// <param name="toRepalce">Replace matched snippet to this</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>First snippet matched by open and close tags</returns>
        public static string SinippetReplaceFirst(string text, (string open, string close) snippetTags, string toRepalce, params byte[] options)
        {
            return SnippetReplaceFirst(text, snippetTags, toRepalce, 0, options);
        }

        /// <summary>
        /// Replaces the first specified snippet in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="snippetTags">Open snippet and close snippet sequence characters.
        /// <param name="toRepalce">Replace matched snippet to this</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>First snippet matched by open and close tags</returns>
        public static string SinippetReplaceFirst(string text, (string open, string close) snippetTags, string toRepalce, int startIndex, params byte[] options)
        {
            return SnippetReplaceFirst(text, snippetTags, toRepalce, startIndex, options);
        }

        /// <summary>
        /// Replaces the first specified snippet in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="snippetTags">Open snippet and close snippet sequence characters.
        /// <param name="toRepalce">Replace matched snippet to this</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>First snippet matched by open and close tags</returns>
        public static string SnippetReplaceFirst(string text, (string open, string close) snippetTags, string toRepalce, int startIndex, params byte[] options)
        {
            StringAndPosition matchReturn = Snippet(text, snippetTags, startIndex, options);

            if (matchReturn.Position == -1) { return ""; }

            ReadOnlySpan<char> result = insert(text, toRepalce, matchReturn.Position, matchReturn.Text.Length, true);

            return result.ToString();
        }

        #endregion

        #region ► ReplaceLast
        /// <summary>
        /// Replaces the last specified snippet in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="openAndCloseTags">Open and close sequence characters slipted by '*' wildcard.
        /// <para>Only use one wildcard and only one open tag and one close tag.</para></param>
        /// <param name="toRepalce">Replace matched snippet to this</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>First snippet matched by open and close tags</returns>
        public static string ReplaceSnippetLast(string text, ( string open, string close) snippetTags, string toRepalce, params byte[] options)
        {
            return ReplaceSnippetLast(text, snippetTags, toRepalce, 0, options);
        }

        /// <summary>
        /// Replaces the last specified snippet in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="snippetTags">Open snippet and close snippet sequence characters.
        /// <param name="toRepalce">Replace matched snippet to this</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>First snippet matched by open and close tags</returns>
        public static string ReplaceSnippetLast(string text, (string open, string close) snippetTags, string toRepalce, int startIndex, params byte[] options)
        {
            ReadOnlySpan<char> result = default;
            int occurPos = 0;
            int pos = startIndex;
            int len = 0;

            while (pos != -1)
            {
                StringAndPosition matchReturn = Snippet(text, snippetTags, pos, options);
                pos = matchReturn.Position;

                if (pos != -1)
                { occurPos = matchReturn.Position; pos++; len = matchReturn.Text.Length; }
            }

            result = insert(text, toRepalce, occurPos, len, true);

            return result.ToString();
        }

        #endregion

        #region ► Replace
        /// <summary>
        /// Replaces all snippets equal a specified snippet in the text.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="openAndCloseTags"></param>
        /// <param name="toRepalce">Replace matched snippet to this</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>First snippet matched by open and close tags</returns>
        public static string ReplaceSnippet(string text, (string open, string close) snippetTags, string toRepalce, params byte[] options)
        {
            return ReplaceSnippet(text, snippetTags, "", toRepalce, 0, options);
        }

        /// <summary>
        /// Replaces all snippets equal a specified snippet in the text.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="openAndCloseTags"></param>
        /// <param name="snippetID">Sequence of character with snippet id to identify respective snippet in source text
        /// <para>The id snippet must be after open tag and before close tag.</para>
        /// <para>If there is others open tag inside snippet(after open father), the snippet id must be before it</para></param>
        /// <param name="toRepalce">Replace matched snippet to this</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>First snippet matched by open and close tags</returns>
        public static string ReplaceSnippet(string text, ( string open, string close) snippetTags, string snippetID, string toReplace, int startIndex, params byte[] options)
        {
            ReadOnlySpan<char> _text = text;
            ReadOnlySpan<char> _toReplace = toReplace;
            ReadOnlySpan<char> result = text;
            int pos = startIndex;
            int len = 0;

            for(; pos < text.Length; pos++)
            {
                StringAndPosition matchReturn = snippet(result, snippetTags.open, snippetTags.close, pos, options);                
                pos = matchReturn.Position;

                if (pos == -1) { break; }

                len = matchReturn.Text.Length;
                result = insert( result, toReplace, pos, len, true );

                //pos += len - 1;
            }

            return result.ToString();
        }

        #endregion

        #endregion

        #region ▼ Insert       

        #region ► InsertBefore

        /// <summary>
        /// Inserts the specified snippet before the first snippet occurrence in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="toInsert">Sequence of character to insert in source text</param>
        /// <param name="beforeSnippet">Sequence of character. Insert 'toInsert' before this</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Source text with sequence character inserted</returns>
        public static string InsertSnippetBefore(string text, string toInsert, (string open, string close) beforeSnippet, int startIndex=0, params byte[] options)
        {
            int pos = startIndex;
            int len = 0;
            ReadOnlySpan<char> result = text;

            while(pos !=-1)
            {
                StringAndPosition matchReturn = snippet(result, beforeSnippet.open, beforeSnippet.close, pos, options);
                pos = matchReturn.Position;
                len = matchReturn.Text.Length;

                if (pos == -1) { return result.ToString(); } // No match found, return original text

                result = insert(result, toInsert, pos, len, false);

                pos += len - 1; // Move position after the inserted text
            }

            return result.ToString();
        }

        #endregion

        #region ► InsertAfter

        /// <summary>
        /// Inserts the specified snippet after the first snippet occurrence in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="toInsert">Sequence of character to insert in source text</param>
        /// <param name="afterIt">Sequence of character. Insert 'toInsert' after this</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Source text with sequence character inserted</returns>
        public static string InsertSnippetAfter(string text, string toInsert, (string open, string close) afterSnippet, int startIndex=0, params byte[] options)
        {
            int pos = startIndex;
            int len = 0;
            ReadOnlySpan<char> result = text;

            while (pos != -1)
            {
                StringAndPosition matchReturn = snippet(result, afterSnippet.open, afterSnippet.close, pos, options);
                len = matchReturn.Text.Length;
                pos = matchReturn.Position + len;

                if (pos == -1) { return result.ToString(); } // No match found, return original text

                result = insert(result, toInsert, pos, toInsert.Length, false);

                pos += len - 1; // Move position after the inserted text
            }

            return result.ToString();
        }

        #endregion

        #endregion

        #region ▼ Remove

        #region ► RemoveFirst

        /// <summary>
        /// Removes the first specified snippet of the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="toRemoveSnippet">Open and close sequence of characters from snippet to remove in source text</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Source text with sequence character removed</returns>
        public static string RemoveSnippetFirst(string text, (string open, string close) toRemoveSnippet, int startIndex = 0, params byte[] options)
        {
            int pos = startIndex;
            int len = 0;
            ReadOnlySpan<char> result = text;

            StringAndPosition matchReturn = snippet(result, toRemoveSnippet.open, toRemoveSnippet.close, pos, options);
            pos = matchReturn.Position;
            len = matchReturn.Text.Length;

            if (pos == -1) { return result.ToString(); } // No match found, return original text

            result = insert(result, "", pos, len, true);

            pos += len - 1; // Move position after the inserted text

            return result.ToString();
        }

        #endregion

        #region ► RemoveLast

        /// <summary>
        /// Removes the last specified snippet of the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="toRemoveSnippet">Open and close sequence of characters from snippet to remove in source text</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Source text with sequence character removed</returns>
        public static string RemoveSnippetLast(string text, (string open, string close) toRemoveSnippet, int startIndex = 0, params byte[] options)
        {
            int pos = startIndex;
            int len = 0;
            int occurPos = -1;
            int occurLen = 0;
            ReadOnlySpan<char> result = text;

            for (; pos < result.Length; pos++)
            {
                StringAndPosition matchReturn = snippet(result, toRemoveSnippet.open, toRemoveSnippet.close, pos, options);
                pos = matchReturn.Position;
                len = matchReturn.Text.Length;
                
                if (pos == -1) { break; } // No match found, return original text

                occurPos = pos;
                occurLen = len;
                pos += len - 1; // Move position after the inserted text
            }

            if (occurPos != -1)
            { result = insert(result, "", occurPos, occurLen, true); }

            return result.ToString();
        }

        #endregion

        #region ► Remove

        /// <summary>
        /// Removes the specified snippet of the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="toRemoveSnippet">Open and close tupla sequence of characters from snippet to remove in source text</param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>Source text with sequence character removed</returns>
        public static string RemoveSnippet(string text, ( string open, string close) toRemoveSnippet, int startIndex=0, params byte[] options)
        {
            int pos = startIndex;
            int len = 0;
            ReadOnlySpan<char> result = text;

            while (pos != -1)
            {
                StringAndPosition matchReturn = snippet(result, toRemoveSnippet.open, toRemoveSnippet.close, pos, options);
                pos = matchReturn.Position;
                len = matchReturn.Text.Length;

                if (pos == -1) { return result.ToString(); } // No match found, return original text

                result = insert(result, "", pos, len, true);

                pos += len - 1; // Move position after the inserted text
            }

            return result.ToString();
        }

        #endregion
               
        #endregion

        #region ▼ Contains

        /// <summary>
        /// Contains the specified snippet in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="openAndCloseTags">Open and close sequence characters slipted by '*' wildcard.
        /// <para>Only use one wildcard and only one open tag and one close tag.</para></param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>True, if contains snippet in source text and false if not contains</returns>
        public static bool ContainsSnippet(string text, (string open, string close) snippetTags, params byte[] options)
        {
            return ContainsSnippet(text, snippetTags, "", 0, options);
        }

        /// <summary>
        /// Contains the specified snippet in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="openAndCloseTags">Open and close sequence characters slipted by '*' wildcard.
        /// <para>Only use one wildcard and only one open tag and one close tag.</para></param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>True, if contains snippet in source text and false if not contains</returns>
        public static bool ContainsSnippet(string text, (string open, string close) snippetTags, int startIndex, params byte[] options)
        {
            return ContainsSnippet(text, snippetTags, "", startIndex, options);
        }

        /// <summary>
        /// Contains the specified snippet in the text.
        /// </summary>
        /// <param name="text">Source text</param>
        /// <param name="openAndCloseTags">Open and close sequence characters slipted by '*' wildcard.
        /// <para>Only use one wildcard and only one open tag and one close tag.</para></param>
        /// <param name="snippetID">Sequence of character with snippet id to identify respective snippet in source text
        /// <para>The id snippet must be after open tag and before close tag.</para>
        /// <para>If there is others open tag inside snippet(after open father), the snippet id must be before it</para></param>
        /// <param name="startIndex">Start position in source text</param>
        /// <param name="options">TextBuilder.Params options</param>
        /// <returns>True, if contains snippet in source text and false if not contains</returns>
        public static bool ContainsSnippet(string text, (string open, string close) snippetTags, string snippetID, int startIndex, params byte[] options)
        {                        
            StringAndPosition matchReturn = Snippet(text, snippetTags, startIndex, options);  
            return matchReturn.Position !=-1;
        }

        #endregion

        #endregion
    }
}
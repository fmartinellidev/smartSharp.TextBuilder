using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

    #region ▼ TextOptions
    /// <summary>
    /// Options to TextBuilder parameters
    /// </summary>
    public static class TextOpt
    {
        /// <summary>
        /// Consider upper or lower case.
        /// </summary>
        public static byte CaseSensitive = 1;

        /// <summary>
        /// Ignore content in single quotes '' when parsing text.
        /// </summary>
        public static byte IgnoreCharsInQuotes = 2;

        /// <summary>
        /// Ignore content in double quotes "" when parsing text.
        /// </summary>
        public static byte IgnoreCharsInDoubleQuotes = 3;

        /// <summary>
        /// Identify dynamic chars of pattern in text and in pattern.
        /// </summary>
        public static byte IgnoreDynamicChars = 4;

        /// <summary>
        /// Do not force search in text by nearest words to return the shortest occurrence.
        /// </summary>
        public static byte MatchGreedyOccurences = 5;

        /// <summary>
        /// Return occurences with only whole word.
        /// </summary>
        public static byte MatchWholeWordOnly = 6;
    }
    #endregion

    #region ▼ TextBuilder

    /// <summary>
    /// Provides advanced text manipulation and pattern-matching utilities.
    /// </summary>
    /// <remarks>
    /// The <see cref="TextMatcher"/> class enables complex text match and edit operations such as wildcard matching,
    /// case-insensitive comparisons, dynamic character handling, and snippet-based matches and edit tools.
    /// Designed for high-precision scenarios including parsing, filtering, and structured text processing.
    /// </remarks>
    public static class TextBuilder
    {
        #region ▼ Matcher

        #region » Enable Match params

        private static void enableMatchParams(ref TextMatcher builder, params byte[] options)
        {
            if (options.Contains(TextOpt.CaseSensitive)) { builder.CaseSensitive = true; }
            if (options.Contains(TextOpt.IgnoreCharsInQuotes)) { builder.IgnoreCharsInQuotes = false; }
            if (options.Contains(TextOpt.IgnoreCharsInDoubleQuotes)) { builder.IgnoreCharsInDoubleQuotes = false; }
            if (options.Contains(TextOpt.IgnoreDynamicChars)) { builder.IgnoreDynamicChars = false; }
            if (options.Contains(TextOpt.MatchGreedyOccurences)) { builder.MatchGreedyOccurences = false; }
            if (options.Contains(TextOpt.MatchWholeWordOnly)) { builder.MatchWholeWordOnly = true; }
        }

        #endregion

        #region ▼ Word Matches

        /// <summary>
        /// Matches a character sequence in the text using literal words, wildcard patterns, and dynamic characters.
        /// </summary>
        /// <remarks>
        /// The <see cref="Match"/> method supports matching string occurrences using wildcard patterns,
        /// case-insensitive comparisons, dynamic character handling, and snippet-based logic.
        /// Designed for high-precision scenarios including literal matches, wildcard-based patterns, and dynamic character sequences.
        /// </remarks>
        /// <param name="text">The source text to search within.</param>
        /// <param name="sequenceToMatch">The character sequence or pattern to match in the source text.</param>
        /// <returns>A string containing the matched character sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> or <paramref name="sequenceToMatch"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sequenceToMatch"/> is invalid or empty.</exception>
        public static StringAndPosition Match(string text, string sequenceToMatch)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (sequenceToMatch == null) throw new ArgumentNullException(nameof(text));
            if (text.Length == 0) throw new ArgumentException("Empty text!");
            if (sequenceToMatch.Length == 0) throw new ArgumentException("Empty sequenceToMatch!");

            TextMatcher matcher = new TextMatcher(text);
            return matcher.Match(sequenceToMatch);
        }

        /// <summary>
        /// Matches a character sequence in the text using literal words, wildcard patterns, and dynamic characters.
        /// </summary>
        /// <remarks>
        /// The <see cref="Match"/> method supports matching string occurrences using wildcard patterns,
        /// case-insensitive comparisons, dynamic character handling, and snippet-based logic.
        /// Designed for high-precision scenarios including literal matches, wildcard-based patterns, and dynamic character sequences.
        /// </remarks>
        /// <param name="text">The source text to search within.</param>
        /// <param name="sequenceToMatch">The character sequence or pattern to match in the source text.</param>
        /// <param name="options">Parameters of matches methods</param>
        /// <returns>A string containing the matched character sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> or <paramref name="sequenceToMatch"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sequenceToMatch"/> is invalid or empty.</exception>
        public static StringAndPosition Match(string text, string sequenceToMatch, params byte[] options)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (sequenceToMatch == null) throw new ArgumentNullException(nameof(text));
            if (text.Length == 0) throw new ArgumentException("Empty text!");
            if (sequenceToMatch.Length == 0) throw new ArgumentException("Empty sequenceToMatch!");

            TextMatcher matcher = new TextMatcher(text);
            enableMatchParams(ref matcher, options);
            return matcher.Match(sequenceToMatch);
        }

        /// <summary>
        /// Matches a character sequence in the text using literal words, wildcard patterns, and dynamic characters.
        /// </summary>
        /// <remarks>
        /// The <see cref="Match"/> method supports matching string occurrences using wildcard patterns,
        /// case-insensitive comparisons, dynamic character handling, and snippet-based logic.
        /// Designed for high-precision scenarios including literal matches, wildcard-based patterns, and dynamic character sequences.
        /// </remarks>
        /// <param name="text">The source text to search within.</param>
        /// <param name="sequenceToMatch">The character sequence or pattern to match in the source text.</param>
        /// <param name="startIndex">Start index position in text to begin match.</param>
        /// <param name="options">Parameters of matches methods</param>
        /// <returns>A string containing the matched character sequence.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="text"/> or <paramref name="sequenceToMatch"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="sequenceToMatch"/> is invalid or empty.</exception>
        public static StringAndPosition Match(string text, string sequenceToMatch, int startIndex, params byte[] options)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (sequenceToMatch == null) throw new ArgumentNullException(nameof(text));
            if (text.Length == 0) throw new ArgumentException("Empty text!");
            if (sequenceToMatch.Length == 0) throw new ArgumentException("Empty sequenceToMatch!");

            TextMatcher matcher = new TextMatcher(text);
            enableMatchParams(ref matcher, options);
            matcher.StartIndex = startIndex;
            return matcher.Match(sequenceToMatch);
        }

        #endregion

        #region ► Contains word
        /// <summary>
        /// Determines whether the source text contains a character sequence, supporting wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="text">The source text to search within.</param>
        /// <param name="sequenceToMatch">The character sequence or pattern to match in the source text.</param>
        /// <returns><c>true</c> if the sequence is found; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/> or <paramref name="sequenceToMatch"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="text"/> or <paramref name="sequenceToMatch"/> is empty.
        /// </exception>

        public static bool Contains(string text, string sequenceToMatch)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (sequenceToMatch == null) throw new ArgumentNullException(nameof(text));
            if (text.Length == 0) throw new ArgumentException("Empty text!");
            if (sequenceToMatch.Length == 0) throw new ArgumentException("Empty sequenceToMatch!");

            TextMatcher matcher = new TextMatcher(text);
            return matcher.Contains(sequenceToMatch);
        }

        /// <summary>
        /// Determines whether the source text contains a character sequence, supporting wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="text">The source text to search within.</param>
        /// <param name="sequenceToMatch">The character sequence or pattern to match in the source text.</param>
        /// <param name="Options">Parameters "TextOpt" used in the match methods.</param>
        /// <returns><c>true</c> if the sequence is found; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/> or <paramref name="sequenceToMatch"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="text"/> or <paramref name="sequenceToMatch"/> is empty.
        /// </exception>
        public static bool Contains(string text, string sequenceToMatch, params byte[] Options)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (sequenceToMatch == null) throw new ArgumentNullException(nameof(text));
            if (text.Length == 0) throw new ArgumentException("Empty text!");
            if (sequenceToMatch.Length == 0) throw new ArgumentException("Empty sequenceToMatch!");

            TextMatcher matcher = new TextMatcher(text);
            enableMatchParams(ref matcher, Options);
            return matcher.Contains(sequenceToMatch);
        }

        #endregion

        #region ► Cont words

        /// <summary>
        /// Counts the number of matches found in the source text, supporting wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="text">The source text to search within.</param>
        /// <param name="sequenceToMatch">The character sequence or pattern to match in the source text.</param>
        /// <returns>The number of matches found.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/> or <paramref name="sequenceToMatch"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="text"/> or <paramref name="sequenceToMatch"/> is empty.
        /// </exception>
        public static int Cont(string text, string sequenceToMatch)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (sequenceToMatch == null) throw new ArgumentNullException(nameof(text));
            if (text.Length == 0) throw new ArgumentException("Empty text!");
            if (sequenceToMatch.Length == 0) throw new ArgumentException("Empty sequenceToMatch!");

            TextMatcher matcher = new TextMatcher(text);
            return matcher.Cont(sequenceToMatch);
        }

        /// <summary>
        /// Counts the number of matches found in the source text, supporting wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="text">The source text to search within.</param>
        /// <param name="sequenceToMatch">The character sequence or pattern to match in the source text.</param>
        /// <param name="Options">Parameters of matches methods</param>
        /// <returns>The number of matches found.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/> or <paramref name="sequenceToMatch"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="text"/> or <paramref name="sequenceToMatch"/> is empty.
        /// </exception>
        public static int Cont(string text, string sequenceToMatch, params byte[] Options)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (sequenceToMatch == null) throw new ArgumentNullException(nameof(text));
            if (text.Length == 0) throw new ArgumentException("Empty text!");
            if (sequenceToMatch.Length == 0) throw new ArgumentException("Empty sequenceToMatch!");

            TextMatcher matcher = new TextMatcher(text);
            enableMatchParams(ref matcher, Options);
            return matcher.Cont(sequenceToMatch);
        }

        #endregion

        #region ▼ Snippet Matches
        /// <summary>
        /// Matches a snippet from the source text using specified open and close tag sequences.
        /// Supports wildcard patterns, dynamic characters, and hierarchical pattern recognition.
        /// </summary>
        /// <remarks>
        /// This method identifies snippets delimited by <paramref name="openTag"/> and <paramref name="closeTag"/>.
        /// It supports nested (child) snippets within parent snippets and correctly distinguishes between them.
        /// Even if a child snippet contains a similar close tag, the matcher will ignore it and match the correct closing tag for the parent snippet.
        /// </remarks>
        /// <param name="text">The source text to search within.</param>
        /// <param name="openTag">The character sequence representing the opening tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="closeTag">The character sequence representing the closing tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <returns>The matched snippet from the source text.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/>, <paramref name="openTag"/>, or <paramref name="closeTag"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="text"/>, <paramref name="openTag"/>, or <paramref name="closeTag"/> is empty.
        /// </exception>
        public static StringAndPosition Snippet(string text, string openTag, string closeTag)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (openTag == null) throw new ArgumentNullException(nameof(text));
            if (closeTag == null) throw new ArgumentNullException(nameof(text));
            if (text.Length == 0) throw new ArgumentException("Empty text!");
            if (openTag.Length == 0) throw new ArgumentException("Empty openTag!");
            if (closeTag.Length == 0) throw new ArgumentException("Empty closeTag!");

            TextMatcher matcher = new TextMatcher(text);
            return matcher.Snippet(openTag, closeTag);
        }

        /// <summary>
        /// Matches a snippet from the source text using specified open and close tag sequences.
        /// Supports wildcard patterns, dynamic characters, and hierarchical pattern recognition.
        /// </summary>
        /// <remarks>
        /// This method identifies snippets delimited by <paramref name="openTag"/> and <paramref name="closeTag"/>.
        /// It supports nested (child) snippets within parent snippets and correctly distinguishes between them.
        /// Even if a child snippet contains a similar close tag, the matcher will ignore it and match the correct closing tag for the parent snippet.
        /// </remarks>
        /// <param name="text">The source text to search within.</param>
        /// <param name="openTag">The character sequence representing the opening tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="closeTag">The character sequence representing the closing tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="options">Parameters of matches methods</param>
        /// <returns>The matched snippet from the source text.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/>, <paramref name="openTag"/>, or <paramref name="closeTag"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="text"/>, <paramref name="openTag"/>, or <paramref name="closeTag"/> is empty.
        /// </exception>
        public static StringAndPosition Snippet(string text, string openTag, string closeTag, params byte[] options)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (openTag == null) throw new ArgumentNullException(nameof(text));
            if (closeTag == null) throw new ArgumentNullException(nameof(text));
            if (text.Length == 0) throw new ArgumentException("Empty text!");
            if (openTag.Length == 0) throw new ArgumentException("Empty openTag!");
            if (closeTag.Length == 0) throw new ArgumentException("Empty closeTag!");

            TextMatcher matcher = new TextMatcher(text);
            enableMatchParams(ref matcher, options);
            return matcher.Snippet(openTag, closeTag);
        }

        /// <summary>
        /// Matches a snippet from the source text using specified open and close tag sequences.
        /// Supports wildcard patterns, dynamic characters, and hierarchical pattern recognition.
        /// </summary>
        /// <remarks>
        /// This method identifies snippets delimited by <paramref name="openTag"/> and <paramref name="closeTag"/>.
        /// It supports nested (child) snippets within parent snippets and correctly distinguishes between them.
        /// Even if a child snippet contains a similar close tag, the matcher will ignore it and match the correct closing tag for the parent snippet.
        /// </remarks>
        /// <param name="text">The source text to search within.</param>
        /// <param name="openTag">The character sequence representing the opening tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="closeTag">The character sequence representing the closing tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="startIndex">Start match by this position in source text</param>
        /// <param name="options">Parameters of matches methods</param>
        /// <returns>The matched snippet from the source text.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/>, <paramref name="openTag"/>, or <paramref name="closeTag"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="text"/>, <paramref name="openTag"/>, or <paramref name="closeTag"/> is empty.
        /// </exception>
        public static StringAndPosition Snippet(string text, string openTag, string closeTag, int startIndex, params byte[] options)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (openTag == null) throw new ArgumentNullException(nameof(text));
            if (closeTag == null) throw new ArgumentNullException(nameof(text));
            if (text.Length == 0) throw new ArgumentException("Empty text!");
            if (openTag.Length == 0) throw new ArgumentException("Empty openTag!");
            if (closeTag.Length == 0) throw new ArgumentException("Empty closeTag!");

            TextMatcher matcher = new TextMatcher(text);
            enableMatchParams(ref matcher, options);
            matcher.StartIndex = startIndex;
            return matcher.Snippet(openTag, closeTag);
        }

        /// <summary>
        /// Determines whether the source text contains a snippet defined by an opening and closing tag sequence.
        /// Supports wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="text">The source text to search within.</param>
        /// <param name="openTag">The character sequence representing the opening tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="closeTag">The character sequence representing the closing tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <returns><c>true</c> if a matching snippet is found; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/>, <paramref name="openTag"/>, or <paramref name="closeTag"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="text"/>, <paramref name="openTag"/>, or <paramref name="closeTag"/> is empty.
        /// </exception>
        public static bool ContainsSnippet(string text, string openTag, string closeTag)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (openTag == null) throw new ArgumentNullException(nameof(text));
            if (closeTag == null) throw new ArgumentNullException(nameof(text));
            if (text.Length == 0) throw new ArgumentException("Empty text!");
            if (openTag.Length == 0) throw new ArgumentException("Empty openTag!");
            if (closeTag.Length == 0) throw new ArgumentException("Empty closeTag!");

            TextMatcher matcher = new TextMatcher(text);
            return matcher.ContainsSnippet(openTag, closeTag,0);
        }

        /// <summary>
        /// Determines whether the source text contains a snippet defined by an opening and closing tag sequence.
        /// Supports wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="text">The source text to search within.</param>
        /// <param name="openTag">The character sequence representing the opening tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="closeTag">The character sequence representing the closing tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="options">Parameters of matches methods.</param>
        /// <returns><c>true</c> if a matching snippet is found; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/>, <paramref name="openTag"/>, or <paramref name="closeTag"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="text"/>, <paramref name="openTag"/>, or <paramref name="closeTag"/> is empty.
        /// </exception>
        public static bool ContainsSnippet(string text, string openTag, string closeTag, params byte[] options)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (openTag == null) throw new ArgumentNullException(nameof(text));
            if (closeTag == null) throw new ArgumentNullException(nameof(text));
            if (text.Length == 0) throw new ArgumentException("Empty text!");
            if (openTag.Length == 0) throw new ArgumentException("Empty openTag!");
            if (closeTag.Length == 0) throw new ArgumentException("Empty closeTag!");

            TextMatcher matcher = new TextMatcher(text);
            enableMatchParams(ref matcher, options);
            return matcher.ContainsSnippet(openTag, closeTag, 0);
        }

        /// <summary>
        /// Counts the number of snippets found in the source text, using specified opening and closing tag sequences.
        /// Supports wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="text">The source text to search within.</param>
        /// <param name="openTag">The character sequence representing the opening tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="closeTag">The character sequence representing the closing tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <returns>The number of matching snippets found in the source text.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/>, <paramref name="openTag"/>, or <paramref name="closeTag"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="text"/>, <paramref name="openTag"/>, or <paramref name="closeTag"/> is empty.
        /// </exception>
        public static int ContSnippets(string text, string openTag, string closeTag, params byte[] options)
        {
            if (text == null) throw new ArgumentNullException(nameof(text));
            if (openTag == null) throw new ArgumentNullException(nameof(text));
            if (closeTag == null) throw new ArgumentNullException(nameof(text));
            if (text.Length == 0) throw new ArgumentException("Empty text!");
            if (openTag.Length == 0) throw new ArgumentException("Empty openTag!");
            if (closeTag.Length == 0) throw new ArgumentException("Empty closeTag!");

            TextMatcher matcher = new TextMatcher(text);
            return matcher.CountSnippets(openTag, closeTag);
        }

        #endregion

        #endregion

        #region ▼ Editor

        #region ▼ Insert words
        /// <summary>
        /// Inserts a sequence of characters into the source text at the specified position.
        /// </summary>
        /// <param name="text">The source text where the sequence will be inserted.</param>
        /// <param name="sequenceToInsert">The sequence of characters to insert.</param>
        /// <param name="position">The zero-based index in the source text where the sequence should be inserted.</param>
        /// <returns>A new string with the inserted sequence.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/> or <paramref name="sequenceToInsert"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="position"/> is less than 0 or greater than the length of <paramref name="text"/>.
        /// </exception>
        public static string Insert(string text, string sequenceToInsert, int position, params byte[] options)
        {
            TextEditor editor = new TextEditor(text);
            return editor.Insert( sequenceToInsert, position);
        }

        /// <summary>
        /// Inserts a sequence of characters into the source text before the first occurrence of a specified sequence.
        /// </summary>
        /// <param name="text">The source text where the insertion will occur.</param>
        /// <param name="sequenceToInsert">The sequence of characters to insert.</param>
        /// <param name="beforeOf">The sequence of characters in the source text before which the insertion should happen.</param>
        /// <param name="options">Parameters of matches methods.</param>
        /// <returns>A new string with the inserted sequence placed before the specified target sequence.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/>, <paramref name="sequenceToInsert"/>, or <paramref name="beforeOf"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="beforeOf"/> is not found in <paramref name="text"/>.
        /// </exception>
        public static string InsertBeforeFirst(string text, string sequenceToInsert, string beforeOf, params byte[] options)
        {
            TextEditor editor = new TextEditor(text);
            return editor.InsertBeforeFirst(sequenceToInsert, beforeOf, options);
        }

        /// <summary>
        /// Inserts a sequence of characters into the source text immediately after the first occurrence of a specified sequence.
        /// </summary>
        /// <param name="text">The source text where the insertion will occur.</param>
        /// <param name="sequenceToInsert">The sequence of characters to insert.</param>
        /// <param name="afterOf">The sequence of characters in the source text after which the insertion should happen.</param>
        /// <param name="options">Parameters of matches methods.</param>
        /// <returns>A new string with the inserted sequence placed after the specified target sequence.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/>, <paramref name="sequenceToInsert"/>, or <paramref name="afterOf"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="afterOf"/> is not found in <paramref name="text"/>.
        /// </exception>
        public static string InsertAfterFirst(string text, string sequenceToInsert, string afterOf, params byte[] options)
        {
            TextEditor editor = new TextEditor(text);
            return editor.InsertAfterFirst(sequenceToInsert, afterOf);
        }

        /// <summary>
        /// Inserts a sequence of characters into the source text before the all occurrence of a specified sequence.
        /// </summary>
        /// <param name="text">The source text where the insertion will occur.</param>
        /// <param name="sequenceToInsert">The sequence of characters to insert.</param>
        /// <param name="beforeOf">The sequence of characters in the source text before which the insertion should happen.</param>
        /// <param name="options">Parameters of matches methods.</param>
        /// <returns>A new string with the inserted sequence placed before the specified target sequence.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/>, <paramref name="sequenceToInsert"/>, or <paramref name="beforeOf"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="beforeOf"/> is not found in <paramref name="text"/>.
        /// </exception>
        public static string InsertBefore(string text, string sequenceToInsert, string beforeOf, params byte[] options)
        {
            TextEditor editor = new TextEditor(text);
            return editor.InsertBefore(sequenceToInsert, beforeOf);
        }

        /// <summary>
        /// Inserts a sequence of characters into the source text immediately after all occurrence of a specified sequence.
        /// </summary>
        /// <param name="text">The source text where the insertion will occur.</param>
        /// <param name="sequenceToInsert">The sequence of characters to insert.</param>
        /// <param name="afterOf">The sequence of characters in the source text after which the insertion should happen.</param>
        /// <param name="options">Parameters of matches methods.</param>
        /// <returns>A new string with the inserted sequence placed after the specified target sequence.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/>, <paramref name="sequenceToInsert"/>, or <paramref name="afterOf"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="afterOf"/> is not found in <paramref name="text"/>.
        /// </exception>
        public static string InsertAfter(string text, string sequenceToInsert, string afterOf, params byte[] options)
        {
            TextEditor editor = new TextEditor(text);
            return editor.InsertAfter(sequenceToInsert, afterOf);
        }

        #endregion

        #region ▼ Remove words
        /// <summary>
        /// Removes occurrences of a character sequence from the source text, supporting wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="text">The source text from which the sequence will be removed.</param>
        /// <param name="sequenceToRemove">The character sequence or pattern to remove. Supports wildcards and dynamic characters.</param>
        /// <param name="options">Optional flags to customize the removal behavior (e.g., case sensitivity, match mode).</param>
        /// <returns>A new string with the matched sequences removed.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/> or <paramref name="sequenceToRemove"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="sequenceToRemove"/> is empty or invalid.
        /// </exception>
        public static string Remove(string text, string sequenceToRemove, params byte[] options)
        {
            TextEditor editor = new TextEditor(text);
            return editor.Remove(sequenceToRemove);
        }

        /// <summary>
        /// Removes first occurrence of a character sequence from the source text, supporting wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="text">The source text from which the sequence will be removed.</param>
        /// <param name="sequenceToRemove">The character sequence or pattern to remove. Supports wildcards and dynamic characters.</param>
        /// <param name="options">Optional flags to customize the removal behavior (e.g., case sensitivity, match mode).</param>
        /// <returns>A new string with the matched sequences removed.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/> or <paramref name="sequenceToRemove"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="sequenceToRemove"/> is empty or invalid.
        /// </exception>
        public static string RemoveFirst(string text, string sequenceToRemove, params byte[] options)
        {
            TextEditor editor = new TextEditor(text);
            return editor.RemoveFirst(sequenceToRemove);
        }

        #endregion

        #region ▼ Replace words
        /// <summary>
        /// Replaces all matched sequence of characters in the source text with a new sequence.
        /// Supports wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="text">The source text where the replacement will occur.</param>
        /// <param name="oldSequence">The character sequence or pattern to be replaced. Supports wildcards and dynamic characters.</param>
        /// <param name="newSequence">The sequence of characters to insert in place of the matched pattern.</param>
        /// <param name="options">Optional flags to customize the replacement behavior (e.g., case sensitivity, match mode).</param>
        /// <returns>A new string with the matched sequences replaced by the specified new sequence.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/> or <paramref name="oldSequence"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="oldSequence"/> is empty or invalid.
        /// </exception>
        public static string Replace(string text, string oldSequence, string newSequence, params byte[] options)
        {
            TextEditor editor = new TextEditor(text);
            return editor.Replace(oldSequence, newSequence);
        }

        /// <summary>
        /// Replaces a first matched sequence of characters in the source text with a new sequence.
        /// Supports wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="text">The source text where the replacement will occur.</param>
        /// <param name="oldSequence">The character sequence or pattern to be replaced. Supports wildcards and dynamic characters.</param>
        /// <param name="newSequence">The sequence of characters to insert in place of the matched pattern.</param>
        /// <param name="options">Optional flags to customize the replacement behavior (e.g., case sensitivity, match mode).</param>
        /// <returns>A new string with the matched sequences replaced by the specified new sequence.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/> or <paramref name="oldSequence"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="oldSequence"/> is empty or invalid.
        /// </exception>
        public static string ReplaceFirst(string text, string oldSequence, string newSequence, params byte[] options)
        {
            TextEditor editor = new TextEditor(text);
            return editor.ReplaceFirst(oldSequence, newSequence);
        }

        #endregion

        #region ▼ Translate words
        /// <summary>
        /// Replaces all words in the source text that match entries from <paramref name="oldSequence"/> with corresponding words from <paramref name="newSequence"/>.
        /// Each sequence is split by semicolons (';'), and replacements are matched by index order.
        /// </summary>
        /// <param name="text">The source text where replacements will be applied.</param>
        /// <param name="oldSequence">A semicolon-separated list of words to be replaced.</param>
        /// <param name="newSequence">A semicolon-separated list of replacement words, matched by index to <paramref name="oldSequence"/>.</param>
        /// <param name="options">Optional flags to customize replacement behavior (e.g., case sensitivity, whole-word match).</param>
        /// <returns>A new string with all specified replacements applied.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/>, <paramref name="oldSequence"/>, or <paramref name="newSequence"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="oldSequence"/> and <paramref name="newSequence"/> have different word counts or contain invalid entries.
        /// </exception>
        public static string Translate(string text, string oldSequence, string newSequence, params byte[] options)
        {
            TextEditor editor = new TextEditor(text);
            return editor.Translate(oldSequence, newSequence);
        }

        /// <summary>
        /// Replaces the first occurrence in the source text of each entry from <paramref name="oldSequence"/> 
        /// with the corresponding word from <paramref name="newSequence"/>. 
        /// Each sequence is split by semicolons (';'), and replacements are matched by index order.
        /// </summary>
        /// <param name="text">The source text where replacements will be applied.</param>
        /// <param name="oldSequence">A semicolon-separated list of words to be replaced.</param>
        /// <param name="newSequence">A semicolon-separated list of replacement words, matched by index to <paramref name="oldSequence"/>.</param>
        /// <param name="options">Optional flags to customize replacement behavior (e.g., case sensitivity, whole-word match).</param>
        /// <returns>A new string with the first occurrence of each specified word replaced.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/>, <paramref name="oldSequence"/>, or <paramref name="newSequence"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="oldSequence"/> and <paramref name="newSequence"/> have different word counts or contain invalid entries.
        /// </exception>
        public static string TranslateFirst(string Text, string OldSequence, string NewSequence, params byte[] options)
        {
            TextEditor editor = new TextEditor(Text);
            return editor.TranslateFirst(OldSequence, NewSequence);
        }

        #endregion

        #region ▼ Append words
        /// <summary>
        /// Append a sequence of characters into the source text at the specified position.
        /// </summary>
        /// <param name="text">The source text where the sequence will be inserted.</param>
        /// <param name="sequenceToAppend">The sequence of characters to append.</param>
        /// <param name="position">The zero-based index in the source text where the sequence should be inserted.</param>
        /// <returns>A new string with the appended sequence.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/> or <paramref name="sequenceToAppend"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="position"/> is less than 0 or greater than the length of <paramref name="text"/>.
        /// </exception>
        public static string Append(string text, string sequenceToAppend)
        {
            TextEditor editor = new TextEditor(text);
            return editor.Append(sequenceToAppend);
        }
        #endregion

        #region ▼ Insert Snippet

        /// <summary>
        /// Inserts a snippet into the source text at the specified position.
        /// </summary>
        /// <param name="text">The source text where the snippet will be inserted.</param>
        /// <param name="snippetToInsert">The sequence of characters to insert.</param>
        /// <param name="position">The zero-based index in the source text where the snippet should be inserted.</param>
        /// <param name="options">Optional flags to customize insertion behavior (e.g., formatting, encoding, or validation rules).</param>
        /// <returns>A new string with the inserted snippet.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/> or <paramref name="snippetToInsert"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="position"/> is less than 0 or greater than the length of <paramref name="text"/>.
        /// </exception>
        public static string InsertSnippet(string text, string snippetToInsert, int position)
        {
            TextEditor editor = new TextEditor(text);
            return editor.InsertSnippet(snippetToInsert, position);
        }

        /// <summary>
        /// Inserts a snippet into the source text before all occurrence of a matched snippet defined by opening and closing tag sequences.
        /// </summary>
        /// <param name="text">The source text where the insertion will occur.</param>
        /// <param name="beforeOfSnippetTagOpen">The character sequence representing the opening tag of the target snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="beforeOfSnippetTagClose">The character sequence representing the closing tag of the target snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="snippetToInsert">The snippet to insert before the matched target snippet.</param>
        /// <param name="options">Optional flags to customize insertion behavior (e.g., formatting, encoding, or validation rules).</param>
        /// <returns>A new string with the inserted snippet placed before the first matched target snippet.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/>, <paramref name="beforeOfSnippetTagOpen"/>, <paramref name="beforeOfSnippetTagClose"/>, or <paramref name="snippetToInsert"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when any of the input parameters are empty or invalid.
        /// </exception>
        public static string InsertSnippetBefore(string text, string beforeOfSnippetTagOpen,
                                                      string beforeOfSnippetTagClose, string snippetToInsert, 
                                                      params byte[] options)
        {
            TextEditor editor = new TextEditor(text);
            return editor.InsertSnippetBefore(snippetToInsert, beforeOfSnippetTagOpen, beforeOfSnippetTagClose, options);
        }

        /// <summary>
        /// Inserts a snippet into the source text after all occurrence of a matched snippet defined by opening and closing tag sequences.
        /// </summary>
        /// <param name="text">The source text where the insertion will occur.</param>
        /// <param name="afterOfSnippetTagOpen">The character sequence representing the opening tag of the target snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="afterOfSnippetTagClose">The character sequence representing the closing tag of the target snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="snippetToInsert">The snippet to insert before the matched target snippet.</param>
        /// <param name="options">Optional flags to customize insertion behavior (e.g., formatting, encoding, or validation rules).</param>
        /// <returns>A new string with the inserted snippet placed before the first matched target snippet.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/>, <paramref name="beforeOfSnippetTagOpen"/>, <paramref name="beforeOfSnippetTagClose"/>, or <paramref name="snippetToInsert"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when any of the input parameters are empty or invalid.
        /// </exception>
        public static string InsertSnippetAfter(string text, string afterOfSnippetTagOpen,
                                                      string afterOfSnippetTagClose, string snippetToInsert, params byte[] options)
        {
            TextEditor editor = new TextEditor(text);
            return editor.InsertSnippetAfter(snippetToInsert, afterOfSnippetTagOpen, afterOfSnippetTagClose, options);
        }

        #endregion

        #region ▼ Remove Snippet
        /// <summary>
        /// Removes all snippets from the source text, defined by matching opening and closing tag sequences.
        /// Supports wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="text">The source text from which the snippet will be removed.</param>
        /// <param name="openTagSnippet">The character sequence representing the opening tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="closeTagSnippet">The character sequence representing the closing tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="options">Optional flags to customize removal behavior (e.g., case sensitivity, nested snippet handling).</param>
        /// <returns>A new string with the matched snippet removed.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/>, <paramref name="openTagSnippet"/>, or <paramref name="closeTagSnippet"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when any of the input parameters are empty or invalid.
        /// </exception>
        public static string RemoveSnippet(string text, string openTagSnippet, string closeTagSnippet, params byte[] options)
        {
            TextEditor editor = new TextEditor(text);
            return editor.RemoveSnippet(openTagSnippet, closeTagSnippet);
        }

        /// <summary>
        /// Removes a first snippet from the source text, defined by matching opening and closing tag sequences.
        /// Supports wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="text">The source text from which the snippet will be removed.</param>
        /// <param name="openTagSnippet">The character sequence representing the opening tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="closeTagSnippet">The character sequence representing the closing tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="options">Optional flags to customize removal behavior (e.g., case sensitivity, nested snippet handling).</param>
        /// <returns>A new string with the matched snippet removed.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/>, <paramref name="openTagSnippet"/>, or <paramref name="closeTagSnippet"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when any of the input parameters are empty or invalid.
        /// </exception>
        public static string RemoveSnippetFirst(string text, string openTagSnippet, string closeTagSnippet, params byte[] options)
        {
            TextEditor editor = new TextEditor(text);
            return editor.RemoveSnippetFirst(openTagSnippet, closeTagSnippet);
        }

        #endregion

        #region ▼ Replace Snippet
        /// <summary>
        /// Replaces all snippets in the source text that are defined by matching opening and closing tag sequences.
        /// Supports wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="text">The source text where the replacements will be applied.</param>
        /// <param name="openTagOldSnippet">The character sequence representing the opening tag of the snippet to replace. Supports wildcards and dynamic characters.</param>
        /// <param name="closeTagOldSnippet">The character sequence representing the closing tag of the snippet to replace. Supports wildcards and dynamic characters.</param>
        /// <param name="newSnippet">The snippet to insert in place of each matched snippet.</param>
        /// <returns>A new string with all matched snippets replaced by the specified <paramref name="newSnippet"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/>, <paramref name="openTagOldSnippet"/>, <paramref name="closeTagOldSnippet"/>, or <paramref name="newSnippet"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when any of the input parameters are empty or invalid.
        /// </exception>
        public static string ReplaceSnippet(string text, string openTagOldSnippet, string closeTagOldSnippet, string newSnippet)
        {
            TextEditor editor = new TextEditor(text);
            return editor.ReplaceSnippet(openTagOldSnippet, closeTagOldSnippet, newSnippet);
        }

        /// <summary>
        /// Replaces first snippet in the source text that are defined by matching opening and closing tag sequences.
        /// Supports wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="text">The source text where the replacements will be applied.</param>
        /// <param name="openTagOldSnippet">The character sequence representing the opening tag of the snippet to replace. Supports wildcards and dynamic characters.</param>
        /// <param name="closeTagOldSnippet">The character sequence representing the closing tag of the snippet to replace. Supports wildcards and dynamic characters.</param>
        /// <param name="newSnippet">The snippet to insert in place of each matched snippet.</param>
        /// <returns>A new string with all matched snippets replaced by the specified <paramref name="newSnippet"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/>, <paramref name="openTagOldSnippet"/>, <paramref name="closeTagOldSnippet"/>, or <paramref name="newSnippet"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when any of the input parameters are empty or invalid.
        /// </exception>
        public static string ReplaceSnippetFirst(string text, string openTagOldSnippet, string closeTagOldSnippet, string newSnippet)
        {
            TextEditor editor = new TextEditor(text);
            return editor.ReplaceSnippetFirst(openTagOldSnippet, closeTagOldSnippet, newSnippet);
        }

        #endregion

        #endregion
    }

    #endregion

    #region ▼ TextMatcher - Match word patterns in text
    /// <summary>
    /// Provides advanced text manipulation and pattern-matching utilities.
    /// </summary>
    /// <remarks>
    /// The <see cref="TextMatcher"/> class enables complex text operations such as wildcard matching,
    /// case-insensitive comparisons, dynamic character handling, and snippet-based transformations.
    /// Designed for high-precision scenarios including filtering, "contains" and "count" tools in text processing.
    /// </remarks>
    public ref struct TextMatcher : IDisposable
    {
        #region ▼ Properties

        /// <summary>
        /// Gets the span source text used for pattern matching operations.
        /// </summary>
        private ReadOnlySpan<char> text;

        /// <summary>
        /// Gets the source text used for pattern matching operations.
        /// </summary>
        public string Text => text.ToString();

        /// <summary>
        /// Length of source text
        /// </summary>
        public int Length => text.Length;

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
        public void setSupressCharsInEnd(int index) => SupressCharsInEnd = index;
                
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
        /// Indicates whether character comparisons should be case-sensitive.
        /// </summary>
        public bool CaseSensitive = true;
        public void EnableCaseSensitive() => CaseSensitive = true;
        public void DisableCaseSensitive() => CaseSensitive = false;

        /// <summary>
        /// Indicates whether content enclosed in single quotes (<c>'</c>) should be ignored during text parsing.
        /// </summary>
        public bool IgnoreCharsInQuotes = false;
        public void EnableIgnoreCharsInQuotes() => IgnoreCharsInQuotes = true;
        public void DisbleIgnoreCharsInQuotes() => IgnoreCharsInQuotes = false;

        /// <summary>
        /// Indicates whether content enclosed in double quotes (<c>"</c>) should be ignored during text parsing.
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
        /// Indicates whether dynamic characters in the pattern and get literal of source text during matching.
        /// </summary>
        public bool IgnoreDynamicChars = false;
        public void EnableIgnoreDynamicChars() => IgnoreDynamicChars = true;
        public void DisableIgnoreDynamicChars() => IgnoreDynamicChars = false;

        /// <summary>
        /// Indicates whether the matching algorithm should use greedy search behavior, returning the longest possible occurrence instead of the nearest or shortest match.
        /// </summary>
        public bool MatchGreedyOccurences = false;
        public void EnableMatchGreedyOccurences() => MatchGreedyOccurences = true;
        public void DisableMatchGreedyOccurences() => MatchGreedyOccurences = false;

        /// <summary>
        /// Indicates whether only whole-word matches should be returned from the source text.
        /// </summary>
        public bool MatchWholeWordOnly  = false;
        public void EnableMatchWholeWordOnly() => MatchWholeWordOnly = true;
        public void DisableMatchWholeWordOnly() => MatchWholeWordOnly = false;

        #endregion

        #region ▼ Constructor
        /// <summary>
        /// Instance TextBuilder and the properties and methods
        /// </summary>
        /// <param name="text">Source text to manipulation</param>
        /// <exception cref="ArgumentNullException">Not acceot null text</exception>
        public TextMatcher(string text)
        {
            if (text is null) { throw new ArgumentNullException(nameof(text)); }
            this.text = text.AsSpan();
        }

        /// <summary>
        /// Instance TextBuilder and the properties and methods
        /// </summary>
        /// <param name="text">Source text to manipulation</param>
        /// <exception cref="ArgumentNullException">Not acceot null text</exception>
        public TextMatcher(ReadOnlySpan<char> text)
        {
            if (text == ReadOnlySpan<char>.Empty) { throw new ArgumentNullException(nameof(text)); }
            this.text = text;
        }

        #endregion

        #region ▼ Match Words

        #region ▼ Constructors

        /// <summary>
        /// Matches a specified sequence within the given text and returns the result along with positional information.
        /// </summary>
        /// <remarks>
        /// This method processes the input text differently based on its length to optimize performance.
        /// To enable interpretation of dynamic characters, set the property <c>paramsDynamicChars</c> to <c>true</c>.
        /// </remarks>
        /// <param name="toMatch">The sequence of characters to match within the text.</param>
        /// <returns>
        /// A <see cref="StringAndPosition"/> object containing the matched sequence and its positional information.
        /// </returns>
        /// <example>
        /// Wildcard simple pattern: "name*." — matches "name: John Doe."
        /// </example>
        /// <example>
        /// Dynamic character word separation pattern: "John_Marie_" — matches "John Marie", "John,Marie", "John(Marie)", and others.
        /// </example>
        /// <example>
        /// Dynamic character number pattern: "iten #" — matches "iten 10", "iten 2000", and more.
        /// </example>
        /// <example>
        /// Dynamic character complete word pattern:
        /// "~act" matches "react", "act~" matches "action", "~act~" matches "characteristics".
        /// </example>
        public StringAndPosition Match(string toMatch)
        {
            if(toMatch == null) {  throw new ArgumentNullException(nameof(toMatch)); }
            if(toMatch.Length == 0) { throw new ArgumentException(nameof(toMatch)); }

            (int pos, int len) result = match(toMatch.AsSpan());
            if (result.pos == -1) { return new StringAndPosition(); }
            else { return new StringAndPosition(text.Slice(result.pos, result.len), result.pos); }
        }

        /// <summary>
        /// Returns the index position where the matched sequence of characters begins in the source text.
        /// Supports wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="toMatch">The character sequence or pattern to search for in the source text.</param>
        /// <returns>The zero-based index of the first match found; returns -1 if no match is found.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="toMatch"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="toMatch"/> is empty or invalid.
        /// </exception>
        public int IndexOf(string toMatch)
        {
            if (toMatch == null) { throw new ArgumentNullException(nameof(toMatch)); }
            if (toMatch.Length == 0) { throw new ArgumentException(nameof(toMatch)); }
            (int pos, int len) result = match(toMatch.AsSpan());
            if (result.pos == -1) { return -1; }
            else { return result.pos; }
        }

        /// <summary>
        /// Returns a tuple containing the index position and length of the matched sequence of characters in the source text.
        /// Supports wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="toMatch">The character sequence or pattern to search for in the source text.</param>
        /// <returns>
        /// A <see cref="Tuple{int, int}"/> where:
        /// <c>Item1</c> is the zero-based index of the first match found, and
        /// <c>Item2</c> is the length of the matched sequence.
        /// Returns <c>null</c> if no match is found.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="toMatch"/> is empty or invalid.
        /// </exception>
        public (int, int) IndexOf(ReadOnlySpan<char> toMatch)
        {
            if (toMatch.Length == 0) { throw new ArgumentException(nameof(toMatch)); }
            (int pos, int len) result = match(toMatch);
            if (result.pos == -1) { return (-1,0); }
            else { return (result.pos, result.len); }
        }

        /// <summary>
        /// Determines whether the source text contains a character sequence, supporting wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="toMatch">The character sequence or pattern to match in the source text.</param>
        /// <returns><c>true</c> if the sequence is found; otherwise, <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="toMatch"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="toMatch"/> is empty.
        /// </exception>
        public bool Contains(string toMatch)
        {
            if (toMatch == null) { throw new ArgumentNullException(nameof(toMatch)); }
            if (toMatch.Length == 0) { throw new ArgumentException(nameof(toMatch)); }

            (int pos, int len) result = match(toMatch.AsSpan());
            if (result.pos == -1) { return false; }
            else { return true; }
        }

        /// <summary>
        /// Counts the number of matches found in the source text, supporting wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="toMatch">The character sequence or pattern to match in the source text.</param>
        /// <returns>The number of matches found.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="toMatch"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="toMatch"/> is empty.
        /// </exception>
        public int Cont(string toMatch)
        {
            if (toMatch == null) { throw new ArgumentNullException(nameof(toMatch)); }
            if (toMatch.Length == 0) { throw new ArgumentException(nameof(toMatch)); }

            int _count = 0;
            int pos = 0;

            while (pos != -1)
            { 
                (pos, int len) = match(toMatch.AsSpan(), pos);
                if (pos == -1) { break; }
                _count++;
                pos+= len;
            }

            return _count;
        }

        #endregion

        #region ► Controller
        /// <summary>
        /// Model match of text with sequence to match.
        /// </summary>
        /// <param name="toMatch">Pattern with words and dynamic chars to search in source text</param>
        /// <returns></returns>
        private (int, int) match(ReadOnlySpan<char> toMatch, int privateStartIndex = -1)
        {
            if (toMatch.Length == 0) { return (-1, 0); }

            #region + Wildcard in start pattern

            bool wildcardStart = false;
            bool wildcardEnd = false;
            if (toMatch[0] == '*') { toMatch = toMatch[1..]; wildcardStart = true; }
            if (toMatch[^1] == '*') { toMatch = toMatch[..^1]; wildcardEnd = true; }

            #endregion

            #region ++ Flags and variable                       

            if (privateStartIndex == -1) { privateStartIndex = StartIndex; }
            int txtPos = privateStartIndex;
            int returnPos = -1;
            int returnLen = -1;

            ReadOnlySpan<char> separatorChars = stackalloc char[] { ' ', '!', '?', '.', ';', ':', ',', '|', '(', ')', '[', ']', '{', '}', '\n', '\t', '\r' };

            #endregion

            while (txtPos > -1)
            {
                int occurPos = -1; int occurLen = 0;
                ReadOnlySpan<char> wildcardMatch = toMatch;
                bool wildcardMode = false;

                while (!wildcardMatch.IsEmpty)
                {
                    #region + Split wildcard parts

                    //The first occurence is a start pos and other occurences is a len
                    int splitPos = wildcardMatch.IndexOf('*');
                    if (splitPos == -1) { splitPos = wildcardMatch.Length; }
                    else { wildcardMode = true; }
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
                        if (splitPos == -1) { splitPos = orMatch.Length; }
                        ReadOnlySpan<char> matchWord = orMatch[..splitPos];
                        if (matchWord.Length < orMatch.Length)
                        { splitPos = matchWord.Length + 1; orMatch = orMatch[splitPos..]; }
                        else { orMatch = ReadOnlySpan<char>.Empty; }

                        #endregion

                        #region + Text IndexOf part
                        (int pos, int len) = indexOf(matchWord, txtPos);
                        if (pos == -1) { continue; }
                        #endregion

                        #region + Verify if OR occurrence is smaller position in text

                        if ((uint)pos < (uint)orPos)
                        { orPos = pos; orLen = len; }

                        #endregion
                    }

                    #endregion

                    #region ? Match not found
                    if (orPos == -1) { occurPos = -1; occurLen = 0; txtPos = -1; break; }
                    #endregion

                    #region + Update return data
                    if (occurPos == -1) { occurPos = orPos; }
                    occurLen = (orPos + orLen) - occurPos;
                    orPos = -1; orLen = 0;
                    #endregion

                    txtPos = occurPos + 1;
                }

                #region + Only whole word only
                if (MatchWholeWordOnly && occurPos > -1 && occurLen > -1)
                {
                    if(text.Slice(occurPos, occurLen).IndexOfAny(separatorChars) !=-1)
                    { continue; }
                }
                #endregion

                if ((uint)returnLen > (uint)occurLen && occurPos != -1)
                {
                    returnPos = occurPos; returnLen = occurLen;
                    txtPos = returnPos + 1;

                    if (MatchGreedyOccurences ||
                        (!wildcardMode && wildcardMatch.IsEmpty)) { break; }
                }
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
            if (SupressCharsInStart != 0) { returnPos += SupressCharsInStart; returnLen -= SupressCharsInStart; }
            if (SupressCharsInEnd != 0) { returnLen -= SupressCharsInEnd; }
            #endregion

            resetProperties();

            return (returnPos, returnLen);
        }

        #endregion

        #region ▼ Private Auxiliar Methods

        #region » Reset properties
        /// <summary>
        /// Reset properties values
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void resetProperties()
        {
            StartIndex = 0;
            SupressCharsInStart = 0;
            SupressCharsInEnd = 0;
            CaseSensitive = true;
            IgnoreCharsInDoubleQuotes = false;
            IgnoreCharsInQuotes = false;
            IgnoreDynamicChars = false;
        }

        #endregion

        #region » IsSeparator
        /// <summary>
        /// If the char is a separator.
        /// </summary>
        /// <param name="c">Char of text</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool IsSeparator(char c) => c == ' ' || c == '!' || c == '?' || c == '.' || c == ';' ||
                                            c == ':' || c == ',' || c == '|' || c == '(' || c == ')' ||
                                            c == '[' || c == ']' || c == '{' || c == '}' || c == '\n' ||
                                            c == '\t' || c == '\r';
        #endregion

        #region » IsDynamicOrSeparator
        /// <summary>
        /// If the char is a dynamic or separator.
        /// </summary>
        /// <param name="c">Char of text</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isDynamicOrSeparatorChar(char c) => c == '_' || c == '#' || c == '~' || c == ' ' || c == '!' ||
                                          c == '?' || c == '.' || c == ';' || c == ':' || c == ',' ||
                                          c == '|' || c == '(' || c == ')' || c == '[' || c == ']' ||
                                          c == '{' || c == '}' || c == '\n' || c == '\t' || c == '\r';

        #endregion

        #region » IsDynamicChar
        /// <summary>
        /// If the char is a dynamic char.
        /// </summary>
        /// <param name="c">Char of text</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private bool isDynamicChar(char c) => c == '_' || c == '#' || c == '~';

        #endregion

        #endregion

        #region ▼ Models

        #region ► indexOf
        /// <summary>
        /// Model/Source index of first occurrence in text.
        /// </summary>
        /// <param name="toMatch">Pattern to match in source text</param>
        /// <param name="privateStartIndex">Start position in source text</param>
        /// <returns></returns>
        private (int position, int additionalLen) indexOf(ReadOnlySpan<char> toMatch, int privateStartIndex = -1)
        {
            #region ** Exception : Empty pattern or text
            if (toMatch.Length == 0) { return (-1, 0); }
            if (text.Length == 0) { return (-1, 0); }
            #endregion

            if (privateStartIndex == -1) privateStartIndex = StartIndex;

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
            int occurLen = 0;
            int patPos = 0;
            int pos = privateStartIndex;

            #endregion

            ref char refText = ref MemoryMarshal.GetReference(text);
            ref char refPat = ref MemoryMarshal.GetReference(toMatch);

            while (occurPos == -1 && pos < txtLength)
            {
                for (; pos < txtLength; pos++)
                {
                    #region + Build pattern part and indexOf

                    if (!IgnoreCharsInQuotes && !IgnoreCharsInDoubleQuotes && CaseSensitive && pos == privateStartIndex)
                    {
                        (occurPos, occurLen) = indexOfLiteralUntilDynamic(ref toMatch, ref patPos, ref pos);

                        if (occurPos == -1) { return (-1, 0); }
                        else if (patPos == patLength) { break; }
                    }

                    #endregion

                    #region + Char in text
                    char c = Unsafe.Add(ref refText, pos);
                    #endregion

                    #region + Pattern char
                    if (patPos == patLength) { break; }
                    char p = Unsafe.Add(ref refPat, patPos);
                    #endregion

                    #region + Ignore in quotes

                    bool isSingleQuote = c == '\'';
                    bool isDoubleQuote = c == '\"';

                    if (IgnoreCharsInQuotes && isSingleQuote)
                    { pos = subIgnoreQuote(pos); }
                    else if (IgnoreCharsInDoubleQuotes && isDoubleQuote)
                    { pos = subIgnoreDubleQuote(pos); }

                    #endregion

                    #region + Is Dynamic char

                    if (!IgnoreDynamicChars)
                    {
                        #region + Word separator char
                        if (p == '_' && IsSeparator(c))
                        {
                            if (occurPos == -1) { occurPos = pos; }
                            occurLen++; patPos++;
                            if (patPos == patLength) { break; }
                            continue;
                        }
                        #endregion

                        #region + Number char

                        if (Char.IsDigit(c) && p == '#')
                        {
                            if (occurPos == -1) { occurPos = pos; }
                            DynamicNumber(ref pos, ref occurLen);
                            patPos++;
                            if (patPos == patLength) { break; }
                            continue;
                        }

                        #endregion
                    }

                    #endregion

                    #region + Ignore sensitive case char

                    if (!CaseSensitive)
                    { c = toCaseChar(c, 0); p = toCaseChar(p, 0); }

                    #endregion

                    #region + Match char
                    if (c == p)
                    {

                        if (occurPos == -1) { occurPos = pos; }
                        occurLen++; patPos++; continue;
                    }
                    #endregion

                    #region + Not match text with pattern

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

        #region » indexOfLiteralUntilDynamic
        /// <summary>
        /// Use index of to get litteral part of pattern until of any dynamic char
        /// </summary>
        /// <param name="toMatch">Patter to match</param>
        /// <param name="patPos">Position index in toMatch. Start 0 and return dynamic char pos or toMatch len
        /// if dynamic char not exists</param
        /// <param name="pos">Position index in text</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private (int, int) indexOfLiteralUntilDynamic(ref ReadOnlySpan<char> toMatch,
                                      ref int patPos, ref int pos)
        {
            int _patStart = patPos;
            int occurLen = 0;
            int occurPos = 0;

            #region + Build pattern to search

            ReadOnlySpan<char> searchChars = stackalloc char[] { '_', '#' };
            patPos = toMatch.IndexOfAny(searchChars);

            if (patPos < 0)
            { patPos = toMatch.Length; }

            ReadOnlySpan<char> _match = toMatch.Slice(_patStart, patPos);

            #endregion

            #region + IndexOf toMatch

            // Executa busca personalizada
            int _textStart = pos;
            pos = text[_textStart..].IndexOf(_match);

            #endregion

            #region ? Match not found
            if (pos == -1) { pos = _textStart; patPos = _patStart; return (-1, 0); }
            #endregion

            occurLen = _match.Length;
            occurPos = pos + _textStart;
            pos = occurPos + occurLen;

            return (occurPos, occurLen);
        }

        #endregion

        #region » Ignore in quotes
        /// <summary>
        /// Jump pos index to end of quote snippet in text
        /// </summary>
        /// <param name="pos">Current position in text</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int subIgnoreQuote(int pos)
        {
            pos++;
            int endQuote = text[pos..].IndexOf('\'');
            if (endQuote < 0) { throw new InvalidOperationException("Non-closed opened apostrophe quote!"); }
            pos += endQuote;
            pos++;

            return pos;
        }

        /// <summary>
        /// Jump pos index to end of double quote snippet in text
        /// </summary>
        /// <param name="pos">Current position in text</param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int subIgnoreDubleQuote(int pos)
        {
            pos++;
            int endQuote = text[pos..].IndexOf('\"');
            if (endQuote < 0) { throw new InvalidOperationException("Non-closed opened double quote!"); }
            pos += endQuote;
            pos++;

            return pos;
        }
        #endregion

        #region » dynamicNumber
        /// <summary>
        /// Builds the length of a dynamic numbers in text by '#' in pattern.
        /// </summary>
        /// <param name="pos">Position in source text</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DynamicNumber(ref int pos, ref int occurLen)
        {
            for (; pos < text.Length; pos++)
            {
                char c = text[pos];
                if ((c < '0' || c > '9') && c != '.' && c != ',')
                { pos--; break; }
                else if (c == '.' || c == ',')
                {
                    if (pos < text.Length)
                    {
                        if (text[pos + 1] < '0' || text[pos + 1] > '9')
                        { pos--; break; }
                    }
                }

                occurLen++;
            }

            return;
        }

        #endregion

        #region » completeWord
        /// <summary>
        /// Completes the word start from pattern from text occurence from the given position when '~' in pattern.
        /// </summary>
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

        /// <summary>
        /// Completes the word end from pattern from text occurence from the given position when '~' in pattern.
        /// </summary>
        /// <param name="pos">Position in text of occurence</param>
        /// <returns></returns>
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

        #region » toCase

        /// <summary>
        /// Parse letter case of a char
        /// </summary>
        /// <param name="character">Char to parse</param>
        /// <param name="toCaseCod">Case format/: 0 is lower and 1 is upper</param>
        /// <returns></returns>
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

        #region ▼ Match Snippets

        #region ► Constructors

        /// <summary>
        /// Matches a snippet in the source text using specified opening and closing tag sequences.
        /// Supports wildcard patterns, dynamic characters, and optional parsing behaviors.
        /// </summary>
        /// <param name="snippetTagsOpen">The character sequence representing the opening tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="snippetTagsClose">The character sequence representing the closing tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="StartIndex">The zero-based index in the source text from which to begin the search.</param>
        /// <param name="options">Optional flags to customize matching behavior (e.g., case sensitivity, greedy matching, quote handling).</param>
        /// <returns>
        /// A <see cref="StringAndPosition"/> object containing the matched snippet and its position in the source text.
        /// Returns <c>null</c> if no matching snippet is found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="snippetTagsOpen"/> or <paramref name="snippetTagsClose"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="snippetTagsOpen"/> or <paramref name="snippetTagsClose"/> is empty or invalid.
        /// </exception>
        public StringAndPosition Snippet(string snippetTagsOpen, string snippetTagsClose, int StartIndex=0, params byte[] options)
        {
            if (snippetTagsOpen == "" || snippetTagsClose == "") { return default; }

            (int position, int length) = snippetCore(snippetTagsOpen.AsSpan(), snippetTagsClose.AsSpan(), StartIndex);

            if (position == -1 || length == 0)
            { return new StringAndPosition(); }

            return new StringAndPosition(text.Slice(position, length).ToString(), position);
        }

        /// <summary>
        /// Matches a snippet in the source text using specified opening and closing tag sequences.
        /// Supports wildcard patterns, dynamic characters, and optional parsing behaviors.
        /// </summary>
        /// <param name="snippetTagsOpen">The character sequence representing the opening tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="snippetTagsClose">The character sequence representing the closing tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="StartIndex">The zero-based index in the source text from which to begin the search.</param>
        /// <returns>
        /// A <c>StringAndPosition</c> object containing the matched snippet and its position in the source text.
        /// Returns <c>null</c> if no matching snippet is found.
        /// </returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="snippetTagsOpen"/> or <paramref name="snippetTagsClose"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="snippetTagsOpen"/> or <paramref name="snippetTagsClose"/> is empty or invalid.
        /// </exception>
        public (int, int) Snippet(ReadOnlySpan<char> snippetTagsOpen, ReadOnlySpan<char> snippetTagsClose, int StartIndex = 0)
        {
            if (snippetTagsOpen == "" || snippetTagsClose == "") { return default; }

            (int position, int length) = snippetCore(snippetTagsOpen, snippetTagsClose, StartIndex);

            if (position == -1 || length == 0)
            { return (position, length); }

            return (position, length);
        }

        #endregion

        #region ► Controller
        /// <summary>
        /// Match a snippet in the source text with open and close tags.
        /// </summary>
        /// <param name="openTag">Snippet open tag</param>
        /// <param name="closeTag">Snippet close tag</param>
        /// <returns>Matched snippet of characters</returns>
        /// <exception cref="Exception">Invalid snippet pattern, if not found wildcard in snippet tags</exception>
        /// <exception cref="InvalidCastException"></exception>
        private (int, int) snippetCore(ReadOnlySpan<char> openTag, ReadOnlySpan<char> closeTag, int privateStartIndex = -1)
        {
            #region ** Exception : The snippet not contain a '*' character to split open and close tag.

            if (openTag.Length == 0 || closeTag.Length == 0)
            { throw new Exception("!Open or/and close snippet empty!"); }

            #endregion

            #region ++ Flags e variables

            int openPos = -1;
            int openLen = openTag.Length;
            int closePos = -2;
            int closeLen = closeTag.Length;

            if (privateStartIndex < 0) { privateStartIndex = StartIndex; }

            int openStartIndex = privateStartIndex;
            int closeStartIndex = privateStartIndex;

            int openTagCount = 0;
            int closeTagCount = 0;
            int returnPos = -2;
            int returnLen = 0;
            int textLen = text.Length;

            #endregion

            /*To find correct close tag of snippet, need found in sequence the same number of open tag of close tags.
            If found open tag, but the next occurence is a open tag too, so the next close is not close dad snipet, 
            is the that child snippet close.*/
            while (openTagCount != closeTagCount || openTagCount == 0)
            {
                #region + Match open tag

                if (openPos < closePos || openPos == -1)
                {
                    (openPos, openLen) = match(openTag, openStartIndex);
                    if( openTagCount == 0) { closeStartIndex = openPos; }
                }

                #endregion

                #region + Patternize open tag removing the wildcard and after it content

                if (openTagCount == 0)
                {
                    int wildcard = openTag.IndexOf('*');
                    if (wildcard != -1)
                    { openTag = openTag.Slice(0, wildcard); }
                }

                #endregion

                #region + Close tag in text

                if ( closePos < openPos )
                {
                    (closePos, closeLen) = match(closeTag, closeStartIndex);
                    if (closePos == -1) { return (-1, 0); }
                }

                #endregion

                if ( (uint)openPos < (uint)closePos )
                {
                    #region ? Open tag position in text is smaller that close tag
                    //Open tag position in text is smaller than close tag So  Because it, by sequence in
                    //the text the open is the next occurence. 
                    openTagCount++; openStartIndex = openPos + 1;
                    if (returnPos < 0) { returnPos = openPos; }

                    #endregion
                }
                else if(openTagCount > closeTagCount)
                {
                    #region ? Close tag position in text is smaller that open tag
                    closeTagCount++; closeStartIndex = closePos + 1;
                    returnLen = closePos - returnPos + closeTag.Length;
                    #endregion
                }
                else
                {
                    openStartIndex = closePos + closeLen;
                }

                if (openPos == -1) { break; }
            }

            if (openTagCount != closeTagCount || (returnPos == -1 || returnLen == 0))
            { return (-1, 0); }

            // If not found open tag, return -1 and 0.
            return (returnPos, returnLen);
        }

        #endregion

        #region ► Contains

        /// <summary>
        /// Match a snippet in the source text with open and close tags.
        /// </summary>
        /// <param name="snippetTags">Open snippet and close snippet sequence characters.
        /// <param name="options">TextBuilde.Params option</param>
        /// <returns>Matched snippet of characters</returns>
        public bool ContainsSnippet(string snippetTagsOpen, string snippetTagsClose, int StartIndex = 0, params byte[] options)
        {
            if (snippetTagsOpen == "" || snippetTagsClose == "") { return default; }

            (int position, int length) = snippetCore(snippetTagsOpen.AsSpan(), snippetTagsClose.AsSpan(), StartIndex);

            if (position == -1) return false;
            else return true;
        }

        #endregion

        #region ► Count

        /// <summary>
        /// Cout of snippets matcheds in the source text with open and close tags.
        /// </summary>
        /// <param name="snippetTags">Open snippet and close snippet sequence characters.
        /// <param name="options">TextBuilde.Params option</param>
        /// <returns>Count int of matched snippets</returns>
        public int CountSnippets(string snippetTagsOpen, string snippetTagsClose, params byte[] options)
        {
            if (snippetTagsOpen == "" || snippetTagsClose == "") { return default; }

            int _count = 0;
            int pos = 0;

            while (pos != -1)
            {
                (pos, int len) = snippetCore(snippetTagsOpen.AsSpan(), snippetTagsClose.AsSpan(), pos);
                if (pos == -1) { break; }
                _count++;
                pos += len;
            }

            return _count;
        }


        #endregion

        #endregion

        public void Dispose()
        {
            // Implement IDisposable if needed
        }
    }
    #endregion

    #region ▼ TextEditor - Edit text
    /// <summary>
    /// Provides advanced text manipulation utilities.
    /// </summary>
    /// <remarks>
    /// The <see cref="TextEditor"/> class enables complex text edit operations such as operation using wildcard matching,
    /// case-insensitive comparisons, dynamic character handling, and snippet-based matches.
    /// Designed for high-precision scenarios including insert, remove and replace tools text processing.
    /// </remarks>
    public ref struct TextEditor:IDisposable
    {
        #region ▼ Edit word

        #region ▼ Properties

        /// <summary>
        /// Source text to edit
        /// </summary>
        public ReadOnlySpan<char> text;

        /// <summary>
        /// Indicates whether the operation was successful.
        /// </summary>
        public bool Success { get; private set; } 
        

        #endregion

        #region ▼ Parameters

        private byte[] Parameters { get; set; }

        private byte isInsertOnly = 0;
        private byte isInsertBeforeFirst = 1;
        private byte isInsertBefore = 2;
        private byte isInsertAfterFirst = 3;
        private byte isInsertAfter = 4;
        private byte isRemoveFirst = 5;
        private byte isRemove = 6;
        private byte isReplaceFirst = 7;
        private byte isReplace = 8;
        private byte isSnippet = 9;

        #endregion

        #region ▼ Constructor
        /// <summary>
        /// Provides text editing tools including insertion, removal, and replacement of words or snippets within the source text.
        /// </summary>
        /// <param name="text">The source text to be edited.</param>
        /// <param name="options">Optional flags to customize editing behavior (e.g., case sensitivity, quote handling, greedy matching).</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="text"/> is empty or contains invalid formatting.
        /// </exception>
        public TextEditor(string text, params byte[] options)
        {
            if(text == null) throw new ArgumentNullException("Text");
            if(text == "") throw new ArgumentException("Text can't be empty!");
            this.text = text;

            Parameters = options;
        }

        /// <summary>
        /// Provides text editing tools including insertion, removal, and replacement of words or snippets within the source text.
        /// </summary>
        /// <param name="text">The memory span source text to be edited.</param>
        /// <param name="options">Optional flags to customize editing behavior (e.g., case sensitivity, quote handling, greedy matching).</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="text"/> is empty or contains invalid formatting.
        /// </exception>
        public TextEditor(ReadOnlySpan<char> text, params byte[] options)
        {
            if (text.Length ==0) throw new ArgumentNullException("text");
            this.text = text;
            Parameters = options;
        }

        #endregion

        #region ► Model       

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private ReadOnlySpan<char> insertCore(ReadOnlySpan<char> toMatchOrSnippetTagOpen, ReadOnlySpan<char> SnippetTagClose, 
                                              ReadOnlySpan<char> toInsert, int pos = 0, byte taskType = 0, byte matchType=0, 
                                              params byte[] options)
        {
            #region + Variables and flags

            int totalLen = text.Length;
            int toMatchLen = toMatchOrSnippetTagOpen.Length;
            int toInsertLen = toInsert.Length;

            // Buffers na stack
            Span<int> bufferPos = stackalloc int[16];
            Span<int> bufferLen = stackalloc int[16];

            // Listas com stack buffer
            using var posList = new ValueList<int>(bufferPos);
            using var lenList = new ValueList<int>(bufferLen);

            #endregion

            #region + Simplify task actions

            bool addByInsertLen = taskType == isInsertAfter || taskType == isInsertAfterFirst ||
                                  taskType == isInsertBefore || taskType == isInsertBeforeFirst ||
                                  taskType == isReplace || taskType == isReplaceFirst;
                                   
            bool addByMatchLen = taskType == isRemove || taskType == isRemoveFirst ||
                                 taskType == isReplace || taskType == isReplaceFirst;

            bool OneMatch = taskType == isReplaceFirst || taskType == isRemoveFirst ||
                            taskType == isRemoveFirst || taskType == isInsertAfterFirst ||
                            taskType == isInsertBeforeFirst;

            bool RemoveMatch = taskType == isReplaceFirst || taskType == isReplace ||
                               taskType == isRemoveFirst || taskType == isRemove;

            #endregion

            #region + Matcher word
                        
            if (taskType > 0)
            {
                var matcher = new TextMatcher(text);
                enableMatchParams(ref matcher, options);
                int currentPos = 0;
                int lenMatch = 0;

                while (pos != -1)
                {
                    #region + IndexOf toMatch

                    matcher.StartIndex = currentPos;

                    if (matchType == 10)
                    { (pos, lenMatch) = matcher.Snippet(toMatchOrSnippetTagOpen, SnippetTagClose, currentPos); }
                    else { (pos, lenMatch) = matcher.IndexOf(toMatchOrSnippetTagOpen); }

                    if (pos == -1)
                        break; // Nenhuma ocorrência adicional

                    #endregion

                    #region + Update currentPos to new search

                    // Atualiza posição para continuar busca
                    pos += (taskType == isInsertAfter || taskType == isInsertAfterFirst ? lenMatch : 0);

                    #endregion

                    #region + Add position and length o toMatch
                    // Armazena posição
                    posList.Add(pos);

                    // Armazena comprimento da correspondência
                    lenList.Add(lenMatch);

                    #endregion

                    #region + Update currentPos to new search

                    // Atualiza posição para continuar busca
                    currentPos = pos + lenMatch;

                    #endregion

                    #region + Add len of toInsert in totalLen

                    if (addByInsertLen)
                    { totalLen += toInsertLen; }

                    #endregion

                    #region + Add len of toMatch in totalLen

                    if (addByMatchLen)
                    { totalLen -= toMatchLen; }

                    #endregion

                    #region + Break if the task use only the first occurrence

                    if (OneMatch) { break; }

                    #endregion
                }

                // Se não encontrou nada
                if (posList.Length == 0)
                    return text;
            }
            else
            {
                totalLen = text.Length + toInsert.Length;
                posList.Add(pos);
            }

            #endregion

            #region + Inserter word

            Span<char> buffer = totalLen <= 1024
                                ? stackalloc char[totalLen]
                                : ArrayPool<char>.Shared.Rent(totalLen);

            int startInText = 0;
            int startInBuffer = 0;
            //int insertsLen = 0;
            int i = 0;
            int posLen = posList.Length;

            for (; i < posLen; i++)
            {
                #region + Copy part before at posList item.

                text.Slice(startInText, (posList[i] - startInText)).CopyTo(buffer.Slice(startInBuffer));
                startInBuffer += posList[i] - startInText;

                #endregion

                #region + Copy toInsert word in posList item

                if (toInsertLen > 0)
                { 
                    toInsert.CopyTo(buffer.Slice(startInBuffer));
                    startInBuffer += toInsertLen;
                }

                #endregion

                #region + Update start to next part of text

                startInText = posList[i];
                if (RemoveMatch)//Add match len to position and add nest text part after match word
                { startInText += lenList[i]; }

                #endregion
            }

            text[startInText..].CopyTo(buffer.Slice(startInBuffer));

            buffer = buffer.TrimEnd();

            #endregion

            Span<char> _result = buffer.Slice(0);
            _result = _result.TrimEnd('\0');
            string result = new string(_result);

            if (totalLen > 1024)
                ArrayPool<char>.Shared.Return(buffer.ToArray()); // ou buffer.Slice(0, totalLength).ToArray()

            return result;
        }

        #endregion

        #region ▼ Insert text
        /// <summary>
        /// Inserts all sequences of characters into the source text at the specified position.
        /// </summary>
        /// <param name="sequenceToInsert">The sequence of characters to insert.</param>
        /// <param name="positionInText">The zero-based index in the source text where the sequence should be inserted.</param>
        /// <returns>A new string with the inserted sequence.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="sequenceToInsert"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        public string Insert(string sequenceToInsert, int positionInText)
        {
            if (sequenceToInsert == null) { throw new ArgumentNullException("sequenceToInsert"); }

            int[] pos = [positionInText];
                       
            return insertCore( "", "", sequenceToInsert, positionInText, isInsertOnly).ToString();
        }

        /// <summary>
        /// Inserts a first sequence of characters into the source text before the first occurrence of a specified sequence.
        /// </summary>
        /// <param name="sequenceToInsert">The sequence of characters to insert.</param>
        /// <param name="beforeOf">The sequence of characters in the source text before which the insertion should happen.</param>
        /// <param name="options">Parameters of matches methods.</param>
        /// <returns>A new string with the inserted sequence placed before the specified target sequence.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="sequenceToInsert"/>, or <paramref name="beforeOf"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="beforeOf"/> is not found in source text.
        /// </exception>
        public string InsertBeforeFirst(string sequenceToInsert, string beforeOf, params byte[] options)
        {
            if (sequenceToInsert == null) { throw new ArgumentNullException("SequenceToInsert"); }
            if (sequenceToInsert == "") { throw new ArgumentException("Can't SequenceToInsert is empty!"); }
            if (beforeOf == null) { throw new ArgumentNullException("BeforeOf"); }
            if (beforeOf == "") { throw new ArgumentException("Can't BeforeOf is empty!"); }

            return insertCore(beforeOf, "", sequenceToInsert,0, isInsertBeforeFirst).ToString();
        }

        /// <summary>
        /// Inserts a first sequence of characters into the source text immediately after the first occurrence of a specified sequence.
        /// </summary>
        /// <param name="sequenceToInsert">The sequence of characters to insert.</param>
        /// <param name="afterOf">The sequence of characters in the source text after which the insertion should happen.</param>
        /// <param name="options">Parameters of matches methods.</param>
        /// <returns>A new string with the inserted sequence placed after the specified target sequence.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="sequenceToInsert"/>, or <paramref name="afterOf"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="afterOf"/> is not found in source text.
        /// </exception>
        public string InsertAfterFirst(string sequenceToInsert, string afterOf, params byte[] options)
        {
            if (sequenceToInsert == null) { throw new ArgumentNullException("SequenceToInsert"); }
            if (sequenceToInsert == "") { throw new ArgumentException("Can't SequenceToInsert is empty!"); }
            if (afterOf == null) { throw new ArgumentNullException("AfterOf"); }
            if (afterOf == "") { throw new ArgumentException("Can't AfterOf is empty!"); }

            return insertCore(afterOf,"", sequenceToInsert, 0, isInsertAfterFirst).ToString();
        }

        /// <summary>
        /// Inserts a first sequence of characters into the source text before the all occurrence of a specified sequence.
        /// </summary>
        /// <param name="sequenceToInsert">The sequence of characters to insert.</param>
        /// <param name="beforeOf">The sequence of characters in the source text before which the insertion should happen.</param>
        /// <param name="options">Parameters of matches methods.</param>
        /// <returns>A new string with the inserted sequence placed before the specified target sequence.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="sequenceToInsert"/>, or <paramref name="beforeOf"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="beforeOf"/> is not found in source text.
        /// </exception>
        public string InsertBefore(string sequenceToInsert, string beforeOf, params byte[] options)
        {
            if (sequenceToInsert == null) { throw new ArgumentNullException("SequenceToInsert"); }
            if (sequenceToInsert == "") { throw new ArgumentException("Can't SequenceToInsert is empty!"); }
            if (beforeOf == null) { throw new ArgumentNullException("BeforeOf"); }
            if (beforeOf == "") { throw new ArgumentException("Can't BeforeOf is empty!"); }

            return insertCore(beforeOf, "", sequenceToInsert, 0, isInsertBefore).ToString();
        }

        /// <summary>
        /// Inserts all sequence of characters into the source text immediately after all occurrence of a specified sequence.
        /// </summary>
        /// <param name="sequenceToInsert">The sequence of characters to insert.</param>
        /// <param name="afterOf">The sequence of characters in the source text after which the insertion should happen.</param>
        /// <param name="options">Parameters of matches methods.</param>
        /// <returns>A new string with the inserted sequence placed after the specified target sequence.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="sequenceToInsert"/>, or <paramref name="afterOf"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="afterOf"/> is not found in source text.
        /// </exception>
        public string InsertAfter(string sequenceToInsert, string afterOf, params byte[] options)
        {
            if (sequenceToInsert == null) { throw new ArgumentNullException("SequenceToInsert"); }
            if (sequenceToInsert == "") { throw new ArgumentException("Can't SequenceToInsert is empty!"); }
            if (afterOf == null) { throw new ArgumentNullException("AfterOf"); }
            if (afterOf == "") { throw new ArgumentException("Can't AfterOf is empty!"); }

            return insertCore(afterOf, "", sequenceToInsert, 0, isInsertAfter).ToString();
        }

        #endregion

        #region ▼ Remove text

        /// <summary>
        /// Removes first occurrence of a character sequence from the source text, supporting wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="sequenceToRemove">The character sequence or pattern to remove. Supports wildcards and dynamic characters.</param>
        /// <param name="options">Optional flags to customize the removal behavior (e.g., case sensitivity, match mode).</param>
        /// <returns>A new string with the matched sequences removed.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/> or <paramref name="sequenceToRemove"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="sequenceToRemove"/> is empty or invalid.
        /// </exception>
        public string RemoveFirst(string sequenceToRemove, params byte[] options )
        {
            if (sequenceToRemove == null) { throw new ArgumentNullException("SequenceToRemove"); }
            if (sequenceToRemove == "") { throw new ArgumentException("Can't SequenceToRemove is empty!"); }

            return insertCore( sequenceToRemove, "","", 0, isRemoveFirst, 0, options).ToString();
        }

        /// <summary>
        /// Removes all occurrences of a character sequence from the source text, supporting wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="sequenceToRemove">The character sequence or pattern to remove. Supports wildcards and dynamic characters.</param>
        /// <param name="options">Optional flags to customize the removal behavior (e.g., case sensitivity, match mode).</param>
        /// <returns>A new string with the matched sequences removed.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/> or <paramref name="sequenceToRemove"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="sequenceToRemove"/> is empty or invalid.
        /// </exception>
        public string Remove(string sequenceToRemove, params byte[] options)
        {
            if (sequenceToRemove == null) { throw new ArgumentNullException("sequenceToInsert"); }

            return insertCore(sequenceToRemove, "","", 0, isRemove, 0, options).ToString();
        }

        #endregion

        #region ▼ Replace text
        /// <summary>
        /// Replaces first matched sequence of characters in the source text with a new sequence.
        /// Supports wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="oldSequence">The character sequence or pattern to be replaced. Supports wildcards and dynamic characters.</param>
        /// <param name="newSequence">The sequence of characters to insert in place of the matched pattern.</param>
        /// <param name="options">Optional flags to customize the replacement behavior (e.g., case sensitivity, match mode).</param>
        /// <returns>A new string with the matched sequences replaced by the specified new sequence.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="oldSequence"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="oldSequence"/> is empty or invalid.
        /// </exception>
        public string ReplaceFirst(string oldSequence, string newSequence, params byte[] options)
        {
            if (oldSequence == null) { throw new ArgumentNullException("OldSequence"); }
            if (oldSequence == "") { throw new ArgumentException("Can't OldSequence is empty!"); }
            if (newSequence == null) { throw new ArgumentNullException("NewSequence"); }
            if (newSequence == "") { throw new ArgumentException("Can't NewSequence is empty!"); }

            return insertCore(oldSequence,"", newSequence, 0, isReplaceFirst).ToString();
        }

        /// <summary>
        /// Replaces all matched sequence of characters in the source text with a new sequence.
        /// Supports wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="oldSequence">The character sequence or pattern to be replaced. Supports wildcards and dynamic characters.</param>
        /// <param name="newSequence">The sequence of characters to insert in place of the matched pattern.</param>
        /// <param name="options">Optional flags to customize the replacement behavior (e.g., case sensitivity, match mode).</param>
        /// <returns>A new string with the matched sequences replaced by the specified new sequence.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="oldSequence"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="oldSequence"/> is empty or invalid.
        /// </exception>
        public string Replace(string oldSequence, string newSequence, params byte[] options)
        {
            if (oldSequence == null) { throw new ArgumentNullException("OldSequence"); }
            if (oldSequence == "") { throw new ArgumentException("Can't OldSequence is empty!"); }
            if (newSequence == null) { throw new ArgumentNullException("NewSequence"); }
            if (newSequence == "") { throw new ArgumentException("Can't NewSequence is empty!"); }

            return insertCore(oldSequence,"", newSequence, 0, isReplace).ToString();
        }

        #endregion

        #region ▼ Translate text
        /// <summary>
        /// Replaces first words in the source text that match entries from <paramref name="oldSequence"/> with corresponding words from <paramref name="newSequence"/>.
        /// Each sequence is split by semicolons (';'), and replacements are matched by index order.
        /// </summary>
        /// <param name="oldSequence">A semicolon-separated list of words to be replaced.</param>
        /// <param name="newSequence">A semicolon-separated list of replacement words, matched by index to <paramref name="oldSequence"/>.</param>
        /// <param name="options">Optional flags to customize replacement behavior (e.g., case sensitivity, whole-word match).</param>
        /// <returns>A new string with all specified replacements applied.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="oldSequence"/>, or <paramref name="newSequence"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="oldSequence"/> and <paramref name="newSequence"/> have different word counts or contain invalid entries.
        /// </exception>
        public string TranslateFirst(string oldSequence, string newSequence, params byte[] options)
        {
            if (oldSequence == null) { throw new ArgumentNullException("OldSequence"); }
            if (oldSequence == "") { throw new ArgumentException("Can't OldSequence is empty!"); }
            if (newSequence == null) { throw new ArgumentNullException("NewSequence"); }
            if (newSequence == "") { throw new ArgumentException("Can't NewSequence is empty!"); }

            #region + Loop to replace all occurs
                        
            ReadOnlySpan<char> oldChars = oldSequence;
            ReadOnlySpan<char> newChars = newSequence;

            int oldCharsPos = 0;
            int newCharsPos = 0;

            while (oldCharsPos != -1)
            {
                oldCharsPos = oldChars.IndexOf(';');
                if( oldCharsPos == -1) { oldCharsPos = oldChars.Length; }

                newCharsPos = newChars.IndexOf(';');
                if (newCharsPos == -1) { newCharsPos = newChars.Length; }

                ReadOnlySpan<char> newText = insertCore(oldChars[..oldCharsPos],"", newChars[..newCharsPos], 0, isReplaceFirst);
                
                text = newText;

                if (oldCharsPos == oldChars.Length || newCharsPos == newChars.Length) { break; }

                oldCharsPos++;
                oldChars = oldChars[oldCharsPos..];

                newCharsPos++;
                newChars = newChars[newCharsPos..];
            }

            #endregion

            return text.ToString();
        }

        /// <summary>
        /// Replaces all words in the source text that match entries from <paramref name="oldSequence"/> with corresponding words from <paramref name="newSequence"/>.
        /// Each sequence is split by semicolons (';'), and replacements are matched by index order.
        /// </summary>
        /// <param name="oldSequence">A semicolon-separated list of words to be replaced.</param>
        /// <param name="newSequence">A semicolon-separated list of replacement words, matched by index to <paramref name="oldSequence"/>.</param>
        /// <param name="options">Optional flags to customize replacement behavior (e.g., case sensitivity, whole-word match).</param>
        /// <returns>A new string with all specified replacements applied.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="oldSequence"/>, or <paramref name="newSequence"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when <paramref name="oldSequence"/> and <paramref name="newSequence"/> have different word counts or contain invalid entries.
        /// </exception>
        public string Translate(string oldSequence, string newSequence, params byte[] options)
        {
            if (oldSequence == null) { throw new ArgumentNullException("OldSequence"); }
            if (oldSequence == "") { throw new ArgumentException("Can't OldSequence is empty!"); }
            if (newSequence == null) { throw new ArgumentNullException("NewSequence"); }
            if (newSequence == "") { throw new ArgumentException("Can't NewSequence is empty!"); }

            #region + Loop to replace all occurs

            ReadOnlySpan<char> oldChars = oldSequence;
            ReadOnlySpan<char> newChars = newSequence;

            int oldCharsPos = 0;
            int newCharsPos = 0;

            while (oldCharsPos != -1)
            {
                oldCharsPos = oldChars.IndexOf(';');
                if (oldCharsPos == -1) { oldCharsPos = oldChars.Length; }

                newCharsPos = newChars.IndexOf(';');
                if (newCharsPos == -1) { newCharsPos = newChars.Length; }

                ReadOnlySpan<char> newText = insertCore(oldChars[..oldCharsPos],"", newChars[..newCharsPos], 0, isReplace);

                text = newText;

                if (oldCharsPos == oldChars.Length || newCharsPos == newChars.Length) { break; }

                oldCharsPos++;
                oldChars = oldChars[oldCharsPos..];

                newCharsPos++;
                newChars = newChars[newCharsPos..];
            }

            #endregion

            return text.ToString();
        }

        #endregion

        #region ▼ Append in text

        /// <summary>
        /// Append sequences of characters into the source text at the specified position.
        /// </summary>
        /// <param name="sequenceToAppend">The sequence of characters to insert.</param>
        /// <returns>A new string with the appended sequence.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="sequenceToAppend"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// </exception>
        public string Append(string sequenceToAppend)
        {
            if (sequenceToAppend == null) { throw new ArgumentNullException("sequenceToInsert"); }

            int pos = text.Length -1;

            return insertCore("", "", sequenceToAppend, pos, isInsertOnly).ToString();
        }

        #endregion

        #endregion

        #region ▼ Edit Snippet

        #region ▼ Insert Snippet
        /// <summary>
        /// Inserts a snippet into the source text at the specified position.
        /// </summary>
        /// <param name="text">The source text where the snippet will be inserted.</param>
        /// <param name="snippetToInsert">The sequence of characters to insert.</param>
        /// <param name="position">The zero-based index in the source text where the snippet should be inserted.</param>
        /// <param name="options">Optional flags to customize insertion behavior (e.g., formatting, encoding, or validation rules).</param>
        /// <returns>A new string with the inserted snippet.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/> or <paramref name="snippetToInsert"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentOutOfRangeException">
        /// Thrown when <paramref name="position"/> is less than 0 or greater than the length of <paramref name="text"/>.
        /// </exception>
        public string InsertSnippet(string snippetToInsert, int position, params byte[] options)
        {
            if (snippetToInsert == null) { throw new ArgumentNullException("snippetToInsert"); }

            int[] pos = [position];

            return insertCore("", "", snippetToInsert, position, isInsertOnly, isSnippet, options).ToString();
        }

        /// <summary>
        /// Inserts a snippet into the source text before all occurrence of a matched snippet defined by opening and closing tag sequences.
        /// </summary>
        /// <param name="tagOpenToBefore">The character sequence representing the opening tag of the target snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="tagCloseToBefore">The character sequence representing the closing tag of the target snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="snippetToInsert">The snippet to insert before the matched target snippet.</param>
        /// <param name="options">Optional flags to customize insertion behavior (e.g., formatting, encoding, or validation rules).</param>
        /// <returns>A new string with the inserted snippet placed before the first matched target snippet.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="tagOpenToBefore"/>, <paramref name="tagCloseToBefore"/>, or <paramref name="snippetToInsert"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when any of the input parameters are empty or invalid.
        /// </exception>
        public string InsertSnippetBefore(string snippetToInsert, string tagOpenToBefore, string tagCloseToBefore, params byte[] options)
        {
            if (snippetToInsert == null) { throw new ArgumentNullException("SequenceToInsert"); }
            if (snippetToInsert == "") { throw new ArgumentException("Can't SnippetToInsert is empty!"); }
            if (tagOpenToBefore == null) { throw new ArgumentNullException("tagOpenToBefore"); }
            if (tagOpenToBefore == "") { throw new ArgumentException("Can't tagOpenToBefore is empty!"); }
            if (tagCloseToBefore == null) { throw new ArgumentNullException("tagCloseToBefore"); }
            if (tagCloseToBefore == "") { throw new ArgumentException("Can't tagCloseToBefore is empty!"); }

            return insertCore(tagOpenToBefore, tagCloseToBefore, snippetToInsert, 0, isInsertBefore, isSnippet).ToString();
        }

        /// <summary>
        /// Inserts a snippet into the source text after all occurrence of a matched snippet defined by opening and closing tag sequences.
        /// </summary>
        /// <param name="tagOpenToAfter">The character sequence representing the opening tag of the target snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="tagCloseToAfter">The character sequence representing the closing tag of the target snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="snippetToInsert">The snippet to insert before the matched target snippet.</param>
        /// <param name="options">Optional flags to customize insertion behavior (e.g., formatting, encoding, or validation rules).</param>
        /// <returns>A new string with the inserted snippet placed before the first matched target snippet.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="tagOpenToAfter"/>, <paramref name="tagCloseToAfter"/>, or <paramref name="snippetToInsert"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when any of the input parameters are empty or invalid.
        /// </exception>
        public string InsertSnippetAfter(string snippetToInsert, string tagOpenToAfter, string tagCloseToAfter, params byte[] options)
        {
            if (snippetToInsert == null) { throw new ArgumentNullException("SequenceToInsert"); }
            if (snippetToInsert == "") { throw new ArgumentException("Can't SnippetToInsert is empty!"); }
            if (tagOpenToAfter == null) { throw new ArgumentNullException("tagOpenToAfter"); }
            if (tagOpenToAfter == "") { throw new ArgumentException("Can't tagOpenToAfter is empty!"); }
            if (tagCloseToAfter == null) { throw new ArgumentNullException("tagCloseToAfter"); }
            if (tagCloseToAfter == "") { throw new ArgumentException("Can't tagCloseToAfter is empty!"); }

            return insertCore(tagOpenToAfter, tagCloseToAfter, snippetToInsert, 0, isInsertAfterFirst, isSnippet).ToString();
        }

        #endregion

        #region ▼ Remove Snippet

        /// <summary>
        /// Removes a first snippet from the source text, defined by matching opening and closing tag sequences.
        /// Supports wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="tagOpenOfSnippet">The character sequence representing the opening tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="tagCloseOfSnippet">The character sequence representing the closing tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="options">Optional flags to customize removal behavior (e.g., case sensitivity, nested snippet handling).</param>
        /// <returns>A new string with the matched snippet removed.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="text"/>, <paramref name="tagOpenOfSnippet"/>, or <paramref name="tagCloseOfSnippet"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when any of the input parameters are empty or invalid.
        /// </exception>
        public string RemoveSnippetFirst(string tagOpenOfSnippet, string tagCloseOfSnippet, params byte[] options)
        {
            if (tagOpenOfSnippet == null) { throw new ArgumentNullException("tagOpenOfSnippet"); }
            if (tagOpenOfSnippet == "") { throw new ArgumentException("Can't tagOpenOfSnippet is empty!"); }
            if (tagCloseOfSnippet == null) { throw new ArgumentNullException("tagCloseOfSnippet"); }
            if (tagCloseOfSnippet == "") { throw new ArgumentException("Can't tagCloseOfSnippet is empty!"); }

            return insertCore(tagOpenOfSnippet, tagCloseOfSnippet, "", 0, isRemoveFirst, isSnippet).ToString();
        }

        /// <summary>
        /// Removes all snippets from the source text, defined by matching opening and closing tag sequences.
        /// Supports wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="tagOpenOfSnippet">The character sequence representing the opening tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="tagCloseOfSnippet">The character sequence representing the closing tag of the snippet. Supports wildcards and dynamic characters.</param>
        /// <param name="options">Optional flags to customize removal behavior (e.g., case sensitivity, nested snippet handling).</param>
        /// <returns>A new string with the matched snippet removed.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="tagOpenOfSnippet"/>, or <paramref name="tagCloseOfSnippet"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when any of the input parameters are empty or invalid.
        /// </exception>
        public string RemoveSnippet(string tagOpenOfSnippet, string tagCloseOfSnippet, params byte[] options)
        {
            if (tagOpenOfSnippet == null) { throw new ArgumentNullException("tagOpenOfSnippet"); }
            if (tagOpenOfSnippet == "") { throw new ArgumentException("Can't tagOpenOfSnippet is empty!"); }
            if (tagCloseOfSnippet == null) { throw new ArgumentNullException("tagCloseOfSnippet"); }
            if (tagCloseOfSnippet == "") { throw new ArgumentException("Can't tagCloseOfSnippet is empty!"); }

            return insertCore(tagOpenOfSnippet, tagCloseOfSnippet, "", 0, isRemove, isSnippet).ToString();
        }

        #endregion

        #region ▼ Replace Snippet

        /// <summary>
        /// Replaces first snippet in the source text that are defined by matching opening and closing tag sequences.
        /// Supports wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="tagOpenOfSnippet">The character sequence representing the opening tag of the snippet to replace. Supports wildcards and dynamic characters.</param>
        /// <param name="tagCloseOfSnippet">The character sequence representing the closing tag of the snippet to replace. Supports wildcards and dynamic characters.</param>
        /// <param name="newSnippet">The snippet to insert in place of each matched snippet.</param>
        /// <returns>A new string with all matched snippets replaced by the specified <paramref name="newSnippet"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="tagOpenOfSnippet"/>, <paramref name="tagCloseOfSnippet"/>, or <paramref name="newSnippet"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when any of the input parameters are empty or invalid.
        /// </exception>
        public string ReplaceSnippetFirst(string tagOpenOfSnippet, string tagCloseOfSnippet, string newSnippet, params byte[] options)
        {
            if (newSnippet == null) { throw new ArgumentNullException("SnippetToInsert"); }
            if (newSnippet == "") { throw new ArgumentException("Can't SnippetToInsert is empty!"); }
            if (tagOpenOfSnippet == null) { throw new ArgumentNullException("tagOpenOfSnippet"); }
            if (tagOpenOfSnippet == "") { throw new ArgumentException("Can't tagOpenOfSnippet is empty!"); }
            if (tagCloseOfSnippet == null) { throw new ArgumentNullException("TagCloseOfSnippet"); }
            if (tagCloseOfSnippet == "") { throw new ArgumentException("Can't TagCloseOfSnippet is empty!"); }

            return insertCore(tagOpenOfSnippet, tagCloseOfSnippet, newSnippet, 0, isReplaceFirst, isSnippet).ToString();
        }

        /// <summary>
        /// Replaces all snippets in the source text that are defined by matching opening and closing tag sequences.
        /// Supports wildcard patterns and dynamic characters.
        /// </summary>
        /// <param name="tagOpenOfSnippet">The character sequence representing the opening tag of the snippet to replace. Supports wildcards and dynamic characters.</param>
        /// <param name="tagCloseOfSnippet">The character sequence representing the closing tag of the snippet to replace. Supports wildcards and dynamic characters.</param>
        /// <param name="newSnippet">The snippet to insert in place of each matched snippet.</param>
        /// <returns>A new string with all matched snippets replaced by the specified <paramref name="newSnippet"/>.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when <paramref name="tagOpenOfSnippet"/>, <paramref name="tagCloseOfSnippet"/>, or <paramref name="newSnippet"/> is <c>null</c>.
        /// </exception>
        /// <exception cref="ArgumentException">
        /// Thrown when any of the input parameters are empty or invalid.
        /// </exception>
        public string ReplaceSnippet( string tagOpenOfSnippet, string tagCloseOfSnippet, string newSnippet, params byte[] options)
        {
            if (newSnippet == null) { throw new ArgumentNullException("SnippetToInsert"); }
            if (newSnippet == "") { throw new ArgumentException("Can't SnippetToInsert is empty!"); }
            if (tagOpenOfSnippet == null) { throw new ArgumentNullException("tagOpenOfSnippet"); }
            if (tagOpenOfSnippet == "") { throw new ArgumentException("Can't tagOpenOfSnippet is empty!"); }
            if (tagCloseOfSnippet == null) { throw new ArgumentNullException("TagCloseOfSnippet"); }
            if (tagCloseOfSnippet == "") { throw new ArgumentException("Can't TagCloseOfSnippet is empty!"); }

            return insertCore(tagOpenOfSnippet, tagCloseOfSnippet, newSnippet, 0, isReplace, isSnippet).ToString();
        }

        #endregion

        #endregion

        #region » Enable Match params

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void enableMatchParams(ref TextMatcher builder, params byte[] options)
        {
            if (options.Contains(TextOpt.CaseSensitive)) { builder.CaseSensitive = true; }
            if (options.Contains(TextOpt.IgnoreCharsInQuotes)) { builder.IgnoreCharsInQuotes = false; }
            if (options.Contains(TextOpt.IgnoreCharsInDoubleQuotes)) { builder.IgnoreCharsInDoubleQuotes = false; }
            if (options.Contains(TextOpt.IgnoreDynamicChars)) { builder.IgnoreDynamicChars = false; }
            if (options.Contains(TextOpt.MatchGreedyOccurences)) { builder.MatchGreedyOccurences = true; }
        }

        #endregion

        public void Dispose()
        {
            // Implement IDisposable if needed
        }
    }

    #endregion

    #region ▼ ValueList 
    public ref struct ValueList<T>
    {
        #region ▼ Properties

        private Span<T> _span;
        private T[] _pooledArray; // Array vindo do pool (para devolver depois)
        public int Length;
        private int _capacity;

        private const int DefaultStackLimit = 16;

        #endregion

        #region ► Constructor
        // Buffer na stack
        public ValueList(Span<T> stackBuffer)
        {
            _span = stackBuffer;
            _pooledArray = null;
            _capacity = stackBuffer.Length;
            Length = 0;
        }
        #endregion

        #region ► Add
        public void Add(T item)
        {
            if (Length >= _capacity)
                Grow();

            _span[Length++] = item;
        }
        #endregion

        #region ► Grow
        private void Grow()
        {
            int newCapacity = _capacity == 0 ? 4 : _capacity * 2;

            // Pegar array do pool
            var newArray = ArrayPool<T>.Shared.Rent(newCapacity);

            // Copiar dados atuais
            _span.Slice(0, Length).CopyTo(newArray);

            // Atualizar referências
            _pooledArray = newArray;
            _span = newArray;
            _capacity = newCapacity;
        }
        #endregion

        #region ► Dispose
        // ❗ Muito importante: devolver ao pool
        public void Dispose()
        {
            if (_pooledArray != null)
            {
                ArrayPool<T>.Shared.Return(_pooledArray);
                _span = default;
                Length = 0;
                _capacity = 0;
            }
        }
        #endregion

        #region ► Clear
        // Limpar sem devolver o array (reutiliza o buffer)
        public void Clear()
        {
            Length = 0;
        }
        #endregion

        public ReadOnlySpan<T> AsSpan() => _span.Slice(0, Length);
        public int Count => Length;

        public T this[int index]
        {
            get => _span[index];
            set => _span[index] = value;
        }
    }
    #endregion
}
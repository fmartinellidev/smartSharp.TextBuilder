using System.Buffers;
using System.Text;

namespace SmartSharp.TextBuilder
{
    #region ▼ TextOptions

    [Flags]
    public enum TextOptions
    {
        /// <summary>
        /// When use "*" pattern, consider end of word. The finder occurs is stoped in end of word.
        /// <para>Ex: "The name is John Doe Silva", patern is "jo*", return only "John". Without parameter, returns "John Doe Silva"</para>
        /// </summary>
        PatternByWord = 1,

        /// <summary>
        /// <para>In match not returns duplicate occurs.</para>
        /// <para>Only removes first occurs of duplicate text.</para>
        /// </summary>
        Distinct = 2,

        /// <summary>
        /// Don't ignore content in single quotes('').
        /// <para>Return if find text in single quotes</para>
        /// </summary>
        IgnoreInSingleQuotes = 3,

        /// <summary>
        /// Return snippets without open and close tags.
        /// <para>Ex: "Extract a snippet of ->text<-", result="text"</para>
        /// </summary>
        RemoveOpenCloseTags = 4,

        /// <summary>
        /// <para>Accept cross matches occurrences to return.</para>
        /// <para>After match an occurrence, will start a new search starting second char of it, so can find a new occurs that 
        /// can  started inside previous occurs. By default, the new searches start after previeous occur end. 
        /// </summary>
        CrossMatches = 5,

        /// <summary>
        /// The list of matches return sorted by position in text
        /// </summary>
        OrderByPosition = 6,

        /// <summary>
        /// The list of matches return sorted by word alphabetical order
        /// </summary>
        OrderByWord = 7,

        /// <summary>
        /// Return occurrence only with parts building in pattern(wildcard).
        /// <para>Return occurrences only pattern parts</para>
        /// </summary>
        OnlyCharsInPattern = 8
    }

    #endregion

    #region ▼ PatternBuffer

    ref struct PatternBuffer
    {
        #region ▼ Properties

        public int TextCount = 0;
        public int WildcardCount = 0;

        #region • Patterns

        private List<char[]> patterns = new();

        #endregion

        #region • PatternSymbol
        private char PatternSymbol = '*';
        #endregion

        #region • Done
        public bool Done
        {
            get
            {
                if (notCaptured == 0) { return true; }
                else { return false; }
            }
        }
        #endregion

        #region • CaptureInStart
        private bool captureInStart = false;
        public bool CaptureInStart
        { get { return captureInStart; } }

        #endregion

        #region • InCapture
        private bool inCapture = false;
        public bool InCapture
        { get { return inCapture; } }

        #endregion

        #region • CaptureInEnd
        private bool captureInEnd = false;
        public bool CaptureInEnd
        { get { return captureInEnd; } }

        #endregion

        #region • Captured
        public void Captured()
        {
            if (notCaptured == 0) { return; }
            notCaptured--;
            index++;

            if (!Done) { inCapture = true; }
            else { inCapture = false; }
        }
        #endregion

        #region • NotCaptured
        public int NotCaptured { get { return notCaptured; } }
        private int notCaptured = 0;
        #endregion

        #region • Index

        public int index = 0;

        #endregion

        #region ► IsEnd
        public bool IsEnd
        {
            get
            {
                return index >= patterns.Count() - 1;
            }
        }
        #endregion

        #region ► IsFirst
        public bool IsFirst
        {
            get { return index == 0; }
        }
        #endregion

        #region ► Reset
        public void Reset() { index = 0; notCaptured = patterns.Count(); }
        #endregion

        #region ► Count
        public int Count => patterns.Count();

        #endregion

        #region ► Text

        public ReadOnlySpan<char> Text
        {
            get
            {
                if (index >= patterns.Count) { return ""; }
                return patterns[index];

                //if (patterns[index][0] ==PatternSymbol)
                //{ 
                //    if (index < patterns.Count()) { return this[index + 1]; }
                //    else {  return ""; }
                //}
                //else { return this[index]; }
            }
        }

        #endregion

        #region ► Length

        public int Length => Text.Length;

        #endregion

        #endregion

        #region ► Initialize

        public PatternBuffer(string pattern)
        {
            if (pattern != null)
            {
                if (pattern.Length > 0)
                { Split(pattern); index = 0; }
                else
                { throw new ArgumentException("TextToSplit is empty!"); }
            }
            else
            { throw new ArgumentException("textToSplit is null!"); }

        }

        public PatternBuffer(ReadOnlySpan<char> pattern)
        {
            if (!pattern.IsEmpty)
            {
                if (pattern.Length > 0)
                { Split(pattern); index = 0; }
                else
                { throw new ArgumentException("TextToSplit is empty!"); }
            }
            else
            { throw new ArgumentException("textToSplit is null!"); }

        }

        public PatternBuffer(string pattern, char patternSymbol)
        {
            if (pattern != null)
            {
                if (pattern.Length > 0)
                { Split(pattern); PatternSymbol = patternSymbol; index = 0; }
                else
                { throw new ArgumentException("TextToSplit is empty!"); }
            }
            else
            { throw new ArgumentException("textToSplit is null!"); }
        }

        #endregion

        #region ► Controller

        public string this[int _index]
        {
            get
            {
                if (_index < 0 || _index >= Count)
                    throw new IndexOutOfRangeException();
                return new string(patterns[_index]);
            }
        }

        #endregion

        #region ▼ Methods

        #region ► SplitWords

        private void Split(ReadOnlySpan<char> pattern)
        {
            int _startIndex = 0;
            List<ReadOnlyMemory<char>> _tempMatches = new();
            int length = 1;

            if (pattern[0] == PatternSymbol)
            { _startIndex++; captureInStart = true; inCapture = true; }

            for (int i = _startIndex; i < pattern.Length; i++)
            {
                if (pattern[i] == PatternSymbol || i == pattern.Length - 1)
                {
                    if (pattern[_startIndex + length - 1] == PatternSymbol) { length--; }
                    char[] literal = pattern.Slice(_startIndex, length).ToArray();
                    patterns.Add(literal);
                    notCaptured++;

                    _startIndex = i + 1;
                    length = 0;
                }

                length++;
            }

            if (pattern[pattern.Length - 1] == PatternSymbol)
            { captureInEnd = true; }
        }

        #endregion

        #endregion
    }

    #endregion

    #region ▼ MatchesBuffer

    #region ▼ StringAndPosition

    public class StringAndPosition
    {
        public int Position { get; set; }
        public char[] Text { get; set; }

        public StringAndPosition(ReadOnlySpan<char> text, int position)
        {
            Position = position;
            Text = text.ToArray();
        }

        public StringAndPosition()
        {
        }

        public void Set(ReadOnlySpan<char> text, int position)
        {
            Position = position;
            Text = text.ToArray();
        }
    }

    #endregion

    #region ▼ StringOrPattern

    public class StringOrPattern
    {
        public bool IsPattern { get; set; }
        public char[] Text { get; set; }

        public StringOrPattern(ReadOnlySpan<char> text, bool isPattern)
        {
            IsPattern = isPattern;
            Text = text.ToArray();
        }

        public StringOrPattern()
        {
        }
    }

    #endregion

    #region ▼ MatchEntry

    public abstract class MatchEntry
    {
        #region • Count
        public abstract int Count { get; }
        #endregion

        #region • LengthTotal
        public virtual int LengthTotal { get; }
        #endregion

        #region • Index
        public virtual int Index { get; set; }
        #endregion

        #region • Text
        public abstract string Text { get; }
        public virtual string GetText(int index)
        { return ""; }
        #endregion

        #region • Position
        public virtual int Position { get; }
        public virtual int GetPosition(int index)
        { throw new InvalidOperationException("Position is not available to 'ToMatchString'!"); }
        #endregion

        #region • IsPattern
        public virtual bool IsPattern { get; }
        public virtual bool GetIsPattern(int index)
        { throw new InvalidOperationException("The IsPattern is available only to 'ToMatchStrings'!"); }
        #endregion

        #region • This
        public virtual string this[int index]
        { get { throw new InvalidOperationException("This property is available only to 'ToMatchStrings' collection!"); } }
        #endregion

        #region • StringAndPosition
        public virtual StringAndPosition Matched { get; }
        public virtual StringAndPosition GetMatched(int index)
        { throw new InvalidOperationException("The Matched method is not available to 'ToMatchStrings' collection!"); }
        #endregion

        #region • Add
        public virtual void Add(ReadOnlySpan<char> text, int position)
        { throw new InvalidOperationException("The Add method is not available to MatchString collection!"); }
        #endregion

        #region • Set
        public virtual void Set(ReadOnlySpan<char> text, int position)
        { throw new InvalidOperationException("Set method only avaliable to MatchString collection!"); }
        #endregion
    }

    #region + MatchString
    public class MatchString : MatchEntry
    {
        #region ▼ Properties

        #region • Data

        private StringAndPosition match;

        #endregion

        #region • Count

        public override int Count
        {
            get
            {
                if (match != null)
                { return 1; }
                return 0;
            }
        }

        #endregion

        #region • Text
        public override string Text
        {
            get { return new string(match.Text); }
        }

        #endregion

        #region • Position
        public override int Position
        {
            get
            {
                if (match != null)
                { return match.Position; }
                return -1;
            }
        }

        #endregion

        #region • Matched
        public override StringAndPosition Matched
        {
            get
            {
                if (match != null)
                { return match; }
                return null;
            }
        }

        #endregion

        #endregion

        #region ▼ Initialize

        public MatchString()
        {
            match = new StringAndPosition("", -1);
        }

        public MatchString(string text, int position)
        {
            match = new StringAndPosition((string)text, position);
        }

        #endregion

        #region ▼ Methods

        public override void Set(ReadOnlySpan<char> text, int position)
        {
            match.Text = text.ToArray();
            match.Position = position;
        }

        #endregion
    }

    #endregion

    #region + MatchesList
    public class MatchesList : MatchEntry
    {
        #region ▼ Properties

        #region • Data

        private List<StringAndPosition> matches;

        #endregion

        #region • Count

        public override int Count
        {
            get
            {
                if (matches != null) { return matches.Count; }
                return -1;
            }
        }

        #endregion

        #region • LengthTotal

        private int lengthTotal = 0;
        public override int LengthTotal
        { get { return lengthTotal; } }

        #endregion

        #region • OrderByPosition

        private bool orderByPosition { get; set; }

        #endregion

        #region • OrderByWord

        private bool orderByWord { get; set; }

        #endregion

        #region • Index

        public override int Index { get; set; }

        #endregion

        #region • Text
        public override string Text
        {
            get
            {
                if (matches == null)
                { return ""; }

                return new string(matches[Index].Text);
            }
        }
        #endregion

        #region • Position
        public override int Position
        {
            get
            {
                if (matches == null)
                { return -1; }

                return matches[Index].Position;
            }
        }
        #endregion

        #region • Matched

        public override StringAndPosition Matched
        { get { return matches[Index]; } }

        #endregion

        #endregion

        #region ▼ Initialize

        public MatchesList()
        {
            orderByPosition = false;
            orderByWord = false;
            matches = new List<StringAndPosition>();
        }

        public MatchesList(TextOptions option)
        {
            if (option.HasFlag(TextOptions.OrderByPosition))
            { orderByPosition = true; orderByWord = false; }
            else if (option.HasFlag(TextOptions.OrderByWord))
            { orderByWord = true; orderByPosition = false; }
            else { orderByWord = false; orderByPosition = false; }
            matches = new List<StringAndPosition>();
        }

        #endregion

        #region ▼ Controller

        public override string GetText(int index)
        {
            if (matches == null)
            { return ""; }

            return new string(matches[index].Text);
        }

        public override int GetPosition(int index)
        {
            if (matches == null)
            { return -1; }

            return matches[index].Position;
        }

        public override StringAndPosition GetMatched(int index)
        { return matches[index]; }

        #endregion

        #region ▼ Methods

        #region ► Add

        public override void Add(ReadOnlySpan<char> text, int position)
        {
            #region ? Order by position

            lengthTotal += text.Length;

            if (matches.Count > 0 && orderByPosition)
            {
                if (matches[0].Position > position)
                { matches.Insert(0, new StringAndPosition(text, position)); }
            }

            #endregion

            #region ? Order by word

            else if (matches.Count > 0 && orderByPosition)
            {
                if (matches[0].Text[0] > text[0])
                { matches.Insert(0, new StringAndPosition(text, position)); }
            }

            #endregion

            #region ! Add match without order

            else
            { matches.Add(new StringAndPosition(text, position)); }

            #endregion
        }

        #endregion

        #endregion
    }

    #endregion

    #region + ToMatchStrings
    public class ToMatchStrings : MatchEntry
    {
        #region ▼ Properties

        #region • Data

        private List<StringOrPattern> toMatches;

        #endregion

        #region • Count

        public override int Count
        {
            get
            {
                if (toMatches != null) { return toMatches.Count; }
                return 0;
            }
        }

        #endregion

        #region • LengthTotal

        private int lengthTotal = 0;
        public override int LengthTotal
        { get { return lengthTotal; } }

        #endregion

        #region • Index

        public override int Index { get; set; }

        #endregion

        #region • IsPattern

        public override bool IsPattern
        {
            get
            {
                if (toMatches != null)
                { return toMatches[Index].IsPattern; }

                return false;
            }
        }

        #endregion

        #region • Text

        public override string Text
        {
            get { return new string(toMatches[Index].Text); }
        }

        #endregion

        #endregion

        #region ▼ Initialize

        public ToMatchStrings(string stringsToList)
        {
            toMatches = new List<StringOrPattern>(10);
            Split(stringsToList + '|');
        }

        public ToMatchStrings(string stringsToList, char separador = '|')
        {
            toMatches = new List<StringOrPattern>(10);
            Split(stringsToList + separador, separador);
        }

        #endregion

        #region ▼ Controller
        public override string this[int index]
        {
            get
            {
                if (toMatches == null) { return ""; }
                return new string(toMatches[index].Text);
            }
        }

        #endregion

        #region ▼ Methods

        #region ► GetText

        public override string GetText(int index)
        {
            return new string(toMatches[index].Text);
        }

        #endregion

        #region ► GetIsPattern

        public override bool GetIsPattern(int index)
        {
            if (toMatches != null)
            { return toMatches[index].IsPattern; }

            return false;
        }

        #endregion

        #region ► Add

        public override void Add(ReadOnlySpan<char> text, int position)
        {
            bool isPattern = text.Contains('*');
            toMatches.Add(new StringOrPattern(text.ToArray(), isPattern));
        }

        #endregion

        #region ► SplitWords

        private void Split(ReadOnlySpan<char> texts, char separador = '|')
        {
            int _startIndex = 0;
            List<ReadOnlyMemory<char>> _tempMatches = new();
            bool isPattern = false;
            int length = 0;

            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] == '*' && !isPattern) isPattern = true;

                if (texts[i] == separador)
                {
                    #region ? Separator in first char
                    if (i == 0) { continue; }
                    #endregion

                    length = i - _startIndex;
                    lengthTotal += length;
                    toMatches.Add(new StringOrPattern(texts.Slice(_startIndex, length).ToArray(), isPattern));
                    _startIndex = i + 1;
                    isPattern = false;
                }
            }
        }

        #endregion

        #endregion
    }

    #endregion

    #endregion

    #endregion

    #region ▼ TextBuilder

    public class TextBuilder
    {
        #region ▼ Properties

        #region • SourceText

        private ReadOnlyMemory<char> sourceText;

        #endregion

        #region ▼ Params

        private bool ignoreApostrophes = false;
        private bool byWord = false;
        private bool distinct = false;
        private bool crossMatches = false;
        private bool removeTags = false;
        private bool orderByPosition = false;
        private bool orderByWord = false;
        private bool onlyPatternParts = false;

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
            ignoreApostrophes = false;
            byWord = false;
            distinct = false;
            crossMatches = false;
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
        public MatchEntry MatchFirst(string stringsToMatch, TextOptions option = default)
        {
            return executeMatch(sourceText.Span, stringsToMatch, 0, 1, option);
        }

        /// <summary>
        /// Match first text in string. Not case sentive.
        /// </summary>
        /// <param name="stringsToMatch">Texts to search in source string.
        /// <para>Use "|" to split various texts. Ex:"john|marie|Jack"</para></param>
        /// <param name="options">Parameters of the search in 'Options' internal class</param>
        /// <returns>Return string of first occurs without case sensitive.</returns>
        private MatchEntry executeMatch(ReadOnlySpan<char> sourceText, string stringsToMatch, int startIndex, int count, TextOptions option)
        {
            if (stringsToMatch == "") { return default; }

            #region + Params           

            setParams(option);

            #endregion

            #region + List words to matches

            MatchEntry listToMatch = new ToMatchStrings(stringsToMatch);

            #endregion

            #region + Matches collection

            MatchEntry occurs;
            if (count == 1)
            { occurs = new MatchString(); }
            else { occurs = new MatchesList(); }

            #endregion      

            for (int i = 0; i < listToMatch.Count; i++)
            {
                #region ? First simple match

                if (!listToMatch.GetIsPattern(i) && count == 1)// If this word not a pattern match
                {
                    StringAndPosition _occur = matchFirst(sourceText, listToMatch[i], startIndex, ignoreApostrophes);
                    if (_occur == null) { return null; }
                    if (_occur.Position < occurs.Position || occurs.Position == -1)
                    { occurs.Set(_occur.Text, _occur.Position); }
                }
                
                #endregion

                #region ? Pattern first match
                else if (listToMatch.GetIsPattern(i) && count == 1)
                {
                    StringAndPosition _occur = matchFirstPattern(sourceText, listToMatch[i], startIndex,
                                                                 ignoreApostrophes, byWord, onlyPatternParts);
                    if (_occur == null) { return null; }
                    if (_occur.Position < occurs.Position || occurs.Position == -1)
                    { occurs.Set(_occur.Text, _occur.Position); }
                }
                #endregion

                #region ? Simple matches
                else if (!listToMatch.GetIsPattern(i) && count > 1)
                {
                    match(sourceText, listToMatch[i], ref occurs, startIndex, count,
                           ignoreApostrophes, byWord, distinct, crossMatches, removeTags);
                }
                #endregion

                #region ! Pattern matches
                else
                {

                }
                #endregion
            }

            if (occurs.Count == 0) { return default; }
            return occurs;
        }

        #endregion

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

        #region ▼ Matches Models

        #region » SetParams

        private void setParams(TextOptions option)
        {
            if (option.HasFlag(TextOptions.IgnoreInSingleQuotes))
            { ignoreApostrophes = true; }
            else { ignoreApostrophes = false; }

            if (option.HasFlag(TextOptions.Distinct))
            { distinct = true; }
            else { distinct = false; }

            if (option.HasFlag(TextOptions.OrderByPosition))
            { orderByPosition = true; }
            else { orderByPosition = false; }

            if (option.HasFlag(TextOptions.OrderByWord))
            { orderByWord = true; }
            else { orderByWord = false; }

            if (option.HasFlag(TextOptions.CrossMatches))
            { crossMatches = true; }
            else { crossMatches = false; }

            if (option.HasFlag(TextOptions.PatternByWord))
            { byWord = true; }
            else { byWord = false; }

            if (option.HasFlag(TextOptions.OnlyCharsInPattern))
            { onlyPatternParts = true; }
            else { onlyPatternParts = false; }
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

        private static StringAndPosition matchFirstPattern(ReadOnlySpan<char> sourceTextSpan, ReadOnlySpan<char> matchSpanText,
                                                   int startIndex, bool ignoreApostrophes, bool byWord, bool onlyPatternParts)
        {
            PatternBuffer pattern = new PatternBuffer(matchSpanText);

            #region + Flags

            bool ignoreIt = false;
            int initOccur = -1;
            int length = -1;

            #endregion

            StringAndPosition occur = new StringAndPosition();
            if (pattern.CaptureInStart) { initOccur = 0; length = 0; }

            for (int pos = startIndex; pos < sourceTextSpan.Length; ++pos)
            {
                #region + Lengths of source text and pattern

                int segmentLength = pattern.Text.Length;
                int sourceTextLength = sourceTextSpan.Length;
                if (pos + segmentLength > sourceTextLength) { break; }

                #endregion

                #region + Ignore apostrophes

                if (ignoreApostrophes)
                {
                    if (sourceTextSpan[pos] == '\'') { ignoreIt = !ignoreIt; continue; }
                    if (ignoreIt) continue;
                }

                #endregion

                bool equalSequence = sourceTextSpan.Slice(pos, segmentLength).SequenceEqual(pattern.Text);

                if (
                     (equalSequence && pattern.Text != "") || //Found equal sequence
                     (IsSeparator(sourceTextSpan[pos]) && pattern.IsEnd && byWord) || //If By word end pattern
                     (pattern.CaptureInEnd && pattern.IsEnd)
                   )
                {
                    #region + Found literal occurence

                    if (equalSequence)
                    {
                        if (length == -1) { length = 0; }

                        if (onlyPatternParts)
                        { pos += segmentLength; initOccur = pos; }
                        else
                        {
                            length += segmentLength;
                            if (!pattern.InCapture) { initOccur = pos; }
                            pos += segmentLength;
                        }

                        pattern.Captured();
                    }

                    #endregion

                    #region + CaptureEnd and end of pattern

                    if (pattern.IsEnd && pattern.Done)
                    {
                        if (pattern.CaptureInEnd) { length = sourceTextLength - initOccur; }
                        return new StringAndPosition(sourceTextSpan.Slice(initOccur, length), initOccur);
                    }

                    #endregion
                }
                else
                {
                    if (!pattern.InCapture) initOccur = pos + 1;
                    if (pattern.InCapture) length++;
                }
            }

            return null;
        }

        #endregion

        #region » match
        private static void match(ReadOnlySpan<char> sourceTextSpan, ReadOnlySpan<char> matchSpanText,
                                  ref MatchEntry occurs, int startIndex, int count, bool ignoreApostrophes,
                                  bool byWord, bool distinct, bool crossMatches, bool removeTags)
        {
            bool ignoreIt = false;
            int posIndex = 0;

            for (int pos = startIndex; pos < sourceTextSpan.Length; ++pos)
            {
                if (pos + matchSpanText.Length > sourceTextSpan.Length) { break; }

                if (ignoreApostrophes)
                {
                    if (sourceTextSpan[pos] == '\'') { ignoreIt = !ignoreIt; continue; }
                    if (ignoreIt) continue;
                }

                if (sourceTextSpan.Slice(pos, matchSpanText.Length).SequenceEqual(matchSpanText))
                {
                    //resultMatches[posIndex++] =
                    occurs.Add(matchSpanText.ToString(), pos);

                    if (distinct || count == 1) { break; }

                    posIndex = pos;
                    pos += matchSpanText.Length;
                }
            }

            return;
        }

        #endregion

        #region » matchFirstPattern



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

        #region » breakByWord

        //private static char breakOrContinueByWord(ref ReadOnlySpan<char> sourceText, ref PatternBuffer pattern,
        //                                          ref StringAndPosition occur, int pos, ref int initOccur)
        //{
        //    char lastChar = '\0';
        //    char nextChar = '\0';
        //    int length = 0;

        //    if (IsSeparator(sourceText[pos]))
        //    {
        //        if (pos != 0) { lastChar = sourceText[pos - 1]; }
        //        else { lastChar = '\0'; }
        //        if ((pos + 1) < sourceText.Length)
        //        { nextChar = sourceText[pos + 1]; }
        //        else { nextChar = '\0'; }

        //        if (!IgnoreSeparator(sourceText[pos], lastChar, nextChar))
        //        {
        //            if (pattern.IsEnd)
        //            {
        //                #region + End of pattern

        //                length = pos - initOccur;
        //                occur.Set(sourceText.Slice(initOccur, length).ToString(), initOccur);

        //                if (doneMatch(ref pattern, ref initOccur, ref pos, length))
        //                { return 'b'; }

        //                #endregion
        //            }
        //            else if (pattern.IsFirst)
        //            {
        //                #region + Start of pattern

        //                initOccur = pos + 1; return 'c';

        //                #endregion
        //            }
        //            else
        //            { initOccur = pos + 1; return 'b'; }
        //        }
        //    }

        //    return '\0';
        //}

        #endregion

        #region » doneMatch

        //private static bool doneMatch(ref PatternBuffer pattern, ref int initOccur, ref int pos, int length)
        //{
        //    if (distinct || count == 1)
        //    { return true; }
        //    else if (crossMatches)
        //    {
        //        #region + CrossMatches

        //        startIndex = initOccur = 1;
        //        pos = startIndex;
        //        pattern.Reset();
        //        if (pattern.InCapture && !byWord) { initOccur = startIndex; }
        //        else { initOccur = -1; }

        //        #endregion
        //    }
        //    else
        //    {
        //        #region + Next occur after current occur

        //        startIndex = initOccur + length;
        //        pos = startIndex;
        //        pattern.Reset();
        //        if (pattern.InCapture && !byWord) { initOccur = startIndex; }
        //        else { initOccur = -1; }

        //        #endregion
        //    }

        //    return false;
        //}

        #endregion

        #region » endOfPatternWildcard

        private bool endOfPatternWildcard(ref ReadOnlySpan<char> sourceText, ref MatchEntry occurs, bool inCapture, int length, int initOccur)
        {
            #region + Early end of capture

            if (!byWord)
            {
                if (inCapture && !byWord) { length = sourceText.Length - initOccur; }
                occurs.Add(sourceText.Slice(initOccur, length).ToString(), initOccur);
                return true;
            }
            else { return false; }

            #endregion
        }

        #endregion

        #region » EndOfMatch

        //private bool endOfMatch(ref ReadOnlySpan<char> sourceText, ref PatternBuffer pattern, int pos, int length, int initOccur)
        //{
        //    if (!pattern.InCapture)
        //    {
        //        #region + Add occur in Occurs StringBuffer

        //        int segmentLength = pattern.Length;
        //        length = (pos - initOccur) + segmentLength;
        //        occurs.Add(sourceText.Slice(initOccur, length).ToString(), initOccur);

        //        #endregion
        //    }
        //    else
        //    {
        //        #region + Early end of capture

        //        if (!endOfPatternWildcard( ref sourceText, pattern.InCapture, length, initOccur))
        //        { return false; }

        //        #endregion
        //    }

        //    #region ? Not duplicate or only one occurs

        //    if (doneMatch(ref pattern, ref initOccur, ref pos, length))
        //    { return true; }

        //    #endregion

        //    return false;
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
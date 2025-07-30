using Microsoft.VisualBasic.FileIO;
using System;
using System.Buffers;
using System.Globalization;
using System.Text;
using System.Xml.Linq;
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
            #region ► Properties

            #region ► Empty
            public bool Empty
            {
                get
                {
                    if (Patterns.Length == 0)
                    { return true; }
                    else { return false; }
                }
            }

            #endregion

            #region • Position

            private Span<int> Position;

            #endregion

            #region • Lengths

            private Span<int> Lengths;

            #endregion
                       
            #region • Patterns

            public ReadOnlySpan<char> Patterns { get; set; }

            #endregion

            #region • Count
            public int Count { get { return Position.Length; } }

            #endregion

            #region • ContainsWildcard

            private Span<bool> containsWildcard;
            public bool ContainsWildcards (int index)
            {
                    if (containsWildcard.Length == 0 || index >= containsWildcard.Length )
                    { return false; }
                    else { return containsWildcard[index]; }
            }

            #endregion

            #region • ContainsOr

            private Span<bool> containsOr;
            public bool ContainsOr(int index)
            {
                if (containsOr.Length == 0 || index >= containsOr.Length)
                { return false; }
                else { return containsOr[index]; }
            }

            #endregion

            #region • ContainsOr

            private Span<bool> containsDynamic;
            public bool ContainsDynamic(int index)
            {
                if (containsDynamic.Length == 0 || index >= containsDynamic.Length)
                { return false; }
                else { return containsDynamic[index]; }
            }

            #endregion

            #endregion

            #region ► Constructors
            public PatternsToMatch(ReadOnlySpan<char> textToMatch)
            {
                Patterns = textToMatch;
                setPatternsCoodinates();
            }

            #endregion

            #region ► Controller

            public ReadOnlySpan<char> this[int index]
            {
                get
                {
                    if (index >= Count)
                    { throw new Exception("Index out of range!"); }

                    if (index < 0)
                    { throw new Exception("Negative index!"); }

                    int startPos = Position[index];
                    int length = Lengths[index];
                    return Patterns.Slice(startPos, length);
                }
            }

            #endregion

            #region ► Methods

            private void setPatternsCoodinates()
            {
                #region + Flags e variables

                int patStart = 0;
                Span<int> _position = new int[50];
                Span<int> _length = new int[50];
                Span<bool> _containsWildcard = new bool[50];
                Span<bool> _containsOr = new bool[50];
                Span<bool> _containsDynamic = new bool[50];
                int _count = 0;

                #endregion

                for (int patIndex = 0; patIndex < Patterns.Length; patIndex++)
                {
                    #region ? If a pattern char

                    if (Patterns[patIndex] == '*')
                    { _containsWildcard[_count] = true; continue; }
                    else if (!_containsWildcard[_count]) { _containsWildcard[_count] = false; }

                    #endregion

                    #region ? If a dynamic char

                    if (Patterns[patIndex] == '#' || Patterns[patIndex] == '_' || Patterns[patIndex] == '@' ||
                             Patterns[patIndex] == '[' || Patterns[patIndex] == ']')
                    { _containsDynamic[_count] = true; continue; }
                    else { _containsDynamic[_count] = false; }

                    #endregion

                    if (Patterns[patIndex] == '|' || patIndex == Patterns.Length - 1)
                    {
                        #region ! Is a sparattor or "||"

                        if (patIndex != Patterns.Length - 1)
                        {
                            if (Patterns[patIndex + 1] != '|')
                            {
                                #region ! Not found separator OR ("||")
                                _containsOr[_count] = true; continue;
                                #endregion
                            }
                            else
                            {
                                #region ! Found separator OR ("||")

                                patIndex += 1;
                                _position[_count] = patStart;
                                _length[_count++] = (patIndex - patStart) - 1;
                                patStart = ++patIndex;

                                #endregion
                            }
                        }
                        else
                        { _position[_count] = patStart; _length[_count++] = Patterns.Length - patStart; }

                        #endregion
                    }
                }

                #region ! Found sepator

                if (_count == 0) { _count = 1; _length[0] = Patterns.Length; }

                Position = _position.Slice(0, _count);
                Lengths = _length.Slice(0, _count);
                containsWildcard = _containsWildcard.Slice(0, _count);
                containsOr = _containsOr.Slice(0, _count);
                containsDynamic = _containsDynamic.Slice(0, _count);

                #endregion
            }

            #endregion
        }

        #endregion

        #region ▼ Properties

        /// <summary>
        /// Ignore if upper or down case.
        /// </summary>
        public static byte ParmsIgnoreCase { get; } =1;

        /// <summary>
        /// Ignore content in single quotes when parsing text.
        /// </summary>
        public static byte ParmsIgnoreInQuotes { get; } = 2;

        /// <summary>
        /// Ignore content in single quotes when parsing text.
        /// </summary>
        public static byte ParmsIgnoreInDoubleQuotes { get; } = 3;

        /// <summary>
        /// Return only captured chars in text and ignore chars in pattern.
        /// </summary>
        public static byte ReturnCapturedSegment { get; } = 4;

        #endregion

        #region ▼ Match Words

        #region ► Match

        public static StringAndPosition Match(ReadOnlySpan<char> text, ReadOnlySpan<char> SequenceToMatch, params byte[] options)
        {
            int occurPos = -1;
            int startIndex = 0;
            ReadOnlySpan<char> word = null;
            int _position = 0;
            int _len = 0;

            PatternsToMatch toMatch = new PatternsToMatch(SequenceToMatch);

            #region + If Ignore in quote

            if (options.Contains(ParmsIgnoreInQuotes))
            {  text = cleanTextInApostrophes(text.ToArray());  }

            #endregion

            #region + If Ignore in double quote

            if (options.Contains(ParmsIgnoreInDoubleQuotes))
            { text =cleanTextInDoubleQuotes(text.ToArray()); }

            #endregion

            for (int i = 0; i < toMatch.Count; i++)
            {
                ReadOnlySpan<char> currentPattern = toMatch[i];

                if (toMatch.ContainsWildcards(i) || (i == 0 && toMatch[i][0] == '*'))
                {
                    #region + Match Pattern

                    (word, _position) = matchPattern(text, currentPattern, startIndex, options);

                    if (word.Length == 0 )
                    {
                        // No match found, continue to next pattern
                        continue;
                    }

                    #endregion
                }
                else
                {
                    #region + Match Literal

                    ( _position, _len ) = matchLitteral(text, toMatch[i], true, options);

                    if (_position == -1)
                    {
                        // No match found, continue to next pattern
                        continue;
                    }
                    else if (_position < occurPos || occurPos == -1)
                    { occurPos = _position; word = text.Slice(_position, _len); }

                    #endregion
                }
            }

            if(word.Length == 0)
            {
                // No match found
                return new StringAndPosition();
            }

            return new StringAndPosition( word.ToString(), _position);
        }

        #endregion

        #region ► matchLitteral

        private static (int,int) matchLitteral(ReadOnlySpan<char> text, ReadOnlySpan<char> pattern, bool firstMatch, byte[] options = null)
        {
            int occurIndex = 0;
            int orPos = 0;
            int returnPos = -1;
            int returnLen = 0;

            while (orPos != -1)
            {
                #region + Or split to or pattern

                orPos = pattern.IndexOf('|');
                if (orPos != -1 && (orPos + 1 < pattern.Length))
                {
                    if( pattern[orPos + 1] =='*' || pattern[orPos + 1] == '|')
                    { throw new Exception("Invalid pattern!"); }
                }

                int patPos = 0;

                if(orPos == -1)
                { patPos = pattern.Length;  }
                else { patPos = orPos; }

                #endregion

                #region + IndexOf pattern in text

                if (options.Contains(ParmsIgnoreCase))
                {
                    occurIndex = CultureInfo.InvariantCulture.CompareInfo.IndexOf(
                                      text, pattern[..patPos], CompareOptions.IgnoreCase);
                }
                else
                {
                    occurIndex = text.IndexOf(pattern[..patPos]);
                }

                #endregion
                                
                #region + If first pos in text

                if ( ( occurIndex < returnPos && occurIndex !=-1) || returnPos ==-1 )
                {
                    returnPos = occurIndex;
                    if (orPos == -1) { returnLen = pattern.Length; }
                    else { returnLen = orPos; } 
                }

                #endregion

                #region + If search all or patterns

                if (patPos == pattern.Length && returnPos == -1)
                { return (-1, 0); }

                #endregion

                #region + Update pattern

                if (orPos != -1) { pattern = pattern.Slice(patPos + 1); }

                #endregion
            }

            return ( returnPos, returnLen );
        }

        #endregion

        #region ► matchPattern

        private static (char[], int pos) matchPattern(ReadOnlySpan<char> text, ReadOnlySpan<char> pattern, int startIndex, byte[] options = null)
        {
            int occurPos = -1;
            int occurLen = 0;
            int occurStart = -1;
            int wildPos = 0;
            //int orPos = -1;
            bool firstMatch = true;

            //ReadOnlySpan<char> currentPattern = pattern;
            ReadOnlySpan<char> textToSearch = text.Slice(startIndex);
            Span<char> matchedText = new char[800];
            int matchedIndex =0;

            while (wildPos != -1)
            {
                wildPos = pattern.IndexOf('*');
                if (wildPos != -1 && (wildPos + 1 < pattern.Length))
                {
                    if (pattern[wildPos + 1] == '*' || pattern[wildPos + 1] == '|')
                    { throw new Exception("Invalid pattern!"); }
                }

                #region + If a wildcard in last char of pattern

                if (pattern.Length == 0) 
                {
                    matchedText = copyToSpan(textToSearch.ToArray(), matchedText, matchedIndex);
                    break;
                }

                #endregion

                #region + Update pattern if first char is a wildcard

                if (wildPos == 0) 
                { occurPos =1; pattern = pattern[1..]; continue; }

                #endregion

                #region + Match pattern in text

                if (wildPos == -1)
                { ( occurPos, occurLen ) = matchLitteral(textToSearch, pattern, firstMatch, options); }
                else { ( occurPos, occurLen ) = matchLitteral(textToSearch, pattern[..wildPos], firstMatch, options); }

                #endregion

                if (occurPos == -1) 
                { return (default, -1); }
                else if(firstMatch && wildPos > 0)
                {
                    #region + If first match storage real start position in text 

                    if (occurStart == -1){ occurStart = occurPos; }

                    matchedText = copyToSpan(text.Slice(occurPos, occurLen).ToArray(), matchedText, matchedIndex);
                    matchedIndex += wildPos;
                    occurPos += wildPos;

                    #endregion
                }
                else
                {
                    #region + If not first match

                    if (wildPos == -1) { occurPos += occurLen; }
                    else { occurPos = wildPos; }

                    matchedText = copyToSpan(textToSearch[..occurPos].ToArray(), matchedText, matchedIndex);
                    matchedIndex += occurPos;

                    #endregion
                }

                #region + Update flags e containers

                if(wildPos == -1) { break; }

                wildPos += 1;
                pattern = pattern[wildPos..];
                textToSearch = textToSearch[occurPos..];
                firstMatch = false;

                #endregion
            }

            return (matchedText.TrimEnd('\0').ToArray(), occurStart);
        }

        #region ► copyToSpan
        private static Span<char> copyToSpan(Span<char> source, Span<char> destination, int destinationIndex)
        {
            #region + Resize destination if needed  
            if ( (destinationIndex + source.Length) > destination.Length )
            { destination = ResizeTo(destination, destinationIndex, source); }
            #endregion

            source.CopyTo( destination[destinationIndex..]);

            return destination;
            
        }

        #endregion

        #region ► Resize
        private static Span<char> ResizeTo(Span<char> buffer, int lengthBuffer, Span<char> source)
        {
            int len = (source.Length + lengthBuffer) + 800;

            Span<char> newBuffer = new char[len];
            buffer.CopyTo(newBuffer);
            return newBuffer;
        }

        #endregion

        private static (char[], int pos) searchPattern(ReadOnlySpan<char> text, ReadOnlySpan<char> pattern, int startIndex, byte[] options = null)
        {
            int pos = text.IndexOf(pattern);
            int occurIndex = 0;

            if (pos != -1)
            { return (text.Slice(occurIndex, pattern.Length).ToArray(), occurIndex); }
            else { return default; }
        }

        #endregion

        #region ► cleanTextInApostrophes

        private static ReadOnlySpan<char> cleanTextInApostrophes(Span<char> text)
        {
            int i = text.IndexOf('\'');
            if (i == -1) { return text; }

            bool inApostrophe = false;

            for (; i < text.Length; i++)
            {
                if (text[i] == '\'' || inApostrophe)
                {
                    if (text[i] == '\'') { inApostrophe = !inApostrophe; }
                    text[i] = (char)0;
                }
                else
                {
                    i = text.IndexOf('\'');
                    if (i == -1) { return text; }
                }
            }

            return text;
        }

        #endregion

        #region ► cleanTextInDoubleQuotes

        private static ReadOnlySpan<char> cleanTextInDoubleQuotes(Span<char> text)
        {
            int i = text.IndexOf('\"');
            if (i == -1) { return text; }

            bool inApostrophe = false;

            for (; i < text.Length; i++)
            {
                if (text[i] == '\"' || inApostrophe)
                {
                    if (text[i] == '\"') { inApostrophe = !inApostrophe; }
                    text[i] = (char)0;
                }
                else
                {
                    i = text.IndexOf('\"');
                    if (i == -1) { return text; }
                }
            }

            return text;
        }

        #endregion

        #endregion

        #region ▼ Match Snippets

        #region ► First occurs

        //public static string ExtractFirstSnippet(ReadOnlySpan<char> text, string snippetTags, params byte[] options)
        //{
            
        //}

        #endregion

        #region » Get Sinppets

        //private static string[] snippets(ReadOnlySpan<char> text, string snippet, string openCloseTags = "", int count = 0, params byte[] options)
        //{
        //   // if (snippet == "" && openCloseTags == "") { return default; }

            
        //}

        #endregion

        #endregion

    }
}
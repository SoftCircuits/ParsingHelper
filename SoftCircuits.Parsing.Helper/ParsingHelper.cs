// Copyright (c) 2019-2020 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace SoftCircuits.Parsing.Helper
{
    /// <summary>
    /// Low-level text parsing helper class.
    /// </summary>
    public class ParsingHelper
    {
        /// <summary>
        /// Characters that make up a line break.
        /// </summary>
        private static readonly char[] LineBreakCharacters = new[] { '\r', '\n' };

        private int InternalIndex;

        /// <summary>
        /// Represents an invalid character. This character is returned when attempting to read
        /// a character at an invalid position. The character value is <c>'\0'</c>.
        /// </summary>
        public const char NullChar = '\0';

        /// <summary>
        /// Specifies regular expression options used by all methods that use
        /// regular expressions.
        /// </summary>
        public RegexOptions RegularExpressionOptions { get; set; }

        /// <summary>
        /// Returns the text currently being parsed.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Constructs a new <see cref="ParsingHelper"></see> instance. Sets the text to be parsed
        /// and sets the current position to the start of that text.
        /// </summary>
        /// <param name="text">The text to be parsed. Can be <c>null</c>.</param>
        /// <param name="regularExpressionOptions">Specifies regular expression options used by
        /// all methods that use regular expressions.</param>
        public ParsingHelper(string text, RegexOptions regularExpressionOptions = RegexOptions.None)
        {
            RegularExpressionOptions = regularExpressionOptions;
            Reset(text);
        }

        /// <summary>
        /// Sets the text to be parsed and sets the current position to the start of that text.
        /// </summary>
        /// <param name="text">The text to be parsed. Can be <c>null</c>.</param>
        public void Reset(string text)
        {
            Text = text ?? string.Empty;
            InternalIndex = 0;
        }

        /// <summary>
        /// Sets the current position to the start of the current text.
        /// </summary>
        public void Reset()
        {
            InternalIndex = 0;
        }

        /// <summary>
        /// Gets or sets the current position within the text being parsed. Safely
        /// handles attempts to set to an invalid position.
        /// </summary>
        public int Index
        {
            get => InternalIndex;
            set
            {
                InternalIndex = value;
                if (InternalIndex < 0)
                    InternalIndex = 0;
                else if (InternalIndex > Text.Length)
                    InternalIndex = Text.Length;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the current position is at the end of the text being parsed.
        /// Otherwise, false.
        /// </summary>
        public bool EndOfText => (InternalIndex >= Text.Length);

        /// <summary>
        /// Returns the number of characters not yet parsed. This is equal to the length
        /// of the text being parsed, minus the current position.
        /// </summary>
        public int Remaining => (Text.Length - InternalIndex);

        /// <summary>
        /// Returns the character at the current position, or <see cref="NullChar"/>
        /// if the current position was at the end of the text being parsed.
        /// </summary>
        /// <returns>The character at the current position.</returns>
        public char Peek()
        {
            Debug.Assert(InternalIndex >= 0 && InternalIndex <= Text.Length);
            return (InternalIndex < Text.Length) ? Text[InternalIndex] : NullChar;
        }

        /// <summary>
        /// Returns the character at the specified number of characters ahead of the
        /// current position, or <see cref="NullChar"></see> if the specified position
        /// is not valid. Does not change the current position.
        /// </summary>
        /// <param name="count">Specifies the position of the character to read as the number
        /// of characters ahead of the current position. May be a negative number.</param>
        /// <returns>The character at the specified position.</returns>
        public char Peek(int count)
        {
            int index = (InternalIndex + count);
            return (index >= 0 && index < Text.Length) ? Text[index] : NullChar;
        }

        /// <summary>
        /// Returns the character at the current position and increments the current position.
        /// Returns <see cref="NullChar"/> if the current position was at the end of the text
        /// being parsed.
        /// </summary>
        /// <returns>The character at the current position.</returns>
        public char Get()
        {
            Debug.Assert(InternalIndex >= 0 && InternalIndex <= Text.Length);
            if (InternalIndex < Text.Length)
                return Text[InternalIndex++];
            return NullChar;
        }

        /// <summary>
        /// Moves the current position ahead one character.
        /// </summary>
        public void Next()
        {
            Debug.Assert(InternalIndex >= 0 && InternalIndex <= Text.Length);
            if (InternalIndex < Text.Length)
                InternalIndex++;
        }

        /// <summary>
        /// Moves the current position ahead the specified number of characters.
        /// </summary>
        /// <param name="count">The number of characters to move ahead. Use negative numbers
        /// to move backwards.</param>
        public void Next(int count) => Index = (InternalIndex + count);

        /// <summary>
        /// Calculates the line and column information for the current position.
        /// </summary>
        /// <returns>A <see cref="ParsePosition"/> that represents the current position.</returns>
        [Obsolete("This method is obsolete and will be removed in a future version. Please use GetLineColumn() instead.")]
        public ParsePosition CalculatePosition() => ParsePosition.CalculatePosition(Text, Index);

        /// <summary>
        /// Determines the line and column values that correspond to the current position.
        /// </summary>
        /// <returns>A <see cref="ParsePosition"/> that represents the current position.</returns>
        public ParsePosition GetLineColumn() => ParsePosition.CalculatePosition(Text, InternalIndex);

        #region Skip characters

        /// <summary>
        /// Moves the current position to the next character that causes <paramref name="predicate"/>
        /// to return <c>false</c>.
        /// </summary>
        /// <param name="predicate">Function to return test each character and return <c>true</c>
        /// for each character that should be skipped.</param>
        public void SkipWhile(Func<char, bool> predicate)
        {
            Debug.Assert(InternalIndex >= 0 && InternalIndex <= Text.Length);
            while (InternalIndex < Text.Length && predicate(Text[InternalIndex]))
                InternalIndex++;
        }

        /// <summary>
        /// Moves the current position to the next character that is not one of the specified
        /// characters.
        /// </summary>
        /// <param name="chars">Characters to skip over.</param>
        public void Skip(params char[] chars) => SkipWhile(chars.Contains);

        /// <summary>
        /// Moves the current position to the next character that is not a whitespace character.
        /// </summary>
        public void SkipWhiteSpace() => SkipWhile(char.IsWhiteSpace);

        /// <summary>
        /// Moves the current position to the next character that is not a whitespace character,
        /// with an option to stop sooner under a specified condition.
        /// </summary>
        /// <param name="option">Specifies a condition where skipping should stop before a
        /// whitespace character is found.</param>
        public void SkipWhiteSpace(SkipWhiteSpaceOption option)
        {
            Debug.Assert(option == SkipWhiteSpaceOption.StopAtEol || option == SkipWhiteSpaceOption.StopAtNextLine);
            SkipWhile(c => char.IsWhiteSpace(c) && !LineBreakCharacters.Contains(c));
            if (option == SkipWhiteSpaceOption.StopAtNextLine && LineBreakCharacters.Contains(Peek()))
                SkipLineBreak();
        }

        /// <summary>
        /// Moves the current position past any characters that match the given regular expression.
        /// </summary>
        /// <param name="regularExpression">The regular expression pattern to match.</param>
        public void SkipRegEx(string regularExpression)
        {
            Regex regex = new Regex(regularExpression, RegularExpressionOptions);
            Match match = regex.Match(Text, Index);
            if (match.Success && match.Index == Index)
                InternalIndex += match.Length;
        }

        #endregion

        #region Skip to characters

        /// <summary>
        /// Moves the current position to the next character that is one of the specified characters
        /// and returns <c>true</c> if a match was found. If none of the specified characters are
        /// found, this method moves the current position to the end of the text being parsed and
        /// returns <c>false</c>.
        /// </summary>
        /// <param name="chars">Characters to skip to.</param>
        /// <returns>True if any of the specified characters were found. Otherwise, false.</returns>
        public bool SkipTo(params char[] chars)
        {
            InternalIndex = Text.IndexOfAny(chars, InternalIndex);
            if (InternalIndex >= 0)
                return true;
            InternalIndex = Text.Length;
            return false;
        }

        /// <summary>
        /// Moves the current position to the next occurrence of the specified string and returns
        /// <c>true</c> if a match was found. If the specified string is not found, this method
        /// moves the current position to the end of the text being parsed and returns <c>false</c>.
        /// </summary>
        /// <param name="s">String to skip to.</param>
        /// <param name="includeToken">If <c>true</c> and a match is found, the matching string is
        /// also skipped.</param>
        /// <returns>True if the specified string was found. Otherwise, false.</returns>
        public bool SkipTo(string s, bool includeToken = false)
        {
            InternalIndex = Text.IndexOf(s, InternalIndex);
            if (InternalIndex >= 0)
            {
                if (includeToken)
                    InternalIndex += s.Length;
                return true;
            }
            InternalIndex = Text.Length;
            return false;
        }

        /// <summary>
        /// Moves the current position to the next occurrence of the specified string and returns
        /// <c>true</c> if a match was found. If the specified string is not found, this method
        /// moves the current position to the end of the text being parsed and returns <c>false</c>.
        /// </summary>
        /// <param name="s">String to skip to.</param>
        /// <param name="comparison">One of the enumeration values that specifies the rules for
        /// search.</param>
        /// <param name="includeToken">If <c>true</c> and a match is found, the matching text is
        /// also skipped.</param>
        /// <returns>True if the specified string was found. Otherwise, false.</returns>
        public bool SkipTo(string s, StringComparison comparison, bool includeToken = false)
        {
            InternalIndex = Text.IndexOf(s, InternalIndex, comparison);
            if (InternalIndex >= 0)
            {
                if (includeToken)
                    InternalIndex += s.Length;
                return true;
            }
            InternalIndex = Text.Length;
            return false;
        }

        /// <summary>
        /// Moves the current position to the start of the next text that matches the given regular
        /// expression and returns <c>true</c> if a match was found. If no match is found, this method
        /// moves the current position to the end of the text being parsed and returns <c>false</c>.
        /// </summary>
        /// <param name="regularExpression">A regular expression that the text must match.</param>
        /// <param name="includeToken">If <c>true</c> and a match is found, the matching text is
        /// also skipped.</param>
        /// <returns>True if a match was found.</returns>
        public bool SkipToRegEx(string regularExpression, bool includeToken = false)
        {
            Regex regex = new Regex(regularExpression, RegularExpressionOptions);
            Match match = regex.Match(Text, Index);
            if (match.Success)
            {
                InternalIndex = match.Index;
                if (includeToken)
                    InternalIndex += match.Length;
                return true;
            }
            InternalIndex = Text.Length;
            return false;
        }

        /// <summary>
        /// Moves the current position to the next line break character and returns true if a line-break
        /// character was found. If no line break characters are found, this method moves to the end of
        /// the text being parsed and returns <c>false</c>.
        /// </summary>
        /// <returns>True if a line break character was found. Otherwise, false.</returns>
        public bool SkipToEndOfLine() => SkipTo(LineBreakCharacters);

        /// <summary>
        /// Moves the current position to the start of the next line and returns true if a line-break
        /// character was found. If no more line break characters are found, this method moves to the
        /// end of the text being parsed and returns <c>false</c>.
        /// </summary>
        /// <returns>True if any more line break characters were found.</returns>
        public bool SkipToNextLine()
        {
            // Move to next line break character
            bool result = SkipToEndOfLine();
            // Move past line break
            SkipLineBreak();
            // Return true if line break characters were found
            return result;
        }

        /// <summary>
        /// Skips over a line break. Current position must be at the first line break character or
        /// the end of the text being parsed.
        /// </summary>
        private void SkipLineBreak()
        {
            Debug.Assert(EndOfText || LineBreakCharacters.Contains(Peek()));
            if (MatchesCurrentPosition(LineBreakCharacters))
                Next(LineBreakCharacters.Length);
            else
                Next();
        }

        #endregion

        #region Parse characters

        /// <summary>
        /// Parses a single character and increments the current position. Returns an empty string
        /// if the current position was at the end of the text being parsed.
        /// </summary>
        /// <returns>A string that contains the parsed character, or an empty string if the current
        /// position was at the end of the text being parsed.</returns>
        public string ParseCharacter() => ParseCharacters(1);

        /// <summary>
        /// Parses the specified number of characters starting at the current position and increments
        /// the current position by the number of characters parsed. Returns a string with the parsed
        /// characters. Returns a shorter string if the end of the text is reached.
        /// </summary>
        /// <param name="count">The number of characters to parse.</param>
        /// <returns>A string with the parsed characters.</returns>
        public string ParseCharacters(int count)
        {
            int remaining = Remaining;
            if (count > remaining)
                count = remaining;
            else if (count < 0)
                count = 0;
            int start = InternalIndex;
            InternalIndex += count;
            return Extract(start, InternalIndex);
        }

        /// <summary>
        /// Parses characters until the next character for which <paramref name="predicate"/>
        /// returns <c>false</c>, and returns the parsed characters. Can return an empty string.
        /// </summary>
        /// <param name="predicate">Function to test each character. Should return <c>true</c>
        /// for each character that should be parsed.</param>
        /// <returns>A string with the parsed characters.</returns>
        public string ParseWhile(Func<char, bool> predicate)
        {
            int start = InternalIndex;
            SkipWhile(predicate);
            return Extract(start, InternalIndex);
        }

        /// <summary>
        /// Parses characters until the next character that is not contained in
        /// <paramref name="chars"/>, and returns a string with the parsed characters.
        /// Can return an empty string.
        /// </summary>
        /// <param name="chars">Characters to parse.</param>
        /// <returns>A string with the parsed characters.</returns>
        public string Parse(params char[] chars) => ParseWhile(chars.Contains);

        /// <summary>
        /// Parses the next line of text and returns <c>true</c> if successful. Returns false if
        /// the current position was at the end of the text being parsed. The current position is
        /// moved past the line-break characters to the start of the following line.
        /// </summary>
        /// <param name="line">Receives the parsed line.</param>
        /// <returns>True if successful; otherwise, false if the current position was at the end
        /// of the text being parsed.</returns>
        public bool ParseLine(out string line)
        {
            if (EndOfText)
            {
                line = string.Empty;
                return false;
            }

            int start = InternalIndex;
            SkipToEndOfLine();
            // Extract this line
            line = Extract(start, InternalIndex);
            // Move to start of next line
            SkipLineBreak();
            return true;
        }

        /// <summary>
        /// Parses quoted text. The character at the current position is assumed to be the starting quote
        /// character. This method parses text up until the matching end quote character. Returns the parsed
        /// text without the quotes and sets the current position to the character following the
        /// end quote. If the text contains two quote characters together, the pair is handled as a
        /// single quote literal and not the end of the quoted text.
        /// </summary>
        /// <returns>Returns the text within the quotes.</returns>
        public string ParseQuotedText()
        {
            StringBuilder builder = new StringBuilder();

            // Get and skip quote character
            char quote = Get();

            // Parse quoted text
            while (!EndOfText)
            {
                // Parse to next quote
                builder.Append(ParseTo(quote));
                // Skip quote
                Next();
                // Two consecutive quotes treated as quote literal
                if (Peek() == quote)
                {
                    builder.Append(quote);
                    Next();
                }
                else break; // Done if single closing quote or end of text
            }
            return builder.ToString();
        }

        /// <summary>
        /// Parses quoted text with options. The character at the current position is assumed to be the
        /// starting quote character. This method parses text up until the next matching end quote character.
        /// Returns the parsed text without the quotes and sets the current position to the character
        /// following the end quote.
        /// </summary>
        /// <param name="escapeChar">Specifies an escape character. If this character is immediately
        /// followed by a quote character, the pair is handled as a single quote literal and not the end
        /// of the quoted text. Set to null for no escape character, in which case the string is
        /// terminated at the next quote character. This parameter can be the same as the quote
        /// character.</param>
        /// <param name="includeEscapeChar">Specifies if the escape characters should be included in
        /// the returned string.</param>
        /// <param name="includeQuotes">Specifies if the enclosing quotes should be included in the
        /// returned string.</param>
        /// <returns>Returns the text within the quotes.</returns>
        public string ParseQuotedText(char? escapeChar, bool includeEscapeChar = false, bool includeQuotes = false)
        {
            if (EndOfText)
                return string.Empty;

            StringBuilder builder = new StringBuilder();

            // Get and skip quote character
            char quote = Get();

            // Add opening quote if requested
            if (includeQuotes)
                builder.Append(quote);

            // Parse quoted text
            if (escapeChar == null)
            {
                // No escape character
                builder.Append(ParseTo(quote));
                Next();
            }
            else if (escapeChar != quote)
            {
                // Custom escape character
                while (!EndOfText)
                {
                    // Parse to next quote or escape character
                    builder.Append(ParseTo(quote, escapeChar.Value));
                    char found = Peek();
                    // Skip character
                    Next();
                    // Quote following escape character treated as quote literal
                    if (found == escapeChar.Value)
                    {
                        if (Peek() == quote)
                        {
                            if (includeEscapeChar)
                                builder.Append(escapeChar.Value);
                            builder.Append(quote);
                            Next();
                        }
                        else
                        {
                            builder.Append(found);
                        }
                    }
                    else break; // Done if single closing quote or end of text
                }
            }
            else
            {
                // Two quotes escapes
                while (!EndOfText)
                {
                    // Parse to next quote
                    builder.Append(ParseTo(quote));
                    // Skip quote
                    Next();
                    // Two consecutive quotes treated as quote literal
                    if (Peek() == quote)
                    {
                        if (includeEscapeChar)
                            builder.Append(quote);
                        builder.Append(quote);
                        Next();
                    }
                    else break; // Done if single closing quote or end of text
                }
            }

            // Add closing quote if requested
            if (includeQuotes && Peek(-1) == quote)
                builder.Append(quote);

            return builder.ToString();
        }

        #endregion

        #region Parse to characters

        /// <summary>
        /// Parses characters until the next occurrence of any one of the specified characters and
        /// returns a string with the parsed characters. If none of the specified characters are found,
        /// this method parses all character up to the end of the text being parsed. Can return an empty
        /// string.
        /// </summary>
        /// <param name="chars">The characters that cause parsing to end.</param>
        /// <returns>A string with the parsed characters.</returns>
        public string ParseTo(params char[] chars)
        {
            int start = InternalIndex;
            SkipTo(chars);
            return Extract(start, InternalIndex);
        }

        /// <summary>
        /// Parses characters until the next occurrence of the specified string and returns a
        /// string with the parsed characters. If the specified string is not found, this method parses
        /// all character to the end of the text being parsed. Can return an empty string.
        /// </summary>
        /// <param name="s">Text that causes parsing to end.</param>
        /// <param name="includeToken">If <c>true</c> and a match is found, the matching text is
        /// also parsed.</param>
        /// <returns>A string with the parsed characters.</returns>
        public string ParseTo(string s, bool includeToken = false)
        {
            int start = InternalIndex;
            SkipTo(s, includeToken);
            return Extract(start, InternalIndex);
        }

        /// <summary>
        /// Parses characters until the next occurrence of the specified string and returns a
        /// string with the parsed characters. If the specified string is not found, this method parses
        /// all character to the end of the text being parsed. Can return an empty string.
        /// </summary>
        /// <param name="s">Text that causes parsing to end.</param>
        /// <param name="comparison">One of the enumeration values that specifies the rules for
        /// comparing the specified string.</param>
        /// <param name="includeToken">If <c>true</c> and a match is found, the matching text is
        /// also parsed.</param>
        /// <returns>A string with the parsed characters.</returns>
        public string ParseTo(string s, StringComparison comparison, bool includeToken = false)
        {
            int start = InternalIndex;
            SkipTo(s, comparison, includeToken);
            return Extract(start, InternalIndex);
        }

        /// <summary>
        /// Parses characters until the start of the next token that matches the given regular
        /// expression and returns a string with the parsed characters. If no match is found, this
        /// method parses all characters to the end of the text being parsed. Can return an empty string.
        /// </summary>
        /// <param name="regularExpression">A regular expression that the text must match.</param>
        /// <param name="includeToken">If <c>true</c> and a match is found, the matching text is
        /// also parsed.</param>
        /// <returns>A string with the parsed characters.</returns>
        public string ParseToRegEx(string regularExpression, bool includeToken = false)
        {
            int start = InternalIndex;
            SkipToRegEx(regularExpression, includeToken);
            return Extract(start, InternalIndex);
        }

        /// <summary>
        /// Parses characters until the next line break character. If no line-break characters are found,
        /// this method parses all characters to the end of the text being parsed.
        /// </summary>
        /// <returns>A string with the parsed characters.</returns>
        public string ParseToEndOfLine()
        {
            int start = InternalIndex;
            SkipToEndOfLine();
            return Extract(start, InternalIndex);
        }

        /// <summary>
        /// Parses characters until the start of the next line. If no more line break characters are
        /// found, this method parses all characters to the end of the text being parsed.
        /// </summary>
        /// <returns>A string with the parsed characters.</returns>
        public string ParseToNextLine()
        {
            int start = InternalIndex;
            SkipToNextLine();
            return Extract(start, InternalIndex);
        }

        #endregion

        #region Parse tokens

        /// <summary>
        /// Parses text using the specified delimiter characters. Skips any characters that are in the
        /// list of delimiters, and then parses any characters that are not in the list of delimiters.
        /// Returns the parsed characters.
        /// </summary>
        /// <param name="delimiters">Token delimiter characters.</param>
        /// <returns>Returns the parsed token.</returns>
        public string ParseToken(params char[] delimiters)
        {
            Skip(delimiters);
            return ParseTo(delimiters);
        }

        /// <summary>
        /// Parses text using the specified predicate to indicate delimiter characters. Skips any
        /// characters for which <paramref name="predicate"/> returns <c>true</c>, and then parses any
        /// characters for which <paramref name="predicate"/> returns <c>false</c>. Returns the parsed
        /// characters.
        /// </summary>
        /// <param name="predicate">Function that returns <c>true</c> for token delimiter
        /// characters.</param>
        /// <returns>Returns the parsed token.</returns>
        public string ParseToken(Func<char, bool> predicate)
        {
            SkipWhile(predicate);
            return ParseWhile(c => !predicate(c));
        }

        /// <summary>
        /// Parses text using a regular expression. Skips up to the start of the matching text, and then
        /// parses the matching text. If no match is found, the current position is set to the end of
        /// the text and an empty string is returned.
        /// </summary>
        /// <param name="regularExpression">A regular expression that the token must match.</param>
        /// <returns>Returns the text of the matching token.</returns>
        public string ParseTokenRegEx(string regularExpression)
        {
            Regex regex = new Regex(regularExpression, RegularExpressionOptions);
            Match match = regex.Match(Text, Index);
            if (match.Success)
            {
                InternalIndex = match.Index + match.Length;
                return match.Value;
            }
            InternalIndex = Text.Length;
            return string.Empty;
        }

        /// <summary>
        /// This method has been deprecated. Please use <see cref="ParseTokens(char[])"/> instead.
        /// </summary>
        [Obsolete("This method has been deprecated and will be removed in a future version. Please use ParseTokens() instead.")]
        public IEnumerable<string> ParseAllTokens(params char[] delimiters) => ParseTokens(delimiters);

        /// <summary>
        /// Parses and returns all tokens to the end of the text being parsed. The specified
        /// characters indicate delimiter characters that are not part of a token.
        /// </summary>
        /// <param name="delimiters">Token delimiter characters.</param>
        /// <param name="count">The maxiumum number of tokens to parse.</param>
        /// <returns>Returns the parsed tokens.</returns>
        public IEnumerable<string> ParseTokens(params char[] delimiters)
        {
            Skip(delimiters);
            while (!EndOfText)
            {
                yield return ParseTo(delimiters);
                Skip(delimiters);
            }
        }

        /// <summary>
        /// Parses and returns up to the specified number of tokens. The specified
        /// characters indicate delimiter characters that are not part of a token.
        /// </summary>
        /// <param name="count">The maxiumum number of tokens to parse.</param>
        /// <param name="delimiters">Token delimiter characters.</param>
        /// <returns>Returns the parsed tokens.</returns>
        public IEnumerable<string> ParseTokens(int count, params char[] delimiters)
        {
            Skip(delimiters);
            while (!EndOfText)
            {
                if (count-- <= 0)
                    break;
                yield return ParseTo(delimiters);
                Skip(delimiters);
            }
        }

        /// <summary>
        /// This method has been deprecated. Please use <see cref="ParseTokens(Func{char, bool})"/> instead.
        /// </summary>
        [Obsolete("This method has been deprecated and will be removed in a future version. Please use ParseTokens() instead.")]
        public IEnumerable<string> ParseAllTokens(Func<char, bool> predicate) => ParseTokens(predicate);

        /// <summary>
        /// Parses and returns all tokens to the end of the text being parsed. <paramref name="predicate"/>
        /// returns <c>true</c> for delimiter characters that are not part of a token.
        /// </summary>
        /// <param name="predicate">Function that returns <c>true</c> for token delimiter
        /// characters.</param>
        /// <returns>Returns the parsed tokens.</returns>
        public IEnumerable<string> ParseTokens(Func<char, bool> predicate)
        {
            SkipWhile(predicate);
            while (!EndOfText)
            {
                yield return ParseWhile(c => !predicate(c));
                SkipWhile(predicate);
            }
        }

        /// <summary>
        /// Parses and returns up to the specified number of tokens. <paramref name="predicate"/>
        /// returns <c>true</c> for delimiter characters that are not part of a token.
        /// </summary>
        /// <param name="count">Specifies the maximum number of tokens to parse.</param>
        /// <param name="predicate">Function that returns <c>true</c> for token delimiter
        /// characters.</param>
        /// <returns>Returns the parsed tokens.</returns>
        public IEnumerable<string> ParseTokens(int count, Func<char, bool> predicate)
        {
            SkipWhile(predicate);
            while (!EndOfText)
            {
                if (count-- <= 0)
                    break;
                yield return ParseWhile(c => !predicate(c));
                SkipWhile(predicate);
            }
        }

        /// <summary>
        /// Parses all tokens that match the given regular expression and sets the current position the end
        /// of the last token. If no matches are found, the current position is set to the end of the text
        /// and an empty collection is returned.
        /// </summary>
        /// <param name="regularExpression">A regular expression that the tokens must match.</param>
        /// <returns>Returns the matching tokens.</returns>
        public IEnumerable<string> ParseTokensRegEx(string regularExpression)
        {
            Regex regex = new Regex(regularExpression, RegularExpressionOptions);
            MatchCollection matches = regex.Matches(Text, Index);
            if (matches.Count > 0)
            {
                // Update current position
                Match lastMatch = matches[matches.Count - 1];
                InternalIndex = lastMatch.Index + lastMatch.Length;
                // Return matches
                foreach (Match match in matches)
                    yield return match.Value;
            }
            else InternalIndex = Text.Length;
        }

        #endregion

        #region Matches current position

        /// <summary>
        /// Compares the given character array to the characters starting at the current position
        /// using a case-sensitive comparison.
        /// </summary>
        /// <returns>Returns <c>true</c> if the given characters match the characters at the current
        /// position. Returns false otherwise.</returns>
        public bool MatchesCurrentPosition(char[] chars)
        {
            if (chars == null || chars.Length == 0 || chars.Length > Remaining)
                return false;
            for (int i = 0; i < chars.Length; i++)
            {
                if (chars[i] != Text[InternalIndex + i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Returns <c>true</c> if the given string matches the characters at the current position, or
        /// <c>false</c> otherwise.
        /// </summary>
        /// <param name="s">String to compare.</param>
        /// <returns>Returns <c>true</c> if the given string matches the characters at the current position,
        /// or <c>false</c> otherwise.</returns>
        public bool MatchesCurrentPosition(string s) => s != null &&
            s.Length != 0 &&
            string.CompareOrdinal(Text, InternalIndex, s, 0, s.Length) == 0;

        /// <summary>
        /// Returns <c>true</c> if the given string matches the characters at the current position, or
        /// <c>false</c> otherwise.
        /// </summary>
        /// <param name="s">String to compare.</param>
        /// <param name="comparison">One of the enumeration values that specifies the rules to use in the
        /// comparison.</param>
        /// <returns>Returns <c>true</c> if the given string matches the characters at the current position,
        /// of <c>false</c> otherwise.</returns>
        public bool MatchesCurrentPosition(string s, StringComparison comparison) => s != null &&
            s.Length != 0 &&
            string.Compare(Text, InternalIndex, s, 0, s.Length, comparison) == 0;

        #endregion

        #region Extraction

        /// <summary>
        /// Extracts a substring of the text being parsed. The substring includes all characters
        /// from the <paramref name="start"/> position to the end of the text.
        /// </summary>
        /// <param name="start">0-based position of first character to be extracted.</param>
        /// <returns>Returns the extracted string.</returns>
        public string Extract(int start)
        {
            if (start < 0 || start > Text.Length)
                throw new ArgumentOutOfRangeException(nameof(start));
            return  Text.Substring(start);
        }

        /// <summary>
        /// Extracts a substring from the specified range of the text being parsed.
        /// </summary>
        /// <param name="start">0-based position of first character to be extracted.</param>
        /// <param name="end">0-based position of the character that follows the last
        /// character to be extracted.</param>
        /// <returns>Returns the extracted string.</returns>
        public string Extract(int start, int end)
        {
            if (start < 0 || start > Text.Length)
                throw new ArgumentOutOfRangeException(nameof(start));
            if (end < start || end > Text.Length)
                throw new ArgumentOutOfRangeException(nameof(end));
            return Text.Substring(start, end - start);
        }

        #endregion

        #region Operator overloads

        public static implicit operator int(ParsingHelper helper) => helper.InternalIndex;

        /// <summary>
        /// Move the current position ahead one character.
        /// </summary>
        public static ParsingHelper operator ++(ParsingHelper helper)
        {
            helper.Next(1);
            return helper;
        }

        /// <summary>
        /// Move the current position back one character.
        /// </summary>
        public static ParsingHelper operator --(ParsingHelper helper)
        {
            helper.Next(-1);
            return helper;
        }

        /// <summary>
        /// Moves the current position ahead (or back) by the specified
        /// number of characters.
        /// </summary>
        public static ParsingHelper operator +(ParsingHelper helper, int count)
        {
            helper.Next(count);
            return helper;
        }

        /// <summary>
        /// Moves the current position back (or ahead) by the specified
        /// number of characters.
        /// </summary>
        public static ParsingHelper operator -(ParsingHelper helper, int count)
        {
            helper.Next(-count);
            return helper;
        }

        #endregion

    }
}

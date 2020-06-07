// Copyright (c) 2019-2020 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SoftCircuits.Parsing.Helper
{
    /// <summary>
    /// Low-level text parsing helper class.
    /// </summary>
    public class ParsingHelper
    {
        private static readonly char[] NewLineCharacters = new[] { '\r', '\n' };

        private int InternalIndex;

        /// <summary>
        /// Represents an invalid character. This character is returned when attempting to read
        /// a character at an invalid position. The character value is <c>'\0'</c>.
        /// </summary>
        public const char NullChar = '\0';

        /// <summary>
        /// Returns the text currently being parsed.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Constructs a new <see cref="ParsingHelper"></see> instance. Sets the text to be parsed
        /// and sets the current position to the start of that text.
        /// </summary>
        /// <param name="text">The text to be parsed. Can be <c>null</c>.</param>
        public ParsingHelper(string text)
        {
            Reset(text);
        }

        /// <summary>
        /// Sets the current position to the start of the current text.
        /// </summary>
        public void Reset()
        {
            InternalIndex = 0;
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
        /// Returns a value that indicates if the current position is at the end of the
        /// text being parsed.
        /// </summary>
        public bool EndOfText => InternalIndex >= Text.Length;

        /// <summary>
        /// Returns the number of characters not yet parsed. This is equal to the length
        /// of the text being parsed, minus the current position.
        /// </summary>
        public int Remaining => Text.Length - InternalIndex;

        /// <summary>
        /// Returns the character at the current position, or <see cref="NullChar"/>
        /// if the current position is at the end of the input text.
        /// </summary>
        /// <returns>The character at the current position.</returns>
        public char Peek()
        {
            Debug.Assert(InternalIndex >= 0);
            return (InternalIndex < Text.Length) ? Text[InternalIndex] : NullChar;
        }

        /// <summary>
        /// Returns the character at the specified number of characters ahead of the
        /// current position, or <see cref="NullChar"></see> if the specified position
        /// is not valid. Does not change the current position.
        /// </summary>
        /// <param name="count">Specifies the position of the character to read as the number
        /// of characters ahead of the current position.</param>
        /// <returns>The character at the specified position.</returns>
        public char Peek(int count)
        {
            int index = (InternalIndex + count);
            return (index >= 0 && index < Text.Length) ? Text[index] : NullChar;
        }

        /// <summary>
        /// Moves the current position ahead one character.
        /// </summary>
        public void Next()
        {
            Debug.Assert(InternalIndex >= 0);
            if (InternalIndex < Text.Length)
                InternalIndex++;
        }

        /// <summary>
        /// Moves the current position ahead the specified number of characters.
        /// </summary>
        /// <param name="count">The number of characters to move ahead. Use negative numbers
        /// to move backwards.</param>
        public void Next(int count) => Index = InternalIndex + count;

        /// <summary>
        /// Moves the current position to the next character that is not one of the specified
        /// characters.
        /// </summary>
        /// <param name="chars">Characters to skip over.</param>
        public void Skip(params char[] chars) => SkipWhile(chars.Contains);

        /// <summary>
        /// Moves the current position to the next non-whitespace character.
        /// </summary>
        public void SkipWhiteSpace() => SkipWhile(char.IsWhiteSpace);

        /// <summary>
        /// Moves the current position to the next character for which
        /// <paramref name="predicate"/> returns <c>false</c>.
        /// </summary>
        /// <param name="predicate">Function to return test each character and return
        /// <c>true</c> when the character should be skipped over.</param>
        public void SkipWhile(Func<char, bool> predicate)
        {
            Debug.Assert(InternalIndex >= 0);
            while (InternalIndex < Text.Length && predicate(Text[InternalIndex]))
                InternalIndex++;
        }

        /// <summary>
        /// Moves to the next occurrence of any one of the specified characters and returns
        /// <c>true</c> if successful. If none of the specified characters are found, this method
        /// moves the current position to the end of the input text and returns <c>false</c>.
        /// </summary>
        /// <param name="chars">Characters to move to.</param>
        /// <returns>Returns a Boolean value that indicates if any of the specified characters
        /// were found.</returns>
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
        /// <c>true</c> if successful. If the specified string is not found, this method moves the
        /// current position to the end of the input text and returns <c>false</c>.
        /// </summary>
        /// <param name="s">Text to move to.</param>
        /// <param name="comparison">One of the enumeration values that specifies the rules for
        /// search.</param>
        /// <returns>Returns a Boolean value that indicates if the specified text was found.
        /// </returns>
        public bool SkipTo(string s, StringComparison comparison = StringComparison.Ordinal)
        {
            InternalIndex = Text.IndexOf(s, InternalIndex, comparison);
            if (InternalIndex >= 0)
                return true;
            InternalIndex = Text.Length;
            return false;
        }

        /// <summary>
        /// Moves the current position to the next newline character. If no new line
        /// characters are found, this method moves to the end of the text being
        /// parsed and returns <c>false</c>.
        /// </summary>
        /// <returns>Returns a Boolean value that indicates if any newline characters
        /// were found.</returns>
        public bool SkipToEndOfLine() => SkipTo(NewLineCharacters);

        /// <summary>
        /// Moves the current position to the start of the next line. If no more
        /// lines are found, this method moves to the end of the text being
        /// parsed and returns <c>false</c>.
        /// </summary>
        /// <returns>Returns a Boolean value that indicates if the next line was
        /// found.</returns>
        public bool SkipToNextLine()
        {
            // Move to start of next new line
            SkipTo(NewLineCharacters);
            // Move past new line
            if (MatchesCurrentPosition(NewLineCharacters))
                Next(NewLineCharacters.Length);
            else
                Next();
            // Next line found if not at end of text
            return InternalIndex < Text.Length;
        }

        /// <summary>
        /// Parses characters until a character is encountered that is not contained in
        /// <paramref name="chars"/> and returns a string with the parsed characters.
        /// Can return an empty string.
        /// </summary>
        /// <param name="chars">Characters to parse.</param>
        /// <returns>A string with the parsed characters.</returns>
        public string Parse(params char[] chars) => ParseWhile(chars.Contains);

        /// <summary>
        /// Parses characters until a character is encountered that causes
        /// <paramref name="predicate"/> to return <c>false</c> and returns the parsed
        /// characters. Can return an empty string.
        /// </summary>
        /// <param name="predicate">Function to test each character.</param>
        /// <returns>A string with the characters parsed.</returns>
        public string ParseWhile(Func<char, bool> predicate)
        {
            int start = InternalIndex;
            SkipWhile(predicate);
            return Extract(start, InternalIndex);
        }

        /// <summary>
        /// Parses characters until the next occurrence of any one of the specified characters and
        /// returns a string with the parsed characters. If none of the specified characters are found,
        /// this method parses all character up to the end of the input text. Can return an empty
        /// string.
        /// </summary>
        /// <param name="chars">Characters to parse until.</param>
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
        /// all character to the end of the input text. Can return an empty string.
        /// </summary>
        /// <param name="s">String to parse until.</param>
        /// <param name="comparison">One of the enumeration values that specifies the rules for
        /// search.</param>
        /// <returns>A string with the parsed characters.</returns>
        public string ParseTo(string s, StringComparison comparison = StringComparison.Ordinal)
        {
            int start = InternalIndex;
            SkipTo(s, comparison);
            return Extract(start, InternalIndex);
        }

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
        /// Parses and returns all tokens to the end of the text being parsed. The specified
        /// characters indicate delimiter characters that are not part of a token.
        /// </summary>
        /// <param name="delimiters">Token delimiter characters.</param>
        /// <returns>Returns the parsed tokens.</returns>
        public IEnumerable<string> ParseAllTokens(params char[] delimiters)
        {
            Skip(delimiters);
            while (!EndOfText)
            {
                yield return ParseTo(delimiters);
                Skip(delimiters);
            }
        }

        /// <summary>
        /// Parses and returns all tokens to the end of the text being parsed. <paramref name="predicate"/>
        /// returns <c>true</c> for delimiter characters that are not part of a token.
        /// </summary>
        /// <param name="predicate">Function that returns <c>true</c> for token delimiter
        /// characters.</param>
        /// <returns>Returns the parsed tokens.</returns>
        public IEnumerable<string> ParseAllTokens(Func<char, bool> predicate)
        {
            SkipWhile(predicate);
            while (!EndOfText)
            {
                yield return ParseWhile(c => !predicate(c));
                SkipWhile(predicate);
            }
        }

        /// <summary>
        /// Parses quoted text. Interprets the character at the current position as the starting quote
        /// character and parses text up until the matching ending quote character. Returns the parsed
        /// text without the quotes and sets the current position to the character following the
        /// ending quote. If the text contains two quote characters together they are interpreted as a
        /// single quote literal and not the end of the quoted text.
        /// </summary>
        /// <returns>Returns the text within the quotes.</returns>
        public string ParseQuotedText()
        {
            // Get quote character
            char quote = Peek();
            // Skip quote
            Next();
            // Parse quoted text
            StringBuilder builder = new StringBuilder();
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
        /// Compares the given string to the characters starting at the current position using a
        /// case-sensitive comparison.
        /// </summary>
        /// <param name="s">String to compare.</param>
        /// <returns>Returns <c>true</c> if the given string matches the characters at the current
        /// position. Returns false otherwise.</returns>
        /// <remarks>Testing showed this is the fastest way to compare part of a string. Faster even
        /// than using Span&lt;T&gt;.
        /// </remarks>
        public bool MatchesCurrentPosition(string s)
        {
            if (s == null || s.Length == 0 || s.Length > Remaining)
                return false;

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] != Text[InternalIndex + i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Compares the given string to the characters starting at the current position using the
        /// specified comparison method. This method is not as fast as
        /// <see cref="MatchesCurrentPosition(string)"></see> and should be used only when a
        /// non-ordinal comparison is required.
        /// </summary>
        /// <param name="s">String to compare.</param>
        /// <param name="comparison">Type of string comparison to use.</param>
        /// <returns>Returns <c>true</c> if the given string matches the text at the current position.
        /// Returns false otherwise.</returns>
        /// <remarks>Testing showed even this method was a little faster than using Span&lt;T&gt;,
        /// although clearly not as fast as <see cref="MatchesCurrentPosition(string)"></see>.
        /// </remarks>
        public bool MatchesCurrentPosition(string s, StringComparison comparison)
        {
            if (s == null || s.Length == 0 || s.Length > Remaining)
                return false;

            return s.Equals(Text.Substring(InternalIndex, s.Length), comparison);
        }

        /// <summary>
        /// Extracts a substring of the text being parsed. The substring includes all characters
        /// from the <paramref name="start"/> position to the end of the text.
        /// </summary>
        /// <param name="start">0-based position of first character to be extracted.</param>
        /// <returns>Returns the extracted string.</returns>
        public string Extract(int start) => Text.Substring(start);

        /// <summary>
        /// Extracts a substring from the specified range of the text being parsed.
        /// </summary>
        /// <param name="start">0-based position of first character to be extracted.</param>
        /// <param name="end">0-based position of the character that follows the last
        /// character to be extracted.</param>
        /// <returns>Returns the extracted string.</returns>
        public string Extract(int start, int end) => Text.Substring(start, end - start);

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

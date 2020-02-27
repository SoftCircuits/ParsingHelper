// Copyright (c) 2019-2020 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using System;
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
        private static readonly char[] NewLineChars = { '\r', '\n' };
        private int InternalIndex;

        /// <summary>
        /// Represents a invalid character. This character is returned when a valid character
        /// is not available, such as when attempting to access a character that is
        /// out-of-bounds. The character is represented as <c>'\0'</c>.
        /// </summary>
        public const char NullChar = '\0';

        /// <summary>
        /// Returns the current text being parsed.
        /// </summary>
        public string Text { get; private set; }

        /// <summary>
        /// Returns the current position within the text being parsed.
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
        /// Constructs a ParsingHelper instance.
        /// </summary>
        /// <param name="text">The text to be parsed.</param>
        public ParsingHelper(string text)
        {
            Reset(text);
        }

        /// <summary>
        /// Resets the current position to the start of the current text.
        /// </summary>
        public void Reset()
        {
            InternalIndex = 0;
        }

        /// <summary>
        /// Sets the text to be parsed and resets the current position to the start of that text.
        /// </summary>
        /// <param name="text">The text to be parsed.</param>
        public void Reset(string text)
        {
            Text = text ?? string.Empty;
            InternalIndex = 0;
        }

        /// <summary>
        /// Indicates if the current position is at the end of the text being parsed.
        /// </summary>
        public bool EndOfText => (InternalIndex >= Text.Length);

        /// <summary>
        /// Returns the number of characters not yet parsed. This is equal to the length of the
        /// text being parsed minus the current position within that text.
        /// </summary>
        public int Remaining => (Text.Length - InternalIndex);

        /// <summary>
        /// Returns the character at the current position, or <see cref="NullChar"></see> if
        /// we're at the end of the text being parsed.
        /// </summary>
        /// <returns>The character at the current position.</returns>
        public char Peek()
        {
            Debug.Assert(InternalIndex >= 0);
            return (InternalIndex < Text.Length) ? Text[InternalIndex] : NullChar;
        }

        /// <summary>
        /// Returns the character at the specified number of characters beyond the current
        /// position, or <see cref="NullChar"></see> if the specified position is out of
        /// bounds of the text being parsed.
        /// </summary>
        /// <param name="count">The number of characters beyond the current position.</param>
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
        /// to move back.</param>
        public void Next(int count)
        {
            InternalIndex += count;
            if (InternalIndex < 0)
                InternalIndex = 0;
            else if (InternalIndex > Text.Length)
                InternalIndex = Text.Length;
        }

        /// <summary>
        /// Moves the current position to the next occurrence of the specified string and returns
        /// <c>true</c> if successful. If the specified string is not found, this method moves the
        /// current position to the end of the input text and returns <c>false</c>.
        /// </summary>
        /// <param name="s">String to find.</param>
        /// <param name="comparison">One of the enumeration values that specifies the rules for
        /// search.</param>
        /// <returns>Returns a Boolean value that indicates if any of the specified characters
        /// were found.</returns>
        public bool SkipTo(string s, StringComparison comparison = StringComparison.Ordinal)
        {
            InternalIndex = Text.IndexOf(s, InternalIndex, comparison);
            if (InternalIndex == -1)
            {
                InternalIndex = Text.Length;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Moves to the next occurrence of any one of the specified characters and returns
        /// <c>true</c> if successful. If none of the specified characters are found, this method
        /// moves the current position to the end of the input text and returns <c>false</c>.
        /// </summary>
        /// <param name="chars">Characters to skip to.</param>
        /// <returns>Returns a Boolean value that indicates if any of the specified characters
        /// were found.</returns>
        public bool SkipTo(params char[] chars)
        {
            InternalIndex = Text.IndexOfAny(chars, InternalIndex);
            if (InternalIndex == -1)
            {
                InternalIndex = Text.Length;
                return false;
            }
            return true;
        }

        /// <summary>
        /// Moves the current position forward to the start of the next line.
        /// </summary>
        /// <returns>Returns a Boolean value that indicates if another line was
        /// found.</returns>
        public bool SkipToNextLine()
        {
            // Move to start of next new line (if not already at a new line)
            char c = Peek();
            if (c != NewLineChars[0] && c != NewLineChars[1])
                SkipToEndOfLine();
            // Move to end of new line
            if (Peek() == NewLineChars[0] && Peek(1) == NewLineChars[1])
                Next();
            Next();
            return (InternalIndex < Text.Length);
        }

        /// <summary>
        /// Moves the current position forward to the next newline character.
        /// </summary>
        /// <returns>Returns a Boolean value that indicates if any newline characters
        /// were found.</returns>
        public bool SkipToEndOfLine() => SkipTo(NewLineChars);

        /// <summary>
        /// Moves the current position to the next character that is not whitespace.
        /// </summary>
        public void SkipWhiteSpace()
        {
            SkipWhile(char.IsWhiteSpace);
        }

        /// <summary>
        /// Moves the current position to the next character that is not one of the specified
        /// characters.
        /// </summary>
        /// <param name="chars">Characters to skip over.</param>
        public void Skip(params char[] chars)
        {
            SkipWhile(chars.Contains);
        }

        /// <summary>
        /// Moves the current position to the next character that causes <paramref name="predicate"/>
        /// to return false.
        /// </summary>
        /// <param name="predicate">Function to test each character.</param>
        public void SkipWhile(Func<char, bool> predicate)
        {
            while (!EndOfText && predicate(Peek()))
                Next();
        }

        /// <summary>
        /// Parses characters until the next occurrence of the specified string and returns a
        /// string with the parsed characters. If the specified string is not found, this method parses
        /// all character to the end of the input text.
        /// </summary>
        /// <param name="s">String to parse until.</param>
        /// <param name="comparison">One of the enumeration values that specifies the rules for
        /// search.</param>
        /// <returns>A string with the characters parsed.</returns>
        public string ParseTo(string s, StringComparison comparison = StringComparison.Ordinal)
        {
            int start = InternalIndex;
            InternalIndex = Text.IndexOf(s, InternalIndex, comparison);
            if (InternalIndex == -1)
                InternalIndex = Text.Length;
            return Extract(start, InternalIndex);
        }

        /// <summary>
        /// Parses characters until the next occurrence of any one of the specified characters and
        /// returns a string with the parsed characters. If none of the specified characters are found,
        /// this method parses all character to the end of the input text.
        /// </summary>
        /// <param name="chars">Characters to parse until.</param>
        /// <returns>A string with the characters parsed.</returns>
        public string ParseTo(params char[] chars)
        {
            int start = InternalIndex;
            InternalIndex = Text.IndexOfAny(chars, InternalIndex);
            if (InternalIndex == -1)
                InternalIndex = Text.Length;
            return Extract(start, InternalIndex);
        }

        /// <summary>
        /// Parses characters until the next character that causes <paramref name="predicate"/> to
        /// return false, and then returns the characters spanned. Can return an empty string.
        /// </summary>
        /// <param name="predicate">Function to test each character.</param>
        /// <returns>A string with the characters parsed.</returns>
        public string ParseWhile(Func<char, bool> predicate)
        {
            int start = InternalIndex;
            while (!EndOfText && predicate(Peek()))
                Next();
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
        /// Parses a quoted string. Interprets the character at the starting position as the quote
        /// character. Two quote characters together within the string are interpreted as a single
        /// quote literal and not the end of the string. Returns the text within the quotes and
        /// sets the current position to the first character after the ending quote character.
        /// </summary>
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
        /// Compares the given string to text at the current position using a case-sensitive comparison.
        /// </summary>
        /// <remarks>Testing showed this is the fastest way to compare part of a string. Faster even
        /// than using Span&lt;T&gt;.</remarks>
        /// <param name="s">String to compare.</param>
        /// <returns>Returns <c>true</c> if the given string matches the text at the current position.
        /// Returns false otherwise.</returns>
        public bool MatchesCurrentPosition(string s)
        {
            if (s == null || s.Length == 0 || Remaining < s.Length)
                return false;

            for (int i = 0; i < s.Length; i++)
            {
                if (s[i] != Text[InternalIndex + i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Compares the given string to text at the current position using the specified comparison.
        /// This method is not as fast as <see cref="MatchesCurrentPosition(string)"></see>. Use this
        /// method when you need a non-ordinal comparison.
        /// </summary>
        /// <remarks>Testing showed even this method was a little faster than using Span&lt;T&gt;,
        /// although clearly not as fast as <see cref="MatchesCurrentPosition(string)"></see>.</remarks>
        /// <param name="s">String to compare.</param>
        /// <param name="comparison">Type of string comparison to use.</param>
        /// <returns>Returns <c>true</c> if the given string matches the text at the current position.
        /// Returns false otherwise.</returns>
        public bool MatchesCurrentPosition(string s, StringComparison comparison)
        {
            if (s == null || s.Length == 0 || Remaining < s.Length)
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

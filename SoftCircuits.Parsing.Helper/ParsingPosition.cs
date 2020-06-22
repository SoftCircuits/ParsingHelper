// Copyright (c) 2019-2020 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using System;

namespace SoftCircuits.Parsing.Helper
{
    /// <summary>
    /// Represents a line and column position within text being parsed.
    /// </summary>
    public class ParsingPosition
    {
        private static readonly char[] NewLineCharacters = new[] { '\r', '\n' };

        /// <summary>
        /// The 1-based line number for this position.
        /// </summary>
        public int Line { get; internal set; }

        /// <summary>
        /// The 1-based column number for this position.
        /// </summary>
        public int Column { get; internal set; }

        internal ParsingPosition()
        {
            Line = 0;
            Column = 0;
        }

        /// <summary>
        /// Calculates the line and column positions for the given text and index.
        /// </summary>
        /// <param name="text">The text to calculate the position for.</param>
        /// <param name="index">The position as an index into <paramref name="text"/>.</param>
        /// <returns>A <see cref="ParsingPosition"/> with the calculated position.</returns>
        public static ParsingPosition CalculatePosition(string text, int index)
        {
            if (text == null)
                throw new ArgumentNullException(nameof(text));
            if (index > text.Length)
                throw new ArgumentOutOfRangeException(nameof(index), "Index must be less than or equal to the length of the string.");

            ParsingPosition position = new ParsingPosition();
            int pos = 0;
            int lineStartPos = 0;

            // Count lines
            while (pos < index)
            {
                pos = text.IndexOfAny(NewLineCharacters, pos);

                // Test if no more lines found
                if (pos < 0 || pos >= index)
                    break;

                // Skip new line
                if (pos < (text.Length - 1) &&
                    text[pos] == NewLineCharacters[0] &&
                    text[pos + 1] == NewLineCharacters[1])
                    pos++;
                pos++;

                // Special case where current position is within newline
                if (pos > index)
                    continue;

                // Update line position
                position.Line++;
                lineStartPos = pos;
            }
            position.Line++;
            position.Column = (index - lineStartPos) + 1;
            return position;
        }
    }
}

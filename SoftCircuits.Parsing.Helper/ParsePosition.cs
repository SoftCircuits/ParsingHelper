// Copyright (c) 2019-2021 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using System;

namespace SoftCircuits.Parsing.Helper
{
    /// <summary>
    /// Represents a line and column position within a string.
    /// </summary>
    [Obsolete("This class is deprecated and will be removed in a future version. Please use ParsePosition instead.")]
    public class ParsingPosition : ParsePosition { }

    /// <summary>
    /// Represents a line and column position within a string.
    /// </summary>
    public class ParsePosition
    {
        /// <summary>
        /// The 1-based line number for this position.
        /// </summary>
        public int Line { get; internal set; }

        /// <summary>
        /// The 1-based column number for this position.
        /// </summary>
        public int Column { get; internal set; }

        internal ParsePosition()
        {
            Line = 0;
            Column = 0;
        }

        /// <summary>
        /// Calculates the line and column position for the given text and index.
        /// </summary>
        /// <param name="text">The text to calculate the position for.</param>
        /// <param name="index">The position as an index into <paramref name="text"/>.</param>
        /// <returns>A <see cref="ParsePosition"/> with the calculated line and column
        /// positions.</returns>
        public static ParsePosition CalculatePosition(string text, int index)
        {
            ParsingHelper helper = new ParsingHelper(text);
            ParsePosition position = new ParsePosition();
            int lineStartPos = 0;

            while (helper < index)
            {
                if (!helper.SkipToNextLine() || helper > index)
                    break;
                position.Line++;
                lineStartPos = helper;
            }
            position.Line++;
            position.Column = (index - lineStartPos) + 1;
            return position;
        }
    }
}

// Copyright (c) 2019-2023 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

namespace SoftCircuits.Parsing.Helper
{
    /// <summary>
    /// Specifies options for the <see cref="ParsingHelper.SkipWhiteSpace(SkipWhiteSpaceOption)"/>
    /// method.
    /// </summary>
    public enum SkipWhiteSpaceOption
    {
        /// <summary>
        /// Stop if a line line break character is found.
        /// </summary>
        StopAtEol,

        /// <summary>
        /// Stop if the start of a new line (after a line break) is found.
        /// </summary>
        StopAtNextLine
    }
}

// Copyright (c) 2019-2020 Jonathan Wood (www.softcircuits.com)
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
        /// Stop at the first new-line character.
        /// </summary>
        StopAtEol,

        /// <summary>
        /// Stop at the start of the next line.
        /// </summary>
        StopAtNextLine
    }
}

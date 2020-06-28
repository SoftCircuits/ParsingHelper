# ParsingHelper

[![NuGet version (SoftCircuits.Parsing.Helper)](https://img.shields.io/nuget/v/SoftCircuits.Parsing.Helper.svg?style=flat-square)](https://www.nuget.org/packages/SoftCircuits.Parsing.Helper/)

```
Install-Package SoftCircuits.Parsing.Helper
```

## Introduction

`ParsingHelper` is a .NET class library that makes it easier to parse text. The library tracks the current position within the text, ensures your code never attempts to access a character at an invalid index, and includes a number of methods that make parsing easier. The library makes your text-parsing code more concise and more robust.

## Examples

Here are a couple of examples to illustrate use of the library.

#### Parse a Sentence into Words

This example parses a sentence into words. This implementation only considers spaces and periods as word delimiters. But you could easily add more characters, or use the overload of `ParsingHelper.ParseTokens()` that accepts a lambda expression.

```cs
ParsingHelper helper = new ParsingHelper("The quick brown fox jumps over the lazy dog.");

List<string> words = helper.ParseTokens(' ', '.').ToList();

CollectionAssert.AreEqual(new[] {
    "The",
    "quick",
    "brown",
    "fox",
    "jumps",
    "over",
    "the",
    "lazy",
    "dog" }, words);
```

#### Command Line

This example parses a command line. It detects both arguments and flags (arguments preceded with `'-'` or `'/'`). It's okay with whitespace between the flag character and flag. And any argument or flag that contains whitespace can be enclosed in quotes.

```cs
ParsingHelper helper = new ParsingHelper("app -v -f /d-o file1 \"file 2\"");
List<string> arguments = new List<string>();
List<string> flags = new List<string>();

char[] flagCharacters = new char[] { '-', '/' };
string arg;
bool isFlag = false;

while (!helper.EndOfText)
{
    // Skip any whitespace
    helper.SkipWhiteSpace();
    // Is this a flag?
    if (flagCharacters.Contains(helper.Peek()))
    {
        isFlag = true;
        // Skip over flag character
        helper++;
        // Allow whitespace between flag character and flag
        helper.SkipWhiteSpace();
    }
    else isFlag = false;
    // Parse item
    if (helper.Peek() == '"')
        arg = helper.ParseQuotedText();
    else
        arg = helper.ParseWhile(c => !char.IsWhiteSpace(c) && !flagCharacters.Contains(c));
    // Add argument to appropriate collection
    if (isFlag)
        flags.Add(arg);
    else
        arguments.Add(arg);
}

CollectionAssert.AreEqual(new[] { "app", "file1", "file 2" }, arguments);
CollectionAssert.AreEqual(new[] { "v", "f", "d", "o" }, flags);
```

#### Regular Expressions

This example uses a regular expression to find all the words in a string that start with the letter "J".

```cs
ParsingHelper helper = new ParsingHelper("Jim Jack Sally Jennifer Bob Gary Jonathan Bill");

IEnumerable<string> results = helper.ParseTokensRegEx(@"\b[J]\w+");

CollectionAssert.AreEqual(new[]
{
    "Jim",
    "Jack",
    "Jennifer",
    "Jonathan"
}, results.ToList());
```

## Documentation

The `ParsingHelper` constructor accepts a string argument that represents the text you are going to parse. If this argument is `null`, it will be safely treated as an empty string.

```cs
ParsingHelper helper = new ParsingHelper("The quick brown fox jumps over the lazy dog.");
```

You can call the `Reset()` method to reset the current position back to the start of the string. The `Reset()` method accepts an optional string argument and, if supplied, will configure the class to begin parsing the new string.

Use the `Peek()` method to read the character at the current position (without changing the current position). The `Peek()` method can optionally accept an integer argument that specifies the character position as the number of characters ahead of the current position. For example, `Peek(1)` would return the character that comes after the character at the current position. (Calling `Peek()` is equal to calling `Peek(0)`.) If the position specified is out of bounds for the current string, `Peek()` returns  `ParsingHelper.NullChar` (equal to `'\0'`).

The `Text` property returns the string being parsed. And the `Index` property returns the current position within the string being parsed. Although you would normally use the navigation methods to change the `Index` value, you can set it directly. If you set the `Index` property an invalid value, it will be adjusted so it is always in the range of 0 to `Text.Length`.

The `EndOfText` property returns `true` when you have reached the end of the text. And the `Remaining` property returns the number of characters still to be parsed (calculated as `Text.Length - Index`).

## Navigation

To advance the parser to the next character position, use the `Next()` method. This method can also accept an optional argument that specifies the number of characters to advance. For example, if you pass `5`, the current position will be advanced five characters. (Calling `Next()` with no arguments is equal to calling `Next(1)`.) The argument to `Next()` can be a negative value if you want to move backwards.

As an alternative to the `Next()` method, `ParserHelper` overloads several operators that can be used as a shortcut to change the current position. These are demonstrated in the following example.

```cs
helper++;            // Same as helper.Next()
helper--;            // Same as helper.Next(-1)
helper += 2;         // Same as helper.Next(2)
helper -= 2;         // Same as helper.Next(-2)
helper = helper + 3; // Same as helper.Next(3)
helper = helper - 3; // Same as helper.Next(-3)
int i = helper;      // Same as i = helper.Index

// Safely moves to the end of the text if you add a number that is too large
helper += 1000000;

// Safely moves to the start of the text if you subtract a number that is too large
helper -= 1000000;
```

This simple example shows how you might print each character in the text being parsed.

```cs
while (!helper.EndOfText)
{
    Console.WriteLine(helper.Peek());
    helper++;
}
```

## Tracking Line and Column Position

For performance reasons, ParsingHelper does not track the current line and column values as it parses. However, you can use the `CalculatePosition()` method to obtain the line and column values that corresponds to the current position. This is useful for providing more information when reporting parsing errors back to the end user.

## Skipping to Characters

In addition to advancing a specified number of characters, the library also provides ways to advance to various tokens.

The `SkipTo()` method advances to the next occurrence of the given string.

```cs
helper.SkipTo("fox");
```

This example advances the current position to the start of the next occurrence of `"fox"`. If no more occurrences are found, this method advances to the very end of the text and returns `false`. The `SkipTo()` method supports an optional `StringComparison` value to specify how characters should be compared.

The `SkipTo()` method is overloaded to also accept any number of `char` arguments (or a `char` array).

```cs
helper.SkipTo('x', 'y', 'z');
```

This example will advance the current position to the first occurrence of any one of the specified characters. If none of the characters are found, this method advances to the end of the text and returns `false`.

Use the `SkipToEndOfLine()` to advance the current position to the first character that is a new-line character (i.e., `'\r'` or `'\n'`). If neither of the characters are found, this method advances to the end of the text and returns `false`. Use the `SkipToNextLine()` to advance the current position to the first character in the next line. If no next line is found, this method advances to the end of the text and returns `false`.

## Skipping Past Characters

To skip over a group of characters, you can use the `Skip()` method. This method accepts any number of `char` arguments (or a `char` array). It will advance the current position to the first character that is not one of the arguments.

The following example would skip over any numeric digits.

```cs
helper.Skip('1', '2', '3', '4', '5', '6', '7', '8', '9', '0');
```

The `SkipWhile()` method accepts a predicate that specifies when this method should stop skipping. The following example would skip over any characters that are not an equal sign:

```cs
helper.SkipWhile(c => c != '=');
```

A common task when parsing is to skip over any whitespace characters. Use the `SkipWhiteSpace()` method to advance the current position to the next character that is not a white space character.

For another example of `SkipWhile()`, here's how it is used to implement the `SkipWhiteSpace()` method.

```cs
public void SkipWhiteSpace()
{
    SkipWhile(char.IsWhiteSpace);
}
```

## Parsing Characters

The `ParseWhile()` method accept a predicate that specifies when this method should stop parsing. It works like the `SkipWhile()` method except that `ParseWhile()` will return the characters that were skipped. (Note that `SkipWhile()` is faster and should be used when you do not need the skipped characters.)

The following example will parse all letters starting from the current position.

```cs
string token = helper.ParseWhile(char.IsLetter);
```

The `ParseTo()` method parses characters until a delimiter character is found, and returns the characters that were parsed. There are two versions of this method: one takes a `param` array of characters that specify the delimiters, and the other accepts a predicate that returns true for characters that are delimiters.

In addition, the library also defines the `ParseToken()` method. This method takes a list of delimiters and will skip all characters that are a delimiter, then parse all characters that are not a delimiter and return the parsed characters. Delimiters can be specified as character parameters, a character array, or a predicate that returns true if the given character is a delimiter.

```cs
string token;
token = helper.ParseToken(' ', '\t', '\r', '\n');
token = helper.ParseToken(char.IsWhiteSpace);
```

## Parsing Quoted Text

You may have an occassion to parse quoted text. In this case, you will probably want the quoted text (without the quotes). The `ParseQuotedText()` method makes this easy.

Call this method with the current position at the first quote character. The method will use the character at the current position to determine what the quote character is. (So the quote character can be any character you choose.)

This method will parse characters until the closing quote is found. If the closing quote is found, it will set the current position to the character after the closing quote and return the text within the quotes. If the closing quote is not found, it will return everything after the starting quote to the end of the string, and will advance the current position to the end of the string.

If `ParseQuotedText()` encounters two quote characters together, it will interpret them as a single quote character and not the end of the quoted text. For example, consider the following example:

```cs
ParsingHelper helper = new ParsingHelper("One two \"three and \"\"four\"\"!");
helper.MoveTo('"');
string token = helper.ParseQuotedText();
```

This example would set the `token` variable to `three and "four"`. The two pairs of quotes are interpreted each as one quote in the text and not the end of the quoted text.

## Extracting Text

It is common to want to extract text tokens as you parse them. You can use the `Extract()` method to do this. The `Extract()` method accepts two integer arguments that specify the 0-based position of the first character to be extracted and the 0-based position of the character that follows the last character to be extracted.

```cs
string token = helper.Extract(start, end);
```

This method is overloaded with a version that only accepts one integer argument. The argument specifies the 0-based position of the first character to be extracted, and this method will extract everything from that position to the end of the text.

Neither of these methods change the current position.

## Comparing Text

Finally, you may need to test if a predefined string is equal to the text at the current location. The `MatchesCurrentPosition()` method tests this. It accepts a string argument and returns a Boolean value that indicates if the specified string matches the text starting at the current location.  The `MatchesCurrentPosition()` method supports an optional `StringComparison` value to specify how characters should be compared. Note that while this method can be handy, it's less performant than most methods in this class. Any type of search function that works by calling this method at each successive position should be avoided where performance matters.

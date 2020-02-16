# ParsingHelper

[![NuGet version (SoftCircuits.Parsing.Helper)](https://img.shields.io/nuget/v/SoftCircuits.Parsing.Helper.svg?style=flat-square)](https://www.nuget.org/packages/SoftCircuits.Parsing.Helper/)

```
Install-Package SoftCircuits.Parsing.Helper
```

## Introduction

`ParsingHelper` is a .NET class library that helps you parse text. It tracks the current position within the text being parsed and provides a number of methods that make parsing easier.

The library also ensures you never attempt to access an out-of-range character, which would throw an exception. This means you can focus on your parsing logic instead of sprinkling your code with checks to make sure you are still within bounds.

## Getting Started

The `ParsingHelper` constructor accepts a string argument that represents the text you are going to parse. If this argument is `null`, it will be safely treated as an empty string.

```cs
ParsingHelper helper = new ParsingHelper("The quick brown fox jumps over the lazy dog.");
```

You can call the `Reset()` method to reset the current position back to the start of the string. The `Reset()` method accepts an optional string argument and, if supplied, will cause the class to begin parsing the new string.

Use the `Peek()` method to read the character at the current position (without changing the current position). The `Peek()` method can optionally accept an integer argument that specifies the character position as the number of characters ahead of the current position. For example, `Peek(1)` would return the character that comes after the character at the current position. (Calling `Peek()` is equal to calling `Peek(0)`.) If the position specified is out of bounds for the current string, `Peek()` returns  `ParsingHelper.NullChar` (equal to `'\0'`).

The `Text` property returns the string being parsed. And the `Index` property returns the current position within the string being parsed.

The `EndOfText` property returns `true` when you have reached the end of the text. And the `Remaining` property returns the number of characters still to be parsed. (The value returned by `Remaining` is equal `Text.Length - Index`.)

## Navigation

To advance the parser to the next position, use the `Next()` method. The `Next()` method advances the current position to the next character. This method can also accept an optional argument that specifies the number of characters to advance. For example, if you pass `5`, the current position will be advanced five characters. (Calling `Next()` with no arguments is equal to calling `Next(1)`.)

As an alternative to the `Next()` method, `ParserHelper` overloads several operators that can be used as a shortcut to change the current position. These are demonstrated in the following example.

```cs
helper++;            // Same as helper.Next()
helper--;            // Same as helper.Next(-1)
helper += 2;         // Same as helper.Next(2)
helper -= 2;         // Same as helper.Next(-2)
helper = helper + 3; // Same as helper.Next(3)
helper = helper - 3; // Same as helper.Next(-3)
int i = helper;      // Same as i = helper.Index

// If you attempt to move past the end of the text (or before the start of the text)
// ParserHelper will safely move the current position to the end or start of the text.
helper += 1000000;
helper -= 1000000;
```


```cs
ParsingHelper helper = new ParsingHelper("abc");

while (!helper.EndOfText)
{
    Console.WriteLine(helper.Peek());
    helper++;
}
```

In addition to advancing a specified number of characters, the library also provides ways to advance to various tokens.

The `SkipTo()` method skips to the next occurrence of a given string.

```cs
helper.SkipTo("abc");
```

This example advances the current position to the start of the next occurrence of `"abc"`. If no more occurrences are found, this method advances to the very end of the text and returns `false`. The `SkipTo()` method supports an optional `StringComparison` value to specify how characters should be compared.

The `SkipTo()` method is overloaded to also accept any number of `char` arguments (or a `char` array).

```cs
helper.SkipTo('x', 'y', 'z');
```

This example will advance the current position to the first occurrence of any one of the specified characters. If none of the characters are found, this method advances to the end of the text and returns `false`.

A common task when parsing is to skip over any whitespace characters. The `SkipWhiteSpace()` method advances the current position to the next character that is not a white space character.

```cs
helper.SkipWhiteSpace();
```

Use the `SkipToEndOfLine()` to advance the current position to the first character that is a new-line character (i.e., `'\r'` or `'\n'`). If neither of the characters are found, this method advances to the end of the text and returns `false`. Use the `SkipToNextLine()` to advance the current position to the first character in the next line. If no next line is found, this method advances to the end of the text and returns `false`.

To skip over a group of characters, you can use the `Skip()` method. This method accepts any number of `char` arguments (or a `char` array). It will advance the current position to the first character that is not one of the arguments.

The following example would skip over any numeric digits.

```cs
helper.Skip('1', '2', '3', '4', '5', '6', '7', '8', '9', '0');
```

## SkipWhile() and ParseWhile() Methods

The `SkipWhile()` and `ParseWhile()` methods both accept a predicate that specifies if the next character should stop processing.

The following example would skip over any characters that are not an equal sign:

```cs
helper.SkipWhile(c => c != '=');
```

For another example, see how `SkipWhile()` is used to implement the `SkipWhiteSpace()` method.

```cs
public void SkipWhiteSpace()
{
    SkipWhile(char.IsWhiteSpace);
}
```

The `ParseWhile()` method is similar to `SkipWhile()` except that `ParseWhile()` will return the characters that were skipped. (Note that `SkipWhile()` will perform slightly faster.)

The following example will parse all letters starting from the current position.

```cs
string token = helper.ParseWhile(char.IsLetter);
```

In addition, the library also defines the `ParseToken()` method. This method takes a list of delimiters and will skip all characters that are a delimiter, then parse all characters that are not a delimiter and return the parsed characters. Delimiters can be specified as character parameters, a character array or a predicate that returns true if the given character is a delimiter.

```cs
string token;
token = helper.ParseToken(' ', '\t', '\r', '\n');
token = helper.ParseToken(char.IsWhiteSpace);
```

## Parsing Quoted Text

You may have an occassion to parse quoted text. In this case, you will probably want the quoted text (without the quotes). The `ParseQuotedText()` method makes this easy.

Call this method with the current position at the first quote character. The method will use the current character to determine what the quote character is. So it accepts both `'"'` and `'\''`, or any other character!

This method will parse characters until the closing quote is found. If the closing quote is found, it will set the current position to the character after the closing quote and return the text within the quotes. If the closing quote is not found, it will return everything after the starting quote to the end of the string, and will advance the current position to the end of the string.

If it encounters two quote characters together, it will interpret them as a single quote character and not the end of the quoted text. For example, consider the following example:

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

# ParsingHelper

[![NuGet version (SoftCircuits.Parsing.Helper)](https://img.shields.io/nuget/v/SoftCircuits.Parsing.Helper.svg?style=flat-square)](https://www.nuget.org/packages/SoftCircuits.Parsing.Helper/)

```
Install-Package SoftCircuits.Parsing.Helper
```

## Overview

ParsingHelper is a .NET class library to assist pasing text. It helps by tracking the current position within the text being parsed and provides a number of helper methods to ease some parsing tasks.

One of the more difficult things to worry about when parsing text with .NET is that you never attempt to read beyond the valid range of the input text. In .NET languages, this produces a run-time exception. Since text parsers often have to deal with varied or malformed input, in most cases you'll want to detect the problem rather than interrupt your code with a run-time exception.

While it's easy to avoid reading beyond the end of a string in a simple program, it can be more difficult in a complex program that is searching for a closing quote or other expected characters, for example. And your code logic becomes much more complex because you must add these special checks throughout your other logic.

ParsingHelper is designed to assist with these tasks, and allow your code to be much more straight forward.

## Getting Started

The ParsingHelper constructor accepts a string argument that represents the text you are going to parse. If this argument is `null`, it will be safely treated as an empty string.

```cs
ParsingHelper helper = new ParsingHelper("abc");
```

If you want to start over, you can call the `Reset()` method. This method sets the current position back to the start of the current string. Optionally, you can pass a string argument to `Reset()`. In this case, ParsingHelper will be configured to start parsing the new string that was provided.

The `EndOfText` property returns `true` when you have reached the end of the text.

The `Remaining` property returns the number of characters still to be parsed. The value returned is equal to the length of the string being parsed minus the current position.

The `ParsingHelper` class also exposes two properties: `Text` and `Index`. `Text` is a read-only property the returns the current text string that is being parsed. `Index` is a read-only property that returns the current position within the current text string.

Finally, there is one static property, `ParsingHelper.NullChar`, which is equal to `'\0'`. This character is returned when you read a character from an invalid position.

## Processing Characters

Use the `Peek()` method to read the character at the current location. In addition, the `Peek()` method can optionally accept an integer argument. This argument specifies the character position as the number of characters ahead of the current position. For example, `Peek(1)` would return the character that comes after the character at the current position.

Note that `Peek()` never changes the current position (even when arguments are passed). To advance to the next position, use the `Next()` method. The `Next()` method advances the current position to the next character.




```cs
ParsingHelper helper = new ParsingHelper("abc");

while (!helper.EndOfText)
{
    Console.WriteLine(helper.Peek());
    helper.Next();
}
```



Consider the following example:

```cs
ParsingHelper helper = new ParsingHelper("abc");
char c = helper.Peek(42);
```

In this case, `c` will be set to `ParsingHelper.NullChar`, which is equal to `'\0'`.

In this example, the problem is obvious. The `Peek()` method returns the value of the character that is the given number of characters ahead of the current position

## XXX

```cs

```

## Parsing Quoted Text


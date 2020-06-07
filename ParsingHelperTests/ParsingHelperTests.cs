// Copyright (c) 2019-2020 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftCircuits.Parsing.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Channels;

namespace ParsingHelperTests
{
    [TestClass]
    public class ParsingHelperTests
    {
        private const string ShortTest = "Four score and seven years ago";
        private const string LongTest = @"Four score and seven years ago our fathers brought forth on this continent,
a new nation, conceived in Liberty, and dedicated to the proposition that all men are created equal.

Now we are engaged in a great civil war, testing whether that nation, or any nation so conceived and so
dedicated, can long endure. We are met on a great battle-field of that war. We have come to dedicate a
portion of that field, as a final resting place for those who here gave their lives that that nation might
live. It is altogether fitting and proper that we should do this.

But, in a larger sense, we can not dedicate -- we can not consecrate -- we can not hallow -- this ground.
The brave men, living and dead, who struggled here, have consecrated it, far above our poor power to add or
detract. The world will little note, nor long remember what we say here, but it can never forget what they
did here. It is for us the living, rather, to be dedicated here to the unfinished work which they who fought
here have thus far so nobly advanced. It is rather for us to be here dedicated to the great task remaining
before us -- that from these honored dead we take increased devotion to that cause for which they gave the
last full measure of devotion -- that we here highly resolve that these dead shall not have died in vain --
that this nation, under God, shall have a new birth of freedom -- and that government of the people, by the
people, for the people, shall not perish from the earth.";

        [TestMethod]
        public void BasicTests()
        {
            ParsingHelper helper = new ParsingHelper(ShortTest);

            // Initial state
            Assert.AreEqual('\0', ParsingHelper.NullChar);
            Assert.AreEqual(ShortTest, helper.Text);
            Assert.AreEqual(0, helper.Index);
            Assert.AreEqual(false, helper.EndOfText);
            Assert.AreEqual(ShortTest.Length, helper.Remaining);

            // Peek
            Assert.AreEqual('F', helper.Peek());
            Assert.AreEqual('o', helper.Peek(1));
            Assert.AreEqual('u', helper.Peek(2));
            Assert.AreEqual('r', helper.Peek(3));
            Assert.AreEqual(ParsingHelper.NullChar, helper.Peek(1000));
            Assert.AreEqual(ParsingHelper.NullChar, helper.Peek(-1000));
            Assert.AreEqual(0, helper.Index);   // Index didn't change

            // Next
            helper.Next();
            Assert.AreEqual(1, helper.Index);
            Assert.AreEqual('o', helper.Peek());
            helper.Next(2);
            Assert.AreEqual(3, helper.Index);
            Assert.AreEqual('r', helper.Peek());
            helper.Next(-2);
            Assert.AreEqual(1, helper.Index);
            Assert.AreEqual('o', helper.Peek());
            Assert.AreEqual(false, helper.EndOfText);
            Assert.AreEqual(ShortTest.Length - helper.Index, helper.Remaining);

            helper.Next(10000);
            Assert.AreEqual(helper.Text.Length, helper.Index);
            Assert.AreEqual(true, helper.EndOfText);
            Assert.AreEqual(0, helper.Remaining);
            helper.Next(-10000);
            Assert.AreEqual(0, helper.Index);
            Assert.AreEqual(false, helper.EndOfText);
            Assert.AreEqual(ShortTest.Length, helper.Remaining);

            helper.Index = 10000;
            Assert.AreEqual(ShortTest.Length, helper.Index);
            Assert.AreEqual(true, helper.EndOfText);
            Assert.AreEqual(0, helper.Remaining);
            helper.Index = -10000;
            Assert.AreEqual(0, helper.Index);
            Assert.AreEqual(false, helper.EndOfText);
            Assert.AreEqual(ShortTest.Length, helper.Remaining);

            helper.Index = 0;
            Assert.AreEqual(0, helper.Index);
            Assert.AreEqual(ShortTest.Length, helper.Remaining);
            Assert.AreEqual(false, helper.EndOfText);
            helper.Index = helper.Text.Length;
            Assert.AreEqual(helper.Text.Length, helper.Index);
            Assert.AreEqual(0, helper.Remaining);
            Assert.AreEqual(true, helper.EndOfText);
            helper.Index = 5;
            Assert.AreEqual(5, helper.Index);
            Assert.AreEqual(ShortTest.Length - 5, helper.Remaining);
            Assert.AreEqual(false, helper.EndOfText);

            helper.Reset();
            Assert.AreEqual(0, helper.Index);
            Assert.AreEqual(ShortTest, helper.Text);

            helper.Reset(null);
            Assert.AreEqual(0, helper.Index);
            Assert.AreEqual(string.Empty, helper.Text);
            Assert.AreEqual(true, helper.EndOfText);
            Assert.AreEqual(0, helper.Remaining);
        }

        [TestMethod]
        public void SkipTests()
        {
            ParsingHelper helper = new ParsingHelper(LongTest);

            // SkipTo
            Assert.IsTrue(helper.SkipTo("score"));
            Assert.AreEqual('s', helper.Peek());
            Assert.AreEqual('c', helper.Peek(1));
            helper.Reset();
            Assert.IsTrue(helper.SkipTo("SCORE", StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual('s', helper.Peek());
            Assert.AreEqual('c', helper.Peek(1));
            helper.Reset();
            Assert.IsTrue(helper.SkipTo('v'));
            Assert.AreEqual('v', helper.Peek());
            Assert.AreEqual('e', helper.Peek(1));
            Assert.IsFalse(helper.SkipTo("XxXxXxX"));
            Assert.AreEqual(LongTest.Length, helper.Index);
            Assert.AreEqual(true, helper.EndOfText);
            Assert.AreEqual(0, helper.Remaining);

            // SkipWhiteSpace
            helper.Reset();
            Assert.IsTrue(helper.SkipTo(' '));
            helper.SkipWhiteSpace();
            Assert.AreEqual('s', helper.Peek());

            // SkipWhile
            helper.SkipWhile(c => "score".Contains(c));
            Assert.AreEqual(' ', helper.Peek());
            Assert.AreEqual('a', helper.Peek(1));

            // SkipToNextLine/SkipToEndOfLine
            helper.Reset();
            helper.SkipToEndOfLine();
            Assert.AreEqual('\r', helper.Peek());
            Assert.AreEqual('\n', helper.Peek(1));
            helper.SkipToNextLine();
            Assert.AreEqual('a', helper.Peek());
            Assert.AreEqual(' ', helper.Peek(1));
            helper.SkipToNextLine();
            helper.SkipToNextLine();
            Assert.AreEqual('N', helper.Peek());
            Assert.AreEqual('o', helper.Peek(1));

            // Skip
            helper.Skip('N', 'o', 'w', ' ', 'e');
            Assert.AreEqual('a', helper.Peek());
            Assert.AreEqual('r', helper.Peek(1));
        }

        [TestMethod]
        public void ParseTests()
        {
            ParsingHelper helper = new ParsingHelper(LongTest);

            Assert.IsTrue(helper.SkipTo("score"));
            Assert.AreEqual("score and seven years ago our ", helper.ParseTo("fathers"));
            Assert.AreEqual('f', helper.Peek());

            helper.Reset();
            Assert.IsTrue(helper.SkipTo("score"));
            Assert.AreEqual("score and seven years ago our ", helper.ParseTo("FATHERS", StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual('f', helper.Peek());

            helper.Reset();
            Assert.IsTrue(helper.SkipTo("score"));
            Assert.AreEqual("score and se", helper.ParseTo('v', 'X', 'Y', 'Z'));
            Assert.AreEqual('v', helper.Peek());

            helper.Reset();
            Assert.IsTrue(helper.SkipTo("score"));
            Assert.AreEqual("score", helper.Parse('e', 'r', 'o', 'c', 's'));
            Assert.AreEqual(' ', helper.Peek());
            Assert.AreEqual(" ", helper.Parse(' '));
            Assert.AreEqual('a', helper.Peek());

            helper.Reset();
            Assert.IsTrue(helper.SkipTo("score"));
            Assert.AreEqual("score and seven years ago our fathers brought forth on this continent", helper.ParseWhile(c => c != ','));
            Assert.AreEqual(',', helper.Peek());

            helper.Next();  // Skip comma
            Assert.AreEqual("a", helper.ParseToken(char.IsWhiteSpace));
            Assert.AreEqual(' ', helper.Peek());
            Assert.AreEqual('n', helper.Peek(1));

            helper.Reset();
            Assert.AreEqual("Four", helper.ParseToken(' ', '\r', '\n'));
            Assert.AreEqual(' ', helper.Peek());

            string parseAllText = "  \t\tthe \r\n\t\t  rain in\t\t    spain\r\n   falls\r\nmainly  on\tthe\r\nplain.    ";
            string[] parseAllResults = new[] { "the", "rain", "in", "spain", "falls", "mainly", "on", "the", "plain" };

            helper.Reset(parseAllText);
            CollectionAssert.AreEqual(parseAllResults, helper.ParseAllTokens(' ', '\t', '\r', '\n', '.').ToList());

            helper.Reset();
            CollectionAssert.AreEqual(parseAllResults, helper.ParseAllTokens(c => " \t\r\n.".Contains(c)).ToList());
        }

        [TestMethod]
        public void QuotedTextTests()
        {
            ParsingHelper helper = new ParsingHelper("He turned and said, \"Yes.\"");
            Assert.IsTrue(helper.SkipTo('"'));
            Assert.AreEqual("Yes.", helper.ParseQuotedText());
            Assert.AreEqual(helper.Text.Length, helper.Index);
            Assert.AreEqual(ParsingHelper.NullChar, helper.Peek());

            helper = new ParsingHelper("He turned and said, \"I turned and said, \"\"Yes\"\".\"");
            Assert.IsTrue(helper.SkipTo('"'));
            Assert.AreEqual("I turned and said, \"Yes\".", helper.ParseQuotedText());
            Assert.AreEqual(helper.Text.Length, helper.Index);
            Assert.AreEqual(ParsingHelper.NullChar, helper.Peek());

            helper = new ParsingHelper("He turned and said, 'I turned and said, ''Yes''.'");
            Assert.IsTrue(helper.SkipTo('\''));
            Assert.AreEqual("I turned and said, 'Yes'.", helper.ParseQuotedText());
            Assert.AreEqual(helper.Text.Length, helper.Index);
            Assert.AreEqual(ParsingHelper.NullChar, helper.Peek());
        }

        [TestMethod]
        public void MatchesCurrentPositionTests()
        {
            ParsingHelper helper = new ParsingHelper(LongTest);
            Assert.IsTrue(helper.SkipTo("consecrated it"));
            Assert.AreEqual(true, helper.MatchesCurrentPosition("consecrated it"));
            Assert.AreEqual(true, helper.MatchesCurrentPosition("CONSECRATED IT", StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual(false, helper.MatchesCurrentPosition("consecrated_it"));
            Assert.AreEqual(false, helper.MatchesCurrentPosition("CONSECRATED_IT", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void ExtractTests()
        {
            ParsingHelper helper = new ParsingHelper(LongTest);
            string s = "consecrated it";
            Assert.IsTrue(helper.SkipTo(s));
            int start = helper.Index;
            helper.Next(s.Length);
            Assert.AreEqual(s, helper.Extract(start, helper.Index));
            Assert.AreEqual(@"consecrated it, far above our poor power to add or
detract. The world will little note, nor long remember what we say here, but it can never forget what they
did here. It is for us the living, rather, to be dedicated here to the unfinished work which they who fought
here have thus far so nobly advanced. It is rather for us to be here dedicated to the great task remaining
before us -- that from these honored dead we take increased devotion to that cause for which they gave the
last full measure of devotion -- that we here highly resolve that these dead shall not have died in vain --
that this nation, under God, shall have a new birth of freedom -- and that government of the people, by the
people, for the people, shall not perish from the earth.", helper.Extract(start));
            Assert.AreEqual(LongTest, helper.Extract(0, LongTest.Length));
            Assert.AreEqual("score", helper.Extract(5, 10));
            Assert.AreNotEqual("score", helper.Extract(5, 11));
            Assert.AreNotEqual("score", helper.Extract(4, 10));
            Assert.AreEqual(string.Empty, helper.Extract(0, 0));
            Assert.AreEqual(string.Empty, helper.Extract(LongTest.Length, LongTest.Length));
        }

        [TestMethod]
        public void OperatorOverloadTests()
        {
            ParsingHelper helper = new ParsingHelper(LongTest);

            for (int i = 0; !helper.EndOfText; i++, helper++)
            {
                Assert.AreEqual(i, helper.Index);
                Assert.AreEqual(LongTest[i], helper.Peek());
            }

            helper.Reset();
            helper++;
            Assert.AreEqual(1, helper);
            helper += 2;
            Assert.AreEqual(3, helper);
            helper = helper + 2;
            Assert.AreEqual(5, helper);
            helper -= 2;
            Assert.AreEqual(3, helper);
            helper = helper - 2;
            Assert.AreEqual(1, helper);
            helper--;
            Assert.AreEqual(0, helper);
            helper += 10000;
            Assert.AreEqual(LongTest.Length, helper);
            helper -= 10000;
            Assert.AreEqual(0, helper);
        }
    }
}

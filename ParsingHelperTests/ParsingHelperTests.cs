// Copyright (c) 2019-2020 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using Microsoft.VisualStudio.TestTools.UnitTesting;
using SoftCircuits.Parsing.Helper;
using System;
using System.Linq;

namespace ParsingHelperTests
{
    [TestClass]
    public class ParsingHelperTests
    {
        private const string Alphabet = "Abcdefghijklmnopqrstuvwxyz";
        private const string Numbers = "1234567890";
        private const string TestString = @"Four score and seven years ago our fathers brought forth on this continent,
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
            ParsingHelper helper = new ParsingHelper(Alphabet);

            Assert.AreEqual('\0', ParsingHelper.NullChar);
            Assert.AreEqual(Alphabet, helper.Text);
            Assert.AreEqual(0, helper.Index);

            Assert.AreEqual('A', helper.Peek());
            Assert.AreEqual('b', helper.Peek(1));
            Assert.AreEqual(ParsingHelper.NullChar, helper.Peek(1000));
            Assert.AreEqual(0, helper.Index);

            helper.Next();
            Assert.AreEqual(1, helper.Index);
            Assert.AreEqual('b', helper.Peek());
            helper.Next(2);
            Assert.AreEqual(3, helper.Index);
            Assert.AreEqual('d', helper.Peek());
            helper.Next(-2);
            Assert.AreEqual(1, helper.Index);
            Assert.AreEqual('b', helper.Peek());

            Assert.AreEqual(false, helper.EndOfText);
            Assert.AreEqual(helper.Text.Length - helper.Index, helper.Remaining);

            helper.Next(-10000);
            Assert.AreEqual(0, helper.Index);
            Assert.AreEqual(false, helper.EndOfText);
            Assert.AreEqual(helper.Text.Length, helper.Remaining);

            helper.Next(10000);
            Assert.AreEqual(helper.Text.Length, helper.Index);
            Assert.AreEqual(true, helper.EndOfText);
            Assert.AreEqual(0, helper.Remaining);

            helper.Index = -10000;
            Assert.AreEqual(0, helper.Index);
            Assert.AreEqual(false, helper.EndOfText);
            Assert.AreEqual(helper.Text.Length, helper.Remaining);

            helper.Index = 10000;
            Assert.AreEqual(helper.Text.Length, helper.Index);
            Assert.AreEqual(true, helper.EndOfText);
            Assert.AreEqual(0, helper.Remaining);

            helper.Reset();
            Assert.AreEqual(0, helper.Index);
            Assert.AreEqual(Alphabet, helper.Text);

            helper.Reset(Numbers);
            Assert.AreEqual(Numbers, helper.Text);
            Assert.AreEqual('1', helper.Peek());
            Assert.AreEqual('2', helper.Peek(1));
            Assert.AreEqual(ParsingHelper.NullChar, helper.Peek(1000));
            Assert.AreEqual(0, helper.Index);

            helper.Reset(null);
            Assert.AreEqual(0, helper.Index);
            Assert.AreEqual(string.Empty, helper.Text);
        }

        [TestMethod]
        public void SkipTests()
        {
            ParsingHelper helper = new ParsingHelper(TestString);

            helper.SkipTo("score");
            Assert.AreEqual('s', helper.Peek());

            helper.Reset();
            helper.SkipTo("SCORE", StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual('s', helper.Peek());

            helper.Reset();
            helper.SkipTo('v');
            Assert.AreEqual('v', helper.Peek());
            Assert.AreEqual('e', helper.Peek(1));
            Assert.AreEqual('n', helper.Peek(2));

            helper.SkipToNextLine();
            Assert.AreEqual('a', helper.Peek());
            Assert.AreEqual(' ', helper.Peek(1));

            helper.SkipToEndOfLine();
            Assert.AreEqual('\r', helper.Peek());
            Assert.AreEqual('\n', helper.Peek(1));

            helper.SkipTo("XxXxXxX");
            Assert.AreEqual(helper.Text.Length, helper.Index);
            Assert.AreEqual(true, helper.EndOfText);

            helper.Reset();
            helper.SkipTo(' ');
            helper.SkipWhiteSpace();
            Assert.AreEqual('s', helper.Peek());

            helper.SkipWhile(c => "score".Contains(c));
            Assert.AreEqual(' ', helper.Peek());
            Assert.AreEqual('a', helper.Peek(1));
        }

        [TestMethod]
        public void ParseTests()
        {
            ParsingHelper helper = new ParsingHelper(TestString);

            helper.SkipTo("score");
            string s = helper.ParseTo("fathers");
            Assert.AreEqual('f', helper.Peek());
            Assert.AreEqual("score and seven years ago our ", s);

            helper.Reset();
            helper.SkipTo("score");
            s = helper.ParseTo("FATHERS", StringComparison.OrdinalIgnoreCase);
            Assert.AreEqual('f', helper.Peek());
            Assert.AreEqual("score and seven years ago our ", s);

            helper.Reset();
            helper.SkipTo("score");
            s = helper.ParseTo('v', 'X', 'Y', 'Z');
            Assert.AreEqual('v', helper.Peek());
            Assert.AreEqual("score and se", s);

            helper.Reset();
            helper.SkipTo("score");
            s = helper.ParseWhile(c => c != ',');
            Assert.AreEqual(',', helper.Peek());
            Assert.AreEqual("score and seven years ago our fathers brought forth on this continent", s);

            helper.Next();  // Skip comma
            s = helper.ParseToken(char.IsWhiteSpace);
            Assert.AreEqual(' ', helper.Peek());
            Assert.AreEqual('n', helper.Peek(1));
            Assert.AreEqual("a", s);

            helper.Reset();
            s = helper.ParseToken(' ', '\r', '\n');
            Assert.AreEqual(' ', helper.Peek());
            Assert.AreEqual("Four", s);
        }

        [TestMethod]
        public void QuotedTextTests()
        {
            ParsingHelper helper = new ParsingHelper("He turned and said, \"Yes.\"");
            helper.SkipTo('"');
            string s = helper.ParseQuotedText();
            Assert.AreEqual("Yes.", s);
            Assert.AreEqual(helper.Text.Length, helper.Index);
            Assert.AreEqual(ParsingHelper.NullChar, helper.Peek());

            helper = new ParsingHelper("He turned and said, \"I turned and said, \"\"Yes\"\".\"");
            helper.SkipTo('"');
            s = helper.ParseQuotedText();
            Assert.AreEqual("I turned and said, \"Yes\".", s);
            Assert.AreEqual(helper.Text.Length, helper.Index);
            Assert.AreEqual(ParsingHelper.NullChar, helper.Peek());

            helper = new ParsingHelper("He turned and said, 'I turned and said, ''Yes''.'");
            helper.SkipTo('\'');
            s = helper.ParseQuotedText();
            Assert.AreEqual("I turned and said, 'Yes'.", s);
            Assert.AreEqual(helper.Text.Length, helper.Index);
            Assert.AreEqual(ParsingHelper.NullChar, helper.Peek());
        }

        [TestMethod]
        public void MatchesCurrentPositionTests()
        {
            ParsingHelper helper = new ParsingHelper(TestString);
            helper.SkipTo("consecrated it");
            Assert.AreEqual(true, helper.MatchesCurrentPosition("consecrated it"));
            Assert.AreEqual(true, helper.MatchesCurrentPosition("CONSECRATED IT", StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual(false, helper.MatchesCurrentPosition("consecrated_it"));
            Assert.AreEqual(false, helper.MatchesCurrentPosition("CONSECRATED_IT", StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void ExtractTests()
        {
            ParsingHelper helper = new ParsingHelper(TestString);
            string s = "consecrated it";
            helper.SkipTo(s);
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
        }

        [TestMethod]
        public void OperatorOverloadTests()
        {
            ParsingHelper helper = new ParsingHelper(TestString);

            int i = 0;
            while (!helper.EndOfText)
            {
                Assert.AreEqual(i++, helper.Index);
                helper++;
            }

            helper.Reset();
            helper++;
            Assert.AreEqual(1, helper);

            helper--;
            Assert.AreEqual(0, helper);

            helper += 2;
            Assert.AreEqual(2, helper);

            helper = helper + 2;
            Assert.AreEqual(4, helper);

            helper += 10000;
            Assert.AreEqual(helper.Text.Length, helper);

            helper -= 10000;
            Assert.AreEqual(0, helper);
        }
    }
}

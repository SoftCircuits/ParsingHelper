// Copyright (c) 2019-2020 Jonathan Wood (www.softcircuits.com)
// Licensed under the MIT license.
//

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using SoftCircuits.Parsing.Helper;

namespace ParsingHelperTests
{
    [TestClass]
    public class ParsingHelperTests
    {
        [TestMethod]
        public void BasicTests()
        {
            string testString = "Abcdefghijklmnopqrstuvwxyz";
            ParsingHelper helper = new ParsingHelper(testString);

            Assert.AreEqual('\0', ParsingHelper.NullChar);

            Assert.AreEqual(0, helper.Index);
            Assert.AreEqual(testString, helper.Text);

            Assert.AreEqual('A', helper.Peek());
            Assert.AreEqual('b', helper.Peek(1));
            Assert.AreEqual(ParsingHelper.NullChar, helper.Peek(1000));
            Assert.AreEqual(0, helper.Index);

            helper.Next();
            Assert.AreEqual('b', helper.Peek());
            helper.Next(2);
            Assert.AreEqual('d', helper.Peek());
            helper.Skip('f', 'e', 'd');
            Assert.AreEqual('g', helper.Peek());

            helper.SkipTo("mno");
            Assert.AreEqual('m', helper.Peek());
            Assert.AreEqual(12, helper.Index);
            helper.SkipTo('u', 't', 'v');
            Assert.AreEqual('t', helper.Peek());
            Assert.AreEqual(19, helper.Index);

            helper.SkipTo('X');
            Assert.AreEqual(26, helper.Index);
            Assert.AreEqual(ParsingHelper.NullChar, helper.Peek());

            helper.Next(-1000);
            Assert.AreEqual(0, helper.Index);

            helper.Reset();
            Assert.AreEqual(0, helper.Index);
            Assert.AreEqual(testString, helper.Text);
        }

        [TestMethod]
        public void AdvancedTests()
        {
            string testString = "Once upon a time, in a \r\n" +
                "land far, far away, there was small boy named \"Henry\".";
            ParsingHelper helper = new ParsingHelper(testString);

            helper.SkipTo("time");
            Assert.AreEqual('t', helper.Peek());
            Assert.IsTrue(helper.MatchesCurrentPosition("time"));

            // New line methods
            helper.SkipToEndOfLine();
            Assert.AreEqual('\r', helper.Peek());

            helper.SkipToNextLine();
            Assert.AreEqual('l', helper.Peek());

            helper.Reset();
            helper.SkipToEndOfLine();
            Assert.AreEqual('\r', helper.Peek());
            helper++;   // Within new line

            helper.SkipToNextLine();
            Assert.AreEqual('l', helper.Peek());

            helper.Reset();
            helper.SkipToNextLine();
            Assert.AreEqual('l', helper.Peek());

            helper.SkipTo("land");
            Assert.IsTrue(helper.MatchesCurrentPosition("land"));
            helper += "land".Length;
            helper.SkipWhiteSpace();
            Assert.AreEqual('f', helper.Peek());

            string s = helper.ParseWhile(c => !char.IsWhiteSpace(c) && c != ',');
            Assert.AreEqual("far", s);
            Assert.AreEqual(',', helper.Peek());

            helper.SkipTo("named");
            helper.Next("named".Length);
            helper.SkipWhiteSpace();
            Assert.AreEqual('\"', helper.Peek());
            s = helper.ParseQuotedText();
            Assert.AreEqual("Henry", s);
            Assert.AreEqual('.', helper.Peek());

            helper.Reset();
            helper.SkipTo("UPON", StringComparison.OrdinalIgnoreCase);
        }

        [TestMethod]
        public void CountWordsTest()
        {
            string testString = "Once upon a time, in a \r\n" +
                "land far, far away, there was small boy named \"Henry\".";
            int words = 0;
            char[] wordChars = "abcdefghijklmnopqrstuvwxyz'".ToCharArray();
            char[] delimiters = { ' ', '\r', '\n', '.', ',', '"' };

            ParsingHelper helper = new ParsingHelper(testString);

            while (true)
            {
                helper.SkipWhile(c => !wordChars.Contains(c));
                if (helper.EndOfText)
                    break;
                helper.SkipWhile(c => wordChars.Contains(c));
                words++;
            }
            Assert.AreEqual(16, words);

            words = 0;
            helper.Reset();
            while (!helper.EndOfText)
            {
                string token = helper.ParseToken(delimiters);
                if (token.Length > 0)
                    words++;
            }
            Assert.AreEqual(16, words);

            words = 0;
            helper.Reset();
            while (!helper.EndOfText)
            {
                string token = helper.ParseToken(c => delimiters.Contains(c));
                if (token.Length > 0)
                    words++;
            }
            Assert.AreEqual(16, words);

            helper.Reset();
            Assert.AreEqual("Once", helper.ParseToken(delimiters));
            Assert.AreEqual("upon", helper.ParseToken(delimiters));
            Assert.AreEqual("a", helper.ParseToken(delimiters));
            Assert.AreEqual("time", helper.ParseToken(delimiters));
            Assert.AreEqual("in", helper.ParseToken(delimiters));
            Assert.AreEqual("a", helper.ParseToken(delimiters));
            Assert.AreEqual("land", helper.ParseToken(delimiters));
        }

        [TestMethod]
        public void OperatorOverloadTest()
        {
            string testString = "Once upon a time, in a \r\n" +
                "land far, far away, there was small boy named \"Henry\".";

            ParsingHelper helper = new ParsingHelper(testString);

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

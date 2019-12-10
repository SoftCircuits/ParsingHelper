using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            helper.Skip(2);
            Assert.AreEqual('d', helper.Peek());

            helper.SkipTo("mno");
            Assert.AreEqual('m', helper.Peek());
            Assert.AreEqual(12, helper.Index);
            helper.SkipTo('u', 't', 'v');
            Assert.AreEqual('t', helper.Peek());
            Assert.AreEqual(19, helper.Index);

            helper.SkipTo('X');
            Assert.AreEqual(26, helper.Index);
            Assert.AreEqual(ParsingHelper.NullChar, helper.Peek());

            helper.Skip(-1000);
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
            helper.SkipToEndOfLine();
            Assert.AreEqual('\r', helper.Peek());

            helper.SkipTo("land");
            Assert.IsTrue(helper.MatchesCurrentPosition("land"));
            helper.Skip("land".Length);
            helper.SkipWhiteSpace();
            Assert.AreEqual('f', helper.Peek());

            string s = helper.ParseWhile(c => !char.IsWhiteSpace(c) && c != ',');
            Assert.AreEqual("far", s);
            Assert.AreEqual(',', helper.Peek());

            helper.SkipTo("named");
            helper.Skip("named".Length);
            helper.SkipWhiteSpace();
            Assert.AreEqual('\"', helper.Peek());
            s = helper.ParseQuotedText();
            Assert.AreEqual("Henry", s);
            Assert.AreEqual('.', helper.Peek());

            helper.Reset();
            helper.SkipTo("UPON", StringComparison.OrdinalIgnoreCase);
        }
    }
}

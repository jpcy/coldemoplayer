using System;
using NUnit.Framework;
using CDP.Core.Extensions;

namespace UnitTests.Core.Extensions
{
    [TestFixture]
    public class StringExtensionsTests
    {
        [Test]
        public void RemoveChars_Ok()
        {
            string input = "abcdefghijklm";
            Assert.That(input.RemoveChars('c', 'h', 'm'), Is.EqualTo("abdefgijkl"));
        }

        [Test]
        public void RemoveChars_NoChars()
        {
            string input = "abc";
            Assert.That(input.RemoveChars(), Is.EqualTo(input));
        }

        [Test]
        public void RemoveChars_EmptyInput()
        {
            string input = string.Empty;
            Assert.That(input.RemoveChars('a', 'b', 'c'), Is.EqualTo(input));
        }

        [Test]
        public void RemoveChars_EmptyResult()
        {
            string input = "abc";
            Assert.That(input.RemoveChars('a', 'b', 'c'), Is.EqualTo(string.Empty));
        }
    }
}

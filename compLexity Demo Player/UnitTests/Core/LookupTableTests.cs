using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CDP.Core;

namespace UnitTests.Core
{
    [TestFixture]
    public class LookupTableTests
    {
        public LookupTable_int from = new LookupTable_int
        (
            new LookupElement<int>("a", 0),
            new LookupElement<int>("b", 1),
            new LookupElement<int>("c", 2),
            new LookupElement<int>("d", 3)
        );

        public LookupTable_int to = new LookupTable_int
        (
            new LookupElement<int>("a", 2),
            new LookupElement<int>("b", 0),
            new LookupElement<int>("c", 3),
            new LookupElement<int>("d", 1)
        );

        [Test]
        public void Convert()
        {
            Convert_Key("a");
            Convert_Key("b");
            Convert_Key("c");
            Convert_Key("d");
        }

        private void Convert_Key(string key)
        {
            int fromValue = from[key];
            int toValue = to[key];
            Assert.That(from.Convert(fromValue, to), Is.EqualTo(toValue));
        }

        public LookupTable_int from_Start = new LookupTable_int
        (
            new LookupElement<int>("a", 0),
            new LookupElement<int>("b", 1),
            new LookupElement<int>("c", 2, true),
            new LookupElement<int>("d", 10)
        );

        public LookupTable_int to_Start = new LookupTable_int
        (
            new LookupElement<int>("a", 2),
            new LookupElement<int>("b", 1),
            new LookupElement<int>("c", 3, true),
            new LookupElement<int>("d", 0)
        );

        [Test]
        public void Convert_Start()
        {
            Convert_Key("a");
            Convert_Key("b");

            // Particularly important that this passes, because it checks that the start element detection doesn't overshoot its maximum offset.
            Convert_Key("d");

            int fromValue = from["c"];
            int toValue = to["c"];

            // The maximum value for "c" in from_Start is 9 (because "d" is 10).
            const int maxOffset = 7;

            for (int offset = 0; offset <= maxOffset; offset++)
            {
                Assert.That(from_Start.Convert(fromValue + offset, to_Start), Is.EqualTo(toValue + offset));
            }
        }
    }
}

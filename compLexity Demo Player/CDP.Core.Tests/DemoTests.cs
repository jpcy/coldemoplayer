using System;
using System.Collections.Generic;
using NUnit.Framework;
using CDP.Core;
using System.Collections;

namespace CDP.Core.Tests
{
    [TestFixture]
    public class DemoTests
    {
        private class DummyDemo : Demo
        {
            public override string GameName
            {
                get { throw new NotImplementedException(); }
            }

            public override string MapName { get; protected set; }
            public override string Perspective { get; protected set; }
            public override TimeSpan Duration { get; protected set; }
            public override IList<Demo.Detail> Details { get; protected set; }
            public override ArrayList Players { get; protected set; }
            public override string MapImagesRelativePath
            {
                get { throw new NotImplementedException(); }
            }

            public override bool CanPlay
            {
                get { throw new NotImplementedException(); }
            }

            public override bool CanAnalyse
            {
                get { throw new NotImplementedException(); }
            }

            public override void Load()
            {
                throw new NotImplementedException();
            }

            public override void Read()
            {
                throw new NotImplementedException();
            }

            public override void Write(string destinationFileName)
            {
                throw new NotImplementedException();
            }
        }

        private DummyDemo demo = null;
        private readonly string name = "mydemo";
        private readonly string fileName = "C:\\Demos\\mydemo.dem";

        [SetUp]
        public void SetUp()
        {
            demo = new DummyDemo();
        }

        [Test]
        public void Property_FileName_Set()
        {
            demo.FileName = fileName;
            Assert.That(demo.FileName, Is.EqualTo(fileName));
            Assert.That(demo.Name, Is.EqualTo(name));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Property_FileName_Set_AlreadySet()
        {
            demo.FileName = fileName;
            demo.FileName = fileName;
        }
    }
}

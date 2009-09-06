using System;
using System.Collections.Generic;
using NUnit.Framework;
using CDP.Core;
using System.Collections;
using Moq;

namespace CDP.Core.Tests
{
    [TestFixture]
    public class DemoTests
    {
        private Mock<Demo> demo;
        private readonly string name = "mydemo";
        private readonly string fileName = "C:\\Demos\\mydemo.dem";

        [SetUp]
        public void SetUp()
        {
            demo = new Mock<Demo>();
        }

        [Test]
        public void Property_FileName_Set()
        {
            demo.Object.FileName = fileName;
            Assert.That(demo.Object.FileName, Is.EqualTo(fileName));
            Assert.That(demo.Object.Name, Is.EqualTo(name));
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Property_FileName_Set_AlreadySet()
        {
            demo.Object.FileName = fileName;
            demo.Object.FileName = fileName;
        }
    }
}

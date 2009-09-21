using System;
using System.Collections.Generic;
using NUnit.Framework;
using CDP.Core;
using System.Collections;
using Moq;

namespace UnitTests.Core
{
    [TestFixture]
    public class DemoTests
    {
        public abstract class MockDemo : Demo
        {
            public void PublicUpdateProgress(long streamPosition, long streamLength)
            {
                UpdateProgress(streamPosition, streamLength);
            }
        }

        private Mock<MockDemo> demo;
        private readonly string name = "mydemo";
        private readonly string fileName = "C:\\Demos\\mydemo.dem";

        [SetUp]
        public void SetUp()
        {
            demo = new Mock<MockDemo>();
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

        [Test]
        public void UpdateProgress_Ok()
        {
            const int progress = 50;

            var eventHandler = new EventHandler<Demo.ProgressChangedEventArgs>((sender, args) =>
            {
                Assert.That(args.Progress, Is.EqualTo(progress));
            });

            demo.Object.ProgressChangedEvent += eventHandler;

            try
            {
                demo.Object.PublicUpdateProgress(progress, 100);
            }
            finally
            {
                demo.Object.ProgressChangedEvent -= eventHandler;
            }
        }

        [Test]
        public void UpdateProgress_ProgressNotChanged()
        {
            var eventHandler = new EventHandler<Demo.ProgressChangedEventArgs>((sender, args) =>
            {
                Assert.Fail();
            });

            demo.Object.ProgressChangedEvent += eventHandler;

            try
            {
                demo.Object.PublicUpdateProgress(0, 100);
            }
            finally
            {
                demo.Object.ProgressChangedEvent -= eventHandler;
            }
        }
    }
}

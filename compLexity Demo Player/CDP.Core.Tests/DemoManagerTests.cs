using System;
using System.Linq;
using NUnit.Framework;
using Moq;
using CDP.Core;
using System.IO;
using System.Collections.Generic;

namespace CDP.Core.Tests
{
    [TestFixture]
    public class DemoManagerTests
    {
        // CreateDemo uses Activator to instantiate a type with a base type of Demo. Can't use a Moq mock.
        public class DemoStub : Demo
        {
            public override string GameName
            {
                get { throw new NotImplementedException(); }
            }
            public override string MapName { get; protected set; }
            public override string Perspective { get; protected set; }
            public override TimeSpan Duration { get; protected set; }
            public override IList<Detail> Details { get; protected set; }
            public DemoHandler PublicHandler { get; private set; }

            public override DemoHandler Handler
            {
                set { PublicHandler = value; }
            }

            public override void Load()
            {
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

        private DemoManager demoManager;
        private Mock<Core.DemoHandler> demoHandlerMock;
        private Mock<Adapters.IFile> fileAdapterMock;
        private Mock<Adapters.IPath> pathAdapterMock;

        [SetUp]
        public void SetUp()
        {
            fileAdapterMock = new Mock<Adapters.IFile>();
            fileAdapterMock.Setup(f => f.OpenRead(It.IsAny<string>())).Returns(new MemoryStream());
            pathAdapterMock = new Mock<Adapters.IPath>();
            demoManager = new DemoManager(fileAdapterMock.Object, pathAdapterMock.Object);
            demoHandlerMock = new Mock<DemoHandler>();
        }

        [Test]
        public void AddPlugin_Ok()
        {
            Mock<Core.DemoHandler> demoHandlerMock = new Mock<DemoHandler>();
            demoManager.AddPlugin(0, typeof(DemoStub), demoHandlerMock.Object);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AddPlugin_WrongDemoTypeBaseClass()
        {
            Mock<Core.DemoHandler> demoHandlerMock = new Mock<DemoHandler>();
            demoManager.AddPlugin(0, typeof(object), demoHandlerMock.Object);
        }

        [Test]
        public void ValidDemoExtensions_Ok()
        {
            SetUpPluginStub(string.Empty, new string[] { "dem", "replay" }, true);
            string[] extensions = demoManager.ValidDemoExtensions();
            Assert.That(extensions.Length, Is.EqualTo(2));
            Assert.That(extensions[0], Is.EqualTo("dem"));
            Assert.That(extensions[1], Is.EqualTo("replay"));
        }

        [Test]
        public void ValidDemoExtensions_NoPlugins()
        {
            Assert.That(demoManager.ValidDemoExtensions().Length, Is.EqualTo(0));
        }

        [Test]
        public void CreateDemo_NoMatchingExtension()
        {
            SetUpPluginStub("replay", new string[] { "dem" }, true);
            Assert.That(demoManager.CreateDemo(string.Empty), Is.Null);
        }

        [Test]
        public void CreateDemo_IsNotValidDemo()
        {
            SetUpPluginStub("dem", new string[] { "dem" }, false);
            Assert.That(demoManager.CreateDemo(string.Empty), Is.Null);
        }

        [Test]
        public void CreateDemo_Ok()
        {
            SetUpPluginStub("dem", new string[] { "dem" }, true);
            DemoStub demo = (DemoStub)demoManager.CreateDemo(string.Empty);
            Assert.That(demo, Is.Not.Null);
            Assert.That(demo.PublicHandler, Is.EqualTo(demoHandlerMock.Object));
        }

        private void SetUpPluginStub(string demoExtension, string[] extensions, bool isValidDemo)
        {
            fileAdapterMock.Setup(f => f.OpenRead(It.IsAny<string>())).Returns(new MemoryStream());
            pathAdapterMock.Setup(p => p.GetExtension(It.IsAny<string>())).Returns(demoExtension);
            demoHandlerMock.Setup(dh => dh.Extensions).Returns(extensions);
            demoHandlerMock.Setup(dh => dh.IsValidDemo(It.IsAny<Stream>())).Returns(isValidDemo);
            demoManager.AddPlugin(0, typeof(DemoStub), demoHandlerMock.Object);
        }
    }
}

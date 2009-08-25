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
        public abstract class DemoDummy : Demo
        {
        }

        public abstract class DemoHandlerDummy : DemoHandler
        {
        }

        // LoadDemo uses Activator to instantiate a type with a base type of Demo. Can't use a Moq mock.
        public class DemoStub : Demo
        {
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


        // LoadPlugins uses Activator to instantiate a type with a base type of DemoHandler. Can't use a Moq mock.
        public class DemoHandlerStub : DemoHandler
        {
            public override string FullName
            {
                get { throw new NotImplementedException(); }
            }

            public override string Name
            {
                get { throw new NotImplementedException(); }
            }

            public override string[] Extensions
            {
                get { return new string[] { "dem" }; }
            }

            public override bool IsValidDemo(Stream stream)
            {
                return DemoManagerTests.IsValidDemo;
            }
        }

        private DemoManager demoManager;
        private Mock<Adapters.IAssembly> assemblyAdapterMock;
        private Mock<Adapters.IFile> fileAdapterMock;
        private Mock<Adapters.IFolder> folderAdapterMock;

        public static bool IsValidDemo { get; private set; }

        [SetUp]
        public void SetUp()
        {
            assemblyAdapterMock = new Mock<Adapters.IAssembly>();
            fileAdapterMock = new Mock<Adapters.IFile>();
            fileAdapterMock.Setup(f => f.OpenRead(It.IsAny<string>())).Returns(new MemoryStream());
            folderAdapterMock = new Mock<Adapters.IFolder>();
            folderAdapterMock.Setup(f => f.GetFiles(It.IsAny<string>(), It.IsAny<string>())).Returns(new string[] { "dummy.dll" });
            demoManager = new DemoManager(assemblyAdapterMock.Object, fileAdapterMock.Object, folderAdapterMock.Object);
            IsValidDemo = true;
        }

        private void LoadDummyPlugin()
        {
            LoadDummyPlugin(new Type[] { typeof(DemoDummy), typeof(DemoHandlerStub) });
        }

        private void LoadDummyPlugin(Type[] types)
        {
            assemblyAdapterMock.Setup(a => a.GetTypes(It.IsAny<string>())).Returns(types);
            demoManager.LoadPlugins(string.Empty);
        }

        [Test]
        [ExpectedException(typeof(ApplicationException), ExpectedMessage = "Assembly \"dummy.dll\" doesn't contain a class that inherits from Demo.")]
        public void LoadPlugins_NoDemoType()
        {
            LoadDummyPlugin(new Type[] { typeof(DemoHandlerDummy) });
        }

        [Test]
        [ExpectedException(typeof(ApplicationException), ExpectedMessage = "Assembly \"dummy.dll\" doesn't contain a class that inherits from DemoHandler.")]
        public void LoadPlugins_NoDemoHandlerType()
        {
            LoadDummyPlugin(new Type[] { typeof(DemoDummy) });
        }

        [Test]
        public void LoadPlugins_Ok()
        {
            LoadDummyPlugin();
        }

        [Test]
        public void ValidDemoExtensions_Ok()
        {
            LoadDummyPlugin();
            string[] extensions = demoManager.ValidDemoExtensions();
            Assert.That(extensions.Length, Is.EqualTo(1));
            Assert.That(extensions[0], Is.EqualTo("dem"));
        }

        [Test]
        public void CreateDemo_NoMatchingExtension()
        {
            LoadDummyPlugin();
            fileAdapterMock.Setup(f => f.OpenRead(It.IsAny<string>())).Returns(new MemoryStream());
            Assert.That(demoManager.CreateDemo("foo.replay"), Is.Null);
        }

        [Test]
        public void CreateDemo_IsNotValidDemo()
        {
            LoadDummyPlugin();
            IsValidDemo = false;
            Assert.That(demoManager.CreateDemo("foo.dem"), Is.Null);
        }

        [Test]
        public void CreateDemo_Ok()
        {
            LoadDummyPlugin(new Type[] { typeof(DemoStub), typeof(DemoHandlerStub) });
            IsValidDemo = true;
            DemoStub demo = (DemoStub)demoManager.CreateDemo("foo.dem");
            Assert.That(demo, Is.Not.Null);
            Assert.That(demo.PublicHandler, Is.InstanceOf(typeof(DemoHandlerStub)));
        }
    }
}

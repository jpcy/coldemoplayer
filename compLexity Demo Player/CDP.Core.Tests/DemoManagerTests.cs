using System;
using System.Linq;
using NUnit.Framework;
using Moq;
using CDP.Core;
using System.IO;
using System.Collections.Generic;
using System.Collections;

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
            public override ArrayList Players { get; protected set; }
            public override string[] IconFileNames
            {
                get { throw new NotImplementedException(); }
            }
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

        // CreateLauncher uses Activator to instantiate a type with a base type of Launcher. Can't use a Moq mock.
        public class LauncherStub : Core.Launcher
        {
            public LauncherStub(Core.Demo demo)
            {
            }

            public override bool Verify()
            {
                throw new NotImplementedException();
            }

            public override void Launch()
            {
                throw new NotImplementedException();
            }
        }

        public class LauncherStubNoCtor : Core.Launcher
        {
            public override bool Verify()
            {
                throw new NotImplementedException();
            }

            public override void Launch()
            {
                throw new NotImplementedException();
            }
        }


        public class DemoDummy : DemoStub { }

        private DemoManager demoManager;
        private Mock<Core.DemoHandler> demoHandlerMock;
        private Mock<IFileSystem> fileSystemMock;

        [SetUp]
        public void SetUp()
        {
            fileSystemMock = new Mock<IFileSystem>();
            fileSystemMock.Setup(f => f.OpenRead(It.IsAny<string>())).Returns(new MemoryStream());
            demoManager = new DemoManager(fileSystemMock.Object);
            demoHandlerMock = new Mock<DemoHandler>();
        }

        [Test]
        public void AddPlugin_Ok()
        {
            Mock<Core.DemoHandler> demoHandlerMock = new Mock<DemoHandler>();
            demoManager.AddPlugin(0, typeof(DemoStub), demoHandlerMock.Object, typeof(LauncherStub));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AddPlugin_WrongDemoTypeBaseClass()
        {
            Mock<Core.DemoHandler> demoHandlerMock = new Mock<DemoHandler>();
            demoManager.AddPlugin(0, typeof(object), demoHandlerMock.Object, typeof(LauncherStub));
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void AddPlugin_WrongLauncherTypeBaseClass()
        {
            Mock<Core.DemoHandler> demoHandlerMock = new Mock<DemoHandler>();
            demoManager.AddPlugin(0, typeof(DemoStub), demoHandlerMock.Object, typeof(object));
        }

        [Test]
        public void GetAllDemoHandlerSettings_Ok()
        {
            Func<Setting[], Mock<Core.DemoHandler>> createDemoHandlerMock = s =>
            {
                var mock = new Mock<DemoHandler>();
                mock.Setup(dh => dh.Settings).Returns(s);
                return mock;
            };

            var setting1 = new Setting("setting1", typeof(bool), false);
            var setting2 = new Setting("setting2", typeof(bool), false);
            var mock1 = createDemoHandlerMock(new Setting[] { setting1 });
            var mock2 = createDemoHandlerMock(new Setting[] { setting2 });
            demoManager.AddPlugin(0, typeof(DemoStub), mock1.Object, typeof(LauncherStub));
            demoManager.AddPlugin(0, typeof(DemoStub), mock2.Object, typeof(LauncherStub));
            var result = demoManager.GetAllDemoHandlerSettings();
            Assert.That(result.Length, Is.EqualTo(2));
            Assert.That(result[0], Is.EqualTo(setting1));
            Assert.That(result[1], Is.EqualTo(setting2));
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
            Assert.That(demo.Handler, Is.EqualTo(demoHandlerMock.Object));
        }

        [Test]
        [ExpectedException(typeof(ApplicationException))]
        public void CreateLauncher_UnknownDemoType()
        {
            var demoHandlerMock = new Mock<DemoHandler>();
            demoManager.AddPlugin(0, typeof(DemoStub), demoHandlerMock.Object, typeof(LauncherStub));
            demoManager.CreateLauncher(new DemoDummy());
        }

        [Test]
        [ExpectedException(typeof(ApplicationException))]
        public void CreateLauncher_NoLauncherCtorWithDemoParameter()
        {
            var demoHandlerMock = new Mock<DemoHandler>();
            demoManager.AddPlugin(0, typeof(DemoStub), demoHandlerMock.Object, typeof(LauncherStubNoCtor));
            demoManager.CreateLauncher(new DemoStub());
        }

        [Test]
        public void CreateLauncher_Ok()
        {
            var demoHandlerMock = new Mock<DemoHandler>();
            demoManager.AddPlugin(0, typeof(DemoStub), demoHandlerMock.Object, typeof(LauncherStub));
            Launcher launcher = demoManager.CreateLauncher(new DemoStub());
            Assert.That(launcher, Is.Not.Null);
        }

        private void SetUpPluginStub(string demoExtension, string[] extensions, bool isValidDemo)
        {
            fileSystemMock.Setup(f => f.OpenRead(It.IsAny<string>())).Returns(new MemoryStream());
            fileSystemMock.Setup(p => p.GetExtension(It.IsAny<string>())).Returns(demoExtension);
            demoHandlerMock.Setup(dh => dh.Extensions).Returns(extensions);
            demoHandlerMock.Setup(dh => dh.IsValidDemo(It.IsAny<Stream>())).Returns(isValidDemo);
            demoManager.AddPlugin(0, typeof(DemoStub), demoHandlerMock.Object, typeof(LauncherStub));
        }
    }
}

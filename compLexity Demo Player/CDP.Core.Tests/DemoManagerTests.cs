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
        // Moq bitches out on CreateDemo_Ok test - it doesn't like Handler being set as virtual.
        public class DummyDemo : Demo
        {
            public override string GameName
            {
                get { throw new NotImplementedException(); }
            }

            public override string MapName
            {
                get
                {
                    throw new NotImplementedException();
                }
                protected set
                {
                    throw new NotImplementedException();
                }
            }

            public override string Perspective
            {
                get
                {
                    throw new NotImplementedException();
                }
                protected set
                {
                    throw new NotImplementedException();
                }
            }

            public override TimeSpan Duration
            {
                get
                {
                    throw new NotImplementedException();
                }
                protected set
                {
                    throw new NotImplementedException();
                }
            }

            public override IList<Demo.Detail> Details
            {
                get
                {
                    throw new NotImplementedException();
                }
                protected set
                {
                    throw new NotImplementedException();
                }
            }

            public override ArrayList Players
            {
                get
                {
                    throw new NotImplementedException();
                }
                protected set
                {
                    throw new NotImplementedException();
                }
            }

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

        private DemoManager demoManager;
        private Mock<Core.DemoHandler> demoHandlerMock;
        private MockProvider<IFileSystem> fileSystem;

        [SetUp]
        public void SetUp()
        {
            fileSystem = new MockProvider<IFileSystem>();
            fileSystem.Mock.Setup(f => f.OpenRead(It.IsAny<string>())).Returns(new MemoryStream());
            ObjectCreator.Reset();
            ObjectCreator.MapToProvider<IFileSystem>(fileSystem);
            demoManager = new DemoManager();
            demoHandlerMock = new Mock<DemoHandler>();
        }

        [Test]
        public void AddPlugin_Ok()
        {
            demoManager.AddPlugin(0, demoHandlerMock.Object);
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
            demoManager.AddPlugin(0, mock1.Object);
            demoManager.AddPlugin(0, mock2.Object);
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
            // Setup.
            string fileName = "C:\\foo.dem";
            SetUpPluginStub("dem", new string[] { "dem" }, true);
            var dummyDemo = new DummyDemo();
            demoHandlerMock.Setup(dh => dh.CreateDemo()).Returns(dummyDemo);

            // Run.
            Demo demo = demoManager.CreateDemo(fileName);

            // Verify.
            Assert.That(demo, Is.EqualTo(dummyDemo));
            Assert.That(demo.FileName, Is.EqualTo(fileName));
            Assert.That(demo.Handler, Is.EqualTo(demoHandlerMock.Object));
        }

        [Test]
        public void CreateLauncher_Ok()
        {
            // Setup.
            Core.ObjectCreator.MapToProvider<IProcessFinder>(new MockProvider<IProcessFinder>());
            var demoMock = new Mock<Demo>();
            demoMock.Setup(d => d.Handler).Returns(demoHandlerMock.Object);
            var launcherMock = new Mock<Launcher>();
            launcherMock.Setup(l => l.Initialise(demoMock.Object)).Verifiable();
            demoHandlerMock.Setup(dh => dh.CreateLauncher()).Returns(launcherMock.Object);

            // Run.
            demoManager.AddPlugin(0, demoHandlerMock.Object);
            var launcher = demoManager.CreateLauncher(demoMock.Object);
            
            // Verify.
            Assert.That(launcher, Is.EqualTo(launcherMock.Object));
            launcherMock.Verify();
        }

        private void SetUpPluginStub(string demoExtension, string[] extensions, bool isValidDemo)
        {
            fileSystem.Mock.Setup(f => f.OpenRead(It.IsAny<string>())).Returns(new MemoryStream());
            fileSystem.Mock.Setup(p => p.GetExtension(It.IsAny<string>())).Returns(demoExtension);
            demoHandlerMock.Setup(dh => dh.Extensions).Returns(extensions);
            demoHandlerMock.Setup(dh => dh.IsValidDemo(It.IsAny<Stream>())).Returns(isValidDemo);
            demoManager.AddPlugin(0, demoHandlerMock.Object);
        }
    }
}

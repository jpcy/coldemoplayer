using System;
using NUnit.Framework;
using Moq;
using CDP.Core;

namespace CDP.Core.Tests
{
    [TestFixture]
    public class LauncherTests
    {
        public class LauncherMock : Launcher
        {
            public string ProcessExecutableFileName
            {
                get { return processExecutableFileName; }
                set { processExecutableFileName = value; }
            }

            public override string CalculateDestinationFileName()
            {
                throw new NotImplementedException();
            }

            public override void Initialise(Demo demo)
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

        private MockProvider<IProcessFinder> processFinder;
        private LauncherMock launcher;

        [SetUp]
        public void SetUp()
        {
            ObjectCreator.Reset();
            processFinder = new MockProvider<IProcessFinder>();
            ObjectCreator.MapToProvider<IProcessFinder>(processFinder);
            launcher = new LauncherMock();
        }

        [Test]
        [ExpectedException(typeof(InvalidOperationException))]
        public void MonitorProcessWorker_ExeFileNameNull()
        {
            launcher.MonitorProcessWorker(0);
        }

        [Test]
        public void MonitorProcessWorker_Ok()
        {
            // Setup.
            bool processFound = false;
            bool processExited = false;
            string exeFileName = "c:\\test.exe";
            launcher.ProcessExecutableFileName = exeFileName;
            var processMock = new Mock<IProcess>();
            processMock.Setup(p => p.FileName).Returns(exeFileName);
            processMock.Setup(p => p.HasExited).Returns(true);
            processFinder.Mock.Setup(pf => pf.FindByName(It.IsAny<string>())).Returns(new IProcess[] { processMock.Object });

            launcher.ProcessFound += new EventHandler<Launcher.ProcessFoundEventArgs>(new Action<object, Launcher.ProcessFoundEventArgs>((sender, eventargs) =>
            {
                processFound = true;
                Assert.That(sender, Is.EqualTo(launcher));
                Assert.That(eventargs.Process, Is.EqualTo(processMock.Object));
            }));

            launcher.ProcessClosed += new EventHandler(new Action<object, EventArgs>((sender, eventargs) =>
            {
                processExited = true;
                Assert.That(sender, Is.EqualTo(launcher));
            }));

            // Run.
            launcher.MonitorProcessWorker(0);

            // Verify.
            Assert.That(processFound, Is.True);
            Assert.That(processExited, Is.True);
        }
    }
}

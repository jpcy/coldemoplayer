using System;
using NUnit.Framework;
using Moq;
using CDP.ViewModel;

namespace CDP.Tests.ViewModel
{
    [TestFixture]
    public class MainTests
    {
        private Main main;
        private Mock<Core.IDemoManager> demoManager;
        private Mock<Core.ViewModelBase> header;
        private Mock<Core.ViewModelBase> address;
        private Mock<Core.ViewModelBase> demos;
        private Mock<Core.ViewModelBase> demo;

        [SetUp]
        public void SetUp()
        {
            Core.ObjectCreator.Reset();
            Core.ObjectCreator.MapToProvider<Core.ISettings>(new MockProvider<Core.ISettings>());
            demoManager = new Mock<Core.IDemoManager>();
            header = new Mock<Core.ViewModelBase>();
            address = new Mock<Core.ViewModelBase>();
            demos = new Mock<Core.ViewModelBase>();
            demo = new Mock<Core.ViewModelBase>();
            main = new Main(demoManager.Object,
                            header.Object,
                            address.Object,
                            demos.Object,
                            demo.Object);
        }

        [Test]
        public void Ctor_Ok()
        {
            Assert.That(main.Header, Is.EqualTo(header.Object));
            Assert.That(main.Address, Is.EqualTo(address.Object));
            Assert.That(main.Demos, Is.EqualTo(demos.Object));
            Assert.That(main.Demo, Is.EqualTo(demo.Object));
        }
    }
}

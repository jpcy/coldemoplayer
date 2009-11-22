using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using CDP.IdTech3;

namespace CDP.IdTech3.Tests
{
    [TestFixture]
    public class EntityTests
    {
        [Test]
        public void CopyConstructor()
        {
            const uint eType = 42;
            const int protocol73eTypeIndex = 12;
            Entity protocol43Entity = new Entity(Protocols.Protocol43);
            protocol43Entity["eType"] = eType;
            Entity protocol73Entity = new Entity(Protocols.Protocol73, protocol43Entity);
            Assert.That((uint)protocol73Entity["eType"], Is.EqualTo(eType));

            // eType has in index of 0 in protocol 43.
            Assert.That((uint)protocol73Entity[protocol73eTypeIndex], Is.EqualTo(eType));
        }
    }
}

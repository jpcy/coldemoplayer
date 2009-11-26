using System;
using Moq;
using CDP.Core;

namespace CDP.Core.Tests
{
    internal class MockProvider<T> : IObjectProvider<T> where T:class
    {
        public Mock<T> Mock { get; private set; }

        public MockProvider()
        {
            Mock = new Mock<T>();
        }

        public T Get(params object[] args)
        {
            return Mock.Object;
        }
    }
}

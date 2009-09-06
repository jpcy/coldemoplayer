using System;
using Ninject.Core;
using Ninject.Core.Activation;
using Ninject.Core.Creation;
using Moq;

namespace CDP.Core.Tests
{
    class MockProvider<T> : SimpleProvider<T> where T:class
    {
        public static Mock<T> Mock { get; set; }

        protected override T CreateInstance(IContext context)
        {
            if (Mock == null)
            {
                Mock = new Mock<T>();
            }

            return Mock.Object;
        }
    }
}

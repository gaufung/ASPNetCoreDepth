using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ServiceCollectionTest
{
    [TestClass] 
    public class ServiceCollectionTest
    {
        [TestMethod]
        public void TestTransient()
        {
            var provider = new ServiceCollection()
                .AddTransient(typeof(IFoo), typeof(Foo))
                .BuildServiceProvider();
            IFoo foo = provider.GetService<IFoo>();
            (foo is Foo).Should().BeTrue();
        }

        [TestMethod]
        public void TestSingleton()
        {
            var foo = new Foo();
            var provider = new ServiceCollection()
                .AddSingleton(typeof(IFoo), foo)
                .BuildServiceProvider();
            var foo2 = provider.GetService<IFoo>();
            (foo.Equals(foo2)).Should().BeTrue();
            var foo3 = provider.GetService<IFoo>();
            (foo.Equals(foo3)).Should().BeTrue();
        }
        
        [TestMethod]
        public void TestScope()
        {
            var root = new ServiceCollection()
                .AddScoped(typeof(IFoo), typeof(Foo))
                .BuildServiceProvider();
            var foo1 = root.GetService<IFoo>();
            using var child = root.CreateScope();
            var foo2 = child.ServiceProvider.GetService<IFoo>();
            (foo1.Equals(foo2)).Should().BeFalse();
        }
    }

    internal interface IFoo
    {
        
    }

    internal class Foo : IFoo
    {
        
    }
}
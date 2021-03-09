using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CatContainer;
using FluentAssertions;

namespace CatTest
{
    [TestClass]
    public class CatTest
    {
        [TestMethod]
        public void TestRegisterFoo()
        {
            Cat cat = new Cat();
            cat.Register<IFoo, Foo>(Lifetime.Transient);
            var foo = cat.GetService(typeof(IFoo));
            (foo is Foo).Should().BeTrue();
        }

        [TestMethod]
        public void TestRegisterFoo2()
        {
            Cat cat = new Cat();

            cat.Register<IFoo, Foo>(Lifetime.Transient);
            cat.Register<IFoo, Foo2>(Lifetime.Transient);
            var foos = cat.GetServices<IFoo>();
            foos.Count().Should().Be(2);
            (foos.First() is Foo2).Should().BeTrue();
            (foos.Last() is Foo).Should().BeTrue();
        }

        [TestMethod]
        public void TestRegisterBar()
        {
            Cat cat = new Cat();
            Bar bar = new Bar();
            cat.Register<Bar>(bar);
            var b = cat.GetService<Bar>();
            b.Equals(bar).Should().BeTrue();
        }

        [TestMethod]
        public void TestSelfContainer()
        {
            Cat root = new Cat();
            root.Register<IFoo, Foo>(Lifetime.Self);
            Cat child = root.CreateChild();
            child.Register<IFoo, Foo>(Lifetime.Self);

            var foo1 = root.GetService<IFoo>();
            var foo2 = child.GetService<IFoo>();

            (foo1.Equals(foo2)).Should().BeFalse();

            var foo3 = child.GetService<IFoo>();
            (foo2.Equals(foo3)).Should().BeTrue();
        }

        [TestMethod]
        public void TestRootContainer()
        {
            Cat root = new Cat();
            root.Register<IFoo, Foo>(Lifetime.Root);
            Cat child = root.CreateChild();

            var foo1 = root.GetService<IFoo>();
            var foo2 = child.GetService<IFoo>();
            (foo1.Equals(foo2)).Should().BeTrue();
        }

        [TestMethod]
        public void TestBazContainer()
        {
            Cat cat = new Cat();
            cat.Register<IFoo, Foo>(Lifetime.Self);
            cat.Register<Baz, Baz>(Lifetime.Self);
            var baz = cat.GetService<Baz>();
            (baz.Foo.Equals(cat.GetService<IFoo>())).Should().BeTrue();
        }

        [TestMethod]
        public void TestFooBarContainer()
        {
            Cat cat = new Cat();
            cat.Register<IFoo, Foo>(Lifetime.Root);
            cat.Register<IBar, Bar>(Lifetime.Root);
            cat.Register(typeof(IFooBar<,>), typeof(FooBar<,>), Lifetime.Root);
            var foobar = cat.GetService<IFooBar<IFoo, IBar>>();
            
        }
    }


    public interface IFoo
    {
        
    }

    public class Foo : IFoo
    {
        
    }

    public class Foo2 : IFoo
    {
        
    }

    public interface IBar
    {
    }

    public class Bar : IBar
    {
        
    }

    public class Baz
    {
        public IFoo Foo { get; set; }

        public Baz(IFoo foo)
        {
            this.Foo = foo;
        }
    }
    
    public interface IFooBar<T1, T2> {}

    public class FooBar<T1, T2> : IFooBar<T1, T2>
    {
        
    }

}
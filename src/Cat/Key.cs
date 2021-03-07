using System;
using System.Linq;

namespace Cat
{
    internal class Key : IEquatable<Key>
    {
        public ServiceRegistry Registry { get;  }

        public Type[] GenericArguments { get; }

        public Key(ServiceRegistry registry, Type[] genericArguments)
        {
            Registry = registry;
            GenericArguments = genericArguments;
        }

        public bool Equals(Key other)
        {
            if (Registry != other.Registry)
            {
                return false;
            }

            if (GenericArguments.Length != other.GenericArguments.Length)
            {
                return false;
            }

            return !GenericArguments.Where((t, i) => t != other.GenericArguments[i]).Any();
        }

        public override int GetHashCode()
        {
            var hashcode = Registry.GetHashCode();
            for (int i = 0; i < this.GenericArguments.Length; i++)
            {
                hashcode ^= GenericArguments[i].GetHashCode();
            }

            return hashcode;
        }

        public override bool Equals(object obj) => obj is Key key && Equals(key);
        
    }
}
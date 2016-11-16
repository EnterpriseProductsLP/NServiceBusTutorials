using System;

namespace Domain
{
    public class Product
    {
        public Guid Id { get; }

        public string Name { get; }

        public Product(Guid id, string name)
        {
            Id = id;
            Name = name;
        }
    }
}
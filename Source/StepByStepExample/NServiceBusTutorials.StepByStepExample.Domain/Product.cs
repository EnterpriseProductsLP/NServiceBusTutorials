using System;

namespace NServiceBusTutorials.StepByStepExample.Domain
{
    public class Product
    {
        public Product(Guid id, string name)
        {
            Id = id;
            Name = name;
        }

        public Guid Id { get; set; }

        public string Name { get; set; }
    }
}
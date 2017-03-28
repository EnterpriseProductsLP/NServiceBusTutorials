using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using Autofac.Core.Lifetime;
using Autofac.Core.Resolving;
using NServiceBusTutorials.ActivePassive.Consumer.Interfaces;

namespace NServiceBusTutorials.ActivePassive.Consumer.DependencyInjection
{
    public class ContainerProvider : IContainer
    {
        private static readonly Lazy<ContainerProvider> Lazy = new Lazy<ContainerProvider>(() => new ContainerProvider());

        private readonly IContainer _container;

        private ContainerProvider()
        {
            var containerBuilder = new ContainerBuilder();
            new DistributedLockRegistrar().Register(containerBuilder);
            _container = containerBuilder.Build();
        }

        public static ContainerProvider Container => Lazy.Value;

        public IComponentRegistry ComponentRegistry => _container.ComponentRegistry;

        public IDisposer Disposer => _container.Disposer;

        public object Tag => _container.Tag;

        public ILifetimeScope BeginLifetimeScope()
        {
            return _container.BeginLifetimeScope();
        }

        public ILifetimeScope BeginLifetimeScope(object tag)
        {
            return _container.BeginLifetimeScope(tag);
        }

        public ILifetimeScope BeginLifetimeScope(Action<ContainerBuilder> configurationAction)
        {
            return _container.BeginLifetimeScope(configurationAction);
        }

        public ILifetimeScope BeginLifetimeScope(object tag, Action<ContainerBuilder> configurationAction)
        {
            return _container.BeginLifetimeScope(tag, configurationAction);
        }

        public event EventHandler<LifetimeScopeBeginningEventArgs> ChildLifetimeScopeBeginning
        {
            add
            {
                _container.ChildLifetimeScopeBeginning += value;
            }

            remove
            {
                _container.ChildLifetimeScopeBeginning -= value;
            }
        }

        public event EventHandler<LifetimeScopeEndingEventArgs> CurrentScopeEnding
        {
            add
            {
                _container.CurrentScopeEnding += value;
            }

            remove
            {
                _container.CurrentScopeEnding -= value;
            }
        }

        public void Dispose()
        {
            _container.Dispose();
        }

        public object ResolveComponent(IComponentRegistration registration, IEnumerable<Parameter> parameters)
        {
            return _container.ResolveComponent(registration, parameters);
        }

        public event EventHandler<ResolveOperationBeginningEventArgs> ResolveOperationBeginning
        {
            add
            {
                _container.ResolveOperationBeginning += value;
            }

            remove
            {
                _container.ResolveOperationBeginning -= value;
            }
        }
    }
}

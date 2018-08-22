using System;
using Autofac;
using JetBrains.Annotations;
using Lykke.Cqrs;

namespace Lykke.Service.Session.Modules
{
    [UsedImplicitly]
    internal class AutofacDependencyResolver : IDependencyResolver
    {
        private readonly ILifetimeScope _context;

        public AutofacDependencyResolver(ILifetimeScope kernel)
        {
            _context = kernel ?? throw new ArgumentNullException(nameof(kernel));
        }

        public object GetService(Type type)
        {
            return _context.Resolve(type);
        }

        public bool HasService(Type type)
        {
            return _context.IsRegistered(type);
        }
    }
}
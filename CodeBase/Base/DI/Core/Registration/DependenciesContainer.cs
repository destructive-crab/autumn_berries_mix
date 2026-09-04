using System;
using System.Collections.Generic;
using destructive_code.Debug;
using destructive_code.ServiceLocators;

namespace destructive_code.DI
{
    public sealed class DependenciesContainer : IDisposable, IServiceLocator
    {
        private readonly Dictionary<Type, Dependency> dependencies = new ();
        public int Count => dependencies.Count;

        public void Dispose()
        {
            foreach (KeyValuePair<Type, Dependency> dependencyPair in dependencies)
            {
                dependencyPair.Value.Dispose();
            }
        }

        public bool Contains<TDependency>() where TDependency : class
        {
            return Contains(typeof(TDependency));
        }

        public bool Contains(Type dependencyType)
        {
            return dependencies.ContainsKey(dependencyType);
        }

        public object GetBlind(Type extensionType)
        {
            if (!Contains(extensionType))
                return null;
            
            return GetDependency(extensionType).GetInstance();
        }

        public Dependency GetDependency(Type type)
        {
            if (dependencies.TryGetValue(type, out Dependency dependency))
            {
                return dependency;
            }

            return null;
        }

        public void Add(Type dependencyType, Dependency dependency)
        {
            if (!dependencies.TryAdd(dependencyType, dependency))
            {
                LGR.PERR($"{dependencyType} DEPENDENCY ALREADY REGISTERED");
            }
        }

        public void Remove<TDependency>()
        {
            if (dependencies.ContainsKey(typeof(TDependency)))
                dependencies.Remove(typeof(TDependency));
        }

        public void SetupSingletons()
        {
            foreach (KeyValuePair<Type, Dependency> dependencyPair in dependencies)
            {
                if (dependencyPair.Value.IsSingleton)
                {
                    GameRoot.I.DIKernel.InjectWithBase(dependencyPair.Value.GetInstance());
                }
            }
        }
    }
}
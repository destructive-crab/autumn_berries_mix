using System;
using System.Collections.Generic;
using System.Reflection;
using destructive_code.ServiceLocators;
using destructive_code.Debug;

namespace destructive_code.DI
{
    public sealed class DIKernel 
    {
        private const BindingFlags BindingFlags =
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic;

        private readonly DependenciesContainer coreContainer = new();
        private          DependenciesContainer localContainer;

        public void ClearLocalDependenciesContainer()
        {
            if(localContainer == null) return;
            
            localContainer?.Dispose();
            localContainer = null;
        }

        #region Registration
        public void RegisterDependenciesToCore(params IDependenciesProvider[] providers)
        {
            LGR.BegB($"Registering dependencies to core from {providers.Length} providers");
            Register(providers, coreContainer);
            LGR.EnB();
        }
        
        public void RegisterLocalDependencies(params IDependenciesProvider[] providers)
        {
            LGR.BegB($"Registering dependencies to local from {providers.Length} providers");
            localContainer = new DependenciesContainer();
            Register(providers, localContainer);
            LGR.EnB();
        }

        private void Register(IDependenciesProvider[] providers, DependenciesContainer container)
        {
            foreach (IDependenciesProvider provider in providers)
            {
                MethodInfo[] methods = provider.GetType().GetMethods(BindingFlags);
            
                foreach (MethodInfo method in methods)
                {
                    if (!Attribute.IsDefined(method, typeof(ProvideAttribute)))
                    {
                        continue;
                    }

                    RegisterDependency(method, provider);
                }
            }

            void RegisterDependency(MethodInfo method, IDependenciesProvider provider)
            {
                Type dependencyType = method.ReturnType;

                bool isSingleton = ((ProvideAttribute) Attribute.GetCustomAttribute(method, typeof(ProvideAttribute))).IsSingleton;

                Func<object> fabric = (Func<object>) method.CreateDelegate(typeof(Func<object>), provider);

                Dependency dependencyRegistration = new Dependency(dependencyType, fabric, isSingleton);

                container.Add(dependencyType, dependencyRegistration);
                LGR.PM($"Registered dependency with type {dependencyType}; IsSingleton: {isSingleton}");
            }
            
            container.SetupSingletons();
        }

        #endregion

        #region Inject

        public void InjectWithBaseAnd(object toInject, params IServiceLocator[] serviceLocators)
        {
            List<IServiceLocator> locators = GetBaseLocators();

            locators.AddRange(serviceLocators);
 
            InjectWith(toInject, locators.ToArray());
        }

        public void InjectWithBase(object toInject)
        {
            List<IServiceLocator> locators = GetBaseLocators();

            InjectWith(toInject, locators.ToArray());
        }

        private List<IServiceLocator> GetBaseLocators()
        {
            List<IServiceLocator> locators = new List<IServiceLocator>();
            
            if(coreContainer.Count > 0) locators.Add(coreContainer);
            if(localContainer != null)  locators.Add(localContainer);
            
            return locators;
        }

        public void InjectWith(object toInject, params IServiceLocator[] serviceLocators)
        {
            Type type = toInject.GetType();

            InjectRegion region = InjectRegion.All;
            
            InjectRegionAttribute customAttribute = (InjectRegionAttribute)toInject.GetType().GetCustomAttribute(typeof(InjectRegionAttribute));
            
            if (customAttribute != null)
            {
                region = customAttribute.Region;
            }
            
            //methods
            if((region & InjectRegion.Methods) == InjectRegion.Methods)
            {
                MethodInfo[] allMethods = type.GetMethods(BindingFlags);

                foreach (MethodInfo info in allMethods)
                {
                    foreach (Attribute attribute in info.GetCustomAttributes())
                    {
                        if (attribute is not InjectAttribute) continue;
                    
                        ParameterInfo[] requiredParameters = info.GetParameters();

                        List<object> parameters = new List<object>();

                        foreach (ParameterInfo parameter in requiredParameters)
                        {
                            parameters.Add(FindIn(parameter.ParameterType, serviceLocators));
                        }

                        info.Invoke(toInject, parameters.ToArray());
                    }
                }
            }
            
            //fields
            if((region & InjectRegion.Fields) == InjectRegion.Fields)
            {
                FieldInfo[] allFields = type.GetFields(BindingFlags);
        
                foreach (FieldInfo info in allFields)
                {
                    foreach (Attribute attribute in info.GetCustomAttributes())
                    {
                        if (attribute is not InjectAttribute) continue;
                        
                        info.SetValue(toInject, FindIn(info.FieldType, serviceLocators));
                    }
                }
            }

            //properties
            if((region & InjectRegion.Properties) == InjectRegion.Properties)
            {
                PropertyInfo[] allProperties = type.GetProperties(BindingFlags);
        
                foreach (PropertyInfo info in allProperties)
                {
                    foreach (Attribute attribute in info.GetCustomAttributes())
                    {
                        if (attribute is not InjectAttribute) continue;

                        info.SetValue(toInject, FindIn(info.PropertyType, serviceLocators));
                    }
                }
            }
        }

        private object FindIn(Type type, params IServiceLocator[] locators)
        {
            for (int i = 0; i < locators.Length; i++)
            {
                IServiceLocator locator = locators[i];
                
                if (locator == null) LGR.PERR($"[DI KERNEL : FIND IN] NULL LOCATOR FOUND. INDEX {i}"); 

                object possibleDependency = locator.GetBlind(type);

                if (possibleDependency != null)
                {
                    return possibleDependency;
                }
            }

            LGR.PERR($"[DI KERNEL : FIND IN] NO DEPENDENCY OF TYPE {type.ToString()} WAS FOUND IN {locators.Length} DEPENDENCY LOCATORS");
            return null;
        }

        #endregion
    }
}
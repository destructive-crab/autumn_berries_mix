using System;
using System.Collections.Generic;
using destructive_code.Debug;

namespace destructive_code.ServiceLocators
{
    public class ServiceLocator<TServiceBase> : IServiceLocator
        where TServiceBase : class
    {
        private readonly Dictionary<Type, TServiceBase> services = new ();
        private readonly List<TServiceBase> servicesList = new ();
        public int Count => servicesList.Count;

        public TServiceBase[] GetAll() => servicesList.ToArray();
        
        public bool Contains<TService>() where TService : class
            => Contains(typeof(TService));
        
        public TService Get<TService>() where TService : class, TServiceBase
        {
            return GetBlind(typeof(TService)) as TService;
        }

        public bool Contains(Type serviceType)
        {
            return services.ContainsKey(serviceType);
        }

        public object GetBlind(Type extensionType)
        {
            if (!services.ContainsKey(extensionType))
            {
                return null;
            }

            return services[extensionType];
        }

        public T[] GetAllOfType<T>() 
            where T : class
        {
            List<T> result = new List<T>();

            foreach (KeyValuePair<Type, TServiceBase> service in services)
            {
                if (service.Key.IsSubclassOf(typeof(T)) || service.Key == typeof(T))
                {
                    result.Add(service.Value as T);
                }
            }

            return result.ToArray();
        }

        public bool TryGet<TService>(out TService serviceEnquire)
            where TService : class, TServiceBase
        {
            services.TryGetValue(typeof(TService), out TServiceBase outService);
            serviceEnquire = outService as TService;

            return serviceEnquire != null;
        }

        public ServiceLocator<TServiceBase> Register<TService>(TService service)
            where TService : TServiceBase
        {
            if (services == null)
            {
                LGR.PERR($"[SERVICE LOCATOR : REGISTER] TRYING TO REGISTER SERVICE {typeof(TService)} BUT INSTANCE IS NULL");
                return this;
            }

            if(services.TryAdd(typeof(TService), service))
            {
                servicesList.Add(service);
            }

            return this;
        }

        public void UnregisterAll()
        {
            List<Type> keys = new List<Type>(services.Keys);
            
            while (services.Count > 0)
            {
                Unregister(keys[0]);
                keys.RemoveAt(0);
            }
        }
        
        public bool Unregister(Type serviceType)
        {
            if (!serviceType.IsSubclassOf(typeof(TServiceBase)))
                return false;
            
            if (services.TryGetValue(serviceType, out TServiceBase service))
            {
                services.Remove(serviceType);
                servicesList.Remove(service);
                
                return true;
            }

            return false;
        }
        
        public bool Unregister<TService>()
            where TService : TServiceBase
        {
            if (services.TryGetValue(typeof(TService), out TServiceBase service))
            {
                services.Remove(typeof(TService));
                servicesList.Remove(service);
                
                return true;
            }

            return false;
        }
        
        public bool Unregister<TService>(out TService returnService)
            where TService : class, TServiceBase
        {
            if (services.TryGetValue(typeof(TService), out TServiceBase service))
            {
                services.Remove(typeof(TService));
                servicesList.Remove(service);
                
                returnService = service as TService;

                return true;
            }

            returnService = null;
            return false;
        }
    }
}
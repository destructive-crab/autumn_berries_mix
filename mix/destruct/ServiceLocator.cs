using destruct.Debug;

namespace destruct.ServiceLocators
{
    public class ServiceLocator<TServiceBase> : IServiceLocator
        where TServiceBase : class
    {
        private readonly Dictionary<Type, TServiceBase> services = new();

        //object is IEnumerable<TFilter> 
        private readonly Dictionary<Type, object> cachedFilters = new(); 
        
        public int Count => services.Count;

        public IEnumerable<TServiceBase> All => services.Values;
        
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
                ClearFilterCache();
            }

            return this;
        }

        public void ClearFilterCache()
        {
            cachedFilters.Clear();
        }
        
        public IEnumerable<TFilter> Filter<TFilter>()
        {
            var type = typeof(TFilter);
            if(!cachedFilters.ContainsKey(type))
            {
                cachedFilters.Add(type, services.Values.OfType<TFilter>());
            }
            
            return cachedFilters[type] as IEnumerable<TFilter>;
        }

        public void RemoveAll()
        {
            List<Type> keys = new List<Type>(services.Keys);
            
            while (services.Count > 0)
            {
                Remove(keys[0]);
                keys.RemoveAt(0);
            }
        }
        
        public bool Remove(Type serviceType)
        {
            if (!serviceType.IsSubclassOf(typeof(TServiceBase)))
                return false;
            
            if (services.TryGetValue(serviceType, out TServiceBase service))
            {
                services.Remove(serviceType);
                
                ClearFilterCache();
            
                return true;
            }
            
            return false;
        }
        
        public bool Remove<TService>()
            where TService : TServiceBase
        {
            if (services.TryGetValue(typeof(TService), out TServiceBase service))
            {
                services.Remove(typeof(TService));
                
                ClearFilterCache();
                
                return true;
            }

            return false;
        }
        
        public bool Remove<TService>(out TService returnService)
            where TService : class, TServiceBase
        {
            if (services.TryGetValue(typeof(TService), out TServiceBase service))
            {
                services.Remove(typeof(TService));
                
                returnService = service as TService;

                ClearFilterCache();
                return true;
            }

            returnService = null;
            return false;
        }
    }
}
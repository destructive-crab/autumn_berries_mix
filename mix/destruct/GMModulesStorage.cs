using destruct;
using destruct.ServiceLocators;

public sealed class GMModulesStorage : IServiceLocator
{
    public int Count => map.Count;
    
    public readonly Dictionary<Type, object> map = new();
    public readonly List<object> all = new();
    
    public readonly List<IGMModuleBoot> boots = new();
    public readonly List<IGMModuleQuit> quits = new();
    public readonly List<IGMModuleTick> ticks = new();
    
    public async Task<string> Boot()
    {
        string output = $"[{boots.Count} MODULES WILL BOOT] \n";
        
        foreach (IGMModuleBoot boot in boots)
        {
            await boot.Boot();
            output += $"- Module {boot} booted \n";
        }
        output += $"[END]";
        return output;
    }

    public async Task Quit()
    {
        foreach (IGMModuleQuit quit in quits)
            await quit.Quit();
    }

    public void Tick()
    {
        foreach (IGMModuleTick tick in ticks)
            tick.Tick();
    }
    
    public TModule Get<TModule>()
        where TModule : class
    {
        if (!map.ContainsKey(typeof(TModule)))
        {
            return null;
        }
        return map[typeof(TModule)] as TModule;
    }
    
    public void AutoRegister<TModule>(object module)
        where TModule : class
    {
        if(map.ContainsKey(typeof(TModule))) return;
        
        if(module is IGMModuleBoot boot)
            boots.Add(boot);
        
        if(module is IGMModuleQuit quit)
            quits.Add(quit);
        
        if(module is IGMModuleTick tick)
            ticks.Add(tick);
        
        all.Add(module);
        map.Add(typeof(TModule), module);
    }
    
    public bool Contains(Type serviceType)
    {
        return map.ContainsKey(serviceType);
    }
    
    public object GetBlind(Type serviceType)
    {
        if(!map.ContainsKey(serviceType)) return null;
        return map[serviceType];
    }
}
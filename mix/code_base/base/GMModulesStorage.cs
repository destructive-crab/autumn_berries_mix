using destruct;
using destruct.ServiceLocators;

public sealed class GMModulesStorage : IServiceLocator
{
    public int Count => map.Count;
    
    public readonly Dictionary<Type, object> map = new();
    public readonly List<object> all = new();
    
    public readonly List<IGMModuleBoot> boots = new();
    public readonly List<IGMModuleQuit> quits = new();
    
    public async Task<string> Boot()
    {
        string output = "";
        
        foreach (IGMModuleBoot boot in boots)
        {
            await boot.Boot();
            output += $"Module {boot.ToString()} booted" + boot.ToString() + "\n";
        }
        return output;
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
        
        all.Add(module);
        map.Add(typeof(TModule), module);
    }
    
    public void RegisterBootable(IGMModuleBoot boot)
    {
        boots.Add(boot);
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
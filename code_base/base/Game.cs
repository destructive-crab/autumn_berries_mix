using destruct.Debug;
using destruct.ServiceLocators;
using Godot;
using GodotTask;

namespace destruct;

public partial class Game 
{
    public static bool AllowDebug { get; private set; }
    
    //STATE
    public static bool Awake { get; private set; } = false;
    public static bool IsBootstrapping { get; private set; } = true;
    
    private static readonly GMModulesStorage debugModulesStorage = new();
    private static readonly GMModulesStorage modulesStorage = new();

    public static readonly IServiceLocator[] ModulesLocators = [modulesStorage, debugModulesStorage];
    
    private static Node currentGameRoot;
    
    public static async GDTask StartGame(bool allowDebug, GameStartPoint point)
    {
        AllowDebug = allowDebug;
        
        point.BuildModules(modulesStorage, debugModulesStorage);
        string output = await modulesStorage.Boot();
        
        if (AllowDebug)
        {
            output = $"\n Game started with from {point.GetType().ToString()}({point.Name}). ";
            output += $"\n With {modulesStorage.Count} modules, including: ";
            
            foreach (object module in modulesStorage.all)
            {
                output += module.ToString() + "; ";
            }
            
            string debugModulesOutput = await debugModulesStorage.Boot();
            
            output += debugModulesOutput;
            output += $"\n With {debugModulesStorage.Count} debug modules, including: ";
            
            foreach (object module in debugModulesStorage.all)
            {
                output += module.ToString() + "; ";
            }
        }
        
        LGR.PM(output);
        IsBootstrapping = false;
        Awake = true;
        
        point.Complete();
        
        point.SetScript(new Variant());

        currentGameRoot = point;
    }
    
    public static async void QuitGame()
    {
        Awake = false;
        foreach (IGMModuleQuit quit in modulesStorage.quits)
        {
            await quit.Quit();
        }

        currentGameRoot.GetTree().Quit();
    }
    
    public static TModule G<TModule>()
        where TModule : class
    {
        return modulesStorage.Get<TModule>();
    }
    
    public static TModule GDb<TModule>()
        where TModule : class
    {
        return debugModulesStorage.Get<TModule>();
    }
    
    public static bool TG<TModule>(out TModule module)
        where TModule : class
    {
        module = modulesStorage.Get<TModule>();
        if (AllowDebug && module == null)
        {
            module = debugModulesStorage.Get<TModule>();
        }
        return module != null;
    }
    
    public static IServiceLocator GetModulesLocator()
    {
        return modulesStorage;
    }
}

public interface IGMModuleBoot
{
    public GDTask Boot();
}

public interface IGMModuleQuit
{
    public GDTask Quit();
}
    
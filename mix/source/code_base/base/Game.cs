using System.Diagnostics;
using destruct.Debug;
using destruct.ServiceLocators;
using mix.gameplay.systems;
using Raylib_cs;

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

    public static AudioService AudioService { get; private set; }
    public static Scene CurrentScene => G<SceneSwitcher>().CurrentScene;

    public static float DeltaTime { get; private set; }
    
    public static async Task StartGame(bool allowDebug, GameStartPoint point)
    {
        Raylib.SetConfigFlags(ConfigFlags.ResizableWindow);
        Raylib.InitWindow(800, 600, "MIX");
        Raylib.InitAudioDevice();

        AudioService = new();
        
        AllowDebug = allowDebug;
        
        point.BuildModules(modulesStorage, debugModulesStorage);
        
        string output = await modulesStorage.Boot();
        
        if (AllowDebug)
        {
            output = $"\n Game started with from {point.GetType().ToString()}(todo entry points). ";
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
        Loop();
    }
    
    public static async void QuitGame()
    {
        Awake = false;
    }

    private static readonly HashSet<PostprocessingOverride> postprocessingOverrides = new();

    public static TOverride NewPostProcessingOverride<TOverride>()
        where TOverride : PostprocessingOverride, new()
    {
        TOverride newOverride = new();

        newOverride.Load();
        newOverride.Defaults();

        postprocessingOverrides.Add(newOverride);
            
        return newOverride;
    }

    public static void RemovePostprocessingShader(PostprocessingOverride postprocessingOverride)
    {
        if (!postprocessingOverrides.Remove(postprocessingOverride))
        {
            postprocessingOverride.Unload();
            LGR.PERR($"Override was not presented in renderer");
        }
    }
    
    public static void Loop()
    {
        Stopwatch stopwatch = new();
        stopwatch.Start();
        
        RenderTexture2D rendererLayer = Raylib.LoadRenderTexture(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
       
        while (Awake && !Raylib.WindowShouldClose())
        {
            if (rendererLayer.Texture.Width != Raylib.GetScreenWidth() ||
                rendererLayer.Texture.Height != Raylib.GetScreenHeight())
            {
                rendererLayer = Raylib.LoadRenderTexture(Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
            }
            
            DeltaTime = Raylib.GetFrameTime();
            
            CurrentScene?.PrepareRenderer();
            
            Raylib.BeginTextureMode(rendererLayer);
            {
                Raylib.ClearBackground(Color.Black);
                
                G<Cursor>().SetToDefault();
                CurrentScene?.Renderer();
                modulesStorage.Tick();
            }
            Raylib.EndTextureMode();
            
            Raylib.BeginDrawing();
            {
                Raylib.ClearBackground(Color.Black);

                if (postprocessingOverrides.Count > 0)
                {
                    RenderTexture2D result = rendererLayer;
                    foreach (var postprocessingOverride in postprocessingOverrides)
                        result = postprocessingOverride.PerformRaylibDrawing(ref result);
                    
                    Raylib.DrawTextureRec(result.Texture, new(0,0,result.Texture.Width, -result.Texture.Height), new(0,0), Color.White);
                }
                else
                {
                    Raylib.DrawTextureRec(rendererLayer.Texture, new(0,0,rendererLayer.Texture.Width, -rendererLayer.Texture.Height), new(0,0), Color.White);
                }
            }
            Raylib.EndDrawing();
            
            AudioService.UpdateAudio();
        }

        Raylib.CloseWindow();
        Raylib.CloseAudioDevice();
        
        Raylib.UnloadTexture(rendererLayer.Texture);
        Assets.UnloadAll();
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
    public Task Boot();
}

public interface IGMModuleQuit
{
    public Task Quit();
}

public interface IGMModuleTick
{
    public void Tick();
}
    
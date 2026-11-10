using destruct;
using destruct.DI;

public sealed partial class SceneSwitcher 
{
    public bool IsSceneLoaded { get; private set; }
    public bool IsPersistentSceneLoaded { get; private set; }
    
    public Scene CurrentScene { get; private set; }
    
    public event Action<Scene> OnSwitchingFromCurrent; //prev
    public event Action<Scene, Scene> OnSwitched; //prev/new
    public static event Action<Scene> OnLoaded; //new

    private static SceneSwitcher sceneSwitcher;

    public SceneSwitcher()
    {
        if (sceneSwitcher == null)
        {
            sceneSwitcher = this;
        }
        else
        {
            throw new Exception("MULTIPLE SCENE SWITCHER INSTANCES ARE NOT ALLOWED");
        }
    }

    public async Task<TScene> SwitchTo<TScene>(TScene scene, Action onSwitched = null)
        where TScene : Scene
    {
        OnSwitchingFromCurrent?.Invoke(CurrentScene);
        IsSceneLoaded = false;
        
        //clearing previous scene if exists
        if (CurrentScene != null)
        {
            await CurrentScene.DisposeScene();
        }
        
        //starting loading new scene
        Scene prevScene = CurrentScene;
        CurrentScene = scene;
        
        CurrentScene.Prepare();

        Game.G<DIKernel>().RegisterLocalDependencies(CurrentScene.GetProviders());

        CurrentScene.Load();
        
        IsSceneLoaded = true;
        
        onSwitched?.Invoke();
        OnSwitched?.Invoke(prevScene, CurrentScene);
        OnLoaded?.Invoke(CurrentScene);

        return CurrentScene as TScene;
    }
}
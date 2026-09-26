using System;
using destruct;
using destruct.DI;
using Godot;
using GodotTask;

public sealed partial class SceneSwitcher : Node
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

    public async GDTask<TScene> SwitchTo<TScene>(PackedScene scene, Action onSwitched = null)
        where TScene : Scene
    {
        OnSwitchingFromCurrent?.Invoke(CurrentScene);
        IsSceneLoaded = false;
        
        //clearing previous scene if exists
        if (CurrentScene != null)
        {
            CurrentScene.DisposeScene();
        }
        
        //starting loading new scene
        Scene prevScene = CurrentScene;
        CurrentScene = scene.Instantiate<Scene>();
        AddChild(CurrentScene);
        
        CurrentScene.Prepare();

        Game.G<DIKernel>().RegisterLocalDependencies(CurrentScene.GetProviders());
        Game.G<INodeFactory>().InjectNodeTree(CurrentScene);

        CurrentScene.Load();
        
        IsSceneLoaded = true;
        
        onSwitched?.Invoke();
        OnSwitched?.Invoke(prevScene, CurrentScene);
        OnLoaded?.Invoke(CurrentScene);

        return CurrentScene as TScene;
    }
}
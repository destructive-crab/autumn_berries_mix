using destruct.DI;
using destruct.scenes;
using destruct.ServiceLocators;
using Godot;
using GodotTask;

public abstract partial class Scene : Node
{
    public readonly ServiceLocator<ISceneSystem> Systems = new();
    
    public abstract IDependenciesProvider[] GetProviders();

    public void Prepare()
    {
        OnPrepared();
        foreach (var sceneLoaded in Systems.Filter<ISceneSystemOnPrepared>())
        {
            sceneLoaded.OnScenePrepared();
        }
    }
    
    protected virtual void OnPrepared() { }

    public void Load()
    {
        OnSceneLoaded();
        foreach (var sceneLoaded in Systems.Filter<ISceneSystemOnSceneLoaded>())
        {
            sceneLoaded.OnSceneLoaded();
        }
    }

    public virtual GDTask DisposeScene() { return GDTask.CompletedTask; }
    
    protected abstract void OnSceneLoaded();
}
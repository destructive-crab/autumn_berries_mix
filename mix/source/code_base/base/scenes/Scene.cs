using destruct.DI;
using destruct.scenes;
using destruct.ServiceLocators;

public abstract class Scene 
{
    public readonly ServiceLocator<ISystem> Systems = new();
    
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

    public virtual Task DisposeScene() { return Task.CompletedTask; }
    
    protected abstract void OnSceneLoaded();

    public virtual void Tick()
    {
        
    }
}
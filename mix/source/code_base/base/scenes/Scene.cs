using System.Text;
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

    public virtual void PrepareRenderer() { }
    public virtual void Renderer() { }

    public string SystemNames()
    {
        StringBuilder sb = new();
        foreach (var s in Systems.All)
        {
            sb.Append(s.DebugName());
            sb.Append('\n');
        }

        return sb.ToString();
    }
}
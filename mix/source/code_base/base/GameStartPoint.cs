using destruct;
using destruct.DI;

public abstract partial class GameStartPoint 
{
    public abstract bool AllowDebug();
    public virtual IDependenciesProvider[] GetProviders()
    {
        return Array.Empty<IDependenciesProvider>();
    }

    public async Task StartGame()
    {
        await Prepare();
        await Game.StartGame(AllowDebug(), this);
    }

    protected virtual Task Prepare()
    {
        return Task.CompletedTask;
    }

    public virtual Task BuildModules(GMModulesStorage modulesStorage, GMModulesStorage debugStorage)
    {
        modulesStorage.AutoRegister<SceneSwitcher>(new SceneSwitcher());
        modulesStorage.AutoRegister<DIKernel>(new DIKernel());
        modulesStorage.AutoRegister<Cursor>(new Cursor());
        //modulesStorage.AutoRegister<AudioSystem>(new AudioSystem());
        
        return Task.CompletedTask;
    }

    public virtual void Complete()
    {
        Game.G<DIKernel>().RegisterDependenciesToCore(GetProviders());
    }
}
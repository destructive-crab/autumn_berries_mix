using System;
using destruct;
using destruct.DI;
using Godot;
using GodotTask;

public abstract partial class GameStartPoint : Node 
{
    public abstract bool AllowDebug();
    public virtual IDependenciesProvider[] GetProviders()
    {
        return Array.Empty<IDependenciesProvider>();
    }

    public override void _Ready()
    {
        StartGame();
    }

    private async void StartGame()
    {
        await Prepare();
        Game.StartGame(AllowDebug(), this);
    }

    protected virtual GDTask Prepare()
    {
        return GDTask.CompletedTask;
    }

    public virtual GDTask BuildModules(GMModulesStorage modulesStorage, GMModulesStorage debugStorage)
    {
        modulesStorage.AutoRegister<SceneSwitcher>(GetNode<SceneSwitcher>("SceneSwitcher"));
        modulesStorage.AutoRegister<DIKernel>(new DIKernel());
        modulesStorage.AutoRegister<INodeFactory>(new NodeFactory());
        //modulesStorage.AutoRegister<AudioSystem>(new AudioSystem());
        
        return GDTask.CompletedTask;
    }

    public virtual void Complete()
    {
        Game.G<DIKernel>().RegisterDependenciesToCore(GetProviders());
    }

}
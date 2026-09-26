namespace destruct.scenes;

public interface ISceneSystem
{
    string DebugName();
}

public interface ISceneSystemOnPrepared
{
    void OnScenePrepared();
}

public interface ISceneSystemOnSceneLoaded
{
    void OnSceneLoaded();
}

public interface ISceneSystemOnSceneProcess
{
    void OnProcess();
}

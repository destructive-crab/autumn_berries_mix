namespace destruct.scenes;

public interface ISceneSystemProcessInputs
{
    void ProcessInputs();
}

public interface ISceneSystemWorldRenderer
{
    void RenderWorld();
}

public interface ISceneSystemPostWorldRenderer
{
    void PostWorldRenderer();
}

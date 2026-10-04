namespace destruct.scenes;

public interface ISceneSystemProcessInputs
{
    void ProcessInputs(bool guiWasOverlapped);
}

public interface ISceneSystemWorldRenderer
{
    void RenderWorld();
}

public interface ISceneSystemPostWorldRenderer
{
    void PostWorldRenderer();
}

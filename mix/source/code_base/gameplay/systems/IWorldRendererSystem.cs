namespace destruct.scenes;

public interface IPreWorldRendererSystem : ISystem
{
    void PreWorldRenderer();
}
public interface IWorldRendererSystem : ISystem
{
    void RenderWorld();
}
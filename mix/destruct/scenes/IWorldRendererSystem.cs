namespace destruct.scenes;

public interface IPreRendererSystem : ISystem
{
    void PreRenderer();
}

public interface IRendererSystem : ISystem
{
    void Render();
}

public interface IWorldRendererSystem : ISystem
{
    void RenderWorld();
}
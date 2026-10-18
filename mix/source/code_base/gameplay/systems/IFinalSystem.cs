using destruct.scenes;

namespace mix.code_base.gameplay.systems;

public interface IFrameBeginningSystem : ISystem
{
    void Begin();
}

public interface IGameLogicSystem : ISystem
{
    void Process();
}

public interface IFinalSystem : ISystem
{
    void FinishFrame();
}
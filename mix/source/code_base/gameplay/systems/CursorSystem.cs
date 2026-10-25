using System.Numerics;
using destruct;
using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using destruct.scenes;
using mix.gameplay.systems;
using mix.inputs;
using Raylib_cs;

namespace mix.code_base.gameplay.systems;

public sealed class CursorSystem : ISystem, IInputsProcessSystem 
{
    public string DebugName() => nameof(CursorSystem);
    [Inject] private ECSWorld world;
    [Inject] private ICameraInputSystem inputService;
    
    public void ProcessInputs(bool guiWasOverlapped)
    {
        if (!guiWasOverlapped)
        {
            if (Raylib.IsMouseButtonDown(MouseButton.Right))
            {
                Game.G<Cursor>().SetToMove();
                return;
            }

            if (world.Pool<Hovered>().Count != 0)
            {
                Game.G<Cursor>().SetToPointer();
                
                if (Raylib.IsMouseButtonDown(MouseButton.Left))
                    Game.G<Cursor>().SetToPressedPointer();
            }
        }
    }
}
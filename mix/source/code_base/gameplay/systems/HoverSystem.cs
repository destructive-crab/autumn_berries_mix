using System.Numerics;
using destruct;
using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using destruct.imgui;
using destruct.scenes;
using mix.code_base;
using mix.inputs;

namespace mix.gameplay.systems;

public struct Hovered {}
public struct Hoverable {}

public sealed class HoverSystem : ISystem, IInputsProcessSystem
{
    public string DebugName() => nameof(HoverSystem);

    private IEnumerable<int> entities = [];
    private ECSWorld world;
    
    private IGeneralInputService inputService;
    
    private EntityComponentPool<Hovered> hoveredPool;

    [Inject]
    private void Inject(ECSWorld world, IGeneralInputService levelInputService)
    {
        hoveredPool = world.Pool<Hovered>();
        world.Filter()
            .In<Hoverable>()
            .In<Scale>()
            .In<Position>()
            .Subscribe((e) =>
            {
                entities = e.ToArray();
            });

        this.world = world;
        this.inputService = levelInputService;
    }

    public void ProcessInputs(bool guiWasOverlapped)
    {
        if (guiWasOverlapped) return;
        
        foreach (int entity in entities)
        {
            Vector2 p = world.Pool<Position>().Get(entity).ToV2();
            
            if (world.Pool<Origin>().Has(entity))
            {
                ref Origin o = ref world.Pool<Origin>().Get(entity);
                p -= o.ToV2();
            }
            
            ref Scale s = ref world.Pool<Scale>().Get(entity);

            Vector2 posInUnits = UTL.TranslatePositionInPixelsToUnits(inputService.GlobalMousePosition);

            var hovered = hoveredPool.Has(entity);
            
            if (!hovered &&
                posInUnits.X > p.X       && posInUnits.Y > p.Y &&
                posInUnits.X < p.X + s.X && posInUnits.Y < p.Y + s.Y)
            {
                hoveredPool.New(entity);
            }
            else if (hovered && !(posInUnits.X > p.X && posInUnits.Y > p.Y &&
                                  posInUnits.X < p.X + s.X && posInUnits.Y < p.Y + s.Y))
            {
                hoveredPool.Remove(entity);
            }
        }
    }
}

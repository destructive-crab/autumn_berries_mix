using System.Collections.Generic;
using destruct;
using destruct.ecs;
using destruct.scenes;

namespace destructive_code.Tilemap;

public sealed class OverlaySystem : ISceneSystem
{
    private int prevPointed = -1;
    private int prevSelected = -1;
    
    private readonly ECSWorld world;

    private Dictionary<int, OverlayNode> overlayMap = new();

    private OverlayFactory factory;
    
    public OverlaySystem(LevelScene level, ECSWorld world)
    {
        this.world = world;
        factory = new(level);
        factory.LoadResources();
        
        world.Pool<Pointed>()     .OnNew += UpdatePointedTile;
        world.Pool<Selected>()    .OnNew += UpdateSelectedTile;
        
        world.Pool<ArrowOverlay>().OnNew += OnNewArrow;
        world.Pool<ArrowOverlay>().OnRemoved += OnArrowRemoved;
    }

    private void OnArrowRemoved(EntityComponentPool<ArrowOverlay> pool, int entityId)
    {
        overlayMap[entityId].QueueFree();
    }

    private void OnNewArrow(EntityComponentPool<ArrowOverlay> pool, int entityId)
    {
        ArrowOverlay a = pool.Get(entityId);
        overlayMap.Add(entityId, factory.CreateArrow(a.Cell.Position, a.Direction));
    }

    ~OverlaySystem()
    {
        world.Pool<Pointed>() .OnNew -= UpdatePointedTile;
        world.Pool<Selected>().OnNew -= UpdateSelectedTile;
    }

    public string DebugName() => "Overlay System";

    private void UpdatePointedTile(EntityComponentPool<Pointed> pool, int entityId)
    {
        if (prevPointed != -1) pool.Remove(prevPointed);
        
        factory.SetPointing(world.Pool<Cell>().Get(world.Pool<Tile>().Get(entityId).AttachedToCell).Position);
        prevPointed = entityId;
    }

    private void UpdateSelectedTile(EntityComponentPool<Selected> pool, int entityId)
    {
        if (prevSelected != -1) pool.Remove(prevSelected);
        
        factory.SetSelection(world.Pool<Cell>().Get(world.Pool<Tile>().Get(entityId).AttachedToCell).Position);
        prevSelected = entityId;
    }
}
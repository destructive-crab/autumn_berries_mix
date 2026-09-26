using destruct;
using destruct.DI;
using destruct.ecs;
using Godot;

namespace autumn_berries_mix.code_base.gameplay;

public sealed partial class TerrainLayer : TileMapLayer
{
    private ECSWorld world;

    [Inject]
    private void Inject(ECSWorld world)
    {
        this.world = world;
        world.Pool<Terrain>().OnNew += OnNewTerrain;
        world.Pool<Terrain>().OnRemoved += OnTerrainRemoved;
    }

    private void OnNewTerrain(EntityComponentPool<Terrain> pool, int entityId)
    {
        Terrain terrain = world.Pool<Terrain>().Get(entityId);
        Cell cell = world.Pool<Cell>().Get(world.Pool<Tile>().Get(entityId).AttachedToCell);
        SetCell(cell.Position, terrain.SourceId, terrain.AtlasPosition);
    }
    
    private void OnTerrainRemoved(EntityComponentPool<Terrain> pool, int entityId)
    {
        Terrain terrain = world.Pool<Terrain>().Get(entityId);
        Cell cell = world.Pool<Cell>().Get(entityId);
        SetCell(cell.Position, -1);
    }
}
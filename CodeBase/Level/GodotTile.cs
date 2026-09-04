using destructive_code.DI;
using destructive_code.Tilemap;
using Godot;

namespace destructive_code.Level;

public abstract class GodotTile : ITile
{
    public Vector2I Position { get; set; }
    public TileAlias TileSetReference;

    public abstract TileMapLayer GetTilemap();

    public GodotTile(TileAlias alias)
    {
        TileSetReference = alias;
    }

    public void OnPlaced() { }

    public void OnRemoved()
    {
        GetTilemap().SetCell(Position, -1);
    }

    public void UpdatePosition(Vector2I pos)
    {
        GetTilemap().SetCell(Position, -1);
        Position = pos;
        GetTilemap().SetCellFromAlias(Position, TileSetReference);
    }

    public abstract ITile Clone(); 

    public string Export() => "";
    public void Import(string json) { }
}

public sealed class TerrainTile : GodotTile
{
    [Inject] private TerrainTileMapLayer terrainLayer;

    public TerrainTile(TileAlias alias) : base(alias) { }
    
    public override TileMapLayer GetTilemap() => terrainLayer;
    public override ITile Clone() => new ObstaclesTile(TileSetReference);
}

public sealed class ObstaclesTile : GodotTile
{
    [Inject] private TerrainTileMapLayer obstaclesTileLayer;

    public ObstaclesTile (TileAlias alias) : base(alias) { }
    
    public override TileMapLayer GetTilemap() => obstaclesTileLayer;
    public override ITile Clone() => new ObstaclesTile(TileSetReference);
}

using Godot;

namespace destructive_code.Tilemap;

public struct TileAlias
{
    public int      SourceId;
    public Vector2I AtlasPosition;

    public TileAlias(int sourceId, Vector2I atlasPosition)
    {
        SourceId = sourceId;
        AtlasPosition = atlasPosition;
    }
}

public static class TileMapLayerExtensions
{
    public static void SetCellFromAlias(this TileMapLayer layer, Vector2I position, TileAlias alias)
    {
        layer.SetCell(position, alias.SourceId, alias.AtlasPosition);
    }
    
    public static void SetCellFromAlias(this TileMapLayer layer, int x, int y, TileAlias alias)
    {
        SetCellFromAlias(layer, new(x,y), alias);
    }
}
using System;
using System.Collections.Generic;
using destructive_code.ServiceLocators;
using Godot;

namespace destructive_code.Tilemap;

public class GameMap<TEnum>
    where TEnum : struct, Enum
{
    public int Width() => width;
    public int Height() => height;
    public Vector2 GamePosition;

    public Rect2I Bounds => new(0, 0, Width(), Height());
    public IEnumerable<MapLayer> Models => layers.Values;

    public readonly ServiceLocator<IGameMapService> LocalServices = new();

    private readonly Dictionary<TEnum, MapLayer> layers = new();

    private int width;
    private int height;
    
    public GameMap(int w, int h)
    {
        width = w;
        height = h;
    }
    
    public MapLayer AddLayer(TEnum alias)
    {
        if (layers.ContainsKey(alias)) throw new Exception($"Layer with {alias} alias already in map");
        MapLayer newLayer = new MapLayer(Width(), Height(), [LocalServices]);
        layers.Add(alias, newLayer);

        return newLayer;
    }

    public MapLayer L(TEnum alias) => layers[alias];

    public void RemoveLayer(TEnum alias)
    {
        if (!layers.ContainsKey(alias)) throw new Exception($"No layer with {alias} alias found");

        layers[alias].Dispose();
        layers.Remove(alias);
    }

    public ITile[] TilesAt(Vector2I currentSelectedTile)
    {
        List<ITile> result = new();
        foreach (KeyValuePair<TEnum, MapLayer> layerPair in layers)
        {
            ITile tile = layerPair.Value.GetTileAt(currentSelectedTile);
            if (tile != null) result.Add(tile);
        }

        return result.ToArray();
    }
}
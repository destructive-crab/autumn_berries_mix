using System;
using destructive_code.Debug;
using destructive_code.ServiceLocators;
using Godot;
using GodotTask;

namespace destructive_code.Tilemap;

public abstract class TileAction
{
    public abstract GDTask Perform();
}

public interface ISelectableTile
{
    TileAction[] Select();
}

public interface ITile
{
    Vector2I Position { get; }
    
    void OnPlaced();
    void OnRemoved();
    void UpdatePosition(Vector2I pos);
    
    ITile Clone();
    
    string Export();
    void Import(string json);
}

public class MapLayer : IDisposable
{
    public int XYToIndex(int x, int y) => UTL.XYToIndex(x, y, LayerWidth, LayerHeight);
    public int V2IToIndex(Vector2I v2i) => XYToIndex(v2i.X, v2i.Y);
    public Vector2I IndexToV2I(int index) => UTL.IndexToV2I(index, LayerWidth, LayerHeight);
    
    public int LayerWidth  { get; private set; }
    public int LayerHeight { get; private set; }
    
    public ITile[] Tiles { get; private set; }

    public event Action<Vector2I> TileChanged;

    private IServiceLocator[] InjectLocators;
        
    public MapLayer(int layerWidth, int layerHeight, IServiceLocator[] injectLocators)
    {
        InjectLocators = injectLocators;
        ResizeLayer(layerWidth, layerHeight);
    }

    public void ForeachTile(Action<ITile> callback)
    {
        for (int i = 0; i < Tiles.Length; i++)
        {
            if (Tiles[i] != null)
            {
                callback.Invoke(Tiles[i]);
            }
        }
    }

    public void ForeachPosition(Action<Vector2I> callback)
    {
        for (int x = 0; x < LayerWidth; x++)
        {
            for (int y = 0; y < LayerHeight; y++)
            {
                callback.Invoke(new(x,y));
            }
        }
    }

    public Rect2I GetCompressedBoundsRect()
    {
        int minFirstTileX  = LayerWidth;
        int maxLastTileX = 0;
        
        int firstNotEmptyLineY = LayerHeight;
        int lastNotEmptyLineY = 0;
        
        for (int y = 0; y < LayerWidth; y++)
        {
            int rowFirstTile = LayerWidth;
            int rowLastTile = -1;
            
            for (int x = 0; x < LayerWidth; x++)
            {
                if (Tiles[y * LayerHeight + x] != null)
                {
                    if (rowFirstTile == LayerWidth)
                    {
                        rowFirstTile = x;
                    }
                    rowLastTile = x;
                } 
            }

            if (firstNotEmptyLineY == LayerHeight && rowLastTile != -1)
                firstNotEmptyLineY = y;
            if (rowLastTile != -1)
                lastNotEmptyLineY = y;
            
            minFirstTileX  = Math.Min(minFirstTileX, rowFirstTile);
            maxLastTileX = Math.Max(maxLastTileX, rowLastTile);
        }

        int newW = (maxLastTileX - minFirstTileX + 1);
        int newH = (lastNotEmptyLineY - firstNotEmptyLineY + 1);

        return new Rect2I(minFirstTileX, firstNotEmptyLineY, newW, newH);
    }
    
    public void ResizeCanvasByRect(Rect2I rect)
    {
        if (rect.End.X > LayerWidth || rect.End.Y > LayerHeight || rect.Position.X < 0 || rect.Position.Y < 0)
        {
            LGR.PM($"Invalid rect provided to tile layer {rect.Position} {rect.End}; layer rect: 0,0 - {LayerWidth}, {LayerHeight}");
            return;
        }
        ITile[] resizedTiles = new ITile[rect.Size.X*rect.Size.Y];

        for (int x = rect.Position.X; x < rect.End.X; x++)
        {
            for (int y = rect.Position.Y; y < rect.End.Y; y++)
            {
                int tileX = x - rect.Position.X;
                int tileY = y - rect.Position.Y;
                int tileI = tileY * rect.Size.X + tileX;
                
                resizedTiles[tileI] = Tiles[y * LayerHeight + x];
                resizedTiles[tileI].UpdatePosition(new Vector2I(tileX, tileY));
            }
        }

        Tiles = resizedTiles;
        LayerWidth = rect.Size.X;
        LayerHeight = rect.Size.Y;
    }

    public void ResizeLayer(int w, int h)
    {
        LayerWidth  = w;
        LayerHeight = h;

        ITile[] oldTiles = Tiles;

        Tiles = new ITile[w * h];
        
        for (int i = 0; i < Tiles.Length; i++)
        {
            Tiles[i] = null;
            
            if (oldTiles != null && i < oldTiles.Length)
            {
                Tiles[i] = oldTiles[i];
                Tiles[i].UpdatePosition(IndexToV2I(i));
            }
        }
    }
    
    public bool TryGetTileAt(int x, int y, out ITile tile)
    {
        tile = GetTileAt(x, y);
        return tile != null;
    }

    public bool HasTileAt(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < LayerWidth && y < LayerHeight)
            return Tiles[XYToIndex(x, y)] != null;

        return false;
    }

    public bool HasTileAt(Vector2I layerPosition)
        => HasTileAt(layerPosition.X, layerPosition.Y);
    
    public ITile GetTileAt(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < LayerWidth && y < LayerHeight)
            return Tiles[XYToIndex(x, y)];

        return null;
    }

    public ITile GetTileAt(Vector2I layerPosition)
        => GetTileAt(layerPosition.X, layerPosition.Y);

    public void PlaceTileAt(ITile tile, int layerX, int layerY)
    {
        int index = XYToIndex(layerX, layerY);

        if (HasTileAt(layerX, layerY))
        {
            Tiles[index].OnRemoved();
        } 
        
        Tiles[index] = tile;
        
        GameRoot.I.DIKernel.InjectWithBaseAnd(tile, InjectLocators);
        
        tile.OnPlaced();
        tile.UpdatePosition(new(layerX, layerY));
    }

    public void PlaceTileAt(ITile tile, Vector2I layerPosition)
        => PlaceTileAt(tile, layerPosition.X, layerPosition.Y);

    public void MoveTile(Vector2I originalPosition, Vector2I newPosition)
    {
        ITile t = Tiles[V2IToIndex(originalPosition)];
        Tiles[V2IToIndex(originalPosition)] = null;
        Tiles[V2IToIndex(newPosition)] = t;
        
        t.UpdatePosition(newPosition);
    }

    public void CopyTilesFromLayer(MapLayer layer, Vector2I positionOffset = default)
    {
        layer.ForeachTile(t =>
        {
            PlaceTileAt(t.Clone(), t.Position + positionOffset);
        });
    }

    public void Dispose()
    {
        //todo
    }
}
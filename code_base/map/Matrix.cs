using System;
using destruct.Debug;
using destruct.ecs;
using destruct.ServiceLocators;
using Godot;

namespace destruct.Level;

public class Matrix<T> 
    where T : IMatrixElement
{
    public int XYToIndex(int x, int y) => UTL.XYToIndex(x, y, LayerWidth, LayerHeight);
    public int V2IToIndex(Vector2I v2i) => XYToIndex(v2i.X, v2i.Y);
    public Vector2I IndexToV2I(int index) => UTL.IndexToV2I(index, LayerWidth, LayerHeight);
    
    public int LayerWidth  { get; private set; }
    public int LayerHeight { get; private set; }
    
    public T[] Cells { get; private set; }

    public Matrix(int layerWidth, int layerHeight)
    {
        ResizeLayer(layerWidth, layerHeight);
    }

    public bool Contains(int x, int y)
    {
        return x >= 0 && y >= 0 && x < LayerWidth && y < LayerHeight;
    }

    public bool Contains(Vector2I p) => Contains(p.X, p.Y);
    
    public void ForeachCell(Action<Vector2I, T> callback)
    {
        for (int i = 0; i < Cells.Length; i++)
            if (Cells[i] != null) 
                callback.Invoke(IndexToV2I(i), Cells[i]);
    }

    public void ForeachPosition(Action<Vector2I> callback)
    {
        for (int x = 0; x < LayerWidth; x++)
            for (int y = 0; y < LayerHeight; y++)
                callback.Invoke(new(x,y));
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
                if (Cells[y * LayerHeight + x] != null)
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
    
    public void CropLayerByRect(Rect2I rect)
    {
        if (rect.End.X > LayerWidth || rect.End.Y > LayerHeight || rect.Position.X < 0 || rect.Position.Y < 0)
        {
            LGR.PM($"Invalid rect provided to tile layer {rect.Position} {rect.End}; layer rect: 0,0 - {LayerWidth}, {LayerHeight}");
            return;
        }
        T[] resizedCells = new T[rect.Size.X*rect.Size.Y];

        for (int x = rect.Position.X; x < rect.End.X; x++)
        {
            for (int y = rect.Position.Y; y < rect.End.Y; y++)
            {
                int tileX = x - rect.Position.X;
                int tileY = y - rect.Position.Y;
                int cellIndex = tileY * rect.Size.X + tileX;
                
                resizedCells[cellIndex] = Cells[y * LayerHeight + x];
                resizedCells[cellIndex].Position = new(tileX, tileY);
            }
        }

        Cells = resizedCells;
        LayerWidth = rect.Size.X;
        LayerHeight = rect.Size.Y;
    }

    public void ResizeLayer(int w, int h)
    {
        LayerWidth  = w;
        LayerHeight = h;

        var oldTiles = Cells;

        Cells = new T[w * h];
        
        for (int i = 0; i < Cells.Length; i++)
        {
            Cells[i] = default;

            if (oldTiles != null && i < oldTiles.Length)
            {
                Cells[i] = oldTiles[i];
            }
            else Cells[i] = default;
        }
    }

    public void Place(int x, int y, T cell)
    {
        if (!Contains(x,y))
        {
            throw new Exception($"Matrix does not contain position {x} {y}");
        }
        Cells[XYToIndex(x,y)] = cell;
        //cell.Position = new Vector2I(x, y);
    }

    public void Place(Vector2I position, T cell) => Place(position.X, position.Y, cell);
    
    public T Get(int x, int y)
    {
        if (x >= 0 && y >= 0 && x < LayerWidth && y < LayerHeight)
        {
            return Cells[XYToIndex(x, y)];
        }

        throw new Exception($"Position ({x} {y}) out of bounds; Layer bounds: {LayerWidth} {LayerHeight}");
    }

    public T Get(Vector2I layerPosition)
        => Get(layerPosition.X, layerPosition.Y);
    
    public void Dispose()
    {
        //todo
    }
}
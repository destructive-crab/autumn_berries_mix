using destruct;
using destruct.Debug;
using destruct.DI;
using destruct.ecs;
using mix.code_base.gameplay.systems;
using mix.gameplay.systems;

namespace mix.code_base;

public struct Cell
{
    public Vec2I     Position;
    public List<int> Entities;
}

public sealed class GameMap
{
    public int LayerWidth { get; private set; }
    public int LayerHeight { get; private set; }
    
    private int[] cells;

    [Inject] private ECSWorld world;

    public int   XYToIndex(int x, int y) => UTL.XYToIndex(x, y, LayerWidth, LayerHeight);
    public int   V2IToIndex(Vec2I v2i) => XYToIndex(v2i.X, v2i.Y);
    public Vec2I IndexToV2I(int index) => UTL.IndexToV2I(index, LayerWidth, LayerHeight);

    public void UpdateLayout(IEnumerable<int> entities)
    {
        foreach (int entity in entities)
        {
            if (!world.Pool<Tile>().Has(entity))
            {
                LGR.PW($"Trying to update layout with invalid entity; to tile component on {entity}");
                continue;
            }

            var p = GetCellPositionForTile(entity);
            GetCell(p).Entities.Add(entity);
        }
    }

    public void Create(int width, int height)
    {
        LayerWidth = width;
        LayerHeight = height;
        
        cells = new int[width * height];
        
        world.BlockFiltersEvaluation();
        {
            for (var i = 0; i < cells.Length; i++)
                cells[i] = CreateNewCellOn(IndexToV2I(i));
        }
        world.UnblockFiltersEvaluationAndEvaluate();
    }

    public void Grow(int width, int height)
    {
        LayerWidth += width;
        LayerHeight += height;
        
        int[] newCells = new int[(LayerWidth) * (LayerHeight)];

        for (int i = 0; i < cells.Length; i++)
            newCells[i] = cells[i];

        world.BlockFiltersEvaluation();
        {
            for (int i = cells.Length; i < newCells.Length; i++)
                newCells[i] = CreateNewCellOn(IndexToV2I(i));
        }
        world.UnblockFiltersEvaluationAndEvaluate();
    }

    public IEnumerable<Vec2I> GetExistingCellsAroundPositionThatHaveEntityWith<TComponent>(Vec2I position)
        where TComponent : struct
    {
        List<Vec2I> result = new(8);
        Vec2I[] offsets = UTL.GetAllOffsets();
        
        for (int i = 0; i < offsets.Length; i++)
        {
            var cellPosition = position + offsets[i];
            if (Contains(cellPosition) && CellHasEntityWith<TComponent>(position))
            {
                result.Add(cellPosition);
            }
        }

        return result;
    }
    
    public bool Contains(Vec2I pos)
    {
        return pos.X >= 0 && pos.Y >= 0 && pos.X < LayerWidth && pos.Y < LayerHeight;
    }
    
    public ref Cell GetCell(Vec2I pos)
    {
        ref Cell cell = ref world.Pool<Cell>().Get(cells[V2IToIndex(pos)]);
        return ref cell;
    }

    public IEnumerable<int> GetEntities(Vec2I pos)
    {
        return GetCell(pos).Entities;
    }

    public bool CellHasEntityWith<TComponent>(Vec2I position) 
        where TComponent : struct
    {
        if (!Contains(position))
            throw new Exception("No cell with position {position] found in map");

        EntityComponentPool<TComponent> pool = world.Pool<TComponent>();
        
        foreach (int entityId in GetEntities(position))
            if (pool.Has(entityId)) return true;

        return false;
    }
    
    public IEnumerable<Vec2I> GetExistingCellsAroundPositionThatDoesNotHaveEntitiesWith<TComponent>(int x, int y)
        where TComponent : struct
        => GetExistingCellsAroundPositionThatDoesNotHaveEntitiesWith<TComponent>(new(x,y));
    
    public IEnumerable<Vec2I> GetExistingCellsAroundPositionThatDoesNotHaveEntitiesWith<TComponent>(Vec2I position)
        where TComponent : struct
    {
        List<Vec2I> result = new(8);
        
        Vec2I[] offsets = UTL.GetAllOffsets();
        for (int i = 0; i < offsets.Length; i++)
        {
            var cellPosition = position + offsets[i];
            if (Contains(cellPosition) && !CellHasEntityWith<TComponent>(position))
            {
                result.Add(cellPosition);
            }
        }

        return result;
    }
    
    private int CreateNewCellOn(Vec2I position)
    {
        int id = world.New().Id;

        world.Pool<Cell>().New(id, (ref Cell c) =>
        {
            c.Position = position;
            c.Entities = new();
        });
        world.Pool<Position>().New(id, (ref Position p) => p.Set(position.X, position.Y));
        world.Pool<Origin>().New(id, (ref Origin o) => o.Set(0.5f, 0.5f));
        world.Pool<Scale>().New(id, (ref Scale s) => s.Set(1,1));
        
        world.Pool<Selectable>().New(id);
        world.Pool<Hoverable>().New(id);

        return id;
    }

    private Vec2I GetCellPositionForTile(int entityId)
    {
        Position posComponent = world.Pool<Position>().Get(entityId); 
        return new Vec2I((int)Math.Round(posComponent.X), (int)Math.Round(posComponent.Y));
    }
}
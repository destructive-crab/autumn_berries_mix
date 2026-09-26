using destruct.Debug;
using destruct.ecs;
using destruct.scenes;
using Raylib_cs;

namespace mix.code_base;

public sealed class RendererSystem : ISceneSystem, ISceneSystemTick
{
    private readonly ECSWorld world;
    
    private readonly EntityComponentPool<Sprite> texturesPool;
    private readonly EntityComponentPool<Position> positionPool;

    private readonly List<int> entities = new();
    
    public RendererSystem(ECSWorld world)
    {
        this.world = world;
        texturesPool = world.Pool<Sprite>();
        positionPool = world.Pool<Position>();
        
        world.Filter()
            .In<Sprite>()
            .In<Position>()
            .Subscribe(UpdateEntitiesToRender)
            .Evaluate();
    }

    private void UpdateEntitiesToRender(IEnumerable<int> entities)
    {
        this.entities.Clear();
        this.entities.AddRange(entities);
        
        this.entities.Sort((a, b) =>
        {
            var aSprite = world.Pool<Sprite>().Get(a);
            var bSprite = world.Pool<Sprite>().Get(b);
            
            var aOrder = aSprite.DrawOrder;
            var bOrder = bSprite.DrawOrder;
            
            var aY = world.Pool<Position>().Get(a).Y - aSprite.Pivot.Y * (aSprite.Texture.Height/(float)Global.PixelsPerUnit) + aSprite.DrawOffsetInPixels.Y;
            var bY = world.Pool<Position>().Get(b).Y - bSprite.Pivot.Y * (bSprite.Texture.Height/(float)Global.PixelsPerUnit) + bSprite.DrawOffsetInPixels.Y;

            if (aOrder > bOrder)
            {
                return 1;
            }
            
            if (aOrder == bOrder)
            {
                if (aY > bY) return 1;
                if (Math.Abs(aY - bY) <= 0.01) return 0;
                    
                return -1;
            }
            return -1;
        });
    }

    public string DebugName() => "Renderer";

    public void Tick()
    {
        foreach (int entity in entities)
        {
            var sprite = texturesPool.Get(entity);
            
            if (sprite.Hidden) continue;
            
            var position = positionPool.Get(entity);

            Raylib.DrawTexture(
                sprite.Texture, 
                (int)Math.Round(position.X * Global.PixelsPerUnit - sprite.Texture.Width*sprite.Pivot.X + sprite.DrawOffsetInPixels.X), 
                (int)Math.Round(position.Y * Global.PixelsPerUnit - sprite.Texture.Height*sprite.Pivot.Y + sprite.DrawOffsetInPixels.Y), 
                Color.White);
        }
    }
}
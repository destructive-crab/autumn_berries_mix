using System.Numerics;
using destruct;
using destruct.Debug;
using destruct.ecs;
using destruct.scenes;
using Raylib_cs;

namespace mix.code_base;

public sealed class SpriteRendererSystem : ISystem, IWorldRendererSystem
{
    public string DebugName() => nameof(SpriteRendererSystem);
    
    private readonly ECSWorld world;
    
    private readonly EntityComponentPool<Sprite> texturesPool;
    private readonly EntityComponentPool<Position> positionPool;

    private readonly List<int> entities = new();
    
    public SpriteRendererSystem(ECSWorld world)
    {
        this.world = world;
        texturesPool = world.Pool<Sprite>();
        positionPool = world.Pool<Position>();
        
        world.Filter()
            .In<Sprite>()
            .In<Position>()
            .In<Origin>()
            .In<Scale>()
            .Subscribe(UpdateEntitiesToRender)
            .Evaluate();
    }

    private void UpdateEntitiesToRender(IEnumerable<int> entities)
    {
        this.entities.Clear();
        this.entities.AddRange(entities);
        
        this.entities.Sort((a, b) => UTL.CompareSpritesRendererOrder(a, b, world));
    }

    public void RenderWorld()
    {
        foreach (int entity in entities)
        {
            Sprite sprite = texturesPool.Get(entity);

            if (sprite.Fragment.Texture == null)
            {
                //LGR.PW($"Null texture on sprite of {entity}");
                continue;
            }
            if (sprite.Hidden) continue;
            
            Position position = positionPool.Get(entity);
            Vector2 origin = world.Pool<Origin>().Get(entity).ToV2();
            Vector2 scale = world.Pool<Scale>().Get(entity).ToV2();
            
            float rotation = 0;
            if (world.HasIn<Rotation>(entity)) rotation = world.GetFrom<Rotation>(entity).Angle;

            origin = new(origin.X * sprite.Fragment.Rect.Width * scale.X, origin.Y * sprite.Fragment.Rect.Height * scale.Y);
            Rectangle sourceRect = sprite.Fragment.Rect;
            if (sprite.MirroredHorizontal)
                sourceRect.Width *= -1;
            
            if (sprite.MirroredVertical)
                sourceRect.Height *= -1;

            Rectangle destinationRect = new(UTL.UnitsToPixels(position) + sprite.DrawOffsetInPixels, sprite.Fragment.Rect.Width, sprite.Fragment.Rect.Height);
            destinationRect.Width *= world.GetFrom<Scale>(entity).X;
            destinationRect.Height *= world.GetFrom<Scale>(entity).Y;

            Raylib.DrawTexturePro(sprite.Fragment.Texture.Get(), sourceRect, destinationRect, origin, rotation, Color.White);

            if (Raylib.IsKeyDown(KeyboardKey.A))
            {
                var originColor = Color.Red;
                var originRadius = 3 - position.ZZZ;
                if (originRadius == 3) originRadius = 0;
                if (position.ZZZ > 1)
                {
                    originColor = Color.Blue;
                }

                if (position.ZZZ == 1) originColor = Color.Green;

                Raylib.DrawCircle((int)UTL.UnitsToPixels(position).X - (int)world.Pool<Origin>().Get(entity).ToV2().X,
                    (int)UTL.UnitsToPixels(position).Y - (int)world.Pool<Origin>().Get(entity).ToV2().Y, originRadius,
                    originColor);
                Raylib.DrawRectangleLinesEx(new(
                    (int)UTL.UnitsToPixels(position).X + sprite.DrawOffsetInPixels.X - (int)origin.X,
                    (int)UTL.UnitsToPixels(position).Y + sprite.DrawOffsetInPixels.Y - (int)origin.Y,
                    (int)sprite.Fragment.Rect.Width, (int)sprite.Fragment.Rect.Height), 1, originColor);
            }
        }
    }
}
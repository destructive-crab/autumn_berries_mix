using destruct.ecs;
using destruct;
using destruct.gameplay.systems;

namespace destruct.tweens;

public interface ISinAngleComponent
{
    public float Speed { get; set; }
    public float CurrentSinAngle { get; set; }
}


public struct YSinDrawOffsetTween : ISinAngleComponent
{
    public float Speed { get; set; }
    public float CurrentSinAngle { get; set; }
}
 
public struct XSinDrawOffsetTween : ISinAngleComponent
{
    public float Speed { get; set; }
    public float CurrentSinAngle { get; set; }
}

public sealed class DrawOffsetSinTweenSystem : IGameLogicSystem
{
    public string DebugName() => nameof(DrawOffsetSinTweenSystem);

    private IFilter xTweens;
    private IFilter yTweens;
    
    private ECSWorld world;

    public void Inject(ECSWorld world)
    {
        this.world = world;
        
        xTweens = world.Filter()
            .In<DrawOffset>()
            .In<XSinDrawOffsetTween>();
        
        yTweens = world.Filter()
            .In<DrawOffset>()
            .In<YSinDrawOffsetTween>();
    }

    public void Process()
    {
        foreach (int entity in xTweens.Entities)
        {
            ref XSinDrawOffsetTween tween = ref world.GetFrom<XSinDrawOffsetTween>(entity);
        }
    }
}
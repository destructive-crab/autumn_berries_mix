namespace destruct.tweens;

public static class SinAngleUpdateUTL
{
    public static void UpdateAngle<TComponent>(ref TComponent component)
        where TComponent : struct, ISinAngleComponent
    {
        component.CurrentSinAngle += Game.DeltaTime*component.Speed;
        
        if (component.CurrentSinAngle >= 360)
            component.CurrentSinAngle = 0;
    }
}
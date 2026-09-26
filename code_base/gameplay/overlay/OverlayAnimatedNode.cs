using Godot;

namespace destructive_code.Tilemap;

public sealed partial class OverlayAnimatedNode : OverlayNode
{
    [Export] private string ReadyAnimation = "ready";
    
    [Export] private string PointingBeganAnimation = "pointing_began";
    [Export] private string PointingEndedAnimation = "pointing_ended";
    
    [Export] private string SelectedAnimation      = "selected";
    [Export] private string DeselectedAnimation    = "deselected";

    private AnimatedSprite2D animatedSprite;

    public override void _Ready()
    {
        animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        animatedSprite.Play("ready");
    }

    public override void OnCellPointed()
    {
        animatedSprite.Play(PointingBeganAnimation);
    }

    public override void OnCellNotPointed()
    {
        animatedSprite.Play(PointingEndedAnimation);
    }

    public override void OnCellSelected()
    {
        animatedSprite.Play(SelectedAnimation);
    }

    public override void OnCellDeselected()
    {
        animatedSprite.Play(DeselectedAnimation);
    }
}
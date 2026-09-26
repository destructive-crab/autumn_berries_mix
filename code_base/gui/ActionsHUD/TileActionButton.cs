using destruct;
using destruct.Debug;
using Godot;

namespace destructive_code.UI;

public partial class TileActionButton : TextureButton
{
    private TileAction tileAction;
    private ActionsPanel panel;

    public void Bind(ActionsPanel panel, TileAction action)
    {
        this.panel = panel;
        tileAction = action;
        
        TextureNormal   = tileAction.Data.Icon;
        TextureDisabled = tileAction.Data.Icon;
        TextureFocused  = tileAction.Data.Icon;
        TextureHover    = tileAction.Data.Icon;
        TextureNormal   = tileAction.Data.Icon;
        TexturePressed  = tileAction.Data.SelectedIcon;
    }

    public override void _Pressed()
    {
        LGR.PM("PRESSEd");
        panel.SelectAction(tileAction);
    }
}
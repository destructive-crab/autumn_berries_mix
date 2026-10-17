using System.Numerics;
using destruct;
using destruct.Debug;
using Raylib_cs;

public sealed class Cursor : IGMModuleBoot, IGMModuleTick
{
    private Texture2D currentTexture;
    
    private Texture2D defaultTexture;
    private Texture2D pointerTexture;
    private Texture2D pointerPressedTexture;
    private Texture2D moveTexture;

    public Task Boot()
    {
        defaultTexture = Raylib.LoadTexture("assets/cursors/default_cursor.png");
        pointerTexture = Raylib.LoadTexture("assets/cursors/pointer_cursor.png");
        pointerPressedTexture = Raylib.LoadTexture("assets/cursors/pointer_pressed_cursor.png");
        moveTexture = Raylib.LoadTexture("assets/cursors/move_cursor.png");

        Raylib.HideCursor();
        SetToDefault();
        
        return Task.CompletedTask;
    }

    public void SetToDefault()
    {
        currentTexture = defaultTexture;
    }
    
    public void SetToPointer()
    {
        currentTexture = pointerTexture;
    }
    
    public void SetToPressedPointer()
    {
        currentTexture = pointerPressedTexture;
    }
    
    public void SetToMove()
    {
        currentTexture = moveTexture;
    }

    public void Tick()
    {
        Vector2 mp = Raylib.GetMousePosition();
        Raylib.DrawTexturePro(currentTexture, new (0,0,currentTexture.Width, currentTexture.Height), new (mp.X, mp.Y, currentTexture.Width*3, currentTexture.Height*3), new(0,0), 0, Color.White);
    }
}
using System.Numerics;
using System.Text;
using destruct.Debug;
using Raylib_cs;

namespace destruct.imgui;

public struct GUIContainer
{
    public Rect Rect;
    
    public GUIContainer(int x, int y, int w, int h)
    {
        Rect = new(x, y, w, h);
    }
}

public struct ButtonStyle
{
    public Color Idle;
    public Color Pressed;
    public Color Hovered;

    public ButtonStyle(Color idle, Color pressed, Color hovered)
    {
        Idle = idle;
        Pressed = pressed;
        Hovered = hovered;
    }
}

public sealed class GUIStyle
{
    public int BorderTileSize;

    public CachedTexture Border;
    
    //top bottom right left borders
    public Rectangle TBorderTile;
    public Rectangle BBorderTile;
    public Rectangle RBorderTile;
    public Rectangle LBorderTile;
    
    //corners
    public Rectangle TLBorderCornerTile;
    public Rectangle TRBorderCornerTile;
    public Rectangle BRBorderCornerTile;
    public Rectangle BLBorderCornerTile;
    
    //top left and right borders 
    public Rectangle TLBorderTile;
    public Rectangle TRBorderTile;
    
    //bottom left and right borders 
    public Rectangle BLBorderTile;
    public Rectangle BRBorderTile;
    
    //left and top and bottom borders 
    public Rectangle LTBorderTile;
    public Rectangle LBBorderTile;
    
    //right and top and bottom borders 
    public Rectangle RTBorderTile;
    public Rectangle RBBorderTile;
    
    //button style
    public ButtonStyle DefaultButton;

    public GUIStyle ImportBorder(int tileSize, string assetPath)
    {
        BorderTileSize = tileSize;
        Border = Assets.LoadTexture(assetPath);
        int t = tileSize;
        
        TLBorderCornerTile = new(0,   0, t, t);
        TLBorderTile       = new(t,   0, t, t);
        TBorderTile        = new(t*2, 0, t, t);
        TRBorderTile       = new(t*3, 0, t, t);
        TRBorderCornerTile = new(t*4, 0, t, t);
        
        BLBorderCornerTile = new(0,   t*4, t, t);
        BLBorderTile       = new(t,   t*4, t, t);
        BBorderTile        = new(t*2, t*4, t, t);
        BRBorderTile       = new(t*3, t*4, t, t);
        BRBorderCornerTile = new(t*4, t*4, t, t);

        LTBorderTile       = new(t*4, t,   t, t);
        LBorderTile        = new(t*4, t*2, t, t);
        LBBorderTile       = new(t*4, t*3, t, t);
        RTBorderTile       = new(0,   t,   t, t);
        RBorderTile        = new(0,   t*2, t, t);
        RBBorderTile       = new(0,   t*3, t, t);
        
        return this;
    }

    public GUIStyle SetDefaultButtonIdleColor(Color color)
    {
        DefaultButton.Idle = color;
        return this;
    }
    
    public GUIStyle SetDefaultButtonPressedColor(Color color)
    {
        DefaultButton.Pressed = color;
        return this;
    }
    
    public GUIStyle SetDefaultButtonHoveredColor(Color color)
    {
        DefaultButton.Hovered = color;
        return this;
    }
}

public static class deGUI
{
    private static readonly List<GUIContainer> containers = new();
    private static readonly List<Rect> lastDrawnElementOfEachContainer = new();
    
    private static GUIStyle Style;
    
    public static Font Font;

    public static GUIContainer Container() => containers.Last();
    
    /// <returns> rectangle of last drawn element with relative position to current container</returns>
    public static Rect LastRelative() => lastDrawnElementOfEachContainer.Last();
    
    /// <returns> rectangle of last drawn element with global position </returns>
    public static Rect LastGlobal() => 
        new(GetGlobalXFromRelativeToCurrentContainer(LastRelative().X), 
            GetGlobalYFromRelativeToCurrentContainer(LastRelative().Y), 
            LastRelative().W, LastRelative().H);

    public static int RelativeCenterX() => ConW() / 2;
    public static int RelativeCenterY() => ConH() / 2;
    public static int GlobalCenterX() => ConX() + RelativeCenterX();
    public static int GlobalCenterY() => ConY() + RelativeCenterY();

    public static int NextX() => LastRelative().X + LastRelative().W;
    public static int NextY() => LastRelative().Y + LastRelative().H;
    
    public static int CenteredXFor(int w) => ConW() / 2 - w / 2;
    public static int CenteredYFor(int h) => ConH() / 2 - h / 2;
    
    public static int ConX() => Container().Rect.X;
    public static int ConY() => Container().Rect.Y;
    public static int ConW() => Container().Rect.W;
    public static int ConH() => Container().Rect.H;

    public static int GetGlobalXFromRelativeToCurrentContainer(int x) 
        => ConX() + x;
    
    public static int GetGlobalYFromRelativeToCurrentContainer(int y) 
        => ConY() + y;

    private static bool guiWasOverlapped = false;

    public static GUIStyle NewStyle()
    {
        Style = new();
        return Style;
    }
    
    public static void BeginGUI()
    {
        if (containers.Count > 0) throw new Exception("Cannot begin GUI until previous GUI is ended");

        BeginContainer(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight());
    }

    public static void EndGUI()
    {
        containers.Clear();
    }

    public static bool GUIWasOverlapped() 
        => guiWasOverlapped;

    public static void BeginContainerCenter(int width, int height)
        => BeginContainer(CenteredXFor(width), CenteredYFor(height), width, height);
    
    public static void BeginContainer(int containerX, int containerY, int width, int height)
    {
        GUIContainer newContainer = new(containerX, containerY, width, height);

        if (containers.Count > 0)
        {
            var lastContainer = Container();
            newContainer.Rect.X = lastContainer.Rect.X + containerX;
            newContainer.Rect.Y = lastContainer.Rect.Y + containerY;

            if (width == -1) newContainer.Rect.W = ConW();
            else             newContainer.Rect.W = width;
        
            if (height == -1) newContainer.Rect.H = ConH();
            else              newContainer.Rect.H = height;
            
            //we do not check gui overlap when there is no containers yet because 
            //first container is always root container with screen size
            guiWasOverlapped = guiWasOverlapped || IsOverlapped(newContainer.Rect);
        }

        containers.Add(newContainer);
        lastDrawnElementOfEachContainer.Add(new Rect());
    }
    
    public static void EndContainer()
    {
        int index = containers.Count - 1;
        
        containers.RemoveAt(index);
        lastDrawnElementOfEachContainer.RemoveAt(index);
    }

    public static void PushLastDrawnElement(int x, int y, int w, int h)
        => PushLastDrawnElement(new(x, y, w, h));
    
    public static void PushLastDrawnElement(Rect elementRect)
    {
        lastDrawnElementOfEachContainer[^1] = elementRect;
    }

    public static void BeginMarginContainerX(int marginX) 
        => BeginContainerCenter(ConW()-marginX*2, ConH());

    public static void BeginMarginTop(int margin) 
        => BeginContainer(ConX(), ConY()+margin, ConW(), ConH() - margin);
    
    public static void YSpace(int space) => PushLastDrawnElement(new(LastRelative().X, NextY(), ConW(), space));

    public static void Border(Rect globalRect)
    {
        int tileS = Style.BorderTileSize;
        int x = globalRect.X;
        int y = globalRect.Y;
        int w = globalRect.W;
        int h = globalRect.H;

        //drawing top bottom borders
        int wLeft = ConW();
        
        //top
        RaylibDrawTextureRect(Style.Border.Get(), x, y - tileS, Style.TLBorderTile);
        RaylibDrawTextureRect(Style.Border.Get(), x + w - tileS, y - tileS, Style.TRBorderTile);
            
        //bottom
        RaylibDrawTextureRect(Style.Border.Get(), x, y + h, Style.BLBorderTile);
        RaylibDrawTextureRect(Style.Border.Get(), x + w - tileS, y + h, Style.BRBorderTile);
        
        wLeft -= tileS * 2;
        int wTilesCount = (int)Math.Ceiling((float)wLeft / tileS);

        for (int i = 0; i < wTilesCount; i++)
        {
            RaylibDrawTextureRect(Style.Border.Get(), x + tileS + i * tileS, y - tileS, Style.TBorderTile);
        }

        for (int i = 0; i < wTilesCount; i++)
        {
            RaylibDrawTextureRect(Style.Border.Get(), x + tileS + i * tileS, y + h, Style.BBorderTile);
        }
        
        //drawing left right borders
        int hLeft = h;

        //left
        RaylibDrawTextureRect(Style.Border.Get(), x - tileS, y, Style.LTBorderTile);
        RaylibDrawTextureRect(Style.Border.Get(), x - tileS, y + h - tileS, Style.LBBorderTile);
        
        //right
        RaylibDrawTextureRect(Style.Border.Get(), x + w, y, Style.RTBorderTile);
        RaylibDrawTextureRect(Style.Border.Get(), x + w, y + h - tileS, Style.RBBorderTile);
        
        hLeft -= tileS * 2;
        int hTilesCount = (int)Math.Ceiling((float)hLeft / tileS);

        //left
        for (int i = 0; i < hTilesCount; i++)
        {
            RaylibDrawTextureRect(Style.Border.Get(), x - tileS, y + (i+1) * tileS, Style.LBorderTile);
        }

        //right
        for (int i = 0; i < hTilesCount; i++)
        {
            RaylibDrawTextureRect(Style.Border.Get(), x + w, y + (i+1) * tileS, Style.RBorderTile);
        }

        //drawing border corners
        RaylibDrawTextureRect(Style.Border.Get(), x - tileS, y - tileS, Style.TLBorderCornerTile);
        RaylibDrawTextureRect(Style.Border.Get(), x + w, y - tileS, Style.TRBorderCornerTile);
        RaylibDrawTextureRect(Style.Border.Get(), x + w, y + h, Style.BRBorderCornerTile);
        RaylibDrawTextureRect(Style.Border.Get(), x - tileS, y + h, Style.BLBorderCornerTile);
    }
    
    public static void ElementBorder()
    {
        Border(LastGlobal());
        
        var r = LastRelative();
        var t = Style.BorderTileSize;
        
        PushLastDrawnElement(r.X - t, r.Y - t, r.W + t, r.H + t);
    }
    
    public static void ContainerBorder()
    {
        Border(Container().Rect);
    }

    public static bool ButtonWFull(string label, int containerY, int h)
        => Button(label, 0, containerY, ConW(), h, Style.DefaultButton);
    
    public static bool Button(string label, int containerX, int containerY, int w, int h, ButtonStyle style)
    {
        int x = ConX() + containerX;
        int y = ConY() + containerY;
        
        bool overlap = IsOverlapped(new(x,y,w,h));
        bool click = overlap && Raylib.IsMouseButtonReleased(MouseButton.Left);
        bool pressed = Raylib.IsMouseButtonDown(MouseButton.Left);
        
        BeginContainer(containerX, containerY, w, h);
        {
            if (overlap && pressed) FillContainer(style.Pressed);
            else if(overlap)        FillContainer(style.Hovered);
            else                    FillContainer(style.Idle);
            
            int fontSize = h / 2;
            TextCenter(label, fontSize, Color.Black);
        }
        EndContainer();
        
        PushLastDrawnElement(containerX, containerY, w, h);

        return click;
    }
    
    public static void Image(Texture2D t)
    {
        Image(t, 0, 0, ConW(), ConH());
    }

    public static void Image(Texture2D t, int containerX, int containerY, int w, int h)
    {
        Raylib.DrawTexturePro(t, new(0, 0, t.Width, t.Height), new(containerX, containerY, w, h), new(0,0), 0, Color.White);
    }

    private static bool IsOverlapped(Rect rect)
    {
        Vector2 mp = Raylib.GetMousePosition();
        return mp.X >= rect.X && mp.Y >= rect.Y && mp.X <= (rect.X + rect.W) && mp.Y <= (rect.Y + rect.H);
    }

    public static void FillContainer(Color color)
    {
        var c = Container().Rect;
        Raylib.DrawRectangle(c.X, c.Y, c.W, c.H, color);
    }

    public static string WrapText(string text, int width, int fontSize)
    {
        string[] words = text.Split(" ");
        
        StringBuilder resultString = new("");
        StringBuilder currentLine  = new("");

        for (var i = 0; i < words.Length; i++)
        {
            string word = words[i];
            float possibleWidth = Raylib.MeasureTextEx(Font, currentLine.ToString(), (float)fontSize, 1).X + 
                                  Raylib.MeasureTextEx(Font, word, fontSize, 1).X;
            
            if (possibleWidth <= width)
            {
                if(i != 0) currentLine.Append(" ");
                currentLine.Append(word);
            }
            else
            {
                resultString.Append(currentLine);
                if (i != words.Length - 1)
                    resultString.Append('\n');
                
                currentLine.Clear();
            }
        }

        if (currentLine.Length > 0)
        {
            resultString.Append(currentLine);
        }
        
        return resultString.ToString();
    }

    public static void Text(string text, int fontSize, int x, int y, Color color)
    {
        Raylib.DrawTextEx(Font, text, new Vector2(ConX() + x, ConY() + y), fontSize, 1, color);
        PushLastDrawnElement(new Rect(x, y, Raylib.MeasureTextEx(Font, text, fontSize, 1)));
    }

    public static void TextXCenter(string text, int containerY, int fontSize, Color color)
    {
        Text(text, fontSize, CenteredXFor((int)Raylib.MeasureTextEx(Font, text, fontSize, 1).X), containerY, color);
    }
    
    public static void TextCenter(string text, int fontSize, Color color)
    {
        TextXCenter(text, RelativeCenterY() - fontSize / 2, fontSize, color);
    }

    //raylib utils
    private static void RaylibDrawTextureRect(Texture2D t, int x, int y, Rectangle rect)
    {
        Raylib.DrawTexturePro(t, rect, new(x,y,rect.Width, rect.Height), Vector2.Zero, 0, Color.White);
    }
}
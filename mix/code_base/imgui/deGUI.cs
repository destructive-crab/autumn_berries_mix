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

public static class deGUI
{
    private static readonly List<GUIContainer> containers = new();
    
    public static Font Font;

    public static GUIContainer Container() => containers.Last();

    public static int CenterX() => ContainerX() + ContainerW() / 2;
    public static int CenterY() => ContainerY() + ContainerH() / 2;
    
    public static int ContainerX() => Container().Rect.X;
    public static int ContainerY() => Container().Rect.Y;
    public static int ContainerW() => Container().Rect.W;
    public static int ContainerH() => Container().Rect.H;

    private static bool guiWasOverlapped = false;
    
    public static void BeginGUI()
    {
        if (containers.Count > 0) throw new Exception("Cannot begin GUI until previous GUI is ended");
        containers.Add(new GUIContainer(0, 0, Raylib.GetScreenWidth(), Raylib.GetScreenHeight()));
    }

    public static void EndGUI()
    {
        containers.Clear();
    }

    public static bool GUIWasOverlapped() 
        => guiWasOverlapped;

    public static void BeginContainer(int containerX, int containerY, int width, int height)
    {
        GUIContainer newContainer = new(0, 0, 0, 0);

        var lastContainer = Container();
        newContainer.Rect.X = lastContainer.Rect.X + containerX;
        newContainer.Rect.Y = lastContainer.Rect.Y + containerY;

        if (width == -1) newContainer.Rect.W = ContainerW();
        else             newContainer.Rect.W = width;
        
        if (height == -1) newContainer.Rect.H = ContainerH();
        else              newContainer.Rect.H = height;

        containers.Add(newContainer);
        guiWasOverlapped = guiWasOverlapped || IsOverlapped(newContainer.Rect);
    }
    
    public static void EndContainer()
    {
        containers.RemoveAt(containers.Count - 1);
    }

    public static bool Button(string label, int containerX, int containerY, int w, int h)
    {
        int x = ContainerX() + containerX;
        int y = ContainerY() + containerY;
        
        bool overlap = IsOverlapped(new(x,y,w,h));
        bool click = overlap && Raylib.IsMouseButtonReleased(MouseButton.Left);
        bool pressed = Raylib.IsMouseButtonDown(MouseButton.Left);
        
        BeginContainer(containerX, containerY, w, h);
        {
            if (overlap && pressed) Rectangle(Color.Gray);
            else if(overlap)        Rectangle(Color.White);
            else                    Rectangle(Color.Gray);
            
            int fontSize = h / 2;
            TextCenter(label, fontSize, Color.Black);
        }
        EndContainer();

        return click;
    }

    private static bool IsOverlapped(Rect rect)
    {
        Vector2 mp = Raylib.GetMousePosition();
        return mp.X >= rect.X && mp.Y >= rect.Y && mp.X <= (rect.X + rect.W) && mp.Y <= (rect.Y + rect.H);
    }

    public static void Rectangle(Color color)
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
        Raylib.DrawTextEx(Font, text, new(ContainerX() + x, ContainerY()+y), fontSize, 1, color);
    }

    public static void TextCenter(string text, int fontSize, Color color)
    {
        var c = Container().Rect;
        int textWidth = Raylib.MeasureText(text, fontSize);
        Text(text, fontSize, c.W/2 - textWidth/2, c.H/2 - fontSize/2, color);
    }
}
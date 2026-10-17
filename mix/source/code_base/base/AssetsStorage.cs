using AsepriteDotNet.Common;
using destruct.Debug;
using destruct.imgui;
using mix.aseprite;
using mix.gameplay.systems;
using Raylib_cs;
using Rectangle = Raylib_cs.Rectangle;

public struct AssetsPath
{
    public const string AutumnTerrain      = "assets/tiles/autumn_terrain.png";
    public const string PointedCellBorder  = "assets/overlay/cell_pointed.png";
    public const string SelectedCellBorder = "assets/overlay/cell_selected.png";
}

public struct TextureFragment
{
    public CachedTexture Texture;
    public Rectangle Rect;
    
    public TextureFragment(CachedTexture texture, Rectangle rect)
    {
        Texture = texture;
        Rect = rect;
    }

    public static TextureFragment Full(CachedTexture texture)
    {
        return new(texture, new(0, 0, texture.Get().Width, texture.Get().Height));
    }
    
    public static TextureFragment FromZeroTo(CachedTexture texture, int w, int h)
    {
        return new(texture, new(0, 0, w, h));
    }
    
    public static TextureFragment WithRect(CachedTexture texture, int x, int y, int w, int h)
    {
        return new(texture, new(x, y, w, h));
    }
}

public abstract class CachedTexture : IDisposable
{
    public abstract Texture2D Get();

    public abstract void Dispose();
}

public sealed class RaylibCachedTexture : CachedTexture
{
    private readonly Texture2D texture;

    public RaylibCachedTexture(string assetPath)
    {
        texture = Raylib.LoadTexture(assetPath);
    }

    ~RaylibCachedTexture() 
        => Dispose();

    public override Texture2D Get() 
        => texture;

    public override void Dispose() 
        => Raylib.UnloadTexture(texture);
}

public sealed class AsepriteCachedTexture : CachedTexture
{
    private readonly Texture2D texture;
    
    public AsepriteCachedTexture(AsepriteDotNet.Texture texture)
    {
        Image image = Raylib.GenImageColor(texture.Size.Width, texture.Size.Height, Color.Blank);

        for (var i = 0; i < texture.Pixels.Length; i++)
        {
            Rgba32 pixel = texture.Pixels[i];
            Raylib.ImageDrawPixel(ref image, 
                i % texture.Size.Height, 
                i / texture.Size.Width, 
                new(pixel.R, pixel.G, pixel.B, pixel.A));
        }

        this.texture = Raylib.LoadTextureFromImage(image);
    }
    
    public override Texture2D Get() 
        => texture;

    public override void Dispose()
    {
        Raylib.UnloadTexture(texture);
    }
}

public static class Assets
{
    private static readonly Dictionary<string, CachedTexture> cachedTextures = new();
    private static readonly Dictionary<string, AnimationsContainer> cachedAnimationContainers = new();
    private static readonly Dictionary<string, Shader> cachedShaders = new();
    
    public static AnimationsContainer LoadAnimationsContainer(string pathToAseFile)
    {
        if (cachedAnimationContainers.TryGetValue(pathToAseFile, out var cache))
            return cache;

        AnimationsContainer newContainer = new(AsepriteImporter.ImportClips(pathToAseFile));
        cachedAnimationContainers.Add(pathToAseFile, newContainer);
        
        return newContainer;
    }

    public static CachedTexture CreateTextureFromAsepriteTextureAndCache(string path, AsepriteDotNet.Texture texture)
    {
        if (cachedTextures.TryGetValue(path, out var cache)) 
            return cache;

        AsepriteCachedTexture res = new AsepriteCachedTexture(texture);
        cachedTextures.Add(path, res);
        return res;
    }
    
    public static CachedTexture LoadTexture(string path)
    {
        if (cachedTextures.TryGetValue(path, out var cache)) 
            return cache;

        RaylibCachedTexture texture = new RaylibCachedTexture(path);
        cachedTextures.Add(path, texture);
        
        return texture;
    }

    public static Shader LoadShader(string path)
    {
        if (cachedShaders.TryGetValue(path, out var cache))
            return cache;

        Shader shader = Raylib.LoadShader(null, path);
        cachedShaders.Add(path, shader);

        return shader;
    }

    public static void UnloadTexture(string path)
    {
        if (cachedTextures.TryGetValue(path, out var cache))
        {
            cache.Dispose();
            cachedTextures.Remove(path);
        }
    }

    public static void UnloadAll()
    {
        foreach (var path in cachedTextures.Keys)
            UnloadTexture(path );
    }
}
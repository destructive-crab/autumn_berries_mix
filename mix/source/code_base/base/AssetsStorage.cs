using AsepriteDotNet;
using AsepriteDotNet.Common;
using AsepriteDotNet.IO;
using AsepriteDotNet.Processors;
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
    public class CacheContainer<TAsset>
    {
        public delegate TAsset LoadFunction(string path);
        public delegate void UnloadFunction(TAsset path);
        
        private readonly Dictionary<string, TAsset> cache = new();
        
        private LoadFunction load;
        private UnloadFunction unload;

        public CacheContainer(LoadFunction load, UnloadFunction unload)
        {
            this.load = load;
            this.unload = unload;
        }

        public TAsset Load(string path)
        {
            if (cache.TryGetValue(path, out var c))
                return c;
            
            TAsset asset = load.Invoke(path);
            cache.Add(path, asset);
            
            return asset;
        }

        public void UnloadAll()
        {
            foreach (TAsset asset in cache.Values)
                unload?.Invoke(asset);
        }
    }

    public static readonly CacheContainer<CachedTexture>       Textures;
    public static readonly CacheContainer<AnimationsContainer> AnimationContainers;
    public static readonly CacheContainer<Shader>              Shaders;
    public static readonly CacheContainer<Sound>               Sounds;

    private const string AseFileExtensions = ".ase";

    static Assets()
    {
        Textures            = new(LoadRaylibTexture, UnloadRaylibTexture);
        AnimationContainers = new(LoadAnimationsContainer, UnloadAnimationsContainer);
        Shaders             = new(LoadShader, Raylib.UnloadShader);
        Sounds              = new(Raylib.LoadSound, Raylib.UnloadSound);
    }
    
    private static CachedTexture LoadRaylibTexture(string path)
    {
        if (Path.GetExtension(path) == AseFileExtensions)
        {
            return new AsepriteCachedTexture(SpriteSheetProcessor.Process(AsepriteFileLoader.FromFile(path), null).TextureAtlas.Texture);
        }
        
        return new RaylibCachedTexture(path);
    }

    private static void UnloadRaylibTexture(CachedTexture t) 
        => t.Dispose();

    private static AnimationsContainer LoadAnimationsContainer(string path) 
        => new(AsepriteImporter.ImportClips(path));
    private static void UnloadAnimationsContainer(AnimationsContainer c)
        => c.Dispose();

    private static Shader LoadShader(string path) => Raylib.LoadShader(null, path);

    public static void UnloadAll()
    {
        AnimationContainers.UnloadAll();
        Textures.UnloadAll();
        Shaders.UnloadAll();
        Sounds.UnloadAll();
    }
}
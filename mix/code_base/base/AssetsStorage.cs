using Raylib_cs;

public struct AssetsPath
{
    public const string AutumnTerrain      = "assets/tiles/autumn_terrain.png";
    public const string PointedCellBorder  = "assets/overlay/cell_pointed.png";
    public const string SelectedCellBorder = "assets/overlay/cell_selected.png";
}

public static class Assets
{
    private static readonly Dictionary<string, Texture2D> cachedTextures = new();
    
    public static Texture2D LoadTexture(string path)
    {
        if (cachedTextures.TryGetValue(path, out var cache)) return cache;

        Texture2D texture = Raylib.LoadTexture(path);
        cachedTextures.Add(path, texture);
        return texture;
    }

    public static void UnloadTexture(string path)
    {
        if (cachedTextures.TryGetValue(path, out var cache))
        {
            Raylib.UnloadTexture(cache);
            cachedTextures.Remove(path);
        }
    }

    public static void UnloadAll()
    {
        foreach (var path in cachedTextures.Keys)
            UnloadTexture(path );
    }
}
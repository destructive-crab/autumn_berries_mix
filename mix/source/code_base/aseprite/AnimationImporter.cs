using AsepriteDotNet;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
using AsepriteDotNet.IO;
using AsepriteDotNet.Processors;
using destruct.Debug;
using mix.gameplay.systems;

namespace mix.code_base.aseprite;

public static class AnimationImporter
{
    public static AnimationClip[] ImportClips(string assetPath)
    {
        AsepriteFile file = AsepriteFileLoader.FromFile(assetPath);

        LGR.BegB("TAGS");
        foreach (AsepriteTag tag in file.Tags)
        {
            LGR.PM(tag.Name);
        }
        LGR.EnB();
        
        LGR.BegB("FRAMES");
        foreach (var frame in file.Frames)
        {
            LGR.PM(frame.Name + " " + frame.Duration.Milliseconds);
        }
        LGR.EnB();

        LGR.BegB("SPRITE SHEET");
        SpriteSheet sheet = SpriteSheetProcessor.Process(file, null);
        foreach (TextureRegion region in sheet.TextureAtlas.Regions)
        {
            LGR.PM(region.Name + " " + region.OriginalFrameIndex);
        }

        LGR.EnB();

        return [];
    }
}
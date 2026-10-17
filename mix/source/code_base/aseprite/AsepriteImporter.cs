using AsepriteDotNet;
using AsepriteDotNet.Aseprite;
using AsepriteDotNet.Aseprite.Types;
using AsepriteDotNet.IO;
using AsepriteDotNet.Processors;
using mix.gameplay.systems;
using Raylib_cs;

namespace mix.aseprite;

public static class AsepriteImporter
{
    public static AnimationClip[] ImportClips(string assetPath)
    {
        AsepriteFile file = AsepriteFileLoader.FromFile(assetPath);
        SpriteSheet sheet = SpriteSheetProcessor.Process(file, null);

        CachedTexture texture 
            = Assets.Textures.Load(assetPath);

        AnimationClip[] clips = new AnimationClip[file.Tags.Length];

        for (int tagIndex = 0; tagIndex < file.Tags.Length; tagIndex++)
        {
            AsepriteTag tag = file.Tags[tagIndex];
            clips[tagIndex].Name = tag.Name;

            clips[tagIndex].Cycle = tag.Repeat == 0;

            AnimationClipFrame[] frames = new AnimationClipFrame[tag.To-tag.From+1];
            for (int regionIndex = tag.From; regionIndex <= tag.To; regionIndex++)
            {
                AsepriteDotNet.Common.Rectangle rect 
                    = sheet.TextureAtlas.Regions[regionIndex].Bounds;
                
                frames[regionIndex-tag.From].FrameDurationMS = file.Frames[regionIndex].Duration.Milliseconds;

                frames[regionIndex - tag.From].Fragment =
                    new(texture, new Rectangle(rect.X, rect.Y, rect.Width, rect.Height));
            }

            clips[tagIndex].Frames = frames;
        }

        return clips;
    }
}
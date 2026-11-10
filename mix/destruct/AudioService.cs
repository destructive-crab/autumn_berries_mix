using Raylib_cs;

public class AudioService
{
    private readonly Dictionary<string, Music> currentMusic = new();
    private readonly Dictionary<string, Sound> currentSounds = new();
    
    public void PlayMusic(string assetPath, int volume, bool loop = true)
    {
        Assets.LoadMusic(assetPath, out var music);
        Raylib.SetMusicVolume(music, volume/100f);
        Raylib.PlayMusicStream(music);
        
        currentMusic.Add(assetPath, music);
    }

    public void StopMusic(string assetPath)
    {
        Raylib.StopMusicStream(currentMusic[assetPath]);
        currentMusic.Remove(assetPath);
    }

    public sealed class SoundLoopController
    {
        public bool Loop = true;

        public void Stop()
        {
            Loop = false;
        }
    }

    public void PlaySound(string assetPath, float volume, float pitch)
    {
        Sound sound = Assets.Sounds.Load(assetPath);
        Raylib.SetSoundVolume(sound, volume);
        Raylib.SetSoundPitch(sound, pitch);
        
        Raylib.PlaySound(sound);
    }
    
    public async Task PlaySoundLoop(string assetPath, float volume, float pitch, SoundLoopController loop)
    {
        Sound sound = Assets.Sounds.Load(assetPath);
        int soundsDurationSeconds = (int)(sound.FrameCount / sound.Stream.SampleRate * 1000);
        
        Raylib.SetSoundVolume(sound, volume);
        Raylib.SetSoundPitch(sound, pitch);
        
        Raylib.PlaySound(sound);
        
        while (loop.Loop)
        {
            Raylib.PlaySound(sound);
            int seconds = 0;
            while (seconds < soundsDurationSeconds && loop.Loop)
            {
                await Task.Delay(1000);
                seconds++;
            }
        }
    }
    
    public void UpdateAudio()
    {
        foreach (Music music in currentMusic.Values)
        {
            Raylib.UpdateMusicStream(music);
        }
    }
}
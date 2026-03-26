namespace RPG.Core
{
    public interface IAudioManager
    {
        void PlayMusic(string name);
        void PlaySFX(string name);
        void StopMusic();
        void SetMusicVolume(float volume);
        void SetSFXVolume(float volume);
    }
}
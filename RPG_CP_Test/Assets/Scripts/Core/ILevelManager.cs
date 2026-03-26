using RPG.Level;

namespace RPG.Core
{
    public interface ILevelManager
    {
        void LoadLevel(int levelIndex);
        void LoadNextLevel();
        void ReloadCurrentLevel();
        LevelData CurrentLevel { get; }
        int CurrentLevelIndex { get; }
    }
}
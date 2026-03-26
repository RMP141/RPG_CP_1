namespace RPG.Core
{
    public interface IGameManager
    {
        void StartGame();
        void PauseGame();
        void ResumeGame();
        void GameOver();
        void RestartGame();
        bool IsGamePaused { get; }
    }
}
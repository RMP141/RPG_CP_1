using RPG.DI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG.Core
{
    public class GameManager : IGameManager
    {
        private bool isPaused = false;
        public bool IsGamePaused => isPaused;

        public void StartGame()
        {
            Time.timeScale = 1;
            isPaused = false;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void GameOver()
        {
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            var uiManager = DIContainer.Instance.Resolve<IUIManager>();
            uiManager?.ShowGameOver();
        }

        public void RestartGame()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
using UnityEngine;
using UnityEngine.SceneManagement;
using RPG.Core;
using RPG.DI;

namespace RPG.UI.Menus
{
    public class GameOverMenu : MonoBehaviour
    {
        [SerializeField] private GameObject gameOverPanel;
        [SerializeField] private UnityEngine.UI.Button restartButton;
        [SerializeField] private UnityEngine.UI.Button mainMenuButton;

        private IGameManager gameManager;

        private void Start()
        {
            gameManager = DIContainer.Instance.Resolve<IGameManager>();

            if (gameOverPanel != null)
                gameOverPanel.SetActive(false);

            SetupButtons();
        }

        private void SetupButtons()
        {
            if (restartButton != null)
                restartButton.onClick.AddListener(RestartGame);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);
        }

        public void ShowGameOver()
        {
            if (gameOverPanel != null)
                gameOverPanel.SetActive(true);

            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void RestartGame()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void ReturnToMainMenu()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("MainMenu");
        }

        private void OnDestroy()
        {
            if (restartButton != null) restartButton.onClick.RemoveAllListeners();
            if (mainMenuButton != null) mainMenuButton.onClick.RemoveAllListeners();
        }
    }
}
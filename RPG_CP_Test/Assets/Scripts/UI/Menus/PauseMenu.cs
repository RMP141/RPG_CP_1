using UnityEngine;
using UnityEngine.SceneManagement;
using RPG.Core;
using RPG.DI;

namespace RPG.UI.Menus
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        [SerializeField] private UnityEngine.UI.Button resumeButton;
        [SerializeField] private UnityEngine.UI.Button saveButton;
        [SerializeField] private UnityEngine.UI.Button mainMenuButton;
        [SerializeField] private UnityEngine.UI.Button quitButton;

        private bool isPaused = false;
        private IGameManager gameManager;
        private ISaveSystem saveSystem;

        private void Start()
        {
            gameManager = DIContainer.Instance.Resolve<IGameManager>();
            saveSystem = DIContainer.Instance.Resolve<ISaveSystem>();

            if (pausePanel != null)
                pausePanel.SetActive(false);

            SetupButtons();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (isPaused)
                    Resume();
                else
                    Pause();
            }
        }

        private void SetupButtons()
        {
            if (resumeButton != null)
                resumeButton.onClick.AddListener(Resume);

            if (saveButton != null)
                saveButton.onClick.AddListener(SaveGame);

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(ReturnToMainMenu);

            if (quitButton != null)
                quitButton.onClick.AddListener(QuitGame);
        }

        public void Pause()
        {
            isPaused = true;
            if (pausePanel != null) pausePanel.SetActive(true);
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        public void Resume()
        {
            isPaused = false;
            if (pausePanel != null) pausePanel.SetActive(false);
            Time.timeScale = 1;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void SaveGame()
        {
            saveSystem?.SaveGame(0);
            Debug.Log("Game saved!");

            // Показываем уведомление
            var uiManager = DIContainer.Instance.Resolve<IUIManager>();
            uiManager?.ShowFloatingText("Game Saved!", Vector3.zero);
        }

        private void ReturnToMainMenu()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("MainMenu");
        }

        private void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }

        private void OnDestroy()
        {
            if (resumeButton != null) resumeButton.onClick.RemoveAllListeners();
            if (saveButton != null) saveButton.onClick.RemoveAllListeners();
            if (mainMenuButton != null) mainMenuButton.onClick.RemoveAllListeners();
            if (quitButton != null) quitButton.onClick.RemoveAllListeners();
        }
    }
}
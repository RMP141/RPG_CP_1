using RPG.Core;
using RPG.DI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG.UI.Menus
{
    public class PauseMenu : MonoBehaviour
    {
        [SerializeField] private GameObject pausePanel;
        private bool isPaused = false;

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

        public void Pause()
        {
            isPaused = true;
            pausePanel.SetActive(true);
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Time.timeScale = 0;
        }

        public void Resume()
        {
            isPaused = false;
            pausePanel.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            Time.timeScale = 1;
        }

        public void SaveGame()
        {
            var saveSystem = DIContainer.Instance.Resolve<ISaveSystem>();
            saveSystem?.SaveGame(0);
        }

        public void ReturnToMainMenu()
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("MainMenu");
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }
    }
}
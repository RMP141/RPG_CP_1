using RPG.Core;
using RPG.DI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG.UI.Menus
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject levelSelectPanel;

        private void Start()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            ShowMainMenu();
        }

        public void StartGame()
        {
            var levelManager = DIContainer.Instance.Resolve<ILevelManager>();
            levelManager?.LoadLevel(1);
        }

        public void ContinueGame()
        {
            var saveSystem = DIContainer.Instance.Resolve<ISaveSystem>();
            if (saveSystem.HasSave(0))
                saveSystem.LoadGame(0);
            else
                StartGame();
        }

        public void ShowMainMenu()
        {
            mainPanel.SetActive(true);
            settingsPanel.SetActive(false);
            levelSelectPanel.SetActive(false);
        }

        public void ShowSettings()
        {
            mainPanel.SetActive(false);
            settingsPanel.SetActive(true);
            levelSelectPanel.SetActive(false);
        }

        public void ShowLevelSelect()
        {
            mainPanel.SetActive(false);
            settingsPanel.SetActive(false);
            levelSelectPanel.SetActive(true);
        }

        public void LoadLevel(int levelIndex)
        {
            var levelManager = DIContainer.Instance.Resolve<ILevelManager>();
            levelManager?.LoadLevel(levelIndex);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }

        public void SetMusicVolume(float volume)
        {
            var audioManager = DIContainer.Instance.Resolve<IAudioManager>();
            audioManager?.SetMusicVolume(volume);
        }

        public void SetSFXVolume(float volume)
        {
            var audioManager = DIContainer.Instance.Resolve<IAudioManager>();
            audioManager?.SetSFXVolume(volume);
        }
    }
}
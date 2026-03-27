using RPG.Core;
using RPG.DI;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace RPG.UI.Menus
{
    public class MainMenu : MonoBehaviour
    {
        [Header("Panels")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject levelSelectPanel;
        [SerializeField] private GameObject loadingPanel;

        [Header("Buttons - Main Panel")]
        [SerializeField] private Button startButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        [Header("Buttons - Settings Panel")]
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private TextMeshProUGUI musicValueText;
        [SerializeField] private TextMeshProUGUI sfxValueText;
        [SerializeField] private Button settingsBackButton;

        [Header("Buttons - Level Select Panel")]
        [SerializeField] private Button level1Button;
        [SerializeField] private Button level2Button;
        [SerializeField] private Button level3Button;
        [SerializeField] private Button levelSelectBackButton;

        [Header("Loading")]
        [SerializeField] private Slider loadingProgressBar;
        [SerializeField] private TextMeshProUGUI loadingText;

        [Header("Audio")]
        [SerializeField] private AudioClip buttonClickSound;
        [SerializeField] private AudioClip hoverSound;

        private IAudioManager audioManager;
        private IGameManager gameManager;
        private ILevelManager levelManager;
        private ISaveSystem saveSystem;

        private void Awake()
        {
            // Получаем сервисы через DI
            audioManager = DIContainer.Instance.Resolve<IAudioManager>();
            gameManager = DIContainer.Instance.Resolve<IGameManager>();
            levelManager = DIContainer.Instance.Resolve<ILevelManager>();
            saveSystem = DIContainer.Instance.Resolve<ISaveSystem>();

            // Загружаем сохраненные настройки
            LoadSettings();
        }

        private void Start()
        {
            SetupButtons();
            ShowMainMenu();

            // Проверяем наличие сохранений
            if (saveSystem != null && saveSystem.HasSave(0))
            {
                continueButton.interactable = true;
            }
            else
            {
                continueButton.interactable = false;
            }

            // Воспроизводим музыку меню
            audioManager?.PlayMusic("MainMenu");
        }

        private void SetupButtons()
        {
            // Main Panel Buttons
            if (startButton != null)
            {
                startButton.onClick.AddListener(OnStartGame);
                AddButtonSound(startButton);
            }

            if (continueButton != null)
            {
                continueButton.onClick.AddListener(OnContinueGame);
                AddButtonSound(continueButton);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(ShowSettings);
                AddButtonSound(settingsButton);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitGame);
                AddButtonSound(quitButton);
            }

            // Settings Panel Buttons
            if (musicSlider != null)
            {
                musicSlider.onValueChanged.AddListener(OnMusicVolumeChanged);
            }

            if (sfxSlider != null)
            {
                sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
            }

            if (settingsBackButton != null)
            {
                settingsBackButton.onClick.AddListener(ShowMainMenu);
                AddButtonSound(settingsBackButton);
            }

            // Level Select Panel Buttons
            if (level1Button != null)
            {
                level1Button.onClick.AddListener(() => LoadLevel(1));
                AddButtonSound(level1Button);
            }

            if (level2Button != null)
            {
                level2Button.onClick.AddListener(() => LoadLevel(2));
                AddButtonSound(level2Button);
            }

            if (level3Button != null)
            {
                level3Button.onClick.AddListener(() => LoadLevel(3));
                AddButtonSound(level3Button);
            }

            if (levelSelectBackButton != null)
            {
                levelSelectBackButton.onClick.AddListener(ShowMainMenu);
                AddButtonSound(levelSelectBackButton);
            }
        }

        private void AddButtonSound(Button button)
        {
            // Добавляем звук при нажатии
            button.onClick.AddListener(() =>
            {
                audioManager?.PlaySFX("ButtonClick");
            });

            // Добавляем звук при наведении
            var trigger = button.gameObject.AddComponent<EventTrigger>();
            var entry = new EventTrigger.Entry();
            entry.eventID = EventTriggerType.PointerEnter;
            entry.callback.AddListener((data) =>
            {
                audioManager?.PlaySFX("ButtonHover");
            });
            trigger.triggers.Add(entry);
        }

        private void OnStartGame()
        {
            audioManager?.PlaySFX("ButtonClick");
            ShowLevelSelect();
        }

        private void OnContinueGame()
        {
            audioManager?.PlaySFX("ButtonClick");

            if (saveSystem != null && saveSystem.HasSave(0))
            {
                ShowLoading();
                saveSystem.LoadGame(0);
            }
        }

        private void OnQuitGame()
        {
            audioManager?.PlaySFX("ButtonClick");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
        }

        private void OnMusicVolumeChanged(float value)
        {
            audioManager?.SetMusicVolume(value);
            if (musicValueText != null)
            {
                musicValueText.text = $"{Mathf.RoundToInt(value * 100)}%";
            }
            PlayerPrefs.SetFloat("MusicVolume", value);
        }

        private void OnSFXVolumeChanged(float value)
        {
            audioManager?.SetSFXVolume(value);
            if (sfxValueText != null)
            {
                sfxValueText.text = $"{Mathf.RoundToInt(value * 100)}%";
            }
            PlayerPrefs.SetFloat("SFXVolume", value);
        }

        private void LoadSettings()
        {
            float musicVol = PlayerPrefs.GetFloat("MusicVolume", 0.7f);
            float sfxVol = PlayerPrefs.GetFloat("SFXVolume", 0.8f);

            if (musicSlider != null) musicSlider.value = musicVol;
            if (sfxSlider != null) sfxSlider.value = sfxVol;

            audioManager?.SetMusicVolume(musicVol);
            audioManager?.SetSFXVolume(sfxVol);
        }

        private void LoadLevel(int levelIndex)
        {
            audioManager?.PlaySFX("ButtonClick");
            ShowLoading();

            if (levelManager != null)
            {
                levelManager.LoadLevel(levelIndex);
            }
            else
            {
                SceneManager.LoadScene("GameScene");
            }
        }

        private void ShowLoading()
        {
            if (loadingPanel != null)
            {
                loadingPanel.SetActive(true);
                StartCoroutine(AnimateLoading());
            }
        }

        private System.Collections.IEnumerator AnimateLoading()
        {
            float progress = 0;
            while (progress < 1)
            {
                progress += Time.deltaTime * 2f;
                if (loadingProgressBar != null)
                    loadingProgressBar.value = progress;
                if (loadingText != null)
                    loadingText.text = $"Loading... {Mathf.RoundToInt(progress * 100)}%";
                yield return null;
            }
        }

        public void ShowMainMenu()
        {
            if (mainPanel != null) mainPanel.SetActive(true);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
            if (loadingPanel != null) loadingPanel.SetActive(false);
        }

        public void ShowSettings()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(true);
            if (levelSelectPanel != null) levelSelectPanel.SetActive(false);
        }

        public void ShowLevelSelect()
        {
            if (mainPanel != null) mainPanel.SetActive(false);
            if (settingsPanel != null) settingsPanel.SetActive(false);
            if (levelSelectPanel != null) levelSelectPanel.SetActive(true);
        }

        private void OnDestroy()
        {
            // Отписываемся от событий
            if (startButton != null) startButton.onClick.RemoveAllListeners();
            if (continueButton != null) continueButton.onClick.RemoveAllListeners();
            if (settingsButton != null) settingsButton.onClick.RemoveAllListeners();
            if (quitButton != null) quitButton.onClick.RemoveAllListeners();
            if (settingsBackButton != null) settingsBackButton.onClick.RemoveAllListeners();
            if (level1Button != null) level1Button.onClick.RemoveAllListeners();
            if (level2Button != null) level2Button.onClick.RemoveAllListeners();
            if (level3Button != null) level3Button.onClick.RemoveAllListeners();
            if (levelSelectBackButton != null) levelSelectBackButton.onClick.RemoveAllListeners();
        }
    }
}
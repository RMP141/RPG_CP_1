using RPG.Core;
using RPG.DI;
using RPG.Level;
using RPG.Quests;
using RPG.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RPG.Bootstrapper
{
    [DefaultExecutionOrder(-100)]
    public class GameBootstrapper : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
            GameObject bootstrapObject = new GameObject("GameBootstrapper");
            DontDestroyOnLoad(bootstrapObject);

            var diContainer = bootstrapObject.AddComponent<DIContainer>();

            RegisterServices(diContainer);

            SceneManager.LoadScene("MainMenu");
            Debug.Log("[GameBootstrapper] Game initialized");
        }

        private static void RegisterServices(DIContainer container)
        {
            container.Register<IGameManager>(new GameManager());
            container.Register<ILevelManager>(new LevelManager());
            container.Register<IQuestManager>(new QuestManager());
            container.Register<IAudioManager>(new AudioManager());
            container.Register<ISaveSystem>(new SaveSystem());
            container.Register<IUIManager>(new UIManager());
        }
    }
}
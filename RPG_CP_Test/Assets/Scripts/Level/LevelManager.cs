using RPG.Core;
using RPG.DI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using RPG.Quests;

namespace RPG.Level
{
    public class LevelManager : ILevelManager
    {
        private LevelData currentLevel;
        private int currentLevelIndex = 0;
        private List<GameObject> spawnedObjects = new List<GameObject>();

        public LevelData CurrentLevel => currentLevel;
        public int CurrentLevelIndex => currentLevelIndex;

        public void LoadLevel(int levelIndex)
        {
            currentLevelIndex = levelIndex;
            ClearLevel();

            currentLevel = Resources.Load<LevelData>($"Levels/Level_{levelIndex}");

            if (currentLevel == null)
            {
                Debug.LogError($"Level {levelIndex} not found!");
                return;
            }

            SceneManager.LoadScene("GameScene");
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SetupEnvironment();
            SpawnLevelObjects();
        }

        private void SetupEnvironment()
        {
            if (currentLevel.skyboxMaterial != null)
            {
                RenderSettings.skybox = currentLevel.skyboxMaterial;
            }

            RenderSettings.fogColor = currentLevel.fogColor;
            RenderSettings.fogDensity = currentLevel.fogDensity;

            if (currentLevel.sunLight != null)
            {
                var sun = Object.Instantiate(currentLevel.sunLight);
                sun.transform.SetParent(null);
            }
        }

        private void SpawnLevelObjects()
        {
            foreach (var enemyData in currentLevel.enemies)
            {
                for (int i = 0; i < enemyData.count; i++)
                {
                    Vector3 offset = Random.insideUnitSphere * enemyData.spawnRadius;
                    offset.y = 0;
                    GameObject enemy = Object.Instantiate(enemyData.enemyPrefab,
                        enemyData.position + offset, Quaternion.Euler(enemyData.rotation));
                    spawnedObjects.Add(enemy);
                }
            }

            foreach (var itemData in currentLevel.items)
            {
                GameObject item = Object.Instantiate(itemData.itemPrefab,
                    itemData.position, Quaternion.identity);
                spawnedObjects.Add(item);
            }

            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = currentLevel.playerStartPosition;
                player.transform.rotation = Quaternion.Euler(currentLevel.playerStartRotation);
            }

            var questManager = DIContainer.Instance.Resolve<IQuestManager>();
            foreach (var questData in currentLevel.quests)
            {
                var quest = new Quest
                {
                    id = questData.id,
                    name = questData.name,
                    description = questData.description,
                    type = questData.type,
                    requiredAmount = questData.requiredAmount,
                    reward = questData.reward
                };
                questManager.AddQuest(quest);
            }
        }

        public void LoadNextLevel()
        {
            LoadLevel(currentLevelIndex + 1);
        }

        public void ReloadCurrentLevel()
        {
            LoadLevel(currentLevelIndex);
        }

        private void ClearLevel()
        {
            foreach (var obj in spawnedObjects)
            {
                if (obj != null) Object.Destroy(obj);
            }
            spawnedObjects.Clear();
        }
    }
}
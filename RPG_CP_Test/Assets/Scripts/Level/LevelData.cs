using UnityEngine;
using System.Collections.Generic;
using RPG.Quests;

namespace RPG.Level
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "RPG/3D/LevelData")]
    public class LevelData : ScriptableObject
    {
        public int levelIndex;
        public string levelName;
        public GameObject levelPrefab;
        public Material skyboxMaterial;
        public Vector3 playerStartPosition;
        public Vector3 playerStartRotation;
        public List<EnemySpawnData> enemies;
        public List<ItemSpawnData> items;
        public List<QuestData> quests;
        public Light sunLight;
        public Color fogColor = new Color(0.5f, 0.5f, 0.5f);
        public float fogDensity = 0.02f;
    }

    [System.Serializable]
    public class EnemySpawnData
    {
        public GameObject enemyPrefab;
        public Vector3 position;
        public Vector3 rotation;
        public int count = 1;
        public float spawnRadius = 5f;
    }

    [System.Serializable]
    public class ItemSpawnData
    {
        public GameObject itemPrefab;
        public Vector3 position;
        public int quantity = 1;
    }

    [System.Serializable]
    public class QuestData
    {
        public int id;
        public string name;
        public string description;
        public QuestType type;
        public int requiredAmount;
        public QuestReward reward;
    }
}
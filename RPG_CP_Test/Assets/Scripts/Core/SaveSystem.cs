using RPG.DI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using RPG.Player;
using RPG.Items;

namespace RPG.Core
{
    [Serializable]
    public class SaveData
    {
        public int level;
        public int health;
        public int mana;
        public int experience;
        public int gold;
        public Vector3 position;
        public int currentLevelIndex;
        public SerializableDictionary<int, int> questProgress;
        public SerializableDictionary<string, int> inventory;
    }

    [Serializable]
    public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
    {
        [SerializeField] private List<TKey> keys = new List<TKey>();
        [SerializeField] private List<TValue> values = new List<TValue>();

        public void OnBeforeSerialize()
        {
            keys.Clear();
            values.Clear();
            foreach (var pair in this)
            {
                keys.Add(pair.Key);
                values.Add(pair.Value);
            }
        }

        public void OnAfterDeserialize()
        {
            Clear();
            for (int i = 0; i < keys.Count; i++)
            {
                Add(keys[i], values[i]);
            }
        }
    }

    public class SaveSystem : ISaveSystem
    {
        private string GetSavePath(int slot) => Path.Combine(Application.persistentDataPath, $"save_{slot}.dat");

        public void SaveGame(int slot)
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            var stats = player?.GetComponent<PlayerStats>();
            var inventory = Inventory.Instance;
            var questManager = DIContainer.Instance.Resolve<IQuestManager>();

            SaveData data = new SaveData
            {
                level = stats?.Level ?? 1,
                health = stats?.CurrentHealth ?? 100,
                mana = stats?.CurrentMana ?? 100,
                experience = stats?.Experience ?? 0,
                gold = inventory?.Gold ?? 0,
                position = player?.transform.position ?? Vector3.zero,
                currentLevelIndex = 1
            };

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = File.Create(GetSavePath(slot));
            formatter.Serialize(stream, data);
            stream.Close();

            Debug.Log($"Game saved to slot {slot}");
        }

        public void LoadGame(int slot)
        {
            string path = GetSavePath(slot);
            if (!File.Exists(path)) return;

            BinaryFormatter formatter = new BinaryFormatter();
            FileStream stream = File.Open(path, FileMode.Open);
            SaveData data = formatter.Deserialize(stream) as SaveData;
            stream.Close();

            var levelManager = DIContainer.Instance.Resolve<ILevelManager>();
            levelManager?.LoadLevel(data.currentLevelIndex);

            Debug.Log($"Game loaded from slot {slot}");
        }

        public bool HasSave(int slot) => File.Exists(GetSavePath(slot));
        public void DeleteSave(int slot) { if (HasSave(slot)) File.Delete(GetSavePath(slot)); }
        public SaveData GetSaveData(int slot) => null;
    }
}
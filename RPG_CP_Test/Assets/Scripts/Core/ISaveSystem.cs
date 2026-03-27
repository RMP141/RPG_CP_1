// Scripts/Core/ISaveSystem.cs
using System;
using System.Collections.Generic;

namespace RPG.Core
{
    public interface ISaveSystem
    {
        void SaveGame(int slot);
        void LoadGame(int slot);
        bool HasSave(int slot);
        void DeleteSave(int slot);
        SaveData GetSaveData(int slot);
    }

    [Serializable]
    public class SaveData
    {
        // Player Stats
        public int level;
        public int health;
        public int mana;
        public int experience;
        public int gold;

        // Player Position
        public float playerPosX;
        public float playerPosY;
        public float playerPosZ;

        // Player Rotation
        public float playerRotX;
        public float playerRotY;
        public float playerRotZ;

        // Level
        public int currentLevelIndex;

        // Quests
        public Dictionary<int, int> questProgress;

        // Inventory
        public Dictionary<string, int> inventory;

        public SaveData()
        {
            questProgress = new Dictionary<int, int>();
            inventory = new Dictionary<string, int>();
        }
    }
}
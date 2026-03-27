// Scripts/Core/SaveSystem.cs
using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using RPG.DI;
using RPG.Player;
using RPG.Items;
using RPG.Quests;

namespace RPG.Core
{
    public class SaveSystem : ISaveSystem
    {
        private string GetSavePath(int slot)
        {
            return Path.Combine(Application.persistentDataPath, $"save_{slot}.dat");
        }

        public void SaveGame(int slot)
        {
            try
            {
                // Находим игрока
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player == null)
                {
                    Debug.LogError("Cannot save: Player not found!");
                    return;
                }

                // Получаем компоненты
                PlayerStats stats = player.GetComponent<PlayerStats>();
                if (stats == null)
                {
                    Debug.LogError("Cannot save: PlayerStats not found!");
                    return;
                }

                Inventory inventory = Inventory.Instance;
                ILevelManager levelManager = DIContainer.Instance.Resolve<ILevelManager>();
                IQuestManager questManager = DIContainer.Instance.Resolve<IQuestManager>();

                // Создаем данные для сохранения
                SaveData data = new SaveData();
                data.level = stats.Level;
                data.health = stats.CurrentHealth;
                data.mana = stats.CurrentMana;
                data.experience = stats.Experience;
                data.gold = inventory != null ? inventory.Gold : 0;

                // Сохраняем позицию
                data.playerPosX = player.transform.position.x;
                data.playerPosY = player.transform.position.y;
                data.playerPosZ = player.transform.position.z;

                // Сохраняем поворот
                data.playerRotX = player.transform.eulerAngles.x;
                data.playerRotY = player.transform.eulerAngles.y;
                data.playerRotZ = player.transform.eulerAngles.z;

                data.currentLevelIndex = levelManager != null ? levelManager.CurrentLevelIndex : 1;

                // Сохраняем прогресс квестов
                if (questManager != null)
                {
                    List<Quest> activeQuests = questManager.GetActiveQuests();
                    foreach (Quest quest in activeQuests)
                    {
                        data.questProgress[quest.id] = quest.currentAmount;
                    }
                }

                // Сохраняем инвентарь
                if (inventory != null)
                {
                    var allItems = inventory.GetAllItems();
                    foreach (var item in allItems)
                    {
                        data.inventory[item.Key] = item.Value;
                    }
                }

                // Сериализуем
                BinaryFormatter formatter = new BinaryFormatter();
                string path = GetSavePath(slot);

                using (FileStream stream = File.Create(path))
                {
                    formatter.Serialize(stream, data);
                }

                Debug.Log($"Game saved successfully to slot {slot}");
            }
            catch (Exception e)
            {
                Debug.LogError($"Save failed: {e.Message}\n{e.StackTrace}");
            }
        }

        public void LoadGame(int slot)
        {
            try
            {
                string path = GetSavePath(slot);
                if (!File.Exists(path))
                {
                    Debug.LogWarning($"No save file found in slot {slot}");
                    return;
                }

                BinaryFormatter formatter = new BinaryFormatter();
                SaveData data;

                using (FileStream stream = File.Open(path, FileMode.Open))
                {
                    data = formatter.Deserialize(stream) as SaveData;
                }

                if (data == null)
                {
                    Debug.LogError("Failed to deserialize save data!");
                    return;
                }

                Debug.Log($"Loading game from slot {slot}");

                // Загружаем уровень
                ILevelManager levelManager = DIContainer.Instance.Resolve<ILevelManager>();
                if (levelManager != null)
                {
                    levelManager.LoadLevel(data.currentLevelIndex);
                }

                // Восстанавливаем данные после загрузки сцены
                GameObject loaderGO = new GameObject("GameLoader");
                GameLoader loader = loaderGO.AddComponent<GameLoader>();
                loader.StartCoroutine(loader.LoadAfterScene(data));
            }
            catch (Exception e)
            {
                Debug.LogError($"Load failed: {e.Message}\n{e.StackTrace}");
            }
        }

        public bool HasSave(int slot)
        {
            return File.Exists(GetSavePath(slot));
        }

        public void DeleteSave(int slot)
        {
            string path = GetSavePath(slot);
            if (File.Exists(path))
            {
                File.Delete(path);
                Debug.Log($"Save slot {slot} deleted");
            }
        }

        public SaveData GetSaveData(int slot)
        {
            string path = GetSavePath(slot);
            if (!File.Exists(path)) return null;

            try
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream stream = File.Open(path, FileMode.Open))
                {
                    return formatter.Deserialize(stream) as SaveData;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Failed to read save data: {e.Message}");
                return null;
            }
        }
    }

    // Вспомогательный класс для загрузки
    public class GameLoader : MonoBehaviour
    {
        public System.Collections.IEnumerator LoadAfterScene(SaveData data)
        {
            yield return null;
            yield return null;

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                // Восстанавливаем позицию
                Vector3 savedPosition = new Vector3(data.playerPosX, data.playerPosY, data.playerPosZ);
                player.transform.position = savedPosition;

                // Восстанавливаем поворот
                Vector3 savedRotation = new Vector3(data.playerRotX, data.playerRotY, data.playerRotZ);
                player.transform.eulerAngles = savedRotation;

                // Восстанавливаем статы
                PlayerStats stats = player.GetComponent<PlayerStats>();
                if (stats != null)
                {
                    stats.SetExperience(data.experience);

                    int healthDiff = data.health - stats.CurrentHealth;
                    if (healthDiff > 0) stats.Heal(healthDiff);
                    else if (healthDiff < 0) stats.TakeDamage(-healthDiff);

                    int manaDiff = data.mana - stats.CurrentMana;
                    if (manaDiff > 0) stats.RestoreMana(manaDiff);
                    else if (manaDiff < 0) stats.UseMana(-manaDiff);
                }
            }

            // Восстанавливаем инвентарь
            Inventory inventory = Inventory.Instance;
            if (inventory != null && data.inventory != null)
            {
                inventory.ClearInventory();
                foreach (var item in data.inventory)
                {
                    inventory.AddItem(item.Key, item.Value);
                }

                int goldDiff = data.gold - inventory.Gold;
                if (goldDiff > 0) inventory.AddGold(goldDiff);
                else if (goldDiff < 0) inventory.RemoveGold(-goldDiff);
            }

            // Восстанавливаем квесты
            IQuestManager questManager = DIContainer.Instance.Resolve<IQuestManager>();
            if (questManager != null && data.questProgress != null)
            {
                foreach (var questProgress in data.questProgress)
                {
                    questManager.UpdateQuestById(questProgress.Key, questProgress.Value);
                }
            }

            Debug.Log("Game data restored successfully!");
            Destroy(gameObject, 0.1f);
        }
    }
}
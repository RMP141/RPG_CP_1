// Scripts/Quests/QuestManager.cs
using RPG.Core;
using RPG.Items;
using RPG.Player;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Quests
{
    public enum QuestType
    {
        Kill,
        Collect,
        ReachLocation,
        Talk,
        Escort
    }

    [Serializable]
    public class QuestReward
    {
        public int experience;
        public int gold;
        public string itemRewardId;

        public void Apply()
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            PlayerStats stats = player?.GetComponent<PlayerStats>();

            if (stats != null)
            {
                stats.AddExperience(experience);
                Debug.Log($"Applied reward: +{experience} XP");
            }

            if (!string.IsNullOrEmpty(itemRewardId) && Inventory.Instance != null)
            {
                Inventory.Instance.AddItem(itemRewardId);
                Debug.Log($"Applied reward: +1 {itemRewardId}");
            }

            if (gold > 0 && Inventory.Instance != null)
            {
                Inventory.Instance.AddGold(gold);
                Debug.Log($"Applied reward: +{gold} Gold");
            }
        }
    }

    [Serializable]
    public class Quest
    {
        public int id;
        public string name;
        public string description;
        public QuestType type;
        public int requiredAmount;
        public int currentAmount;
        public bool isCompleted;
        public QuestReward reward;

        public event Action<Quest> OnProgressUpdated;
        public event Action<Quest> OnCompleted;

        public void UpdateProgress(int amount = 1)
        {
            if (isCompleted) return;

            int oldAmount = currentAmount;
            currentAmount = Mathf.Min(requiredAmount, currentAmount + amount);

            if (oldAmount != currentAmount)
            {
                Debug.Log($"Quest progress: {name} - {currentAmount}/{requiredAmount}");
                OnProgressUpdated?.Invoke(this);
            }

            if (currentAmount >= requiredAmount)
            {
                Complete();
            }
        }

        public void Complete()
        {
            if (isCompleted) return;

            isCompleted = true;
            reward?.Apply();
            OnCompleted?.Invoke(this);

            Debug.Log($"✨ Quest completed: {name} ✨");
        }

        // Добавляем метод для восстановления прогресса из сохранения
        public void RestoreProgress(int progress)
        {
            if (isCompleted) return;

            int oldAmount = currentAmount;
            currentAmount = Mathf.Min(requiredAmount, progress);

            if (oldAmount != currentAmount)
            {
                Debug.Log($"Quest progress restored: {name} - {currentAmount}/{requiredAmount}");
                OnProgressUpdated?.Invoke(this);
            }

            if (currentAmount >= requiredAmount)
            {
                Complete();
            }
        }

        public float Progress => (float)currentAmount / requiredAmount;
        public string ProgressText => $"{currentAmount}/{requiredAmount}";
    }

    public class QuestManager : IQuestManager
    {
        private List<Quest> activeQuests = new List<Quest>();
        private List<Quest> completedQuests = new List<Quest>();

        public event Action<Quest> OnQuestAdded;
        public event Action<Quest> OnQuestUpdated;
        public event Action<Quest> OnQuestCompleted;

        public void AddQuest(Quest quest)
        {
            if (activeQuests.Exists(q => q.id == quest.id))
            {
                Debug.LogWarning($"Quest {quest.name} already active!");
                return;
            }

            if (completedQuests.Exists(q => q.id == quest.id))
            {
                Debug.LogWarning($"Quest {quest.name} already completed!");
                return;
            }

            quest.OnProgressUpdated += OnQuestProgressUpdated;
            quest.OnCompleted += OnQuestCompletedHandler;

            activeQuests.Add(quest);
            OnQuestAdded?.Invoke(quest);

            Debug.Log($"📜 Quest added: {quest.name}");
        }

        public void UpdateQuest(QuestType type, int value = 1)
        {
            List<Quest> questsCopy = new List<Quest>(activeQuests);

            foreach (Quest quest in questsCopy)
            {
                if (quest.type == type && !quest.isCompleted)
                {
                    quest.UpdateProgress(value);
                }
            }
        }

        public void UpdateQuestById(int questId, int progress)
        {
            Quest quest = activeQuests.Find(q => q.id == questId);
            if (quest != null && !quest.isCompleted)
            {
                // Используем метод RestoreProgress внутри класса Quest
                quest.RestoreProgress(progress);
            }
            else
            {
                Debug.LogWarning($"Quest {questId} not found or already completed");
            }
        }

        public void CompleteQuest(int questId)
        {
            Quest quest = activeQuests.Find(q => q.id == questId);
            if (quest != null && !quest.isCompleted)
            {
                quest.Complete();
            }
        }

        public List<Quest> GetActiveQuests()
        {
            return new List<Quest>(activeQuests);
        }

        public List<Quest> GetCompletedQuests()
        {
            return new List<Quest>(completedQuests);
        }

        public bool HasActiveQuest(int questId)
        {
            return activeQuests.Exists(q => q.id == questId);
        }

        public bool IsQuestCompleted(int questId)
        {
            return completedQuests.Exists(q => q.id == questId);
        }

        public int GetQuestProgress(int questId)
        {
            Quest quest = activeQuests.Find(q => q.id == questId);
            return quest != null ? quest.currentAmount : 0;
        }

        public void ClearAllQuests()
        {
            foreach (Quest quest in activeQuests)
            {
                quest.OnProgressUpdated -= OnQuestProgressUpdated;
                quest.OnCompleted -= OnQuestCompletedHandler;
            }

            activeQuests.Clear();
            completedQuests.Clear();

            Debug.Log("All quests cleared");
        }

        private void OnQuestProgressUpdated(Quest quest)
        {
            OnQuestUpdated?.Invoke(quest);
        }

        private void OnQuestCompletedHandler(Quest quest)
        {
            activeQuests.Remove(quest);
            completedQuests.Add(quest);

            quest.OnProgressUpdated -= OnQuestProgressUpdated;
            quest.OnCompleted -= OnQuestCompletedHandler;

            OnQuestCompleted?.Invoke(quest);
        }

        public int ActiveQuestCount => activeQuests.Count;
        public int CompletedQuestCount => completedQuests.Count;
    }
}
using RPG.Core;
using RPG.Player;
using System;
using System.Collections.Generic;
using UnityEngine;
using RPG.Items;

namespace RPG.Quests
{
    public enum QuestType { Kill, Collect, ReachLocation, Talk, Escort }

    [Serializable]
    public class QuestReward
    {
        public int experience;
        public int gold;
        public string itemRewardId;

        public void Apply()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            var stats = player?.GetComponent<PlayerStats>();
            stats?.AddExperience(experience);

            if (!string.IsNullOrEmpty(itemRewardId))
                Inventory.Instance?.AddItem(itemRewardId);
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

            currentAmount = Mathf.Min(requiredAmount, currentAmount + amount);
            OnProgressUpdated?.Invoke(this);

            if (currentAmount >= requiredAmount)
                Complete();
        }

        public void Complete()
        {
            if (isCompleted) return;
            isCompleted = true;
            reward?.Apply();
            OnCompleted?.Invoke(this);
        }

        public float Progress => (float)currentAmount / requiredAmount;
    }

    public class QuestManager : IQuestManager
    {
        private List<Quest> activeQuests = new List<Quest>();
        private List<Quest> completedQuests = new List<Quest>();

        public event Action<Quest> OnQuestUpdated;
        public event Action<Quest> OnQuestCompleted;

        public void AddQuest(Quest quest)
        {
            if (activeQuests.Exists(q => q.id == quest.id)) return;
            if (completedQuests.Exists(q => q.id == quest.id)) return;

            activeQuests.Add(quest);
            quest.OnProgressUpdated += q => OnQuestUpdated?.Invoke(q);
            quest.OnCompleted += q =>
            {
                activeQuests.Remove(q);
                completedQuests.Add(q);
                OnQuestCompleted?.Invoke(q);
            };
        }

        public void UpdateQuest(QuestType type, int value = 1)
        {
            foreach (var quest in activeQuests)
            {
                if (quest.type == type && !quest.isCompleted)
                    quest.UpdateProgress(value);
            }
        }

        public void CompleteQuest(int questId)
        {
            activeQuests.Find(q => q.id == questId)?.Complete();
        }

        public List<Quest> GetActiveQuests() => new List<Quest>(activeQuests);
        public List<Quest> GetCompletedQuests() => new List<Quest>(completedQuests);
    }
}
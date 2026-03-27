// Scripts/UI/QuestLog.cs
using RPG.Core;
using RPG.DI;
using RPG.Quests;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace RPG.UI
{
    public class QuestLog : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Transform questContainer;
        [SerializeField] private GameObject questItemPrefab;

        private IQuestManager questManager;
        private List<GameObject> activeQuestItems = new List<GameObject>();

        void Start()
        {
            questManager = DIContainer.Instance.Resolve<IQuestManager>();

            if (questManager != null)
            {
                // Подписываемся на события
                questManager.OnQuestAdded += OnQuestAdded;
                questManager.OnQuestUpdated += OnQuestUpdated;
                questManager.OnQuestCompleted += OnQuestCompleted;

                RefreshAllQuests();
            }
            else
            {
                Debug.LogError("❌ QuestManager not found!");
            }
        }

        private void RefreshAllQuests()
        {
            Debug.Log("=== Refreshing Quest Log ===");

            foreach (var item in activeQuestItems)
            {
                if (item != null)
                    Destroy(item);
            }
            activeQuestItems.Clear();

            List<Quest> activeQuests = questManager.GetActiveQuests();
            Debug.Log($"Active quests count: {activeQuests.Count}");

            foreach (Quest quest in activeQuests)
            {
                AddQuestItem(quest);
            }
        }

        private void AddQuestItem(Quest quest)
        {
            if (questItemPrefab == null)
            {
                Debug.LogError("❌ QuestItemPrefab is null!");
                return;
            }

            if (questContainer == null)
            {
                Debug.LogError("❌ QuestContainer is null!");
                return;
            }

            Debug.Log($"➕ Adding quest item: {quest.name}");

            GameObject questObj = Instantiate(questItemPrefab, questContainer);
            activeQuestItems.Add(questObj);

            QuestView view = questObj.GetComponent<QuestView>();
            if (view != null)
            {
                view.SetQuest(quest);
            }
            else
            {
                TextMeshProUGUI text = questObj.GetComponent<TextMeshProUGUI>();
                if (text == null)
                    text = questObj.AddComponent<TextMeshProUGUI>();

                text.text = $"{quest.name}: {quest.currentAmount}/{quest.requiredAmount}";
                text.fontSize = 14;
                text.color = quest.isCompleted ? Color.green : Color.white;
            }
        }

        private void OnQuestAdded(Quest quest)
        {
            Debug.Log($"📜 Quest added to UI: {quest.name}");
            AddQuestItem(quest);
        }

        private void OnQuestUpdated(Quest quest)
        {
            Debug.Log($"🔄 Quest updated in UI: {quest.name} - {quest.currentAmount}/{quest.requiredAmount}");
        }

        private void OnQuestCompleted(Quest quest)
        {
            Debug.Log($"✅ Quest completed in UI: {quest.name}");

            for (int i = activeQuestItems.Count - 1; i >= 0; i--)
            {
                if (activeQuestItems[i] == null)
                {
                    activeQuestItems.RemoveAt(i);
                    continue;
                }

                QuestView view = activeQuestItems[i].GetComponent<QuestView>();
                if (view != null && view.HasQuest(quest.id))
                {
                    Destroy(activeQuestItems[i]);
                    activeQuestItems.RemoveAt(i);
                    break;
                }
            }
        }

        private void OnDestroy()
        {
            if (questManager != null)
            {
                questManager.OnQuestAdded -= OnQuestAdded;
                questManager.OnQuestUpdated -= OnQuestUpdated;
                questManager.OnQuestCompleted -= OnQuestCompleted;
            }
        }
    }
}
// Scripts/UI/QuestView.cs
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPG.Quests;

namespace RPG.UI
{
    public class QuestView : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private TextMeshProUGUI questNameText;
        [SerializeField] private TextMeshProUGUI questProgressText;
        [SerializeField] private Slider progressSlider;
        [SerializeField] private Image progressFill;

        [Header("Colors")]
        [SerializeField] private Color activeColor = Color.white;
        [SerializeField] private Color completedColor = Color.green;
        [SerializeField] private Gradient progressGradient;

        private Quest currentQuest;
        private int currentQuestId;

        public void SetQuest(Quest quest)
        {
            currentQuest = quest;
            currentQuestId = quest.id;

            Debug.Log($"📋 QuestView.SetQuest: {quest.name}, ID: {quest.id}");

            if (questNameText != null)
            {
                questNameText.text = quest.name;
                questNameText.color = quest.isCompleted ? completedColor : activeColor;
            }

            UpdateProgress();

            quest.OnProgressUpdated += OnProgressUpdated;
            quest.OnCompleted += OnQuestCompleted;
        }

        private void UpdateProgress()
        {
            if (currentQuest == null) return;

            if (questProgressText != null)
            {
                questProgressText.text = $"{currentQuest.currentAmount}/{currentQuest.requiredAmount}";
            }

            float progress = currentQuest.Progress;
            if (progressSlider != null)
            {
                progressSlider.value = progress;
            }

            if (progressFill != null && progressGradient != null)
            {
                progressFill.color = progressGradient.Evaluate(progress);
            }
        }

        private void OnProgressUpdated(Quest quest)
        {
            if (quest != currentQuest) return;
            UpdateProgress();
        }

        private void OnQuestCompleted(Quest quest)
        {
            if (quest != currentQuest) return;

            if (questNameText != null)
            {
                questNameText.color = completedColor;
            }
        }

        // ДОБАВЬТЕ ЭТОТ МЕТОД
        public bool HasQuest(int questId)
        {
            return currentQuestId == questId;
        }

        public int GetQuestId()
        {
            return currentQuestId;
        }

        private void OnDestroy()
        {
            if (currentQuest != null)
            {
                currentQuest.OnProgressUpdated -= OnProgressUpdated;
                currentQuest.OnCompleted -= OnQuestCompleted;
            }
        }
    }
}
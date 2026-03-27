using RPG.Quests;
using System.Collections.Generic;

namespace RPG.Core
{
    public interface IQuestManager
    {
        void AddQuest(Quest quest);
        void UpdateQuest(QuestType type, int value = 1);
        void UpdateQuestById(int questId, int progress);
        void CompleteQuest(int questId);
        List<Quest> GetActiveQuests();
        List<Quest> GetCompletedQuests();
        event System.Action<Quest> OnQuestUpdated;
        event System.Action<Quest> OnQuestCompleted;
        event System.Action<Quest> OnQuestAdded;
    }
}
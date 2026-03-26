using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using RPG.Quests;

namespace RPG.UI.HUD
{
    public class QuestLog : MonoBehaviour
    {
        [SerializeField] private Transform questContainer;
        [SerializeField] private GameObject questPrefab;

        private List<GameObject> questItems = new List<GameObject>();

        public void UpdateQuests(List<Quest> quests)
        {
            foreach (var item in questItems)
                Destroy(item);
            questItems.Clear();

            foreach (var quest in quests)
            {
                var questObj = Instantiate(questPrefab, questContainer);
                var text = questObj.GetComponent<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = $"{quest.name}: {quest.currentAmount}/{quest.requiredAmount}";
                }
                questItems.Add(questObj);
            }
        }
    }
}
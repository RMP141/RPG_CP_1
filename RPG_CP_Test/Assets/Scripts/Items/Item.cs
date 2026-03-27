using RPG.Core;
using RPG.DI;
using RPG.Player;
using RPG.Quests;
using UnityEngine;

namespace RPG.Items
{
    public class Item : MonoBehaviour
    {
        [SerializeField] private string itemId;
        [SerializeField] private string itemName;
        [SerializeField] private int value;
        [SerializeField] private ItemType type;
        [SerializeField] private int healAmount;
        [SerializeField] private int manaAmount;
        [SerializeField] private GameObject pickupEffect;

        public enum ItemType
        {
            Consumable,
            Weapon,
            Armor,
            Quest,
            Currency
        }

        public void Collect()
        {
            Debug.Log($"Collecting item: {itemName} (Type: {type}, Value: {value})");

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            PlayerStats stats = player?.GetComponent<PlayerStats>();
            Inventory inventory = Inventory.Instance;

            // Получаем менеджер квестов через DI
            IQuestManager questManager = DIContainer.Instance.Resolve<IQuestManager>();

            switch (type)
            {
                case ItemType.Consumable:
                    if (healAmount > 0)
                    {
                        stats?.Heal(healAmount);
                        Debug.Log($"Healed for {healAmount}");
                    }
                    if (manaAmount > 0)
                    {
                        stats?.RestoreMana(manaAmount);
                        Debug.Log($"Restored mana for {manaAmount}");
                    }
                    break;

                case ItemType.Currency:
                    if (inventory != null)
                    {
                        inventory.AddGold(value);
                        Debug.Log($"Added {value} gold! Total: {inventory.Gold}");
                    }
                    break;

                default:
                    inventory?.AddItem(itemId);
                    break;
            }

            if (questManager != null)
            {
                questManager.UpdateQuest(QuestType.Collect, 1);
                Debug.Log($"Quest updated: Collect +1");
            }
            else
            {
                Debug.LogError("QuestManager is null!");
            }

            // Эффект подбора
            if (pickupEffect != null)
            {
                Instantiate(pickupEffect, transform.position, Quaternion.identity);
            }

            // Уничтожаем предмет
            Destroy(gameObject);
        }
    }
}
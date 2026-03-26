using RPG.Player;
using UnityEngine;
using RPG.Core;

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
            var player = GameObject.FindGameObjectWithTag("Player");
            var stats = player?.GetComponent<PlayerStats>();
            var inventory = Inventory.Instance;

            switch (type)
            {
                case ItemType.Consumable:
                    if (healAmount > 0) stats?.Heal(healAmount);
                    if (manaAmount > 0) stats?.RestoreMana(manaAmount);
                    break;

                case ItemType.Currency:
                    inventory?.AddGold(value);
                    break;

                default:
                    inventory?.AddItem(itemId);
                    break;
            }

            if (pickupEffect != null)
                Instantiate(pickupEffect, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }
    }
}
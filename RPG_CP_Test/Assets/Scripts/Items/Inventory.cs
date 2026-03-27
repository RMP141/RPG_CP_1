using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Items
{
    public class Inventory : MonoBehaviour
    {
        private static Inventory instance;
        public static Inventory Instance => instance;

        [SerializeField] private int maxSlots = 20;
        [SerializeField] private int startingGold = 100;

        private Dictionary<string, int> items = new Dictionary<string, int>();
        private int gold;

        public int Gold => gold;
        public event Action<int> OnGoldChanged;
        public event Action<Dictionary<string, int>> OnInventoryChanged;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            DontDestroyOnLoad(gameObject);

            gold = startingGold;
        }

        public void AddItem(string itemId, int amount = 1)
        {
            if (items.ContainsKey(itemId))
                items[itemId] += amount;
            else
                items[itemId] = amount;

            OnInventoryChanged?.Invoke(items);
        }

        public void RemoveItem(string itemId, int amount = 1)
        {
            if (!items.ContainsKey(itemId)) return;

            items[itemId] -= amount;
            if (items[itemId] <= 0)
                items.Remove(itemId);

            OnInventoryChanged?.Invoke(items);
        }

        public void AddGold(int amount)
        {
            gold += amount;
            OnGoldChanged?.Invoke(gold);
        }

        public bool RemoveGold(int amount)
        {
            if (gold < amount) return false;
            gold -= amount;
            OnGoldChanged?.Invoke(gold);
            return true;
        }

        public void ClearInventory()
        {
            items.Clear();
            OnInventoryChanged?.Invoke(items);
            Debug.Log("Inventory cleared");
        }

        public Dictionary<string, int> GetAllItems()
        {
            return new Dictionary<string, int>(items);
        }

        public bool HasItem(string itemId, int amount = 1)
        {
            return items.ContainsKey(itemId) && items[itemId] >= amount;
        }
    }
}
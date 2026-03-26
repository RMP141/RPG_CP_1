using UnityEditor.Overlays;

namespace RPG.Core
{
    public interface ISaveSystem
    {
        void SaveGame(int slot);
        void LoadGame(int slot);
        bool HasSave(int slot);
        void DeleteSave(int slot);
        SaveData GetSaveData(int slot);
    }
}
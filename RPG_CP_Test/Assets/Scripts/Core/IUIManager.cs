using UnityEngine;

namespace RPG.Core
{
    public interface IUIManager
    {
        void ShowMessage(string message, float duration = 2f);
        void ShowDamageText(string damage, Vector3 position);
        void ShowFloatingText(string text, Vector3 position);
        void UpdateUI();
        void ShowGameOver();
        void ShowPauseMenu();
        void HidePauseMenu();
    }
}
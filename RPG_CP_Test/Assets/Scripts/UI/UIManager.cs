using UnityEngine;
using RPG.Core;

namespace RPG.UI
{
    public class UIManager : IUIManager
    {
        private GameObject messagePanel;
        private GameObject gameOverPanel;
        private GameObject pausePanel;

        public UIManager()
        {
            // Создаем UI элементы
            CreateUI();
        }

        private void CreateUI()
        {
            // Создаем Canvas
            var canvasGO = new GameObject("UICanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasGO.AddComponent<UnityEngine.UI.CanvasScaler>();
            canvasGO.AddComponent<UnityEngine.UI.GraphicRaycaster>();
            Object.DontDestroyOnLoad(canvasGO);
        }

        public void ShowMessage(string message, float duration = 2f)
        {
            Debug.Log($"[UI] {message}");
        }

        public void ShowDamageText(string damage, Vector3 position)
        {
            // Создаем текст урона в мире
            var damageTextGO = new GameObject("DamageText");
            damageTextGO.transform.position = position;
            var textMesh = damageTextGO.AddComponent<TextMesh>();
            textMesh.text = damage;
            textMesh.fontSize = 24;
            textMesh.color = Color.red;
            textMesh.anchor = TextAnchor.MiddleCenter;

            Object.Destroy(damageTextGO, 1f);
        }

        public void ShowFloatingText(string text, Vector3 position)
        {
            var floatingText = new GameObject("FloatingText");
            floatingText.transform.position = position;
            var textMesh = floatingText.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.fontSize = 20;
            textMesh.color = Color.yellow;

            Object.Destroy(floatingText, 1.5f);
        }

        public void UpdateUI() { }
        public void ShowGameOver() => Debug.Log("Game Over");
        public void ShowPauseMenu() => Debug.Log("Pause Menu");
        public void HidePauseMenu() => Debug.Log("Resume");
    }
}
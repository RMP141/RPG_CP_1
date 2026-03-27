using UnityEngine;
using TMPro;
using System.Collections;

namespace RPG.UI
{
    public class DamageTextAnimation : MonoBehaviour
    {
        [SerializeField] private float floatSpeed = 1f;
        [SerializeField] private float fadeSpeed = 2f;
        [SerializeField] private float lifetime = 1f;

        private TextMeshProUGUI text;
        private Vector3 startPosition;
        private Color startColor;

        private void Start()
        {
            text = GetComponent<TextMeshProUGUI>();
            startPosition = transform.position;
            startColor = text.color;

            StartCoroutine(Animate());
        }

        private IEnumerator Animate()
        {
            float elapsed = 0;

            while (elapsed < lifetime)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / lifetime;

                // Поднимаемся вверх
                transform.position = startPosition + Vector3.up * (t * floatSpeed);

                // Затухаем
                Color newColor = startColor;
                newColor.a = 1 - t;
                text.color = newColor;

                yield return null;
            }

            Destroy(gameObject);
        }
    }
}
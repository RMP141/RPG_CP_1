using UnityEngine;
using RPG.Core;
using RPG.DI;

namespace RPG.Quests
{
    public class QuestTrigger : MonoBehaviour
    {
        [Header("Quest Settings")]
        [SerializeField] private int questId;
        [SerializeField] private QuestType requiredType = QuestType.ReachLocation;
        [SerializeField] private int requiredAmount = 1;
        [SerializeField] private string questName = "Quest";

        [Header("Trigger Settings")]
        [SerializeField] private bool destroyOnTrigger = true;
        [SerializeField] private bool oneTimeTrigger = true;
        [SerializeField] private float triggerDelay = 0f;

        [Header("Visual Effects")]
        [SerializeField] private GameObject triggerEffect;
        [SerializeField] private AudioClip triggerSound;

        private bool hasTriggered = false;
        private IQuestManager questManager;
        private IAudioManager audioManager;

        private void Start()
        {
            // Получаем сервисы через DI
            questManager = DIContainer.Instance.Resolve<IQuestManager>();
            audioManager = DIContainer.Instance.Resolve<IAudioManager>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (oneTimeTrigger && hasTriggered) return;

            if (other.CompareTag("Player"))
            {
                Activate();
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (oneTimeTrigger && hasTriggered) return;

            if (other.CompareTag("Player"))
            {
                Activate();
            }
        }

        public void Activate()
        {
            if (hasTriggered && oneTimeTrigger) return;

            hasTriggered = true;

            if (triggerDelay > 0)
            {
                Invoke(nameof(ExecuteQuestTrigger), triggerDelay);
            }
            else
            {
                ExecuteQuestTrigger();
            }
        }

        private void ExecuteQuestTrigger()
        {
            if (questManager != null)
            {
                if (questId > 0)
                {
                    questManager.CompleteQuest(questId);
                }
                else
                {
                    questManager.UpdateQuest(requiredType, requiredAmount);
                }

                Debug.Log($"[QuestTrigger] Quest triggered: {questName} ({requiredType})");
            }

            if (triggerEffect != null)
            {
                Instantiate(triggerEffect, transform.position, Quaternion.identity);
            }

            if (triggerSound != null && audioManager != null)
            {
                var audioGO = new GameObject("TempAudio");
                var audioSource = audioGO.AddComponent<AudioSource>();
                audioSource.clip = triggerSound;
                audioSource.Play();
                Destroy(audioGO, triggerSound.length);
            }

            if (destroyOnTrigger)
            {
                Destroy(gameObject, 0.1f);
            }
        }

        public void SetQuest(int id, QuestType type, int amount)
        {
            questId = id;
            requiredType = type;
            requiredAmount = amount;
        }

        public void ResetTrigger()
        {
            hasTriggered = false;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;

            Collider col = GetComponent<Collider>();
            if (col != null)
            {
                if (col is BoxCollider box)
                {
                    Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
                    Gizmos.DrawWireCube(box.center, box.size);
                }
                else if (col is SphereCollider sphere)
                {
                    Gizmos.DrawWireSphere(transform.position, sphere.radius);
                }
            }
        }
    }
}
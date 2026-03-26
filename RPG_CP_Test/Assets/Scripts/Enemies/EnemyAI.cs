using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using RPG.Core;
using RPG.Quests;
using RPG.DI;
using RPG.Player;

namespace RPG.Enemies
{
    public enum EnemyState
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Return,
        Dead
    }

    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(Animator))]
    [RequireComponent(typeof(EnemyHealth))]
    public class EnemyAI : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float patrolRadius = 10f;
        [SerializeField] private float patrolWaitTime = 2f;
        [SerializeField] private float moveSpeed = 3.5f;

        [Header("Detection Settings")]
        [SerializeField] private float chaseRange = 15f;
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float loseInterestRange = 20f;
        [SerializeField] private LayerMask obstacleLayer;

        [Header("Combat Settings")]
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private int damage = 10;
        [SerializeField] private float attackAnimationDelay = 0.5f;

        [Header("Patrol Points")]
        [SerializeField] private Transform[] patrolPoints;

        [Header("Visual Effects")]
        [SerializeField] private GameObject deathEffect;
        [SerializeField] private GameObject hitEffect;

        // Компоненты
        private Transform player;
        private NavMeshAgent agent;
        private Animator animator;
        private EnemyHealth health;
        private Collider enemyCollider;

        // Позиции
        private Vector3 startPosition;
        private Quaternion startRotation;

        // Состояние
        private EnemyState currentState = EnemyState.Patrol;
        private float lastAttackTime;
        private int currentPatrolIndex;
        private float waitTimer;

        // Сервисы (получаем через DI)
        private IAudioManager audioManager;
        private IQuestManager questManager;

        // Свойства
        public EnemyState CurrentState => currentState;
        public bool IsDead => health != null && health.IsDead;
        public Vector3 CurrentTarget => agent.destination;

        private void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            animator = GetComponent<Animator>();
            health = GetComponent<EnemyHealth>();
            enemyCollider = GetComponent<Collider>();

            // Настройка NavMeshAgent
            if (agent != null)
            {
                agent.speed = moveSpeed;
                agent.stoppingDistance = attackRange;
                agent.autoBraking = true;
                agent.autoRepath = true;
            }
        }

        private void Start()
        {
            // Поиск игрока
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
            else
            {
                Debug.LogError("[EnemyAI] Player not found!");
            }

            // Сохраняем начальную позицию и поворот
            startPosition = transform.position;
            startRotation = transform.rotation;

            // Получаем сервисы через DI контейнер
            audioManager = DIContainer.Instance.Resolve<IAudioManager>();
            questManager = DIContainer.Instance.Resolve<IQuestManager>();

            // Подписываемся на событие смерти
            if (health != null)
            {
                health.OnDeath += HandleDeath;
            }

            // Случайное начальное состояние
            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                currentPatrolIndex = Random.Range(0, patrolPoints.Length);
            }

            // Запускаем патрулирование
            currentState = EnemyState.Patrol;
        }

        private void Update()
        {
            if (IsDead) return;
            if (player == null) return;

            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            UpdateState(distanceToPlayer);
            ExecuteState();
            UpdateAnimations();

            // Проверка видимости игрока
            if (currentState == EnemyState.Chase && !CanSeePlayer())
            {
                currentState = EnemyState.Return;
            }
        }

        private bool CanSeePlayer()
        {
            if (player == null) return false;

            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Проверка луча
            RaycastHit hit;
            if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, distanceToPlayer, obstacleLayer))
            {
                return hit.collider.CompareTag("Player");
            }

            return true;
        }

        private void UpdateState(float distanceToPlayer)
        {
            if (currentState == EnemyState.Dead) return;

            switch (currentState)
            {
                case EnemyState.Idle:
                    if (distanceToPlayer < chaseRange && CanSeePlayer())
                        currentState = EnemyState.Chase;
                    else if (waitTimer <= 0)
                        currentState = EnemyState.Patrol;
                    break;

                case EnemyState.Patrol:
                    if (distanceToPlayer < chaseRange && CanSeePlayer())
                        currentState = EnemyState.Chase;
                    break;

                case EnemyState.Chase:
                    if (distanceToPlayer < attackRange)
                        currentState = EnemyState.Attack;
                    else if (distanceToPlayer > loseInterestRange || !CanSeePlayer())
                        currentState = EnemyState.Return;
                    break;

                case EnemyState.Attack:
                    if (distanceToPlayer > attackRange)
                        currentState = EnemyState.Chase;
                    else if (distanceToPlayer > loseInterestRange)
                        currentState = EnemyState.Return;
                    break;

                case EnemyState.Return:
                    if (Vector3.Distance(transform.position, startPosition) < 0.5f)
                    {
                        currentState = EnemyState.Idle;
                        waitTimer = Random.Range(1f, 3f);
                    }
                    else if (distanceToPlayer < chaseRange && CanSeePlayer())
                    {
                        currentState = EnemyState.Chase;
                    }
                    break;
            }
        }

        private void ExecuteState()
        {
            switch (currentState)
            {
                case EnemyState.Idle:
                    Idle();
                    break;
                case EnemyState.Patrol:
                    Patrol();
                    break;
                case EnemyState.Chase:
                    Chase();
                    break;
                case EnemyState.Attack:
                    Attack();
                    break;
                case EnemyState.Return:
                    ReturnToStart();
                    break;
            }
        }

        private void Idle()
        {
            if (agent != null)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            if (waitTimer > 0)
            {
                waitTimer -= Time.deltaTime;
            }
        }

        private void Patrol()
        {
            if (agent == null) return;

            agent.isStopped = false;
            agent.speed = moveSpeed * 0.6f; // Медленнее при патруле

            if (patrolPoints != null && patrolPoints.Length > 0)
            {
                PatrolBetweenPoints();
            }
            else
            {
                PatrolRandom();
            }
        }

        private void PatrolBetweenPoints()
        {
            if (!agent.hasPath || agent.remainingDistance < 0.5f)
            {
                Transform targetPoint = patrolPoints[currentPatrolIndex];
                if (targetPoint != null)
                {
                    agent.SetDestination(targetPoint.position);

                    if (Vector3.Distance(transform.position, targetPoint.position) < 1f)
                    {
                        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
                        waitTimer = Random.Range(1f, 2f);
                        currentState = EnemyState.Idle;
                    }
                }
            }
        }

        private void PatrolRandom()
        {
            if (!agent.hasPath || agent.remainingDistance < 0.5f)
            {
                Vector3 randomPoint = startPosition + Random.insideUnitSphere * patrolRadius;
                randomPoint.y = startPosition.y;

                NavMeshHit hit;
                if (NavMesh.SamplePosition(randomPoint, out hit, patrolRadius, NavMesh.AllAreas))
                {
                    agent.SetDestination(hit.position);
                }

                waitTimer = Random.Range(2f, 5f);
                currentState = EnemyState.Idle;
            }
        }

        private void Chase()
        {
            if (agent == null || player == null) return;

            agent.isStopped = false;
            agent.speed = moveSpeed;
            agent.SetDestination(player.position);

            // Поворот в сторону игрока
            Vector3 directionToPlayer = (player.position - transform.position).normalized;
            directionToPlayer.y = 0;
            if (directionToPlayer != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }

        private void Attack()
        {
            if (agent != null)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;
            }

            // Поворот в сторону игрока
            if (player != null)
            {
                Vector3 directionToPlayer = (player.position - transform.position).normalized;
                directionToPlayer.y = 0;
                if (directionToPlayer != Vector3.zero)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
                }
            }

            if (Time.time >= lastAttackTime + attackCooldown)
            {
                lastAttackTime = Time.time;
                animator.SetTrigger("Attack");
                StartCoroutine(PerformAttack());
            }
        }

        private IEnumerator PerformAttack()
        {
            yield return new WaitForSeconds(attackAnimationDelay);

            if (player != null && Vector3.Distance(transform.position, player.position) <= attackRange)
            {
                var playerStats = player.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    playerStats.TakeDamage(damage);
                    audioManager?.PlaySFX("EnemyAttack");

                    // Эффект удара
                    if (hitEffect != null && player != null)
                    {
                        Instantiate(hitEffect, player.position + Vector3.up, Quaternion.identity);
                    }
                }
            }
        }

        private void ReturnToStart()
        {
            if (agent == null) return;

            agent.isStopped = false;
            agent.speed = moveSpeed;
            agent.SetDestination(startPosition);

            // Если вернулись на стартовую позицию
            if (Vector3.Distance(transform.position, startPosition) < 0.5f)
            {
                transform.rotation = startRotation;
            }
        }

        private void UpdateAnimations()
        {
            if (animator == null) return;

            float speed = agent != null ? agent.velocity.magnitude : 0f;
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsMoving", speed > 0.1f);
            animator.SetBool("IsChasing", currentState == EnemyState.Chase);
            animator.SetBool("IsAttacking", currentState == EnemyState.Attack);
        }

        private void HandleDeath()
        {
            currentState = EnemyState.Dead;

            // Останавливаем NavMeshAgent
            if (agent != null)
            {
                agent.isStopped = true;
                agent.enabled = false;
            }

            // Отключаем коллайдер
            if (enemyCollider != null)
            {
                enemyCollider.enabled = false;
            }

            // Анимация смерти
            if (animator != null)
            {
                animator.SetTrigger("Die");
            }

            // Эффект смерти
            if (deathEffect != null)
            {
                Instantiate(deathEffect, transform.position, Quaternion.identity);
            }

            // Обновляем квест на убийство
            if (questManager != null)
            {
                questManager.UpdateQuest(QuestType.Kill, 1);
            }

            // Добавляем опыт игроку
            if (player != null)
            {
                var playerStats = player.GetComponent<PlayerStats>();
                if (playerStats != null)
                {
                    playerStats.AddExperience(20);
                }
            }

            // Воспроизводим звук смерти
            audioManager?.PlaySFX("EnemyDeath");

            // Уничтожаем объект через 3 секунды
            Destroy(gameObject, 3f);
        }

        // Публичные методы для внешнего взаимодействия
        public void TakeDamage(int damageAmount)
        {
            if (health != null && !IsDead)
            {
                health.TakeDamage(damageAmount);

                // Реакция на получение урона
                if (animator != null)
                {
                    animator.SetTrigger("Hit");
                }

                // Если получили урон, переходим в режим преследования
                if (currentState != EnemyState.Dead && currentState != EnemyState.Attack)
                {
                    currentState = EnemyState.Chase;
                }
            }
        }

        public void SetState(EnemyState newState)
        {
            if (!IsDead)
            {
                currentState = newState;
            }
        }

        public void ResetEnemy()
        {
            currentState = EnemyState.Return;
            if (health != null)
            {
                health.ResetHealth();
            }
            if (agent != null && agent.isActiveAndEnabled)
            {
                agent.Warp(startPosition);
            }
            else
            {
                transform.position = startPosition;
            }
            transform.rotation = startRotation;
        }

        private void OnDrawGizmosSelected()
        {
            // Визуализация диапазонов
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, attackRange);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, chaseRange);

            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(startPosition, patrolRadius);

            // Визуализация точек патруля
            if (patrolPoints != null)
            {
                Gizmos.color = Color.green;
                foreach (var point in patrolPoints)
                {
                    if (point != null)
                    {
                        Gizmos.DrawWireSphere(point.position, 0.5f);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            if (health != null)
            {
                health.OnDeath -= HandleDeath;
            }
        }
    }
}
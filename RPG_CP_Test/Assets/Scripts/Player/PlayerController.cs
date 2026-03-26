using UnityEngine;
using System.Collections;
using RPG.Core;
using RPG.Combat;
using RPG.Quests;
using RPG.Items;
using RPG.Enemies;
using RPG.DI;

namespace RPG.Player
{
    [RequireComponent(typeof(CharacterController))]
    [RequireComponent(typeof(Animator))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movement Settings")]
        [SerializeField] private float walkSpeed = 5f;
        [SerializeField] private float runSpeed = 8f;
        [SerializeField] private float jumpHeight = 1.5f;
        [SerializeField] private float gravity = -9.81f;

        [Header("Combat Settings")]
        [SerializeField] private float attackRange = 2f;
        [SerializeField] private float attackCooldown = 0.5f;
        [SerializeField] private int attackDamage = 20;
        [SerializeField] private Transform attackPoint;

        [Header("Camera Settings")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float cameraPitchLimit = 80f;

        [Header("Ability Settings")]
        [SerializeField] private GameObject fireballPrefab;
        [SerializeField] private int manaCost = 20;

        // Компоненты
        private CharacterController controller;
        private Animator animator;
        private PlayerStats stats;

        // Движение
        private Vector3 velocity;
        private bool isGrounded;
        private bool isAttacking;
        private float lastAttackTime;
        private float currentSpeed;

        // Камера
        private float cameraPitch;

        // Сервисы (получаем через DI)
        private IAudioManager audioManager;
        private IQuestManager questManager;
        private IGameManager gameManager;

        // Свойства
        public bool IsAttacking => isAttacking;
        public bool IsMoving => new Vector3(velocity.x, 0, velocity.z).magnitude > 0.1f;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            animator = GetComponent<Animator>();
            stats = GetComponent<PlayerStats>();

            if (playerCamera == null)
                playerCamera = Camera.main;
        }

        private void Start()
        {
            // Получаем сервисы через DI контейнер
            audioManager = DIContainer.Instance.Resolve<IAudioManager>();
            questManager = DIContainer.Instance.Resolve<IQuestManager>();
            gameManager = DIContainer.Instance.Resolve<IGameManager>();

            // Блокируем курсор для игры от первого лица
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Подписываемся на события смерти
            if (stats != null)
            {
                stats.OnPlayerDeath += HandlePlayerDeath;
            }
        }

        private void Update()
        {
            if (gameManager != null && gameManager.IsGamePaused) return;
            if (stats != null && stats.CurrentHealth <= 0) return;

            HandleCamera();
            HandleMovement();
            HandleJump();
            HandleAttack();
            HandleAbility();
            UpdateAnimations();
        }

        private void HandleCamera()
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            cameraPitch -= mouseY;
            cameraPitch = Mathf.Clamp(cameraPitch, -cameraPitchLimit, cameraPitchLimit);

            transform.Rotate(0, mouseX, 0);
            playerCamera.transform.localRotation = Quaternion.Euler(cameraPitch, 0, 0);
        }

        private void HandleMovement()
        {
            isGrounded = controller.isGrounded;

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = -2f;
            }

            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            Vector3 move = transform.right * horizontal + transform.forward * vertical;
            move.Normalize();

            bool isRunning = Input.GetKey(KeyCode.LeftShift);
            currentSpeed = isRunning ? runSpeed : walkSpeed;

            controller.Move(move * currentSpeed * Time.deltaTime);

            // Применяем гравитацию
            velocity.y += gravity * Time.deltaTime;
            controller.Move(velocity * Time.deltaTime);

            // Обновляем анимацию
            float speed = new Vector3(move.x, 0, move.z).magnitude;
            animator.SetFloat("Speed", speed);
            animator.SetBool("IsRunning", isRunning);
        }

        private void HandleJump()
        {
            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                animator.SetTrigger("Jump");
                audioManager?.PlaySFX("Jump");
            }
        }

        private void HandleAttack()
        {
            if (Input.GetMouseButtonDown(0) && !isAttacking && Time.time >= lastAttackTime + attackCooldown)
            {
                StartCoroutine(AttackCoroutine());
            }
        }

        private IEnumerator AttackCoroutine()
        {
            isAttacking = true;
            lastAttackTime = Time.time;
            animator.SetTrigger("Attack");

            // Небольшая задержка перед проверкой попадания (для синхронизации с анимацией)
            yield return new WaitForSeconds(0.2f);

            // Проверяем попадание по врагам
            Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange);
            bool hitSomething = false;

            foreach (var hit in hitEnemies)
            {
                if (hit.CompareTag("Enemy"))
                {
                    var enemyHealth = hit.GetComponent<EnemyHealth>();
                    if (enemyHealth != null)
                    {
                        enemyHealth.TakeDamage(attackDamage);
                        hitSomething = true;

                        // Показываем текст урона
                        var uiManager = DIContainer.Instance.Resolve<IUIManager>();
                        uiManager?.ShowDamageText(attackDamage.ToString(), hit.transform.position);

                        // Отбрасываем врага
                        Rigidbody enemyRb = hit.GetComponent<Rigidbody>();
                        if (enemyRb != null)
                        {
                            Vector3 knockback = (hit.transform.position - transform.position).normalized * 5f;
                            enemyRb.AddForce(knockback, ForceMode.Impulse);
                        }
                    }
                }
            }

            // Воспроизводим звук
            if (hitSomething)
            {
                audioManager?.PlaySFX("Hit");
            }
            else
            {
                audioManager?.PlaySFX("Swing");
            }

            yield return new WaitForSeconds(attackCooldown - 0.2f);
            isAttacking = false;
        }

        private void HandleAbility()
        {
            if (Input.GetKeyDown(KeyCode.Q) && stats != null && stats.CurrentMana >= manaCost)
            {
                UseAbility();
            }
        }

        private void UseAbility()
        {
            stats.UseMana(manaCost);
            animator.SetTrigger("Cast");

            if (fireballPrefab != null && attackPoint != null)
            {
                GameObject projectile = Instantiate(fireballPrefab, attackPoint.position, transform.rotation);
                var proj = projectile.GetComponent<Projectile>();
                if (proj != null)
                {
                    proj.direction = transform.forward;
                    proj.damage = stats.Damage;
                    proj.owner = gameObject;
                }
            }

            audioManager?.PlaySFX("Fireball");
        }

        private void UpdateAnimations()
        {
            animator.SetFloat("VerticalVelocity", velocity.y);
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetBool("IsAttacking", isAttacking);
        }

        private void OnTriggerEnter(Collider other)
        {
            // Сбор предметов
            if (other.CompareTag("Item"))
            {
                var item = other.GetComponent<Item>();
                if (item != null)
                {
                    item.Collect();
                    audioManager?.PlaySFX("Pickup");

                    // Обновляем квест на сбор предметов
                    questManager?.UpdateQuest(QuestType.Collect, 1);
                }
            }

            // Триггер квеста
            if (other.CompareTag("QuestTrigger"))
            {
                var trigger = other.GetComponent<QuestTrigger>();
                trigger?.Activate();
            }

            // Зона завершения уровня
            if (other.CompareTag("LevelExit"))
            {
                var levelManager = DIContainer.Instance.Resolve<ILevelManager>();
                levelManager?.LoadNextLevel();
                audioManager?.PlaySFX("LevelComplete");
            }
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            // Урон от столкновения с врагами
            if (hit.collider.CompareTag("Enemy"))
            {
                var enemyAI = hit.collider.GetComponent<EnemyAI>();
                if (enemyAI != null && enemyAI.CurrentState == EnemyState.Attack)
                {
                    TakeDamage(10);

                    // Отталкивание
                    Vector3 pushDirection = (transform.position - hit.transform.position).normalized;
                    controller.Move(pushDirection * 2f);
                }
            }
        }

        public void TakeDamage(int damage)
        {
            if (stats != null)
            {
                stats.TakeDamage(damage);
                audioManager?.PlaySFX("PlayerHurt");
                StartCoroutine(DamageFlash());
            }
        }

        private IEnumerator DamageFlash()
        {
            var renderer = GetComponentInChildren<SkinnedMeshRenderer>();
            if (renderer != null)
            {
                var originalColor = renderer.material.color;
                renderer.material.color = Color.red;
                yield return new WaitForSeconds(0.1f);
                renderer.material.color = originalColor;
            }
        }

        private void HandlePlayerDeath()
        {
            isAttacking = false;
            animator.SetTrigger("Die");
            controller.enabled = false;

            // Показываем меню Game Over
            var uiManager = DIContainer.Instance.Resolve<IUIManager>();
            uiManager?.ShowGameOver();
        }

        private void OnDrawGizmosSelected()
        {
            if (attackPoint != null)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawWireSphere(attackPoint.position, attackRange);
            }
        }

        private void OnDestroy()
        {
            if (stats != null)
            {
                stats.OnPlayerDeath -= HandlePlayerDeath;
            }
        }
    }
}
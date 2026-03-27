using UnityEngine;

namespace RPG.Core
{
    public class CameraFollow : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform target;

        [Header("Distance")]
        [SerializeField] private float distance = 5f;
        [SerializeField] private float minDistance = 2f;
        [SerializeField] private float maxDistance = 10f;

        [Header("Height")]
        [SerializeField] private float height = 2f;
        [SerializeField] private float heightOffset = 1f;

        [Header("Rotation")]
        [SerializeField] private float mouseSensitivity = 2f;
        [SerializeField] private float minYAngle = -20f;
        [SerializeField] private float maxYAngle = 80f;

        [Header("Smoothing")]
        [SerializeField] private float smoothSpeed = 10f;

        private float currentX = 0f;
        private float currentY = 0f;
        private float currentDistance;

        private void Start()
        {
            // Ищем игрока
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                    target = player.transform;
            }

            // Начальные углы
            Vector3 angles = transform.eulerAngles;
            currentX = angles.y;
            currentY = angles.x;
            currentDistance = distance;

            // Блокируем курсор для игры
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void LateUpdate()
        {
            if (target == null) return;

            // Вращение камеры мышкой
            currentX += Input.GetAxis("Mouse X") * mouseSensitivity;
            currentY -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);

            // Зум колесиком мыши
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            currentDistance -= scroll * 2f;
            currentDistance = Mathf.Clamp(currentDistance, minDistance, maxDistance);

            // Вычисляем позицию камеры
            Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
            Vector3 desiredPosition = target.position + rotation * new Vector3(0, height, -currentDistance);

            // Плавное движение
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);

            // Смотрим на игрока
            transform.LookAt(target.position + Vector3.up * heightOffset);
        }

        public void SetTarget(Transform newTarget)
        {
            target = newTarget;
        }

        private void OnDrawGizmosSelected()
        {
            if (target != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(target.position + Vector3.up * heightOffset, 0.5f);
            }
        }
    }
}
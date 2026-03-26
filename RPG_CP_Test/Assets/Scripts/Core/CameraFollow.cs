using UnityEngine;

namespace RPG.Core
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private Vector3 offset = new Vector3(0, 2, -5);
        [SerializeField] private float smoothSpeed = 0.125f;

        private void LateUpdate()
        {
            if (target == null)
            {
                target = GameObject.FindGameObjectWithTag("Player")?.transform;
                return;
            }

            Vector3 desiredPosition = target.position + offset;
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;

            transform.LookAt(target);
        }
    }
}
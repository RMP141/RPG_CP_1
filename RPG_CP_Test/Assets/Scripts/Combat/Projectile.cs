using RPG.Enemies;
using RPG.Player;
using UnityEngine;

namespace RPG.Combat
{
    public class Projectile : MonoBehaviour
    {
        [SerializeField] private float speed = 20f;
        [SerializeField] private float lifetime = 3f;
        [SerializeField] private GameObject hitEffect;
        [SerializeField] private float explosionRadius = 3f;

        public Vector3 direction;
        public int damage;
        public GameObject owner;

        private void Start() => Destroy(gameObject, lifetime);

        private void Update()
        {
            transform.position += direction * speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == owner) return;

            if (other.CompareTag("Enemy") && owner.CompareTag("Player"))
            {
                other.GetComponent<EnemyHealth>()?.TakeDamage(damage);
            }
            else if (other.CompareTag("Player") && owner.CompareTag("Enemy"))
            {
                other.GetComponent<PlayerStats>()?.TakeDamage(damage);
            }

            if (hitEffect != null)
                Instantiate(hitEffect, transform.position, Quaternion.identity);

            if (explosionRadius > 0)
            {
                Collider[] hits = Physics.OverlapSphere(transform.position, explosionRadius);
                foreach (var hit in hits)
                {
                    if (hit.CompareTag("Enemy") && hit.gameObject != other)
                        hit.GetComponent<EnemyHealth>()?.TakeDamage(damage / 2);
                }
            }

            Destroy(gameObject);
        }
    }
}
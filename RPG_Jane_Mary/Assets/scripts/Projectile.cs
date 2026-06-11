using UnityEngine;
using FishNet.Object;

public class Projectile : NetworkBehaviour
{
    [Header("Основные настройки")]
    public float speed = 15f;
    public float damage = 20f;
    public float lifetime = 3f;

    [Header("Настройки самонаведения")]
    public float homingStrength = 5f;
    public float detectionRange = 10f;
    public float detectionAngle = 45f;
    public LayerMask enemyLayer;

    private Transform _target;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        if (IsServer) Invoke(nameof(DestroyProjectile), lifetime);
        FindTarget();
    }

    private void DestroyProjectile()
    {
        if (IsServer) ServerManager.Despawn(gameObject);
    }

    void Update()
    {
        if (_target != null)
        {
            Vector3 direction = (_target.position + Vector3.up - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, homingStrength * Time.deltaTime);
        }
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void FindTarget()
    {
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);
        float closestAngle = detectionAngle;
        Transform bestTarget = null;

        foreach (var col in enemies)
        {
            if (Owner.IsValid && col.transform.root == Owner.FirstObject.transform.root) continue;
            Vector3 dirToEnemy = (col.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToEnemy);
            if (angle < closestAngle) { closestAngle = angle; bestTarget = col.transform; }
        }
        _target = bestTarget;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        if (Owner.IsValid && other.transform.root == Owner.FirstObject.transform.root) return;

        // Ищем скрипт Health
        Health targetHealth = other.GetComponentInParent<Health>();

        if (targetHealth != null)
        {
            bool isMonsterBullet = !Owner.IsValid;
            bool targetIsPlayer = other.CompareTag("Player");

            if (isMonsterBullet && targetIsPlayer)
            {
                targetHealth.TakeDamage(0, damage);
                DestroyProjectile();
            }
            else if (!targetIsPlayer)
            {
                // --- НОВОЕ: Берем PlayerScore того, кто выпустил пулю ---
                PlayerScore attacker = null;
                if (Owner.IsValid && Owner.FirstObject != null)
                {
                    attacker = Owner.FirstObject.GetComponent<PlayerScore>();
                }

                // Наносим урон с указанием атакующего
                targetHealth.TakeDamageWithAttacker(0, damage, attacker);
                DestroyProjectile();
            }
        }
        else if (!other.isTrigger)
        {
            DestroyProjectile();
        }
    }
}
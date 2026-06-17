using UnityEngine;
using FishNet.Object; // Добавили

public class Projectile : NetworkBehaviour // Изменили на NetworkBehaviour
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

    // В FishNet Start заменяем на OnStartNetwork или OnStartServer
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        // Удаляем объект через время (только на сервере)
        if (IsServer) Invoke(nameof(DestroyProjectile), lifetime);

        FindTarget();
    }

    private void DestroyProjectile()
    {
        ServerManager.Despawn(gameObject);
    }

    void Update()
    {
        // Поворот к цели (пусть работает у всех для красоты)
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
            Vector3 dirToEnemy = (col.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToEnemy);

            if (angle < closestAngle)
            {
                closestAngle = angle;
                bestTarget = col.transform;
            }
        }
        _target = bestTarget;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer) return;

        // 1. Пытаемся взять компонент здоровья у того, в кого попали
        if (other.TryGetComponent<IDamageable>(out var target))
        {
            // ПРОВЕРКА: Не попали ли мы в того, кто выпустил эту пулю?
            // Если у пули нет Owner (это враг), то она может бить Игрока.
            // Если Owner есть (это игрок), она не должна бить Игрока.

            bool hitByAI = (Owner.ClientId == -1 || Owner == null); // Пуля от врага
            bool targetIsPlayer = other.CompareTag("Player");

            if (hitByAI && targetIsPlayer)
            {
                target.TakeDamage(0, damage); // Враг попал в игрока
                DestroyProjectile();
            }
            else if (!targetIsPlayer)
            {
                target.TakeDamage(0, damage); // Игрок или враг попал в моба
                DestroyProjectile();
            }
        }
        else if (!other.isTrigger)
        {
            // Попадание в стену
            DestroyProjectile();
        }
    }
}
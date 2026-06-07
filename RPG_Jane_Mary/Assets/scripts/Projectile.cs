using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Основные настройки")]
    public float speed = 15f;
    public float damage = 20f;
    public float lifetime = 3f;

    [Header("Настройки самонаведения (Aim Assist)")]
    public float homingStrength = 5f;    // Сила доводки (чем выше, тем сильнее магнитит)
    public float detectionRange = 10f;   // Радиус поиска цели
    public float detectionAngle = 45f;   // Угол обзора (в градусах). 45 = только перед собой.
    public LayerMask enemyLayer;         // Слой врагов

    private Transform _target;

    void Start()
    {
        Destroy(gameObject, lifetime);
        FindTarget(); // Ищем цель один раз при запуске
    }

    void Update()
    {
        // Если цель есть и она жива, плавно поворачиваемся к ней
        if (_target != null)
        {
            Vector3 direction = (_target.position + Vector3.up - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Slerp плавно поворачивает шарик в сторону цели
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, homingStrength * Time.deltaTime);
        }

        // Всегда летим вперед (с учетом поворота)
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

            // Проверяем, входит ли враг в наш "конус прицеливания"
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
        // Проверяем, чтобы не попасть в самого себя (игрока)
        if (other.CompareTag("Player")) return;

        if (other.TryGetComponent<IDamageable>(out var target))
        {
            target.TakeDamage(0, damage);
            Destroy(gameObject);
        }
        else if (!other.isTrigger)
        {
            // Если врезались в стену (не триггер), тоже удаляем
            Destroy(gameObject);
        }
    }

    // Отрисовка радиуса поиска в редакторе для удобства
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
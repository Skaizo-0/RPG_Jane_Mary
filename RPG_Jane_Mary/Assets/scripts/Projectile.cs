using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Основные настройки")]
    public float speed = 15f;
    public float damage = 20f;
    public float lifetime = 3f;

    [Header("Настройки самонаведения (Aim Assist)")]
    public float homingStrength = 5f;    
    public float detectionRange = 10f;   
    public float detectionAngle = 45f;   
    public LayerMask enemyLayer;         

    private Transform _target;

    void Start()
    {
        Destroy(gameObject, lifetime);
        FindTarget(); 
    }

    void Update()
    {
        
        if (_target != null)
        {
            // вычисляем направление к цели
            Vector3 direction = (_target.position + Vector3.up - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(direction);

            // Slerp  поворачивает шарик в сторону цели
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, homingStrength * Time.deltaTime);
        }

        
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void FindTarget()
    {
        // поиск всех коллайдеров врагов в радиусе detectionRange
        Collider[] enemies = Physics.OverlapSphere(transform.position, detectionRange, enemyLayer);
        float closestAngle = detectionAngle;
        Transform bestTarget = null;

        foreach (var col in enemies)
        {
            Vector3 dirToEnemy = (col.transform.position - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, dirToEnemy);

            // враг находится в пределах видимости и он ближе к центру прицела чем предыдущий
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
    
    if (other.TryGetComponent<IDamageable>(out var target))
    {
        
        target.TakeDamage(0, damage);
        
   
        Destroy(gameObject);
    }
  
    else if (!other.isTrigger)
    {
        Destroy(gameObject);
    }
}

    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
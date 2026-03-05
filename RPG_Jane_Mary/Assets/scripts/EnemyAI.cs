using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    public enum Type { Melee, Ranged }
    public Type enemyType;
    public Transform player;
    public Animator animator;

    public float chaseDistance = 15f; // Радиус агрессии
    public float attackDist = 2.5f;   // Дистанция для атаки
    public float speed = 2f;

    [Header("Для Мага")]
    public GameObject magicPrefab;
    public Transform firePoint;

    private Health _health;
    private float _attackCooldown = 2f;
    private float _lastAttackTime;

    void Start()
    {
        _health = GetComponent<Health>();
        _health.OnDeath += () => {
            this.enabled = false;
            if (GetComponent<CharacterController>()) GetComponent<CharacterController>().enabled = false;
        };

        // Если это маг, он должен бить издалека
        if (enemyType == Type.Ranged) attackDist = 8f;
    }

    void Update()
    {
        if (player == null || _health.currentHp <= 0) return;

        float dist = Vector3.Distance(transform.position, player.position);

        // Если игрок в радиусе видимости
        if (dist < chaseDistance)
        {
            // Всегда смотрим на игрока
            Vector3 lookDir = player.position - transform.position;
            lookDir.y = 0;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), 5f * Time.deltaTime);

            if (dist > attackDist)
            {
                // Идем к игроку
                transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
                animator.SetFloat("Speed", 0.5f);
            }
            else
            {
                // Остановились и атакуем
                animator.SetFloat("Speed", 0);
                if (Time.time > _lastAttackTime + _attackCooldown)
                {
                    Attack();
                }
            }
        }
        else
        {
            animator.SetFloat("Speed", 0);
        }
    }

    void Attack()
    {
        _lastAttackTime = Time.time;
        string trigger = (enemyType == Type.Melee) ? "AttackPh" : "AttackMa";
        animator.SetTrigger(trigger);

        // Урон наносим с задержкой (под анимацию)
        if (enemyType == Type.Melee) Invoke("ApplyMeleeDamage", 0.6f);
        else Invoke("LaunchMagic", 0.6f);
    }

    void ApplyMeleeDamage()
    {
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackDist + 1f)
            player.GetComponent<IDamageable>()?.TakeDamage(10, 0);
    }

    void LaunchMagic()
    {
        if (firePoint && magicPrefab)
            Instantiate(magicPrefab, firePoint.position, transform.rotation);
    }
}
using UnityEngine;
public enum EnemyType { Melee, Ranged }
public class EnemyAI : MonoBehaviour
{
    public EnemyType type;
    public Transform player;
    public Animator animator;
    public float speed = 3f;
    public float chaseDistance = 10f;

    [Header("Настройки атаки")]
    public float attackDistance = 2.2f;
    public float attackCooldown = 2.5f;
    private float nextAttackTime;
    private bool isAttacking = false; // Блокировщик движения

    [Header("Для Мага")]
    public GameObject magicPrefab;
    public Transform firePoint;

    void Update()
    {
        if (player == null || isAttacking) return; // Если атакуем — ничего не делаем в Update

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance < chaseDistance)
        {
            SmoothRotateToPlayer();

            if (distance > attackDistance)
            {
                MoveToPlayer();
            }
            else
            {
                StopMoving();
                if (Time.time >= nextAttackTime)
                {
                    StartAttack();
                }
            }
        }
        else
        {
            StopMoving();
        }
    }

    void StartAttack()
    {
        isAttacking = true; // Запрещаем двигаться
        string trigger = (type == EnemyType.Melee) ? "AttackPh" : "AttackMa";
        animator.SetTrigger(trigger);

        nextAttackTime = Time.time + attackCooldown;

        // Ждем завершения анимации (например, 1.5 секунды), прежде чем снова разрешить ходить
        Invoke("EndAttack", 1.5f);

        // Нанесение урона
        if (type == EnemyType.Melee) Invoke("DealMeleeDamage", 0.6f);
        else Invoke("SpawnMagic", 0.6f);
    }

    void EndAttack()
    {
        isAttacking = false; // Снова можно ходить
    }

    void MoveToPlayer()
    {
        transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        animator.SetFloat("Speed", 0.5f);
    }

    void StopMoving()
    {
        animator.SetFloat("Speed", 0);
    }

    void SmoothRotateToPlayer()
    {
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }
    }

    void DealMeleeDamage()
    {
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackDistance + 0.5f)
        {
            player.GetComponent<Health>().TakeDamage(10, 0);
        }
    }

    void SpawnMagic()
    {
        if (firePoint != null) Instantiate(magicPrefab, firePoint.position, transform.rotation);
    }
}
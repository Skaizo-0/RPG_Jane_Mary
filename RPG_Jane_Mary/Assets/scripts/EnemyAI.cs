using UnityEngine;


public enum EnemyType { Melee, Ranged }

public class EnemyAI : MonoBehaviour
{
    public EnemyType enemyType;
    public Transform player;
    public Animator animator;

    public float chaseDistance = 15f;
    public float attackDist = 2.5f;
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


        if (enemyType == EnemyType.Ranged) attackDist = 8f;

 
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
    }

    void Update()
    {

        if (player == null || _health.CurrentHealth <= 0)
        {
            StopMoving();
            return;
        }

        float dist = Vector3.Distance(transform.position, player.position);

        if (dist < chaseDistance)
        {
            SmoothRotateToPlayer();

            if (dist > attackDist)
            {
                MoveToPlayer();
            }
            else
            {
                StopMoving();
                if (Time.time > _lastAttackTime + _attackCooldown)
                {
                    Attack();
                }
            }
        }
        else
        {
            StopMoving();
        }
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
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), 5f * Time.deltaTime);
        }
    }

    void Attack()
    {
        _lastAttackTime = Time.time;
     
        string trigger = (enemyType == EnemyType.Melee) ? "AttackPh" : "AttackMa";
        animator.SetTrigger(trigger);

        if (enemyType == EnemyType.Melee) Invoke("ApplyMeleeDamage", 0.6f);
        else Invoke("LaunchMagic", 0.6f);
    }

    void ApplyMeleeDamage()
    {

        if (player != null && Vector3.Distance(transform.position, player.position) <= attackDist + 1f)
        {
           
            if (player.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(10, 0);
            }
        }
    }

    void LaunchMagic()
    {
        if (firePoint && magicPrefab)
        {
            Instantiate(magicPrefab, firePoint.position, transform.rotation);
        }
    }
}
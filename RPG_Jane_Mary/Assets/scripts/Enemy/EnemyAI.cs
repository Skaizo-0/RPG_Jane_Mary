using UnityEngine;

public enum EnemyType { Melee, Ranged }

public class EnemyAI : MonoBehaviour
{
    public EnemyType enemyType;
    public Transform player;
    public Animator animator;
    public CharacterController controller;

    [Header("Настройки ИИ")]
    public float chaseDistance = 15f;
    public float attackDist = 2.5f;
    public float speed = 2f;
    public bool isPeaceful;

    [Header("Для Мага")]
    public GameObject magicPrefab;
    public Transform firePoint;

    public EnemyStateMachine StateMachine { get; private set; }
    public IdleState IdleState { get; private set; }
    public AggroState AggroState { get; private set; }
    public AttackState AttackState { get; private set; }
    public FleeState FleeState { get; private set; }

    public Health Health { get; private set; }
    protected float _attackCooldown = 2f;
    protected float _lastAttackTime;

    protected virtual void Awake()
    {
        Health = GetComponent<Health>();
        controller = GetComponent<CharacterController>();

        StateMachine = new EnemyStateMachine();
        IdleState = new IdleState(this, StateMachine);
        AggroState = new AggroState(this, StateMachine);
        AttackState = new AttackState(this, StateMachine);
        FleeState = new FleeState(this, StateMachine);
    }

    protected virtual void Start()
    {
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (enemyType == EnemyType.Ranged) attackDist = 8f;

        // ИСПРАВЛЕННАЯ ЛОГИКА АГРА
        Health.OnHealthChanged += (cur, max) =>
        {
            if (gameObject.CompareTag("Boss"))
            {
                // Босс агрится сразу при первом же уроне
                if (cur < max) isPeaceful = false;
            }
            // Для остальных мобов ничего не меняем — они остаются в том режиме, 
            // который ты задала в инспекторе (мирные так мирные).
        };

        StateMachine.Initialize(IdleState);
    }

    protected virtual void Update()
    {
        if (player == null || Health.CurrentHealth <= 0)
        {
            StopMoving();
            return;
        }
        StateMachine.CurrentState.Update();
    }

    public void MoveToPlayer()
    {
        Vector3 dir = (player.position - transform.position).normalized;
        dir.y = 0;
        controller.Move(dir * speed * Time.deltaTime);
        animator.SetFloat("Speed", 0.5f);
    }

    public void MoveInDirection(Vector3 dir)
    {
        dir.y = 0;
        controller.Move(dir * speed * Time.deltaTime);
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 5f * Time.deltaTime);
        animator.SetFloat("Speed", 0.5f);
    }

    public void StopMoving() { animator.SetFloat("Speed", 0); }

    public void SmoothRotateToPlayer()
    {
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), 5f * Time.deltaTime);
    }

    public virtual void BossPerformAction() { if (enemyType == EnemyType.Ranged) LaunchMagic(); else ApplyMeleeDamage(); }

    public virtual void TryAttackLogic()
    {
        if (Time.time > _lastAttackTime + _attackCooldown)
        {
            _lastAttackTime = Time.time;
            string trigger = (enemyType == EnemyType.Melee) ? "AttackPh" : "AttackMa";
            animator.SetTrigger(trigger);
            if (enemyType == EnemyType.Melee) Invoke("ApplyMeleeDamage", 0.6f);
            else Invoke("LaunchMagic", 0.6f);
        }
    }

    public virtual void ApplyMeleeDamage()
    {
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackDist + 1f)
        {
            if (player.TryGetComponent<IDamageable>(out var target)) target.TakeDamage(10, 0);
        }
    }

    protected void LaunchMagic()
    {
        if (firePoint && magicPrefab)
        {
            Vector3 targetDir = (player.position + Vector3.up - firePoint.position).normalized;
            Instantiate(magicPrefab, firePoint.position, Quaternion.LookRotation(targetDir));
        }
    }
}
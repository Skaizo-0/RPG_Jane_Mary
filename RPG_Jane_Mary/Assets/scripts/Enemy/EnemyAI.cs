using UnityEngine;
public enum EnemyType { Melee, Ranged }
public class EnemyAI : MonoBehaviour
{
    public EnemyType enemyType;
    public Transform player;
    public Animator animator;

    [Header("Настройки ИИ")]
    public float chaseDistance = 15f;
    public float attackDist = 2.5f;
    public float speed = 2f;
    public bool isPeaceful; // Для ТЗ: мирный режим

    [Header("Для Мага")]
    public GameObject magicPrefab;
    public Transform firePoint;

    // Ссылки на машину состояний и сами состояния
    public EnemyStateMachine StateMachine { get; private set; }
    public IdleState IdleState { get; private set; }
    public AggroState AggroState { get; private set; }
    public AttackState AttackState { get; private set; }
    public FleeState FleeState { get; private set; }

    public Health Health { get; private set; }
    private float _attackCooldown = 2f;
    private float _lastAttackTime;

    private void Awake()
    {
        Health = GetComponent<Health>();

        // Инициализация FSM
        StateMachine = new EnemyStateMachine();
        IdleState = new IdleState(this, StateMachine);
        AggroState = new AggroState(this, StateMachine);
        AttackState = new AttackState(this, StateMachine);
        FleeState = new FleeState(this, StateMachine);
    }

    protected virtual void Start()
    {
        // ... (твой старый код Start) ...

        // ПОДПИСКА НА УРОН
        Health.OnHealthChanged += (cur, max) =>
        {
            // Если это Босс (можно определить по типу или тегу)
            // Он выходит из мирного режима при первом ударе
            if (gameObject.CompareTag("Boss") && cur < max)
            {
                isPeaceful = false;
            }

            // Если это обычный моб - мы НИЧЕГО не делаем. 
            // Он остается isPeaceful = true, поэтому AggroState не включится.
            // Он просто дождется, пока cur < 30% и уйдет в FleeState из Idle.
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

        // ВСЯ логика теперь внутри текущего состояния
        StateMachine.CurrentState.Update();
    }

    // --- Методы, которые вызываются из Состояний ---

    public void MoveToPlayer()
    {
        transform.position = Vector3.MoveTowards(transform.position, player.position, speed * Time.deltaTime);
        animator.SetFloat("Speed", 0.5f);
    }

    public void MoveInDirection(Vector3 dir)
    {
        dir.y = 0;
        transform.position += dir * speed * Time.deltaTime;
        if (dir != Vector3.zero)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 5f * Time.deltaTime);
        animator.SetFloat("Speed", 0.5f);
    }

    public void StopMoving()
    {
        animator.SetFloat("Speed", 0);
    }

    public void SmoothRotateToPlayer()
    {
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        if (lookDir != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), 5f * Time.deltaTime);
        }
    }

    public virtual void BossPerformAction()
    {
        // Обычные и редкие мобы просто вызывают свою стандартную логику урона
        // Мы можем просто оставить это место пустым или вызвать магию
        if (enemyType == EnemyType.Ranged)
        {
            LaunchMagic();
        }
        else
        {
            ApplyMeleeDamage();
        }
    }

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
            if (player.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(10, 0);
            }
        }
    }

    private void LaunchMagic()
    {
        if (firePoint && magicPrefab)
        {
            Instantiate(magicPrefab, firePoint.position, transform.rotation);
        }
    }
}
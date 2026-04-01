using UnityEngine;
public enum ElementType { Fire, Ice, Earth, Ether }
public class BossAI : EnemyAI
{
    [Header("Настройки Босса (Лаба 6)")]
    public ElementType currentElement;
    public bool isMeleeWeapon;

    [Header("Ссылки на оружие")]
    public GameObject rangedStaff;
    public GameObject meleeStaff;

    [Header("Уникальные эффекты стихий")]
    public AudioSource audioSource;
    public AudioClip[] elementSounds;
    public GameObject[] elementProjectiles;

    [Header("Баланс Босса")]
    public float damageMultiplier = 1f;

    public StrongAttackState StrongAttackState { get; private set; }
    public DefensiveState DefensiveState { get; private set; }
    public EnragedState EnragedState { get; private set; }

    private int _attackCounter = 0;
    private bool _isEnraged = false;
    private bool _isFleeing = false;

    protected override void Awake()
    {
        base.Awake(); // Создает Idle, Aggro, Attack, Flee
        StrongAttackState = new StrongAttackState(this, StateMachine);
        DefensiveState = new DefensiveState(this, StateMachine);
        EnragedState = new EnragedState(this, StateMachine);
    }

    protected override void Start()
    {
        base.Start(); // Здесь включится агр при первом ударе
        SetupBoss();
    }

    protected override void Update()
    {
        if (player == null || Health.CurrentHealth <= 0) return;

        // 1. УСЛОВИЕ БЕГСТВА (HP < 15%)
        // Если здоровья критически мало, босс бросает всё и убегает
        if (!_isFleeing && Health.CurrentHealth < (Health.MaxHealth * 0.15f))
        {
            _isFleeing = true;
            StateMachine.ChangeState(FleeState);
            return;
        }

        // 2. ПЕРЕХОД В ЯРОСТЬ (HP < 50%)
        // Босс злится, увеличивает урон и проигрывает анимацию Roar
        if (!_isEnraged && Health.CurrentHealth < (Health.MaxHealth * 0.5f))
        {
            _isEnraged = true;
            damageMultiplier = 2f; // Урон в 2 раза выше
            StateMachine.ChangeState(EnragedState);
            return;
        }

        base.Update();
    }

    void SetupBoss()
    {
        if (rangedStaff != null) rangedStaff.SetActive(!isMeleeWeapon);
        if (meleeStaff != null) meleeStaff.SetActive(isMeleeWeapon);
        attackDist = isMeleeWeapon ? 2.5f : 8f;
        _attackCooldown = 3f;
    }

    public override void TryAttackLogic()
    {
        // В ярости атакует в 2 раза быстрее
        float actualCD = _isEnraged ? _attackCooldown / 2f : _attackCooldown;
        if (Time.time < _lastAttackTime + actualCD) return;

        _lastAttackTime = Time.time;
        _attackCounter++;

        if (_attackCounter >= 3)
        {
            _attackCounter = 0;
            StateMachine.ChangeState(StrongAttackState);
        }
        else
        {
            string trigger = isMeleeWeapon ? "AttackPh" : "AttackMa";
            animator.SetTrigger(trigger);

            // Шанс уйти в защиту только если босс НЕ в ярости
            if (!_isEnraged && Random.value > 0.8f)
                StateMachine.ChangeState(DefensiveState);
        }
    }

    public override void BossPerformAction()
    {
        // Если игрок успел убежать далеко, пока босс махал палкой — не стреляем
        if (Vector3.Distance(transform.position, player.position) > attackDist + 3f) return;

        int index = (int)currentElement;
        if (isMeleeWeapon)
        {
            if (elementSounds.Length > index && elementSounds[index] != null)
                audioSource.PlayOneShot(elementSounds[index]);

            ApplyBossDamage(10f * damageMultiplier);
        }
        else
        {
            if (elementProjectiles.Length > index && elementProjectiles[index] != null)
            {
                // Направление точно на игрока
                Vector3 targetDir = (player.position + Vector3.up - firePoint.position).normalized;
                Instantiate(elementProjectiles[index], firePoint.position, Quaternion.LookRotation(targetDir));
            }
        }
    }

    private void ApplyBossDamage(float finalDamage)
    {
        if (player != null && Vector3.Distance(transform.position, player.position) <= attackDist + 1.5f)
        {
            if (player.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(finalDamage, 0);
            }
        }
    }
}
using UnityEngine;
using FishNet.Object;

// Объявляем ElementType здесь, чтобы ошибка исчезла
public enum ElementType { Fire, Ice, Earth, Ether }

public class BossAI : EnemyAI
{
    [Header("Настройки Босса")]
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
        base.Awake();
        StrongAttackState = new StrongAttackState(this, StateMachine);
        DefensiveState = new DefensiveState(this, StateMachine);
        EnragedState = new EnragedState(this, StateMachine);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        SetupBoss();
    }

    protected override void Update()
    {
        if (!IsServer || player == null || Health.CurrentHealth <= 0) return;

        if (!_isFleeing && Health.CurrentHealth < (Health.MaxHealth * 0.15f))
        {
            _isFleeing = true;
            StateMachine.ChangeState(FleeState);
            return;
        }

        if (!_isEnraged && Health.CurrentHealth < (Health.MaxHealth * 0.5f))
        {
            _isEnraged = true;
            damageMultiplier = 2f;
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
        if (!IsServer) return;

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

            if (!_isEnraged && Random.value > 0.8f)
                StateMachine.ChangeState(DefensiveState);
        }
    }

    // Теперь метод override работает, так как он есть в EnemyAI
    public override void BossPerformAction()
    {
        if (!IsServer || player == null) return;
        if (Vector3.Distance(transform.position, player.position) > attackDist + 3f) return;

        int index = (int)currentElement;

        if (isMeleeWeapon)
        {
            PlayElementSoundObserversRpc(index);
            ApplyBossDamage(10f * damageMultiplier);
        }
        else
        {
            if (elementProjectiles.Length > index && elementProjectiles[index] != null)
            {
                Vector3 targetDir = (player.position + Vector3.up - firePoint.position).normalized;
                GameObject projectile = Instantiate(elementProjectiles[index], firePoint.position, Quaternion.LookRotation(targetDir));
                ServerManager.Spawn(projectile);
            }
        }
    }

    [ObserversRpc]
    private void PlayElementSoundObserversRpc(int index)
    {
        if (elementSounds.Length > index && elementSounds[index] != null && audioSource != null)
        {
            audioSource.PlayOneShot(elementSounds[index]);
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
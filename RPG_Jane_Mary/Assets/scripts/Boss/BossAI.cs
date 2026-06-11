using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

// Объявляем типы стихий
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
    public AudioSource bossAudioSource; // Переименовал, чтобы не путать с базовым
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
        // Инициализация уникальных состояний босса
        StrongAttackState = new StrongAttackState(this, StateMachine);
        DefensiveState = new DefensiveState(this, StateMachine);
        EnragedState = new EnragedState(this, StateMachine);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        // Настраиваем оружие при старте на сервере
        SetupBoss();
        // Синхронизируем видимость оружия у всех клиентов
        SyncBossVisualsObserversRpc(isMeleeWeapon);
    }

    protected override void Update()
    {
        // Только сервер управляет фазами босса
        if (!IsServer) return;
        if (Health.CurrentHealth <= 0) return;

        // Фаза бегства (15% HP)
        if (!_isFleeing && Health.CurrentHealth < (Health.MaxHealth * 0.15f))
        {
            _isFleeing = true;
            StateMachine.ChangeState(FleeState);
            return;
        }

        // Фаза ярости (50% HP)
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

        attackDist = isMeleeWeapon ? 2.5f : 10f;
        _attackCooldown = 3f;
    }

    // Синхронизируем визуальное состояние оружия
    [ObserversRpc(BufferLast = true)]
    private void SyncBossVisualsObserversRpc(bool meleeActive)
    {
        if (rangedStaff != null) rangedStaff.SetActive(!meleeActive);
        if (meleeStaff != null) meleeStaff.SetActive(meleeActive);
    }

    public override void TryAttackLogic()
    {
        if (!IsServer || player == null) return;

        // Если в ярости - атакует в 2 раза чаще
        float actualCD = _isEnraged ? _attackCooldown / 2f : _attackCooldown;
        if (Time.time < _lastAttackTime + actualCD) return;

        _lastAttackTime = Time.time;
        _attackCounter++;

        // Каждый 3-й удар - суперудар
        if (_attackCounter >= 3)
        {
            _attackCounter = 0;
            StateMachine.ChangeState(StrongAttackState);
        }
        else
        {
            // Обычная атака
            string trigger = isMeleeWeapon ? "AttackPh" : "AttackMa";

            // Запускаем анимацию у всех
            PlayBossAttackAnimationObserversRpc(trigger);

            // Шанс уйти в защиту (если не в ярости)
            if (!_isEnraged && Random.value > 0.8f)
                StateMachine.ChangeState(DefensiveState);
        }
    }

    [ObserversRpc]
    private void PlayBossAttackAnimationObserversRpc(string trigger)
    {
        if (animator != null) animator.SetTrigger(trigger);
    }

    // Метод вызывается из анимации (Animation Event)
    public override void BossPerformAction()
    {
        if (!IsServer || player == null) return;

        int index = (int)currentElement;

        if (isMeleeWeapon)
        {
            // Ближний бой: звук + урон
            PlayElementSoundObserversRpc(index);
            ApplyBossDamage(10f * damageMultiplier);
        }
        else
        {
            // Дальний бой: спавн магического снаряда стихии
            if (elementProjectiles.Length > index && elementProjectiles[index] != null)
            {
                Vector3 targetDir = (player.position + Vector3.up * 1.3f - firePoint.position).normalized;
                GameObject projectile = Instantiate(elementProjectiles[index], firePoint.position, Quaternion.LookRotation(targetDir));

                // СПАВНИМ В СЕТИ (чтобы все увидели шар)
                ServerManager.Spawn(projectile);
            }
        }
    }

    [ObserversRpc]
    private void PlayElementSoundObserversRpc(int index)
    {
        if (elementSounds.Length > index && elementSounds[index] != null && bossAudioSource != null)
        {
            bossAudioSource.PlayOneShot(elementSounds[index]);
        }
    }

    private void ApplyBossDamage(float finalDamage)
    {
        if (player == null) return;

        float dist = Vector3.Distance(transform.position, player.position);
        if (dist <= attackDist + 2f)
        {
            // Ищем здоровье в родителе (фикс для 2-го игрока)
            Health targetHealth = player.GetComponentInParent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(finalDamage, 0);
                Debug.Log($"[SERVER] Босс нанес {finalDamage} урона игроку {player.name}");
            }
        }
    }
}
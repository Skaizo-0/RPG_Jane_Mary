using UnityEngine;
using FishNet.Object; // Обязательно для сети

public class PlayerCombat : NetworkBehaviour // Изменили на NetworkBehaviour
{
    public Animator animator;
    public Transform firePoint;
    public GameObject magicPrefab;

    [Header("Настройки урона")]
    public float physDamage = 20f;
    public float physRange = 2.5f;
    public LayerMask enemyLayer;

    [Header("Исправление поворота Mixamo")]
    public float physRotationOffset = 0f;
    public float magicRotationOffset = 0f;

    [Header("Кулдаун магии (ТЗ)")]
    public float magicCooldown = 3f;
    private float _lastMagicTime = -10f;

    private IInputService _input;
    private Transform _cam;

    public float MagicReadyProgress => Mathf.Clamp01((Time.time - _lastMagicTime) / magicCooldown);

    public void Construct(IInputService input)
    {
        _input = input;
        if (Camera.main != null) _cam = Camera.main.transform;
    }

    void Update()
    {
        // Только владелец персонажа может нажимать на кнопки атаки
        if (!IsOwner || _input == null) return;

        if (_input.AttackPhys)
        {
            // 1. Поворачиваемся локально (для мгновенного отклика)
            RotateToCamera(physRotationOffset);

            // 2. Просим сервер выполнить атаку
            ProcessPhysicalAttackServerRpc();
        }

        if (_input.AttackMag && Time.time >= _lastMagicTime + magicCooldown)
        {
            _lastMagicTime = Time.time;
            RotateToCamera(magicRotationOffset);

            // Просим сервер заспавнить магию
            ProcessMagicAttackServerRpc();
        }
    }

    private void RotateToCamera(float offset)
    {
        if (_cam == null) _cam = Camera.main.transform;

        Vector3 camForward = _cam.forward;
        camForward.y = 0;
        if (camForward != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(camForward);
            lookRotation *= Quaternion.Euler(0, offset, 0);
            transform.rotation = lookRotation;
        }
    }

    // --- СЕТЕВАЯ ЛОГИКА ---

    [ServerRpc] // Выполняется на сервере
    private void ProcessPhysicalAttackServerRpc()
    {
        // Сервер говорит всем клиентам запустить анимацию
        PlayAttackAnimationObserversRpc("AttackPhys");

        // Сервер сам вызывает расчет урона (потому что только серверу доверяем HP)
        DealPhysDamage();
    }

    [ServerRpc] // Выполняется на сервере
    private void ProcessMagicAttackServerRpc()
    {
        // Сервер говорит всем клиентам запустить анимацию
        PlayAttackAnimationObserversRpc("AttackMag");

        // Магия заспавнится через Animation Event (метод ShootMagic)
        // Но в сетевой игре ShootMagic тоже должен быть серверным.
    }

    [ObserversRpc] // Выполняется у всех игроков на экране
    private void PlayAttackAnimationObserversRpc(string triggerName)
    {
        if (animator != null) animator.SetTrigger(triggerName);
    }

    // Этот метод вызывается анимацией (Animation Event)
    public void ShootMagic()
    {
        // Только сервер имеет право создавать сетевые объекты
        if (!IsServer) return;

        if (magicPrefab != null && firePoint != null)
        {
            GameObject ball = Instantiate(magicPrefab, firePoint.position, firePoint.rotation);

            // ОЧЕНЬ ВАЖНО: В FishNet нужно "заспавнить" объект в сети, чтобы все его увидели
            ServerManager.Spawn(ball, Owner);
        }
    }

    // Этот метод вызывается анимацией или из RPC
    public void DealPhysDamage()
    {
        // Только сервер обсчитывает урон
        if (!IsServer) return;

        Vector3 pos = transform.position + transform.forward * 1.5f + Vector3.up;
        Collider[] enemies = Physics.OverlapSphere(pos, physRange, enemyLayer);

        foreach (var enemy in enemies)
        {
            if (enemy.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(physDamage, 0);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 pos = transform.position + transform.forward * 1.5f + Vector3.up;
        Gizmos.DrawWireSphere(pos, physRange);
    }
}
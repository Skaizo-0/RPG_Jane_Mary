using UnityEngine;
using FishNet.Object; // Обязательно для сети

public class PlayerCombat : NetworkBehaviour
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

    // Переменная для хранения направления выстрела на сервере
    private Vector3 _serverShootDirection;

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
            RotateToCamera(physRotationOffset);
            ProcessPhysicalAttackServerRpc();
        }

        if (_input.AttackMag && Time.time >= _lastMagicTime + magicCooldown)
        {
            _lastMagicTime = Time.time;
            RotateToCamera(magicRotationOffset);

            // КРИТИЧЕСКИЙ ФИКС: Берем направление нашей камеры и шлем его серверу
            Vector3 lookDirection = _cam.forward;
            ProcessMagicAttackServerRpc(lookDirection);
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

    [ServerRpc]
    private void ProcessPhysicalAttackServerRpc()
    {
        PlayAttackAnimationObserversRpc("AttackPhys");
        DealPhysDamage();
    }

    [ServerRpc]
    private void ProcessMagicAttackServerRpc(Vector3 lookDir)
    {
        // На сервере запоминаем, куда именно целился игрок (любой: первый или второй)
        _serverShootDirection = lookDir;
        _serverShootDirection.y = 0; // Чтобы пуля не летела в небо или в землю

        // Поворачиваем персонажа на сервере, чтобы ShootMagic сработал верно
        if (_serverShootDirection != Vector3.zero)
        {
            transform.forward = _serverShootDirection;
        }

        PlayAttackAnimationObserversRpc("AttackMag");
    }

    [ObserversRpc]
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
            // Используем transform.rotation, который мы обновили в RPC выше
            GameObject ball = Instantiate(magicPrefab, firePoint.position, transform.rotation);

            // Заспавнить в сети, чтобы все увидели
            ServerManager.Spawn(ball, Owner);
        }
    }

    // Этот метод вызывается анимацией или из RPC
    public void DealPhysDamage()
    {
        if (!IsServer) return;

        Vector3 pos = transform.position + transform.forward * 1.5f + Vector3.up;
        Collider[] enemies = Physics.OverlapSphere(pos, physRange, enemyLayer);

        foreach (var enemy in enemies)
        {
            // Ищем здоровье в родителе (фикс для костей Mixamo)
            Health targetHealth = enemy.GetComponentInParent<Health>();
            if (targetHealth != null)
            {
                targetHealth.TakeDamage(physDamage, 0);
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
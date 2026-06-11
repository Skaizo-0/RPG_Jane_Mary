using UnityEngine;
using FishNet.Object;

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

    // --- НОВОЕ: Ссылка на свои очки ---
    private PlayerScore _myScore;

    public float MagicReadyProgress => Mathf.Clamp01((Time.time - _lastMagicTime) / magicCooldown);

    public void Construct(IInputService input)
    {
        _input = input;
        if (Camera.main != null) _cam = Camera.main.transform;
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        // Запоминаем ссылку на свои очки
        _myScore = GetComponent<PlayerScore>();
    }

    void Update()
    {
        if (!IsOwner) return;

        if (_input == null)
        {
            _input = new StandaloneInput();
        }

        if (_input.AttackPhys)
        {
            RotateToCamera(physRotationOffset);
            ProcessPhysicalAttackServerRpc();
        }

        if (_input.AttackMag && Time.time >= _lastMagicTime + magicCooldown)
        {
            _lastMagicTime = Time.time;
            RotateToCamera(magicRotationOffset);
            ProcessMagicAttackServerRpc();
        }
    }

    private void RotateToCamera(float offset)
    {
        if (_cam == null && Camera.main != null) _cam = Camera.main.transform;
        if (_cam == null) return;

        Vector3 camForward = _cam.forward;
        camForward.y = 0;
        if (camForward != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(camForward);
            lookRotation *= Quaternion.Euler(0, offset, 0);
            transform.rotation = lookRotation;
        }
    }

    [ServerRpc]
    private void ProcessPhysicalAttackServerRpc()
    {
        PlayAttackAnimationObserversRpc("AttackPhys");
        DealPhysDamage();
    }

    [ServerRpc]
    private void ProcessMagicAttackServerRpc()
    {
        PlayAttackAnimationObserversRpc("AttackMag");
    }

    [ObserversRpc]
    private void PlayAttackAnimationObserversRpc(string triggerName)
    {
        if (animator != null) animator.SetTrigger(triggerName);
    }

    public void ShootMagic()
    {
        if (!IsServer) return;

        if (magicPrefab != null && firePoint != null)
        {
            GameObject ball = Instantiate(magicPrefab, firePoint.position, firePoint.rotation);
            ServerManager.Spawn(ball, Owner);
        }
    }

    public void DealPhysDamage()
    {
        if (!IsServer) return;

        Vector3 pos = transform.position + transform.forward * 1.5f + Vector3.up;
        Collider[] enemies = Physics.OverlapSphere(pos, physRange, enemyLayer);

        foreach (var enemy in enemies)
        {
            Health targetHealth = enemy.GetComponentInParent<Health>();
            if (targetHealth != null)
            {
                // ИЗМЕНЕНО: Передаем _myScore, чтобы сервер знал, кому дать очко
                targetHealth.TakeDamageWithAttacker(physDamage, 0, _myScore);
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
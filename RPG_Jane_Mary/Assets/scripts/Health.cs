using UnityEngine;
using System;
using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing; // Обязательно

public class Health : NetworkBehaviour, IDamageable
{
    [Header("Параметры HP")]
    public float maxHp = 100f;

    // В FishNet 4.0+ используем readonly SyncVar<T>
    private readonly SyncVar<float> _currentHp = new SyncVar<float>();

    public static event Action<GameObject> OnEnemyDeath;
    public event Action<float, float> OnHealthChanged;
    public static event Action OnPlayerDeath;

    // Читаем значение через .Value
    public float CurrentHealth => _currentHp.Value;
    public float MaxHealth => maxHp;

    [Header("Ссылки")]
    public Animator animator;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        // Подписываемся на обновление здоровья
        _currentHp.OnChange += OnHpChanged;

        if (IsServer)
        {
            _currentHp.Value = maxHp;
        }
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        // Отписываемся при уничтожении
        _currentHp.OnChange -= OnHpChanged;
    }

    private void OnHpChanged(float prev, float next, bool asServer)
    {
        OnHealthChanged?.Invoke(next, maxHp);
    }

    public void TakeDamage(float phys, float mag)
    {
        if (!IsServer) return;

        if (_currentHp.Value <= 0) return;

        // Меняем через .Value
        _currentHp.Value = Mathf.Clamp(_currentHp.Value - (phys + mag), 0, maxHp);

        if (_currentHp.Value > 0)
        {
            PlayHitObserversRpc();
        }
        else
        {
            Die();
        }
    }

    [ObserversRpc]
    private void PlayHitObserversRpc()
    {
        if (animator) animator.SetTrigger("GetHit");
        if (CompareTag("Player")) StartCoroutine(StunRoutine());
    }

    public void SetHealth(float amount)
    {
        if (IsServer) _currentHp.Value = amount;
    }

    private void Die()
    {
        if (!IsServer) return;

        PlayDieObserversRpc();

        if (gameObject.CompareTag("Player"))
        {
            OnPlayerDeath?.Invoke();
            DisableMovementObserversRpc();
        }
        else
        {
            OnEnemyDeath?.Invoke(gameObject);
            ServerManager.Despawn(gameObject);
        }
    }

    [ObserversRpc]
    private void PlayDieObserversRpc()
    {
        if (animator) animator.SetTrigger("Die");
    }

    [ObserversRpc]
    private void DisableMovementObserversRpc()
    {
        var move = GetComponent<PlayerMovement>();
        if (move != null) move.enabled = false;
    }

    private IEnumerator StunRoutine()
    {
        var move = GetComponent<PlayerMovement>();
        if (move != null) move.enabled = false;
        yield return new WaitForSeconds(0.5f);
        if (_currentHp.Value > 0 && move != null) move.enabled = true;
    }
}
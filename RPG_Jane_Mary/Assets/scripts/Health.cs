using UnityEngine;
using System;
using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class Health : NetworkBehaviour, IDamageable
{
    [Header("Параметры HP")]
    public float maxHp = 100f;
    private readonly SyncVar<float> _currentHp = new SyncVar<float>();
    private bool _isDead = false;

    public static event Action<GameObject> OnEnemyDeath;
    public event Action<float, float> OnHealthChanged;
    public static event Action OnPlayerDeath;
    public static System.Action OnAnyPlayerDeath;

    public float CurrentHealth => _currentHp.Value;
    public float MaxHealth => maxHp;
    public Animator animator;

    // Переменная для хранения последнего ударившего (на сервере)
    private PlayerScore _lastAttacker;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        _currentHp.OnChange += OnHpChanged;
        if (IsServer) _currentHp.Value = maxHp;
        _isDead = false;
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        _currentHp.OnChange -= OnHpChanged;
    }

    private void OnHpChanged(float prev, float next, bool asServer)
    {
        OnHealthChanged?.Invoke(next, maxHp);
    }

    // ИЗМЕНЕНО: теперь принимает объект атакующего
    public void TakeDamage(float phys, float mag)
    {
        // Этот метод для совместимости с интерфейсом, вызываем расширенный
        TakeDamageExtended(phys, mag, null);
    }

    public void TakeDamageExtended(float phys, float mag, PlayerScore attacker)
    {
        if (!IsServer || _isDead) return;

        // Запоминаем, кто ударил последним
        if (attacker != null) _lastAttacker = attacker;

        if (_currentHp.Value <= 0) return;

        _currentHp.Value = Mathf.Clamp(_currentHp.Value - (phys + mag), 0, maxHp);

        if (_currentHp.Value > 0) PlayHitObserversRpc();
        else Die();
    }

    [ObserversRpc]
    private void PlayHitObserversRpc()
    {
        if (animator) animator.SetTrigger("GetHit");
        if (CompareTag("Player")) StartCoroutine(StunRoutine());
    }

    private void Die()
    {
        if (!IsServer || _isDead) return;
        _isDead = true;

        // Если это моб и у него был атакующий - начисляем ему очко
        if (!gameObject.CompareTag("Player") && _lastAttacker != null)
        {
            _lastAttacker.AddPointServerRpc();
            Debug.Log($"[SERVER] Очко начислено игроку: {_lastAttacker.OwnerId}");
        }

        PlayDieObserversRpc();

        if (gameObject.CompareTag("Player"))
        {
            OnPlayerDeath?.Invoke();
            OnAnyPlayerDeath?.Invoke();
            DisablePlayerSystemsObserversRpc(true);
        }
        else
        {
            OnEnemyDeath?.Invoke(gameObject);
            StartCoroutine(DestroyAfterDelay());
        }
    }

    [ObserversRpc]
    private void PlayDieObserversRpc()
    {
        if (animator)
        {
            animator.SetTrigger("Die");
            animator.ResetTrigger("AttackPhys");
            animator.ResetTrigger("AttackMag");
        }
    }

    [ObserversRpc]
    private void DisablePlayerSystemsObserversRpc(bool isDisabled)
    {
        var move = GetComponent<PlayerMovement>();
        if (move != null) move.enabled = !isDisabled;
        var combat = GetComponent<PlayerCombat>();
        if (combat != null) combat.enabled = !isDisabled;
    }

    [ObserversRpc]
    public void ShowEndGameUIObserversRpc(bool isVictory)
    {
        UI_GameOver ui = UnityEngine.Object.FindAnyObjectByType<UI_GameOver>();
        if (ui != null)
        {
            if (isVictory) ui.ShowVictory();
            else ui.ShowDefeat();
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        ServerManager.Despawn(gameObject);
    }

    public void SetHealth(float amount) { if (IsServer) _currentHp.Value = amount; }
    private IEnumerator StunRoutine() { var move = GetComponent<PlayerMovement>(); if (move != null) move.enabled = false; yield return new WaitForSeconds(0.5f); if (_currentHp.Value > 0 && move != null) move.enabled = true; }
}
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

    private PlayerScore _lastAttacker;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        _currentHp.OnChange += OnHpChanged;

        if (IsServer) _currentHp.Value = maxHp;

        // СБРОС СОСТОЯНИЯ ПРИ СПАВНЕ
        _isDead = false;

        // Принудительно включаем системы (на случай если в префабе они выключены)
        DisablePlayerSystemsObserversRpc(false);
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

    public void TakeDamage(float phys, float mag)
    {
        TakeDamageWithAttacker(phys, mag, null);
    }

    public void TakeDamageWithAttacker(float phys, float mag, PlayerScore attacker)
    {
        if (!IsServer || _isDead) return;

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

        if (!gameObject.CompareTag("Player") && _lastAttacker != null)
        {
            _lastAttacker.AddPointServerRpc();
        }

        PlayDieObserversRpc();

        if (gameObject.CompareTag("Player"))
        {
            OnPlayerDeath?.Invoke();
            OnAnyPlayerDeath?.Invoke();
            DisablePlayerSystemsObserversRpc(true);

            // Если есть MatchManager, сообщаем о поражении
            if (MatchManager.Instance != null) MatchManager.Instance.HandleDefeat();
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
            // Убрали ResetTrigger("AttackPhys"), так как его нет в аниматоре
            animator.SetTrigger("Die");
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
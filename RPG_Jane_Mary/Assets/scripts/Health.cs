using UnityEngine;
using System;
using System.Collections;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class Health : NetworkBehaviour, IDamageable
{
    [Header("Параметры HP")]
    public float maxHp = 100f;

    // Синхронизируемая переменная здоровья
    private readonly SyncVar<float> _currentHp = new SyncVar<float>();

    // События для UI и логики игры
    public static event Action<GameObject> OnEnemyDeath;
    public event Action<float, float> OnHealthChanged;
    public static event Action OnPlayerDeath;

    public float CurrentHealth => _currentHp.Value;
    public float MaxHealth => maxHp;

    [Header("Ссылки")]
    public Animator animator;

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        // Подписываемся на изменения здоровья (сработает у всех)
        _currentHp.OnChange += OnHpChanged;

        if (IsServer)
        {
            _currentHp.Value = maxHp;
        }
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
        if (!IsServer) return;

        if (_currentHp.Value <= 0) return;

        // Вычитаем урон на сервере
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

        // Эффект оглушения только для игрока
        if (CompareTag("Player")) StartCoroutine(StunRoutine());
    }

    private void Die()
    {
        if (!IsServer) return;

        PlayDieObserversRpc();

        if (gameObject.CompareTag("Player"))
        {
            // Оповещаем UI о смерти
            OnPlayerDeath?.Invoke();
            // Выключаем управление у всех
            DisableMovementObserversRpc(true);
        }
        else
        {
            OnEnemyDeath?.Invoke(gameObject);
            // Удаляем моба из сети через небольшую задержку, чтобы проигралась анимация
            StartCoroutine(DestroyAfterDelay());
        }
    }

    private IEnumerator DestroyAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        ServerManager.Despawn(gameObject);
    }

    [ObserversRpc]
    private void PlayDieObserversRpc()
    {
        if (animator) animator.SetTrigger("Die");
    }

    [ObserversRpc]
    private void DisableMovementObserversRpc(bool isDisabled)
    {
        var move = GetComponent<PlayerMovement>();
        if (move != null) move.enabled = !isDisabled;
    }

    // --- НОВЫЕ МЕТОДЫ ДЛЯ РЕСТАРТА (RESPAWN) ---

    [ServerRpc(RequireOwnership = false)] // Позволяет мертвому игроку вызвать этот метод
    public void RequestRespawnServerRpc()
    {
        if (!IsServer) return;

        // 1. Сбрасываем здоровье на сервере
        _currentHp.Value = maxHp;

        // 2. Оповещаем всех клиентов, что игрок "ожил"
        RespawnObserversRpc();
    }

    [ObserversRpc]
    private void RespawnObserversRpc()
    {
        var move = GetComponent<PlayerMovement>();
        if (move != null) move.enabled = true;

        if (animator)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        if (IsOwner && move != null)
        {
            // Ищем объект с названием "SpawnPoint" на сцене
            GameObject spawnObj = GameObject.Find("Spawn_POint");

            if (spawnObj != null)
            {
                // Телепортируемся в его позицию + чуть-чуть вверх
                move.Teleport(spawnObj.transform.position + Vector3.up * 1.5f);
            }
            else
            {
                // Если не нашли объект, спавнимся в (0,0,0) - это подстраховка
                move.Teleport(new Vector3(0, 5f, 0));
                Debug.LogWarning("Объект SpawnPoint не найден! Игрок заспавнен в центре мира.");
            }
        }
    }

    // --- ВСПОМОГАТЕЛЬНОЕ ---

    private IEnumerator StunRoutine()
    {
        var move = GetComponent<PlayerMovement>();
        if (move != null) move.enabled = false;
        yield return new WaitForSeconds(0.5f);
        if (_currentHp.Value > 0 && move != null) move.enabled = true;
    }

    public void SetHealth(float amount)
    {
        if (IsServer) _currentHp.Value = amount;
    }
}
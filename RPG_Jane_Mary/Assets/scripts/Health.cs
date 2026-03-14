using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class Health : MonoBehaviour, IDamageable
{
    public float maxHp = 100f;
    private float _currentHp;

    // Реализация интерфейса для UI
    public float CurrentHealth => _currentHp;
    public float MaxHealth => maxHp;

    public Slider worldHpSlider; // Полоска над головой (World Space)
    public Animator animator;

    // Событие для Game Over (статическое, чтобы ловить везде)
    public static event Action OnPlayerDeath;

    private void Start()
    {
        _currentHp = maxHp;
        UpdateUI();
    }

    public void TakeDamage(float phys, float mag)
    {
        if (_currentHp <= 0) return;

        _currentHp = Mathf.Clamp(_currentHp - (phys + mag), 0, maxHp);
        UpdateUI();

        if (_currentHp > 0)
        {
            if (animator) animator.SetTrigger("GetHit");
            // Если это игрок, включаем стан
            if (CompareTag("Player")) StartCoroutine(StunRoutine());
        }
        else
        {
            Die();
        }
    }

    private void UpdateUI()
    {
        if (worldHpSlider != null) worldHpSlider.value = _currentHp / maxHp;
    }

    private void Die()
    {
        if (animator) animator.SetTrigger("Die");

        if (gameObject.CompareTag("Player"))
        {
            OnPlayerDeath?.Invoke(); // Сообщаем системе Game Over
            GetComponent<PlayerMovement>().enabled = false; // Выключаем WASD
        }
        else
        {
            Destroy(gameObject, 3f);
        }
    }

    private IEnumerator StunRoutine()
    {
        var move = GetComponent<PlayerMovement>();
        if (move != null) move.enabled = false;
        yield return new WaitForSeconds(0.5f); // Сделал чуть дольше для ТЗ
        if (_currentHp > 0 && move != null) move.enabled = true;
    }
}
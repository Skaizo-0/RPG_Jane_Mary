using UnityEngine;
using System;
using System.Collections;

public class Health : MonoBehaviour, IDamageable
{
    [Header("Параметры HP")]
    public float maxHp = 100f;
    private float _currentHp;
    public static event Action<GameObject> OnEnemyDeath;


    public float CurrentHealth => _currentHp;
    public float MaxHealth => maxHp;

    [Header("Ссылки")]
    public Animator animator;


    public event Action<float, float> OnHealthChanged;


    public static event Action OnPlayerDeath;

    private void Start()
    {
        _currentHp = maxHp;
        NotifyHealthChanged();
    }

    public void TakeDamage(float phys, float mag)
    {
        if (_currentHp <= 0) return;

        _currentHp = Mathf.Clamp(_currentHp - (phys + mag), 0, maxHp);


        NotifyHealthChanged();

        if (_currentHp > 0)
        {
            if (animator) animator.SetTrigger("GetHit");


            if (CompareTag("Player")) StartCoroutine(StunRoutine());
        }
        else
        {
            Die();
        }
    }

    public void SetHealth(float amount)
    {
        _currentHp = amount;
        NotifyHealthChanged();
    }


    private void NotifyHealthChanged()
    {

        OnHealthChanged?.Invoke(_currentHp, maxHp);
    }

    private void Die()
    {
        if (animator) animator.SetTrigger("Die");

        if (gameObject.CompareTag("Player"))
        {
            OnPlayerDeath?.Invoke();


            var move = GetComponent<PlayerMovement>();
            if (move != null) move.enabled = false;
        }
        else
        {
            OnEnemyDeath?.Invoke(gameObject);
            Destroy(gameObject, 3f);
        }
    }

    private IEnumerator StunRoutine()
    {
        var move = GetComponent<PlayerMovement>();
        if (move != null) move.enabled = false;

        yield return new WaitForSeconds(0.5f);


        if (_currentHp > 0 && move != null) move.enabled = true;
    }
}
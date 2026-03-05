using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class Health : MonoBehaviour, IDamageable
{
    public float maxHp = 100f;
    public float currentHp;
    public Slider hpSlider;
    public Animator animator;

    public event Action OnDeath;

    private void Start()
    {
        currentHp = maxHp;
        if (hpSlider != null) hpSlider.value = 1f;
    }

    public void TakeDamage(float phys, float mag)
    {
        if (currentHp <= 0) return;

        currentHp = Mathf.Clamp(currentHp - (phys + mag), 0, maxHp);
        if (hpSlider != null) hpSlider.value = currentHp / maxHp;

        if (currentHp > 0)
        {
            if (animator) animator.SetTrigger("GetHit");
            StartCoroutine(StunRoutine());
        }
        else
        {
            Die();
        }
    }

    private void Die()
    {
        if (animator) animator.SetTrigger("Die");
        OnDeath?.Invoke();

        // Если это не игрок, удаляем объект через 3 секунды
        if (!gameObject.CompareTag("Player"))
        {
            Destroy(gameObject, 3f);
        }
    }

    private IEnumerator StunRoutine()
    {
        var move = GetComponent<PlayerMovement>();
        if (move != null) move.enabled = false;
        yield return new WaitForSeconds(0.2f);
        if (currentHp > 0 && move != null) move.enabled = true;
    }
}
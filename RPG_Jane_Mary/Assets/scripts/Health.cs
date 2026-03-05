using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Health : MonoBehaviour
{
    [Header("Параметры HP")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Интерфейс и Визуал")]
    public Slider hpSlider; // Слайдер над головой
    public Animator animator;

    [Header("Настройки состояния")]
    private bool isDead = false;
    private bool isStunned = false; // Флаг оглушения

    void Start()
    {
        currentHealth = maxHealth;

        // Настройка слайдера (значения от 0 до 1)
        if (hpSlider != null)
        {
            hpSlider.maxValue = 1;
            hpSlider.value = 1;
        }
    }

    // Главный метод получения урона
    public void TakeDamage(float physicalDamage, float magicDamage)
    {
        if (isDead) return;

        float totalDamage = physicalDamage + magicDamage;
        currentHealth -= totalDamage;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Обновляем полоску HP
        if (hpSlider != null)
        {
            hpSlider.value = currentHealth / maxHealth;
        }

        Debug.Log(gameObject.name + " получил урон: " + totalDamage + ". Осталось HP: " + currentHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Анимация получения урона
            if (animator != null) animator.SetTrigger("GetHit");

            // Если это Игрок, включаем "Стан", чтобы убрать плавание
            if (gameObject.CompareTag("Player") && !isStunned)
            {
                StartCoroutine(StunRoutine(0.1f)); // 0.5 сек нельзя ходить
            }
        }
    }

    // Корутина для временного отключения управления
    private IEnumerator StunRoutine(float duration)
    {
        isStunned = true;

        // Находим скрипты управления
        var movement = GetComponent<PlayerMovement>();
        var combat = GetComponent<PlayerCombat>();

        // Выключаем их
        if (movement != null) movement.enabled = false;
        if (combat != null) combat.enabled = false;

        // Ждем время проигрывания анимации боли
        yield return new WaitForSeconds(duration);

        // Включаем обратно, если еще живы
        if (!isDead)
        {
            if (movement != null) movement.enabled = true;
            if (combat != null) combat.enabled = true;
            isStunned = false;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator != null) animator.SetTrigger("Die");

        // Логика смерти ГЕРОЯ
        if (gameObject.CompareTag("Player"))
        {
            Debug.Log("ГЕРОЙ ПОГИБ - КОНЕЦ ИГРЫ");

            // Навсегда выключаем управление
            var movement = GetComponent<PlayerMovement>();
            if (movement != null) movement.enabled = false;

            var controller = GetComponent<CharacterController>();
            if (controller != null) controller.enabled = false;

            // Здесь в будущем можно добавить: gameOverPanel.SetActive(true);
        }
        // Логика смерти МОБА
        else
        {
            Debug.Log("МОБ УНИЧТОЖЕН");

            // Отключаем ИИ, чтобы он не пытался ходить мертвым
            var ai = GetComponent<EnemyAI>();
            if (ai != null) ai.enabled = false;

            // Моб исчезает через 3 секунды
            Destroy(gameObject, 3f);
        }
    }
}
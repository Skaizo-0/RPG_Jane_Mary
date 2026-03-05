using UnityEngine;
using UnityEngine.UI; // Обязательно для работы со слайдером

public class Health : MonoBehaviour
{
    [Header("Параметры HP")]
    public float maxHealth = 100f;
    public float currentHealth;

    [Header("Интерфейс")]
    public Slider hpSlider; // Сюда перетащи Slider из Canvas над головой

    [Header("Анимации")]
    public Animator animator;

    private bool isDead = false;

    void Start()
    {
        currentHealth = maxHealth;

        // Настройка слайдера при старте
        if (hpSlider != null)
        {
            hpSlider.maxValue = 1;
            hpSlider.value = 1;
        }
    }

    // Метод получения урона (разделение по ТЗ)
    public void TakeDamage(float physicalDamage, float magicDamage)
    {
        if (isDead) return;

        float totalDamage = physicalDamage + magicDamage;
        currentHealth -= totalDamage;

        // Ограничиваем HP, чтобы не ушло в минус
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        // Обновляем полоску HP
        if (hpSlider != null)
        {
            hpSlider.value = currentHealth / maxHealth;
        }

        Debug.Log(gameObject.name + " получил урон: " + totalDamage);

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Анимация реакции на урон (микро-стан)
            if (animator != null) animator.SetTrigger("GetHit");
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        if (animator != null) animator.SetTrigger("Die");

        // Логика из ТЗ: Герой — конец игры, мобы — исчезают
        if (gameObject.CompareTag("Player"))
        {
            Debug.Log("КОНЕЦ ИГРЫ (ГЕРОЙ ПОГИБ)");
            // Отключаем управление
            GetComponent<PlayerMovement>().enabled = false;
            GetComponent<CharacterController>().enabled = false;
        }
        else
        {
            Debug.Log("МОБ ПОГИБ");
            // Моб исчезает через 3 секунды (чтобы успела проиграться анимация смерти)
            Destroy(gameObject, 3f);
        }
    }
}
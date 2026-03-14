using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UI_HUD : MonoBehaviour
{
    [Header("HP Игрока")]
    public Health playerHealth;
    public Slider hudHpSlider;
    public TextMeshProUGUI hpText;

    [Header("Кулдаун Магии")]
    public PlayerCombat playerCombat;
    public Image magicIconOverlay; // Черная картинка поверх иконки

    void Update()
    {
        // 1. Обновляем полоску в углу
        float hpNormalized = playerHealth.CurrentHealth / playerHealth.MaxHealth;
        hudHpSlider.value = hpNormalized;
        hpText.text = $"{(int)playerHealth.CurrentHealth} / {(int)playerHealth.MaxHealth}";

        // 2. Обновляем кулдаун (затемнение иконки)
        if (magicIconOverlay != null)
        {
            // fillAmount будет уменьшаться от 1 до 0 по мере готовности
            magicIconOverlay.fillAmount = 1 - playerCombat.MagicReadyProgress;
        }
    }
}
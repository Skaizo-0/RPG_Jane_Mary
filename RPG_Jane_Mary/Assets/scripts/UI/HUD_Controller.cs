using UnityEngine;

public class HUD_Controller
{
    private UI_HUD _view;
    private Health _health;
    private PlayerCombat _combat;
    private PlayerScore _score;

    public HUD_Controller(UI_HUD view, Health health, PlayerCombat combat)
    {
        _view = view;
        _health = health;
        _combat = combat;

        if (_health != null)
        {
            _score = _health.GetComponent<PlayerScore>();
        }
    }

    public void UpdateHud()
    {
        // Если вид или здоровье потеряны - обновлять нечего
        if (_view == null || _health == null) return;

        // 1. Обновление здоровья
        float hpNormalized = _health.CurrentHealth / _health.MaxHealth;
        if (_view.hudHpSlider != null)
            _view.hudHpSlider.value = hpNormalized;

        if (_view.hpText != null)
            _view.hpText.text = $"{(int)_health.CurrentHealth} / {(int)_health.MaxHealth}";

        // 2. Обновление кулдауна магии
        if (_view.magicIconOverlay != null && _combat != null)
        {
            _view.magicIconOverlay.fillAmount = 1 - _combat.MagicReadyProgress;
        }

        // 3. ОБНОВЛЕНИЕ ОЧКОВ
        // Пытаемся найти компонент очков, если он еще не привязан
        if (_score == null && _health != null)
        {
            _score = _health.GetComponent<PlayerScore>();
        }

        if (_score != null && _view.scoreText != null)
        {
            _view.scoreText.text = $"Убито: {_score.CurrentScore}";
        }
        else if (_view.scoreText != null)
        {
            // Если скрипт PlayerScore не найден на префабе
            _view.scoreText.text = "Убито: 0";
        }
    }
}
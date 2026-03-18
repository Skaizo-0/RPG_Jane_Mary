public class HUD_Controller
{
    private UI_HUD _view;
    private Health _health;
    private PlayerCombat _combat;

    public HUD_Controller(UI_HUD view, Health health, PlayerCombat combat)
    {
        _view = view;
        _health = health;
        _combat = combat;
    }

    public void UpdateHud()
    {
        if (_view == null) return;

        float hpNormalized = _health.CurrentHealth / _health.MaxHealth;
        _view.hudHpSlider.value = hpNormalized;
        _view.hpText.text = $"{(int)_health.CurrentHealth} / {(int)_health.MaxHealth}";

        if (_view.magicIconOverlay != null)
        {
            _view.magicIconOverlay.fillAmount = 1 - _combat.MagicReadyProgress;
        }
    }
}
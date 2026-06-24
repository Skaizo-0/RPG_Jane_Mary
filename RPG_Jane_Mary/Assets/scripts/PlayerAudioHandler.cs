using UnityEngine;

public class PlayerAudioHandler : MonoBehaviour
{
    [Header("Настройки звука")]
    public AudioClip hurtSound; // Перетащите сюда звук стона/удара

    private void OnEnable()
    {
        // Подписываемся на событие получения урона
        Health.OnPlayerHit += PlayHurtSound;
    }

    private void OnDisable()
    {
        // Обязательно отписываемся (Лекция 7)
        Health.OnPlayerHit -= PlayHurtSound;
    }

    private void PlayHurtSound()
    {
        // Получаем сервис звука и играем SFX (Лекция 3)
        var audioService = ServiceLocator.Get<IAudioService>();
        if (audioService != null && hurtSound != null)
        {
            audioService.PlaySfx(hurtSound);
        }
    }
}
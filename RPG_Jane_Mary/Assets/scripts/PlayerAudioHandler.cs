using UnityEngine;

public class PlayerAudioHandler : MonoBehaviour
{
    [Header("Настройки звука")]
    public AudioClip hurtSound; 

    private void OnEnable()
    {
        //подписка на событие получения урона
        Health.OnPlayerHit += PlayHurtSound;
    }

    private void OnDisable()
    {
        //отписка
        Health.OnPlayerHit -= PlayHurtSound;
    }

    private void PlayHurtSound()
    {
        var audioService = ServiceLocator.Get<IAudioService>();
        if (audioService != null && hurtSound != null)
        {
            audioService.PlaySfx(hurtSound);
        }
    }
}
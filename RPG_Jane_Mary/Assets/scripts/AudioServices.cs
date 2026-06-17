using UnityEngine;

// Интерфейс (Абстракция из Лекции 3)
public interface IAudioService
{
    void SetVolume(float volume);
    void PlayMusic(AudioClip clip);
}

// Конкретная реализация (Мозг сервиса)
public class UnityAudioService : IAudioService
{
    private AudioSource _audioSource;

    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
    }

    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;

        // Ищем AudioSource на сцене
        // Мы ищем его на GameplayManager, где висит ваш ScoreController
        if (_audioSource == null)
        {
            GameObject manager = GameObject.Find("GameplayManager");
            if (manager != null)
            {
                _audioSource = manager.GetComponent<AudioSource>();
            }
        }

        if (_audioSource != null)
        {
            _audioSource.clip = clip;
            _audioSource.loop = false; // Победная мелодия играет 1 раз
            _audioSource.Play();
            Debug.Log($"[AudioService] Играет победная музыка: {clip.name}");
        }
        else
        {
            Debug.LogError("На объекте GameplayManager не найден компонент AudioSource!");
        }
    }
}
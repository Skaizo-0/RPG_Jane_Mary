using UnityEngine;

public interface IAudioService
{
    void PlayMusic(AudioClip clip);
    void PlaySfx(AudioClip clip); // Для разовых звуков (победа/проигрыш)
    void SetVolume(float volume);
}

public class UnityAudioService : IAudioService
{
    private AudioSource _source;

    public void SetVolume(float volume)
    {
        // Устанавливаем общую громкость звука в Unity
        AudioListener.volume = volume;
        Debug.Log($"[AudioService] Громкость установлена на: {volume}");
    }

    public void PlayMusic(AudioClip clip)
    {
        var source = GetSource();
        if (source == null || clip == null) return;
        source.clip = clip;
        source.loop = true;
        source.Play();
    }

    public void PlaySfx(AudioClip clip)
    {
        var source = GetSource();
        if (source == null || clip == null) return;
        source.PlayOneShot(clip);
    }

    private AudioSource GetSource()
    {
        if (_source == null)
        {
            // Ищем объект на сцене, как в Лекции 3
            GameObject manager = GameObject.Find("GameplayManager");
            if (manager != null) _source = manager.GetComponent<AudioSource>();
        }
        return _source;
    }
}
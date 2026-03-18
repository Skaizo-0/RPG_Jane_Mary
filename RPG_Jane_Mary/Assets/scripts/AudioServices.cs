using UnityEngine;


public interface IAudioService
{
    void SetVolume(float volume);
    void PlayMusic(AudioClip clip);
}


public class UnityAudioService : IAudioService
{
    public void SetVolume(float volume)
    {
        AudioListener.volume = volume;
        Debug.Log($"Громкость изменена на: {volume}");
    }

    public void PlayMusic(AudioClip clip)
    {

        if (clip != null)
        {
            Debug.Log($"Играет музыка: {clip.name}");
        }
    }
}
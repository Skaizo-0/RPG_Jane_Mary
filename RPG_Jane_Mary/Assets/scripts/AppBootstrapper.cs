using UnityEngine.SceneManagement;
using UnityEngine;

public class AppBootstrapper : MonoBehaviour
{
    private void Awake()
    {
        // Создаем сервисы один раз на всю игру
        var audioService = new UnityAudioService();
        var saveService = new GameRepository(); // Твой репозиторий

        // Регистрируем их где-то (например, в статическом классе или ServiceLocator)
        ServiceLocator.Register<IAudioService>(audioService);
        ServiceLocator.Register<ISaveService>(saveService);

        DontDestroyOnLoad(gameObject);
        SceneManager.LoadScene("MainMenu"); // Переходим в меню
    }
}
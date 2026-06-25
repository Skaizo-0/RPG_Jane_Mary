    using UnityEngine.SceneManagement;
    using UnityEngine;

    public class AppBootstrapper : MonoBehaviour
    {
        private void Awake()
        {

            var audioService = new UnityAudioService();
            var saveService = new GameRepository();

            ServiceLocator.Register<IAudioService>(audioService);
            ServiceLocator.Register<ISaveService>(saveService);

            DontDestroyOnLoad(gameObject);
            SceneManager.LoadScene("MainMenu"); 
        }
    }
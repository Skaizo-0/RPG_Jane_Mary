using UnityEngine;

public class MainMenuBootstrapper : MonoBehaviour
{
    public MainMenuView view; 

    private void Start()
    {

        var audioService = ServiceLocator.Get<IAudioService>();


        var controller = new MainMenuController(view, audioService);

        Debug.Log("MVC Главного меню инициализировано");
    }
}
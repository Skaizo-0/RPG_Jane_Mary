using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    public GameObject mainButtonsPanel; // Панель, где кнопки Play, Settings, Exit
    public GameObject settingsWindow;   // Панель, где Слайдер и кнопка Back

    public void PlayGame() => SceneManager.LoadScene(1);

    public void OpenSettings()
    {
        settingsWindow.SetActive(true);    // Включаем настройки
        mainButtonsPanel.SetActive(false); // Выключаем главные кнопки
    }

    public void CloseSettings()
    {
        settingsWindow.SetActive(false);   // Выключаем настройки
        mainButtonsPanel.SetActive(true);  // Возвращаем главные кнопки
    }

    public void SetVolume(float vol) => AudioListener.volume = vol;

    public void ExitGame() => Application.Quit();
}
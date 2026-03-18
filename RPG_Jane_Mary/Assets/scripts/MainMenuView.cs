using UnityEngine;
using UnityEngine.UI;

public class MainMenuView : MonoBehaviour
{
    [Header("Панели")]
    public GameObject mainButtonsPanel;
    public GameObject settingsWindow;

    [Header("Кнопки")]
    public Button playButton;
    public Button settingsButton;
    public Button backButton;


    [Header("Настройки")]
    public Slider volumeSlider;
}
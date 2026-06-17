using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuView : MonoBehaviour
{
    [Header("Панели")]
    public GameObject mainButtonsPanel;
    public GameObject settingsWindow;
    public GameObject connectionPanel;
    public GameObject lobbyPanel; // ПАНЕЛЬ ЛОББИ

    [Header("Кнопки")]
    public Button playButton;
    public Button settingsButton;
    public Button backButton;
    public Button hostButton;
    public Button clientButton;
    public Button startMatchButton; // Кнопка старта (только для Хоста)

    [Header("Элементы Relay")]
    public TMP_InputField joinCodeInputField;
    public TextMeshProUGUI hostCodeDisplay;
    public TextMeshProUGUI playerCountText; // Текст "Игроков: 1"

    [Header("Настройки")]
    public Slider volumeSlider;
}
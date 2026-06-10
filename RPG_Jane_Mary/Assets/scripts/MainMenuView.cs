using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuView : MonoBehaviour
{
    [Header("Сетевой Префаб")]
    public FishNet.Object.NetworkObject playerPrefab; // ПРЕФАБ ГГ ТУТ

    [Header("Панели")]
    public GameObject mainButtonsPanel;
    public GameObject settingsWindow;
    public GameObject connectionPanel;
    public GameObject lobbyPanel;

    [Header("Кнопки")]
    public Button playButton;
    public Button settingsButton;
    public Button backButton;
    public Button hostButton;
    public Button clientButton;
    public Button startMatchButton;

    [Header("Элементы")]
    public TMP_InputField ipInputField;
    public TextMeshProUGUI playerCountText;

    [Header("Настройки")]
    public Slider volumeSlider;
}
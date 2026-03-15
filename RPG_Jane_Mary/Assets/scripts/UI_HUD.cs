using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Это "View" по лекции. Она ничего не знает о логике.
public class UI_HUD : MonoBehaviour
{
    [Header("Ссылки на элементы интерфейса")]
    public Slider hudHpSlider;
    public TextMeshProUGUI hpText;
    public Image magicIconOverlay;
}
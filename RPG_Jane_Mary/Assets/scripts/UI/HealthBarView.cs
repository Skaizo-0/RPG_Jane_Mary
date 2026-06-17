using UnityEngine;
using UnityEngine.UI;

public class HealthBarView : MonoBehaviour
{
    public Slider slider;
    public Health healthTarget; 

    private void Awake()
    {
        if (healthTarget != null)
            healthTarget.OnHealthChanged += UpdateSlider;
    }

    private void OnDestroy()
    {
        if (healthTarget != null)
            healthTarget.OnHealthChanged -= UpdateSlider;
    }

    private void UpdateSlider(float current, float max)
    {
        if (slider != null)
        {
            slider.value = current / max;
        }
    }
}
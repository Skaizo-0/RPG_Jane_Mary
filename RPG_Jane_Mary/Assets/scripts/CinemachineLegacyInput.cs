using UnityEngine;
using Unity.Cinemachine;

public class CinemachineLegacyInput : MonoBehaviour
{
    private CinemachineOrbitalFollow _orbital;

    void Start()
    {
        _orbital = GetComponent<CinemachineOrbitalFollow>();
        if (_orbital != null)
        {
            // Устанавливаем камеру на среднее кольцо при старте
            _orbital.VerticalAxis.Value = 0.5f;
        }
    }

    void Update()
    {
        if (_orbital == null || Cursor.lockState != CursorLockMode.Locked) return;

        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");

        // Горизонталь
        _orbital.HorizontalAxis.Value += mouseX * 2.0f;

        // ВЕРТИКАЛЬ (Исправлено):
        // 1. Убрали deltaTime (теперь будет быстро)
        // 2. Сделали "+" вместо "-", чтобы мышь вверх = камера вверх
        float sensitivityY = 0.05f;
        float newValue = _orbital.VerticalAxis.Value + (mouseY * sensitivityY);
        _orbital.VerticalAxis.Value = Mathf.Clamp(newValue, 0.01f, 0.99f);

        if (Mathf.Abs(mouseX) > 0.01f || Mathf.Abs(mouseY) > 0.01f)
        {
            Debug.Log($"[LOG_CAM] Оси: H:{_orbital.HorizontalAxis.Value} V:{_orbital.VerticalAxis.Value}");
        }
    }
}
using UnityEngine;

public interface IInputService
{
    Vector3 MoveAxis { get; }
    bool IsRunning { get; } // Добавлено для бега
    bool AttackPhys { get; }
    bool AttackMag { get; }
}

public class StandaloneInput : IInputService
{
    public Vector3 MoveAxis => new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
    public bool IsRunning => Input.GetKey(KeyCode.LeftShift); // Shift из ТЗ
    public bool AttackPhys => Input.GetMouseButtonDown(0);
    public bool AttackMag => Input.GetMouseButtonDown(1);
}
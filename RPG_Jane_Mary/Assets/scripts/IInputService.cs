using UnityEngine;

public interface IInputService
{
    Vector3 MoveAxis { get; }
    Vector2 LookAxis { get; } // НОВОЕ: для вращения камеры
    bool IsRunning { get; }
    bool AttackPhys { get; }
    bool AttackMag { get; }
    bool PausePressed { get; }
}

public class StandaloneInput : IInputService
{
    public Vector3 MoveAxis => new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

    // НОВОЕ: Считываем стандартные оси мыши Unity
    public Vector2 LookAxis => new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

    public bool IsRunning => Input.GetKey(KeyCode.LeftShift);
    public bool AttackPhys => Input.GetMouseButtonDown(0);
    public bool AttackMag => Input.GetMouseButtonDown(1);
    public bool PausePressed => Input.GetKeyDown(KeyCode.Escape);
}
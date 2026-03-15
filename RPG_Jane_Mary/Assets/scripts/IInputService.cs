using UnityEngine;

public interface IInputService
{
    Vector3 MoveAxis { get; }
    bool IsRunning { get; } 
    bool AttackPhys { get; }
    bool AttackMag { get; }
    bool PausePressed { get; }
}

public class StandaloneInput : IInputService
{
    public Vector3 MoveAxis => new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
    public bool IsRunning => Input.GetKey(KeyCode.LeftShift); 
    public bool AttackPhys => Input.GetMouseButtonDown(0);
    public bool AttackMag => Input.GetMouseButtonDown(1);

    public bool PausePressed => Input.GetKeyDown(KeyCode.Escape);
}
using UnityEngine;
using FishNet.Object;

public class PlayerMovement : NetworkBehaviour
{
    public CharacterController controller;
    public Animator animator;
    public float walkSpeed = 3f;
    public float runSpeed = 6f;

    private IInputService _input;
    private Transform _cam;
    private float _gravityVelocity;

    // Метод для Bootstrapper
    public void Construct(IInputService input)
    {
        _input = input;
        _cam = Camera.main.transform;
    }

    // Объединенный метод OnStartClient (только ОДИН раз)
    public override void OnStartClient()
    {
        base.OnStartClient();

        if (IsOwner)
        {
            _cam = Camera.main.transform;

            // Регистрируем себя в Бутстраппере, когда появились в сети
            if (Bootstrapper.Instance != null)
            {
                var combat = GetComponent<PlayerCombat>();
                var health = GetComponent<Health>();
                Bootstrapper.Instance.RegisterPlayer(this, combat, health);
            }
        }
        else
        {
            // Чужим игрокам выключаем контроллер
            if (controller != null) controller.enabled = false;
        }
    }

    void Update()
    {
        if (!IsOwner || _input == null) return;

        Vector3 inputDir = _input.MoveAxis;

        if (inputDir.magnitude > 0.1f)
        {
            Vector3 camForward = _cam.forward;
            Vector3 camRight = _cam.right;
            camForward.y = 0; camRight.y = 0;

            Vector3 moveDir = (camForward.normalized * inputDir.z + camRight.normalized * inputDir.x).normalized;

            float currentSpeed = _input.IsRunning ? runSpeed : walkSpeed;
            controller.Move(moveDir * currentSpeed * Time.deltaTime);

            transform.forward = Vector3.Slerp(transform.forward, moveDir, 10f * Time.deltaTime);
            animator.SetFloat("Speed", _input.IsRunning ? 1f : 0.5f, 0.1f, Time.deltaTime);
        }
        else
        {
            animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
        }

        ApplyGravity();
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded) _gravityVelocity = -2f;
        else _gravityVelocity += -9.81f * Time.deltaTime;

        controller.Move(new Vector3(0, _gravityVelocity, 0) * Time.deltaTime);
    }

    public void Teleport(Vector3 pos)
    {
        if (IsOwner)
        {
            controller.enabled = false;
            transform.position = pos;
            controller.enabled = true;
        }
    }
}
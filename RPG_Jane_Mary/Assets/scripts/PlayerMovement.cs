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

    // ИСПОЛЬЗУЕМ OnStartNetwork для настройки физики
    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        if (controller == null) controller = GetComponent<CharacterController>();

        // ИСПРАВЛЕНИЕ ОШИБКИ FN0007:
        // Вместо IsOwner используем Owner.IsLocalClient
        // Контроллер должен быть ВКЛЮЧЕН, если это наш игрок ИЛИ если это Сервер.
        if (Owner.IsLocalClient || IsServer)
        {
            controller.enabled = true;
        }
        else
        {
            // Выключаем только на клиентах-зрителях (чужие игроки в твоем окне)
            controller.enabled = false;
        }

        // Регистрируем себя в Бутстраппере
        if (Owner.IsLocalClient && Bootstrapper.Instance != null)
        {
            _cam = Camera.main.transform;
            _gravityVelocity = 0;

            var combat = GetComponent<PlayerCombat>();
            var health = GetComponent<Health>();
            Bootstrapper.Instance.RegisterPlayer(this, combat, health);
        }
    }

    void Update()
    {
        // В Update использовать IsOwner можно!
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
        // Применяем гравитацию только если контроллер включен
        if (controller == null || !controller.enabled) return;

        if (controller.isGrounded) _gravityVelocity = -2f;
        else _gravityVelocity += -9.81f * Time.deltaTime;

        controller.Move(new Vector3(0, _gravityVelocity, 0) * Time.deltaTime);
    }

    public void Teleport(Vector3 pos)
    {
        if (IsOwner)
        {
            controller.enabled = false;
            _gravityVelocity = 0;
            transform.position = pos;

            Physics.SyncTransforms();

            controller.enabled = true;
        }
    }
}
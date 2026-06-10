using UnityEngine;
using FishNet.Object;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerMovement : NetworkBehaviour
{
    public CharacterController controller;
    public Animator animator;
    public float walkSpeed = 3f;
    public float runSpeed = 6f;

    private IInputService _input;
    private Transform _cam;
    private float _gravityVelocity;

    public void Construct(IInputService input)
    {
        _input = input;
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        // КЛЮЧЕВОЙ ФИКС: Контроллер должен быть включен на сервере и у владельца
        if (controller == null) controller = GetComponent<CharacterController>();
        if (Owner.IsLocalClient || IsServer)
        {
            controller.enabled = true;
            _gravityVelocity = 0f;
        }
        else
        {
            controller.enabled = false;
        }

        // Запускаем безопасную регистрацию
        if (Owner.IsLocalClient)
        {
            StartCoroutine(WaitAndRegister());
        }
    }

    private IEnumerator WaitAndRegister()
    {
        // 1. Ждем, пока игрок выберется из технической сцены "MovedObjectsHolder" 
        // и попадет в реальную игровую сцену
        while (gameObject.scene.name != "SampleScene")
        {
            yield return new WaitForSeconds(0.1f);
        }

        Debug.Log($"[NETWORK] Игрок {gameObject.name} прибыл в сцену {gameObject.scene.name}. Регистрируем...");

        // 2. Ждем, пока появится Бутстраппер (он может грузиться долю секунды)
        while (Bootstrapper.Instance == null)
        {
            yield return new WaitForSeconds(0.1f);
        }

        // 3. Теперь регистрируемся
        _input = new StandaloneInput();
        var combat = GetComponent<PlayerCombat>();
        var health = GetComponent<Health>();
        Bootstrapper.Instance.RegisterPlayer(this, combat, health);

        Debug.Log("[NETWORK] Управление и Камера активированы!");
    }

    void Update()
    {
        if (!IsOwner) return;
        if (_input == null) return; // Управление не включится, пока не пройдет регистрация

        Vector3 inputDir = _input.MoveAxis;

        if (inputDir.magnitude > 0.1f)
        {
            if (_cam == null && Camera.main != null) _cam = Camera.main.transform;
            if (_cam == null) return;

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
        if (controller == null || !controller.enabled) return;
        if (controller.isGrounded && _gravityVelocity < 0) _gravityVelocity = -2f;
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
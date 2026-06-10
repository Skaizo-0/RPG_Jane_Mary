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
    private bool _isGameStarted = false;

    public void Construct(IInputService input)
    {
        _input = input;
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        if (Owner.IsLocalClient)
        {
            // Подписываемся на событие загрузки сцены через полный путь Unity
            UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
            StartCoroutine(CheckInitialScene());
        }
    }

    private void OnDestroy()
    {
        if (Owner.IsLocalClient)
            UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SampleScene") StartCoroutine(ReliableSpawnRoutine());
    }

    private IEnumerator CheckInitialScene()
    {
        yield return null; // Ждем кадр для инициализации
        if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "SampleScene")
            StartCoroutine(ReliableSpawnRoutine());
    }

    private IEnumerator ReliableSpawnRoutine()
    {
        _isGameStarted = false;
        GameObject sp = null;

        // 1. Ждем, пока сцена SampleScene станет активной
        while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "SampleScene")
            yield return new WaitForSeconds(0.1f);

        // 2. СИЛОВОЙ ПЕРЕНОС объекта в игровую сцену
        // Это исправит то, что игроки висят в MainMenu на твоем скриншоте
        Scene gameScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("SampleScene");
        if (gameScene.isLoaded)
        {
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(gameObject, gameScene);
        }

        // 3. Поиск точки спавна
        int attempts = 0;
        while (sp == null && attempts < 50)
        {
            sp = GameObject.Find("SpawnPoint");
            if (sp == null)
            {
                attempts++;
                yield return new WaitForSeconds(0.05f);
            }
        }

        if (sp != null)
        {
            _isGameStarted = true;
            Teleport(sp.transform.position + Vector3.up * 2f);
            Debug.Log("[NETWORK] Успешный спавн в игровой сцене!");
        }
        else
        {
            Debug.LogError("[NETWORK] SpawnPoint не найден на сцене SampleScene!");
        }
    }

    void Update()
    {
        if (!IsOwner || !_isGameStarted) return;

        float horizontal = (_input != null) ? _input.MoveAxis.x : Input.GetAxis("Horizontal");
        float vertical = (_input != null) ? _input.MoveAxis.z : Input.GetAxis("Vertical");
        bool isRunning = (_input != null) ? _input.IsRunning : Input.GetKey(KeyCode.LeftShift);

        Vector3 inputDir = new Vector3(horizontal, 0, vertical);

        if (inputDir.magnitude > 0.1f)
        {
            if (_cam == null && Camera.main != null) _cam = Camera.main.transform;
            if (_cam == null) return;

            Vector3 camForward = _cam.forward;
            Vector3 camRight = _cam.right;
            camForward.y = 0; camRight.y = 0;

            Vector3 moveDir = (camForward.normalized * inputDir.z + camRight.normalized * inputDir.x).normalized;
            controller.Move(moveDir * (isRunning ? runSpeed : walkSpeed) * Time.deltaTime);

            transform.forward = Vector3.Slerp(transform.forward, moveDir, 10f * Time.deltaTime);
            animator.SetFloat("Speed", isRunning ? 1f : 0.5f, 0.1f, Time.deltaTime);
        }
        else
        {
            animator.SetFloat("Speed", 0f, 0.1f, Time.deltaTime);
        }

        ApplyGravity();
    }

    private void ApplyGravity()
    {
        if (controller == null || !controller.enabled || !_isGameStarted) return;
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
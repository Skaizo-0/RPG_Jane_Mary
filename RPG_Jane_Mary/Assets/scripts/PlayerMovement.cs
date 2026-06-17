using UnityEngine;
using FishNet.Object;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
public class PlayerMovement : NetworkBehaviour
{
    [Header("Ссылки")]
    public CharacterController controller;
    public Animator animator;

    [Header("Настройки движения")]
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float rotationSpeed = 15f;

    // СТАТИЧЕСКИЙ СПИСОК: Нужен, чтобы враги (EnemyAI) могли найти игроков на сервере
    public static readonly List<PlayerMovement> AllPlayers = new List<PlayerMovement>();

    private IInputService _input;
    private Transform _cam;
    private float _gravityVelocity;
    private bool _isGameStarted = false;

    // Метод для инициализации ввода (вызывается из Bootstrapper или при старте)
    public void Construct(IInputService input)
    {
        _input = input;
    }

    // Добавьте это в ваш скрипт PlayerMovement
    public override void OnStartServer()
    {
        base.OnStartServer();

        if (!AllPlayers.Contains(this)) AllPlayers.Add(this);

        // Запускаем серверную проверку сцены
        StartCoroutine(ServerMoveToGameSceneRoutine());
    }

    private IEnumerator ServerMoveToGameSceneRoutine()
    {
        // Указываем полное имя UnityEngine.SceneManagement.SceneManager
        UnityEngine.SceneManagement.Scene gameScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("SampleScene");

        int attempts = 0;
        while (!gameScene.isLoaded && attempts < 50)
        {
            yield return new WaitForSeconds(0.1f);
            // Повторяем поиск каждый раз, пока сцена не станет валидной
            gameScene = UnityEngine.SceneManagement.SceneManager.GetSceneByName("SampleScene");
            attempts++;
        }

        if (gameScene.isLoaded)
        {
            // Используем полное имя для переноса объекта
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(gameObject, gameScene);
            Debug.Log($"[SERVER] Игрок {name} успешно перемещен в {gameScene.name}");

            // После переноса — телепорт к точке спавна
            GameObject sp = GameObject.Find("SpawnPoint");
            if (sp != null)
            {
                ForceTeleport(sp.transform.position + Vector3.up * 2f);
            }
        }
        else
        {
            Debug.LogError("[SERVER] Не удалось найти SampleScene спустя 5 секунд! Проверьте имя сцены в Build Settings.");
        }
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        AllPlayers.Remove(this);
    }

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        // Если это наш персонаж (LocalPlayer) или мы Хост (IsServer)
        if (Owner.IsLocalClient || IsServer)
        {
            if (_input == null && Owner.IsLocalClient) _input = new StandaloneInput();
            StartCoroutine(ReliableSpawnRoutine());
        }
    }

    private IEnumerator ReliableSpawnRoutine()
    {
        _isGameStarted = false;

        // 1. Ждем, пока сцена SampleScene загрузится окончательно
        while (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name != "SampleScene")
        {
            yield return new WaitForSeconds(0.2f);
        }

        // 2. Ищем точку спавна на сцене
        GameObject sp = null;
        int attempts = 0;
        while (sp == null && attempts < 50)
        {
            sp = GameObject.Find("SpawnPoint");
            if (sp == null) { attempts++; yield return new WaitForSeconds(0.1f); }
        }

        if (sp != null)
        {
            // 3. Телепортируем игрока (на сервере и на клиенте)
            ForceTeleport(sp.transform.position + Vector3.up * 2f);

            // 4. Регистрация в системе (Bootstrapper привязывает камеру и HUD)
            if (IsOwner && Bootstrapper.Instance != null)
            {
                var combat = GetComponent<PlayerCombat>();
                var health = GetComponent<Health>();
                Bootstrapper.Instance.RegisterPlayer(this, combat, health);
            }

            _isGameStarted = true;
            Debug.Log($"[PLAYER] {name} успешно заспавнен в SampleScene");
        }
        else
        {
            Debug.LogError("[PLAYER] SpawnPoint не найден на сцене SampleScene!");
        }
    }

    void Update()
    {
        // Только владелец управляет своим персонажем
        if (!IsOwner || !_isGameStarted) return;

        // Блокируем курсор
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (Camera.main != null) _cam = Camera.main.transform;
        if (_cam == null) return;

        // Считываем WASD из сервиса ввода
        float h = (_input != null) ? _input.MoveAxis.x : Input.GetAxis("Horizontal");
        float v = (_input != null) ? _input.MoveAxis.z : Input.GetAxis("Vertical");
        bool isRunning = (_input != null) ? _input.IsRunning : Input.GetKey(KeyCode.LeftShift);

        if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
        {
            // Рассчитываем угол поворота относительно камеры
            float targetAngle = Mathf.Atan2(h, v) * Mathf.Rad2Deg + _cam.eulerAngles.y;
            Quaternion targetRotation = Quaternion.Euler(0f, targetAngle, 0f);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

            // Движение
            float currentSpeed = isRunning ? runSpeed : walkSpeed;
            Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            controller.Move(moveDir * currentSpeed * Time.deltaTime);

            // Анимация (0.5 - ходьба, 1.0 - бег)
            float animValue = isRunning ? 1f : 0.5f;
            animator.SetFloat("Speed", animValue, 0.1f, Time.deltaTime);
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

        if (controller.isGrounded)
        {
            _gravityVelocity = -2f;
        }
        else
        {
            _gravityVelocity += -9.81f * Time.deltaTime;
        }

        controller.Move(new Vector3(0, _gravityVelocity, 0) * Time.deltaTime);
    }

    public void ForceTeleport(Vector3 pos)
    {
        bool wasEnabled = controller.enabled;
        controller.enabled = false;
        transform.position = pos;
        Physics.SyncTransforms();
        controller.enabled = wasEnabled;
    }

    // Метод для вызова телепорта извне (например, при смерти)
    public void Teleport(Vector3 pos)
    {
        ForceTeleport(pos);
    }
}
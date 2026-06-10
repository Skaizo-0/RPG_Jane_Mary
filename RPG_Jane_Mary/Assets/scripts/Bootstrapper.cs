using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine; // Для новой Cinemachine (v3)
using System.Collections;

public class Bootstrapper : MonoBehaviour
{
    // Синглтон, чтобы заспавненный игрок мог найти этот скрипт
    public static Bootstrapper Instance;

    [Header("Игрок")]
    public PlayerMovement playerMove;
    public PlayerCombat playerCombat;
    public Health playerHealth;

    [Header("Интерфейс (MVC View)")]
    public UI_HUD uiHudView;

    [Header("Панели Меню")]
    public GameObject pausePanel;
    public GameObject gameMenuPanel;

    private GameInteractor _interactor;
    private HUD_Controller _hudController;
    private IInputService _input;

    void Awake()
    {
        // КРИТИЧЕСКИЙ ФИКС: Бутстраппер должен выжить при смене сцены!
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        var repo = new GameRepository();
        _interactor = new GameInteractor(repo);
        _input = new StandaloneInput();

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (pausePanel) pausePanel.SetActive(false);
        if (gameMenuPanel) gameMenuPanel.SetActive(false);
    }

    void Update()
    {
        if (_hudController == null || _input == null) return;

        _hudController.UpdateHud();

        if (_input.PausePressed)
        {
            TogglePause();
        }
    }

    public void RegisterPlayer(PlayerMovement move, PlayerCombat combat, Health health)
    {
        // Привязываем локального игрока к бутстрапперу
        playerMove = move;
        playerCombat = combat;
        playerHealth = health;

        // Инициализируем контроллер интерфейса для этого игрока
        _hudController = new HUD_Controller(uiHudView, playerHealth, playerCombat);

        Debug.Log($"[BOOTSTRAPPER] Игрок {move.name} успешно зарегистрирован!");

        // Запускаем поиск камеры
        StartCoroutine(SetupCameraWithRetry(move.transform));
    }

    private IEnumerator SetupCameraWithRetry(Transform target)
    {
        int attempts = 0;
        while (attempts < 30)
        {
            var v3Cam = Object.FindAnyObjectByType<CinemachineCamera>();
            if (v3Cam != null)
            {
                v3Cam.Follow = target;
                v3Cam.LookAt = target;
                Debug.Log($"[CAMERA] Новая Cinemachine Camera привязана!");
                yield break;
            }

            var freeLook = Object.FindAnyObjectByType<CinemachineFreeLook>();
            if (freeLook != null)
            {
                freeLook.Follow = target;
                freeLook.LookAt = target;
                Debug.Log($"[CAMERA] Старая Cinemachine FreeLook привязана!");
                yield break;
            }

            attempts++;
            yield return new WaitForSeconds(0.1f);
        }
        Debug.LogError("[CAMERA] ОШИБКА: Камера не найдена!");
    }

    public void TogglePause()
    {
        if (gameMenuPanel != null && gameMenuPanel.activeSelf)
        {
            gameMenuPanel.SetActive(false);
            pausePanel.SetActive(true);
        }
        else if (pausePanel != null)
        {
            bool isPaused = !pausePanel.activeSelf;
            pausePanel.SetActive(isPaused);
            Time.timeScale = isPaused ? 0f : 1f;
            Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isPaused;
        }
    }

    public void OpenGameMenu()
    {
        if (pausePanel) pausePanel.SetActive(false);
        if (gameMenuPanel) gameMenuPanel.SetActive(true);
    }

    public void CloseAllMenus()
    {
        if (pausePanel) pausePanel.SetActive(false);
        if (gameMenuPanel) gameMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SaveGame()
    {
        if (playerHealth == null) return;
        PlayerData data = new PlayerData { Hp = playerHealth.CurrentHealth, MaxHp = playerHealth.MaxHealth, Position = playerMove.transform.position };
        _interactor.SaveGame(data);
    }

    public void LoadGame()
    {
        if (playerHealth == null) return;
        _interactor.LoadGame();
        PlayerData data = _interactor.Data;
        playerHealth.SetHealth(data.Hp);
        playerMove.Teleport(data.Position);
        CloseAllMenus();
    }

    public void GoToMainMenu() { SceneManager.LoadScene(0); }
    public void QuitGame() { Application.Quit(); }
}
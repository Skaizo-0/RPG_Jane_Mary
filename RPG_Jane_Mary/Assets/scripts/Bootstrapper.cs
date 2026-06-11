using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Cinemachine; // ДОБАВИЛИ ЭТУ СТРОЧКУ (Или "using Cinemachine;")


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
        Instance = this;

        var repo = new GameRepository();
        _interactor = new GameInteractor(repo);
        _input = new StandaloneInput();

        // НЕ разблокируй мышь тут, если мы уже в игре!
        // Пусть за это отвечает MainMenuController или метод HideMenus
        pausePanel.SetActive(false);
        gameMenuPanel.SetActive(false);
    }

    void Update()
    {
        // Если игрока еще нет или HUD не создан — не обновляем
        if (_hudController == null || _input == null) return;

        _hudController.UpdateHud();

        if (_input.PausePressed)
        {
            TogglePause();
        }
    }

    public void RegisterPlayer(PlayerMovement move, PlayerCombat combat, Health health)
    {
        playerMove = move;
        playerCombat = combat;
        playerHealth = health;

        // Передаем ввод заспавненному игроку
        playerMove.Construct(_input);
        if (playerCombat != null) playerCombat.Construct(_input);

        // Инициализируем контроллер интерфейса для этого игрока
        _hudController = new HUD_Controller(uiHudView, playerHealth, playerCombat);

        Debug.Log("Сетевой игрок успешно зарегистрирован!");

        // ПРИВЯЗКА КАМЕРЫ (Универсальный способ для Unity 6)

        // Ищем любую активную Cinemachine камеру
        var cmCamera = Object.FindFirstObjectByType<CinemachineCamera>();
        var freeLook = Object.FindFirstObjectByType<CinemachineFreeLook>();

        if (cmCamera != null)
        {
            cmCamera.Follow = move.transform;
            cmCamera.LookAt = move.transform;

            // ВЫКЛЮЧАЕМ Solo, если он был включен случайно
            // cmCamera.IsLive = true; // Для v3 это делается так

            Debug.Log("Cinemachine Camera привязана!");
        }
        else if (freeLook != null)
        {
            freeLook.Follow = move.transform;
            freeLook.LookAt = move.transform;
            Debug.Log("Cinemachine FreeLook привязана!");
        }
        else
        {
            Debug.LogError("Критическая ошибка: Камера Cinemachine не найдена на сцене!");
        }

        // ВАЖНО: Принудительно блокируем мышь при регистрации, 
        // чтобы камера начала слушать движения
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void TogglePause()
    {
        if (gameMenuPanel.activeSelf)
        {
            gameMenuPanel.SetActive(false);
            pausePanel.SetActive(true);
        }
        else
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
        pausePanel.SetActive(false);
        gameMenuPanel.SetActive(true);
    }

    public void CloseAllMenus()
    {
        pausePanel.SetActive(false);
        gameMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SaveGame()
    {
        if (playerHealth == null) return;

        PlayerData data = new PlayerData
        {
            Hp = playerHealth.CurrentHealth,
            MaxHp = playerHealth.MaxHealth,
            Position = playerMove.transform.position
        };

        EnemyAI[] allEnemies = FindObjectsOfType<EnemyAI>();
        foreach (var enemy in allEnemies)
        {
            Health h = enemy.GetComponent<Health>();
            if (h != null && h.CurrentHealth > 0)
            {
                data.Enemies.Add(new EnemySaveData
                {
                    Type = enemy.enemyType.ToString(),
                    Position = enemy.transform.position,
                    CurrentHp = h.CurrentHealth
                });
            }
        }

        _interactor.SaveGame(data);
    }

    public void LoadGame()
    {
        if (playerHealth == null) return;

        _interactor.LoadGame();
        PlayerData data = _interactor.Data;

        playerHealth.SetHealth(data.Hp);
        playerMove.Teleport(data.Position);

        EnemyAI[] currentEnemies = FindObjectsOfType<EnemyAI>();
        for (int i = 0; i < currentEnemies.Length; i++)
        {
            if (i < data.Enemies.Count)
            {
                currentEnemies[i].transform.position = data.Enemies[i].Position;
                Health h = currentEnemies[i].GetComponent<Health>();
                if (h != null) h.SetHealth(data.Enemies[i].CurrentHp);
                currentEnemies[i].gameObject.SetActive(true);
            }
            else
            {
                currentEnemies[i].gameObject.SetActive(false);
            }
        }
        CloseAllMenus();
    }

    public void GoToMainMenu() { SceneManager.LoadScene(0); }
    public void QuitGame() { Application.Quit(); }
}
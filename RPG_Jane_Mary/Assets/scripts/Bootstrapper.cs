using UnityEngine;
using UnityEngine.SceneManagement;

public class Bootstrapper : MonoBehaviour
{
    [Header("Игрок")]
    public PlayerMovement playerMove;
    public PlayerCombat playerCombat;
    public Health playerHealth;

    [Header("Интерфейс (MVC View)")]
    public UI_HUD uiHudView;

    [Header("Панели Меню")]
    public GameObject pausePanel;    // Экран паузы (3 кнопки)
    public GameObject gameMenuPanel; // Игровое меню (3 кнопки)

    private GameInteractor _interactor;
    private HUD_Controller _hudController;
    private IInputService _input;

    void Awake()
    {
        // 1. Инициализация Репозитория и Интерактора (Паттерн из ТЗ)
        var repo = new GameRepository();
        _interactor = new GameInteractor(repo);

        // 2. Инициализация Ввода
        _input = new StandaloneInput();
        playerMove.Construct(_input);
        playerCombat.Construct(_input);

        // 3. Создание Контроллера интерфейса (MVC подход)
        _hudController = new HUD_Controller(uiHudView, playerHealth, playerCombat);

        // Выключаем меню при старте
        CloseAllMenus();
    }

    void Update()
    {
        // Обновление HUD через контроллер
        _hudController.UpdateHud();

        // Нажатие ESC
        if (_input.PausePressed)
        {
            TogglePause();
        }
    }

    // --- ЛОГИКА ПЕРЕКЛЮЧЕНИЯ ЭКРАНОВ ---

    public void TogglePause()
    {
        // Если открыто Игровое меню — возвращаемся на экран Паузы
        if (gameMenuPanel.activeSelf)
        {
            gameMenuPanel.SetActive(false);
            pausePanel.SetActive(true);
        }
        else
        {
            // Переключаем сам экран паузы
            bool isPaused = !pausePanel.activeSelf;
            pausePanel.SetActive(isPaused);

            Time.timeScale = isPaused ? 0f : 1f;
            Cursor.lockState = isPaused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isPaused;
        }
    }

    public void OpenGameMenu() // Кнопка "Игровое меню" на экране паузы
    {
        pausePanel.SetActive(false);
        gameMenuPanel.SetActive(true);
    }

    public void CloseAllMenus() // Кнопка "Продолжить"
    {
        pausePanel.SetActive(false);
        gameMenuPanel.SetActive(false);
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // --- ФУНКЦИИ КНОПОК ---

    public void SaveGame()
    {
        PlayerData data = new PlayerData
        {
            Hp = playerHealth.CurrentHealth,
            MaxHp = playerHealth.MaxHealth,
            Position = playerMove.transform.position
        };

        // НАХОДИМ ВСЕХ МОБОВ НА СЦЕНЕ
        EnemyAI[] allEnemies = FindObjectsOfType<EnemyAI>();
        foreach (var enemy in allEnemies)
        {
            Health h = enemy.GetComponent<Health>();
            if (h.CurrentHealth > 0) // Сохраняем только живых
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
        Debug.Log($"Сохранено! Мобов в живых: {data.Enemies.Count}");
    }

    public void LoadGame()
    {
        _interactor.LoadGame();
        PlayerData data = _interactor.Data;

        // 1. Восстанавливаем игрока
        playerHealth.SetHealth(data.Hp);
        playerMove.Teleport(data.Position);

        // 2. Восстанавливаем мобов
        // Самый простой способ для лабы: найти текущих и обновить их позиции/ХП
        EnemyAI[] currentEnemies = FindObjectsOfType<EnemyAI>();

        // Если количество совпадает, просто расставляем их
        for (int i = 0; i < currentEnemies.Length; i++)
        {
            if (i < data.Enemies.Count)
            {
                currentEnemies[i].transform.position = data.Enemies[i].Position;
                currentEnemies[i].GetComponent<Health>().SetHealth(data.Enemies[i].CurrentHp);
                currentEnemies[i].gameObject.SetActive(true);
            }
            else
            {
                // Если в сейве моба нет (он был убит), убираем его со сцены
                currentEnemies[i].gameObject.SetActive(false);
            }
        }

        Debug.Log("Загрузка завершена!");
        CloseAllMenus();
    }

    public void GoToMainMenu() { SceneManager.LoadScene(0); }
    public void QuitGame() { Application.Quit(); }
}
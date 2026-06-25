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
    public GameObject pausePanel;   
    public GameObject gameMenuPanel;

    [Header("Префабы врагов")]
    public GameObject meleeAggroPrefab;  
    public GameObject meleePeacefulPrefab;
    public GameObject rangedAggroPrefab;   
    public GameObject rangedPeacefulPrefab;
    public GameObject bossPrefab;

    public ScoreController scoreController;

    private GameInteractor _interactor;
    private HUD_Controller _hudController;
    private IInputService _input;

    void Awake()  
    {
       
        var repo = new GameRepository();
        _interactor = new GameInteractor(repo);

        
        _input = new StandaloneInput();
        playerMove.Construct(_input);
        playerCombat.Construct(_input);

       
        _hudController = new HUD_Controller(uiHudView, playerHealth, playerCombat);

       
        CloseAllMenus();
    }

    void Update()
    {
       
        _hudController.UpdateHud();


        if (_input.PausePressed)
        {
            TogglePause();
        }
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
        PlayerData data = new PlayerData
        {
            Hp = playerHealth.CurrentHealth,
            MaxHp = playerHealth.MaxHealth,
            Position = playerMove.transform.position,
            Kills = scoreController.KillCount
        };

      
        EnemyAI[] allEnemies = FindObjectsOfType<EnemyAI>();
        foreach (var enemy in allEnemies)
        {
            Health h = enemy.GetComponent<Health>();
            if (h.CurrentHealth > 0) 
            {
                string type = enemy.CompareTag("Boss") ? "Boss" : enemy.enemyType.ToString();
                data.Enemies.Add(new EnemySaveData
                {
                    Type = enemy.enemyType.ToString(),
                    Position = enemy.transform.position,
                    CurrentHp = h.CurrentHealth,
                    IsPeaceful = enemy.isPeaceful,
                   
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
        if (data == null) return;
        scoreController.SetScore(data.Kills);

        playerHealth.SetHealth(data.Hp);
        playerMove.Teleport(data.Position);

        //убираем старых мобов
        EnemyAI[] currentEnemies = FindObjectsOfType<EnemyAI>();
        foreach (var enemy in currentEnemies) Destroy(enemy.gameObject);

        //спавн новых мобов
        foreach (var enemyData in data.Enemies)
        {
            GameObject prefabToSpawn = null;
            if (enemyData.Type == "Boss")
            {
                prefabToSpawn = bossPrefab;
            }
            else if (enemyData.Type == "Melee")
            {
                prefabToSpawn = enemyData.IsPeaceful ? meleePeacefulPrefab : meleeAggroPrefab;
            }
            else if (enemyData.Type == "Ranged")
            {
                prefabToSpawn = enemyData.IsPeaceful ? rangedPeacefulPrefab : rangedAggroPrefab;
            }

            if (prefabToSpawn != null)
            {
                GameObject newEnemy = Instantiate(prefabToSpawn, enemyData.Position, Quaternion.identity);
                newEnemy.GetComponent<Health>().SetHealth(enemyData.CurrentHp);

                EnemyAI ai = newEnemy.GetComponent<EnemyAI>();
                if (ai != null)
                {
                    ai.player = playerMove.transform;
                    ai.isPeaceful = enemyData.IsPeaceful;
                }
            }
        }

        Debug.Log($"Загрузка завершена! Создано мобов: {data.Enemies.Count}");
        CloseAllMenus();
    }

    public void GoToMainMenu() { SceneManager.LoadScene(0); }
    public void QuitGame() { Application.Quit(); }
}
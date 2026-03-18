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
            Position = playerMove.transform.position
        };

      
        EnemyAI[] allEnemies = FindObjectsOfType<EnemyAI>();
        foreach (var enemy in allEnemies)
        {
            Health h = enemy.GetComponent<Health>();
            if (h.CurrentHealth > 0) 
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

        
        playerHealth.SetHealth(data.Hp);
        playerMove.Teleport(data.Position);

       
        EnemyAI[] currentEnemies = FindObjectsOfType<EnemyAI>();


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

                currentEnemies[i].gameObject.SetActive(false);
            }
        }

        Debug.Log("Загрузка завершена!");
        CloseAllMenus();
    }

    public void GoToMainMenu() { SceneManager.LoadScene(0); }
    public void QuitGame() { Application.Quit(); }
}
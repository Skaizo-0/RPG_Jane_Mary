using UnityEngine;
using UnityEngine.SceneManagement; 

public class UI_GameOver : MonoBehaviour
{
    [Header("Настройки")]
    public GameObject gameOverPanel; 

    private void OnEnable()
    {
        Health.OnPlayerDeath += ShowGameOverScreen;
    }

    private void OnDisable()
    {
        Health.OnPlayerDeath -= ShowGameOverScreen;
    }

    private void ShowGameOverScreen()
    {
        gameOverPanel.SetActive(true);

        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }


    public void RestartGame()
    {
 
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
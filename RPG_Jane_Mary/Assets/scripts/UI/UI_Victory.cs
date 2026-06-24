using UnityEngine;

public class UI_Victory : MonoBehaviour
{
    public GameObject victoryPanel;

    public void Show()
    {
        victoryPanel.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(0); // В главное меню
    }
}
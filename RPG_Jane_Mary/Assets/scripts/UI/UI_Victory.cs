using UnityEngine;
using UnityEngine.SceneManagement;

public class UI_Victory : MonoBehaviour
{
    [Header("Настройки")]
    public GameObject victoryPanel; // Перетащите сюда панель победы в инспекторе

    public void ShowVictoryScreen()
    {
        victoryPanel.SetActive(true);

        // Останавливаем время в игре
        Time.timeScale = 0f;

        // Разблокируем курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Метод для кнопки "В главное меню" или "Заново"
    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
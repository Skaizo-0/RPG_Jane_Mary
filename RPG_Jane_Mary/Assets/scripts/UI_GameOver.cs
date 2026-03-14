using UnityEngine;
using UnityEngine.SceneManagement; // Нужно для перезагрузки уровня

public class UI_GameOver : MonoBehaviour
{
    [Header("Настройки")]
    public GameObject gameOverPanel; // Ссылка на саму панель (фон + текст + кнопка)

    private void OnEnable()
    {
        // Подписываемся на событие смерти из скрипта Health
        Health.OnPlayerDeath += ShowGameOverScreen;
    }

    private void OnDisable()
    {
        // Обязательно отписываемся, чтобы не было ошибок при смене сцен
        Health.OnPlayerDeath -= ShowGameOverScreen;
    }

    private void ShowGameOverScreen()
    {
        // 1. Показываем панель
        gameOverPanel.SetActive(true);

        // 2. Останавливаем время в игре (мобы и игрок замирают)
        Time.timeScale = 0f;

        // 3. Возвращаем курсор мыши, чтобы можно было нажать на кнопку
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    // Этот метод мы назначим на кнопку "Restart"
    public void RestartGame()
    {
        // 1. Возвращаем время в нормальное состояние (ВАЖНО!)
        Time.timeScale = 1f;

        // 2. Перезагружаем текущую сцену заново
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
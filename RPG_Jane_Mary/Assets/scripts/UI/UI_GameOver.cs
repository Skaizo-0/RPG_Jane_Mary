using UnityEngine;

public class UI_GameOver : MonoBehaviour
{
    public GameObject gameOverPanel;
    public AudioClip lossSound;

    private void OnEnable() => Health.OnPlayerDeath += HandleDeath;
    private void OnDisable() => Health.OnPlayerDeath -= HandleDeath;

    private void HandleDeath()
    {
        // 1. Получаем сервис и играем звук
        ServiceLocator.Get<IAudioService>().PlaySfx(lossSound);

        // 2. Показываем панель
        gameOverPanel.SetActive(true);

        // 3. Останавливаем мир
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().name);
    }
}
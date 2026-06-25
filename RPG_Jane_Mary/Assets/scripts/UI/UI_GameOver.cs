using UnityEngine;

public class UI_GameOver : MonoBehaviour
{
    public GameObject gameOverPanel;
    public AudioClip lossSound;

    private void OnEnable() => Health.OnPlayerDeath += HandleDeath;
    private void OnDisable() => Health.OnPlayerDeath -= HandleDeath;

    private void HandleDeath()
    {
        ServiceLocator.Get<IAudioService>().PlaySfx(lossSound);

        gameOverPanel.SetActive(true);

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
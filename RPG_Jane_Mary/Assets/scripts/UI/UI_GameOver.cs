using UnityEngine;
using FishNet.Object; // Нужно для IsClient/IsOwner

public class UI_GameOver : MonoBehaviour
{
    public GameObject gameOverPanel;

    private void OnEnable() => Health.OnPlayerDeath += ShowGameOverScreen;
    private void OnDisable() => Health.OnPlayerDeath -= ShowGameOverScreen;

    private void ShowGameOverScreen()
    {
        gameOverPanel.SetActive(true);
        // В мультиплеере Time.timeScale = 0 может сломать сеть! 
        // Лучше просто выключить ввод игрока.
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void RestartGame()
    {
        // Находим нашего локального игрока
        Health localHealth = null;
        foreach (var h in FindObjectsOfType<Health>())
        {
            if (h.IsOwner && h.CompareTag("Player"))
            {
                localHealth = h;
                break;
            }
        }

        if (localHealth != null)
        {
            // Просим сервер нас возродить
            localHealth.RequestRespawnServerRpc();

            // Прячем меню
            gameOverPanel.SetActive(false);
            Time.timeScale = 1f; // Если вы его меняли
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
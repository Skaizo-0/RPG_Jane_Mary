using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class PlayerScore : NetworkBehaviour
{
    // Сетевая переменная очков. Срабатывает OnChange для обновления UI
    [SyncVar(OnChange = nameof(OnScoreChanged))]
    private int _score = 0;

    public int CurrentScore => _score;

    // Метод для сервера: добавить очко
    [ServerRpc(RequireOwnership = false)]
    public void AddPointServerRpc()
    {
        _score++;
    }

    private void OnScoreChanged(int prev, int next, bool asServer)
    {
        // Когда очки меняются, мы просим Бутстраппер обновить HUD, 
        // но только если это наш локальный игрок
        if (IsOwner && Bootstrapper.Instance != null)
        {
            // Мы обновим HUD через HUD_Controller, который уже есть в Bootstrapper
        }
    }
}
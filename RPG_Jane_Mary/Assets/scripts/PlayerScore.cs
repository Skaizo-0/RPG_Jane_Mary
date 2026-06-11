using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing; // Обязательно для SyncVar<T>

public class PlayerScore : NetworkBehaviour
{
    // В FishNet 4.0+ используем readonly SyncVar<T> вместо атрибута [SyncVar]
    private readonly SyncVar<int> _score = new SyncVar<int>();

    // Свойство для чтения очков (теперь через .Value)
    public int CurrentScore => _score.Value;

    // Метод для сервера: добавить очко
    [ServerRpc(RequireOwnership = false)]
    public void AddPointServerRpc()
    {
        if (!IsServer) return;

        // Меняем значение через .Value
        _score.Value++;

        Debug.Log($"[SERVER] Очки игрока {OwnerId} теперь: {_score.Value}");
    }
}
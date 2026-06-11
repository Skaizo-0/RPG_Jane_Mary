using UnityEngine;
using FishNet.Object;
using FishNet.Managing.Scened;
using System.Collections;
using FishNet;

public class MatchManager : NetworkBehaviour
{
    public static MatchManager Instance;
    private bool _matchEnded;

    private void Awake()
    {
        Instance = this;
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        // Мы можем оставить подписку здесь, но теперь Health вызывает это и вручную
        Health.OnAnyPlayerDeath += HandleDefeat;
        _matchEnded = false;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        Health.OnAnyPlayerDeath -= HandleDefeat;
    }

    // ИСПРАВЛЕНИЕ: Добавлено слово public, чтобы ушла ошибка CS0122
    public void HandleDefeat()
    {
        if (!IsServer || _matchEnded) return;
        _matchEnded = true;

        Debug.Log("[SERVER] КОНЕЦ ИГРЫ: ПОРАЖЕНИЕ");

        // ОСТАНАВЛИВАЕМ ВСЁ (Пауза для всех)
        StopAllActivityObserversRpc();

        // Показываем окна через первого попавшегося игрока
        foreach (var client in ServerManager.Clients.Values)
        {
            if (client.FirstObject != null)
            {
                var health = client.FirstObject.GetComponent<Health>();
                if (health != null) health.ShowEndGameUIObserversRpc(false);
                break;
            }
        }

        StartCoroutine(ReturnToLobbyRoutine());
    }

    public void HandleVictory()
    {
        if (!IsServer || _matchEnded) return;
        _matchEnded = true;

        Debug.Log("[SERVER] КОНЕЦ ИГРЫ: ПОБЕДА");

        // ОСТАНАВЛИВАЕМ ВСЁ (Пауза для всех)
        StopAllActivityObserversRpc();

        foreach (var client in ServerManager.Clients.Values)
        {
            if (client.FirstObject != null)
            {
                var health = client.FirstObject.GetComponent<Health>();
                if (health != null) health.ShowEndGameUIObserversRpc(true);
                break;
            }
        }

        StartCoroutine(ReturnToLobbyRoutine());
    }

    [ObserversRpc]
    private void StopAllActivityObserversRpc()
    {
        // Отключаем движение всем
        PlayerMovement[] allMoves = Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        foreach (var m in allMoves) m.enabled = false;

        // Отключаем атаки всем
        PlayerCombat[] allCombats = Object.FindObjectsByType<PlayerCombat>(FindObjectsSortMode.None);
        foreach (var c in allCombats) c.enabled = false;

        // Отключаем ИИ всем мобам
        EnemyAI[] allEnemies = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (var e in allEnemies) e.enabled = false;

        // Показываем курсор
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private IEnumerator ReturnToLobbyRoutine()
    {
        // Ждем 6 секунд, чтобы игроки увидели экран смерти/победы
        yield return new WaitForSeconds(6f);

        if (IsServer)
        {
            Debug.Log("[SERVER] Возвращение в главное меню...");
            SceneLoadData sld = new SceneLoadData("MainMenu");
            sld.ReplaceScenes = ReplaceOption.All;
            InstanceFinder.SceneManager.LoadGlobalScenes(sld);
        }

        _matchEnded = false;
    }
}
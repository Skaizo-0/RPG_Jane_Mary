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
        Health.OnAnyPlayerDeath += HandleDefeat;
        _matchEnded = false;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();
        Health.OnAnyPlayerDeath -= HandleDefeat;
    }

    private void HandleDefeat()
    {
        if (!IsServer || _matchEnded) return;
        _matchEnded = true;

        Debug.Log("[SERVER] КОНЕЦ ИГРЫ: ПОРАЖЕНИЕ");

        // 1. Останавливаем мир для всех
        StopAllActivityObserversRpc();

        // 2. Показываем окна
        foreach (var client in ServerManager.Clients.Values)
        {
            if (client.FirstObject != null)
            {
                client.FirstObject.GetComponent<Health>().ShowEndGameUIObserversRpc(false);
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

        // 1. Останавливаем мир для всех
        StopAllActivityObserversRpc();

        // 2. Показываем окна
        foreach (var client in ServerManager.Clients.Values)
        {
            if (client.FirstObject != null)
            {
                client.FirstObject.GetComponent<Health>().ShowEndGameUIObserversRpc(true);
                break;
            }
        }

        StartCoroutine(ReturnToLobbyRoutine());
    }

    [ObserversRpc]
    private void StopAllActivityObserversRpc()
    {
        // ВЫКЛЮЧАЕМ ВСЁ
        // 1. Движение и бой игроков
        PlayerMovement[] players = Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);
        foreach (var p in players) p.enabled = false;

        PlayerCombat[] combats = Object.FindObjectsByType<PlayerCombat>(FindObjectsSortMode.None);
        foreach (var c in combats) c.enabled = false;

        // 2. ИИ врагов
        EnemyAI[] enemies = Object.FindObjectsByType<EnemyAI>(FindObjectsSortMode.None);
        foreach (var e in enemies) e.enabled = false;

        Debug.Log("[CLIENT] Геймплей остановлен (Пауза финала)");
    }

    private IEnumerator ReturnToLobbyRoutine()
    {
        // Даем 7 секунд насладиться результатом
        yield return new WaitForSeconds(7f);

        if (IsServer)
        {
            Debug.Log("[SERVER] Возврат в меню...");
            SceneLoadData sld = new SceneLoadData("MainMenu");
            sld.ReplaceScenes = ReplaceOption.All;
            InstanceFinder.SceneManager.LoadGlobalScenes(sld);
        }

        _matchEnded = false;
    }
}
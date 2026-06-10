using UnityEngine;
using UnityEngine.SceneManagement;
using FishNet;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Connection;
using System.Collections;
using System.Collections.Generic;

public class MainMenuController
{
    private readonly MainMenuView _view;
    private readonly IAudioService _audioService;
    private static Transform _manualSpawnPoint;

    public MainMenuController(MainMenuView view, IAudioService audioService)
    {
        _view = view;
        _audioService = audioService;

        _view.playButton.onClick.AddListener(OpenConnectionMenu);
        _view.settingsButton.onClick.AddListener(OpenSettings);
        _view.backButton.onClick.AddListener(CloseSettings);
        _view.volumeSlider.onValueChanged.AddListener(SetVolume);

        _view.hostButton.onClick.AddListener(StartHostLocal);
        _view.clientButton.onClick.AddListener(StartClientLocal);

        if (_view.startMatchButton != null)
            _view.startMatchButton.onClick.AddListener(StartMatchAndLoadScene);

        _view.startMatchButton.gameObject.SetActive(false);

        // КРИТИЧЕСКИЙ ФИКС ДУБЛИКАТОВ: Сначала отписываемся, потом подписываемся
        InstanceFinder.SceneManager.OnLoadEnd -= OnSceneLoaded;
        InstanceFinder.SceneManager.OnLoadEnd += OnSceneLoaded;

        StaticUpdateLoop.OnUpdate += UpdateLobbyUI;

        Debug.Log("[LOG] MainMenuController запущен.");
    }

    private void OpenConnectionMenu()
    {
        _view.mainButtonsPanel.SetActive(false);
        _view.connectionPanel.SetActive(true);
    }

    private void StartHostLocal()
    {
        Debug.Log("[LOG] Старт HOST...");
        InstanceFinder.ServerManager.StartConnection();
        InstanceFinder.ClientManager.StartConnection();

        _view.connectionPanel.SetActive(false);
        _view.lobbyPanel.SetActive(true);
        if (_view.startMatchButton != null) _view.startMatchButton.gameObject.SetActive(true);
    }

    private void StartClientLocal()
    {
        string ip = _view.ipInputField.text;
        if (string.IsNullOrEmpty(ip)) ip = "127.0.0.1";

        InstanceFinder.TransportManager.Transport.SetClientAddress(ip);
        InstanceFinder.ClientManager.StartConnection();

        _view.connectionPanel.SetActive(false);
        _view.lobbyPanel.SetActive(true);
    }

    private void UpdateLobbyUI()
    {
        if (_view.lobbyPanel != null && _view.lobbyPanel.activeSelf)
        {
            int count = 0;
            if (InstanceFinder.IsServerStarted) count = InstanceFinder.ServerManager.Clients.Count;
            else if (InstanceFinder.IsClientStarted) count = 1;

            if (_view.playerCountText != null)
                _view.playerCountText.text = $"Игроков в лобби: {count}";
        }
    }

    private void StartMatchAndLoadScene()
    {
        if (!InstanceFinder.IsServerStarted) return;

        Debug.Log("[LOG] Загрузка игровой сцены...");
        _view.gameObject.SetActive(false);

        SceneLoadData sld = new SceneLoadData("SampleScene");
        sld.ReplaceScenes = ReplaceOption.All;
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
    }

    private void OnSceneLoaded(SceneLoadEndEventArgs args)
    {
        if (!InstanceFinder.IsServerStarted) return;

        bool isGameScene = false;
        foreach (var s in args.LoadedScenes) if (s.name == "SampleScene") isGameScene = true;

        if (isGameScene)
        {
            Debug.Log("[SERVER] Сцена готова. Спавним игроков.");
            SpawnPlayersForAll();
        }
    }

    private void SpawnPlayersForAll()
    {
        GameObject spObj = GameObject.Find("Spawn_POint");
        Vector3 basePos = spObj != null ? spObj.transform.position : Vector3.zero;

        foreach (var conn in InstanceFinder.ServerManager.Clients.Values)
        {
            // ФИКС: Если игрок уже заспавнен (FirstObject не null), пропускаем его, чтобы не было дублей!
            if (conn.FirstObject != null) continue;

            Vector3 spawnOffset = new Vector3(conn.ClientId * 2.0f, 1.5f, 0f);
            Vector3 finalPos = basePos + spawnOffset;

            NetworkObject nob = Object.Instantiate(_view.playerPrefab, finalPos, Quaternion.identity);
            InstanceFinder.ServerManager.Spawn(nob, conn);

            Debug.Log($"[SERVER] Игрок {conn.ClientId} заспавнен.");
        }
    }

    public static void SetGlobalSpawnPoint(Transform t) => _manualSpawnPoint = t;

    private void OpenSettings() { _view.settingsWindow.SetActive(true); _view.mainButtonsPanel.SetActive(false); }
    private void CloseSettings() { _view.settingsWindow.SetActive(false); _view.mainButtonsPanel.SetActive(true); _view.connectionPanel.SetActive(false); _view.lobbyPanel.SetActive(false); }
    private void SetVolume(float vol) => _audioService.SetVolume(vol);
}

public static class StaticUpdateLoop
{
    public static System.Action OnUpdate;
    private class Hook : MonoBehaviour { void Update() => OnUpdate?.Invoke(); }
    static StaticUpdateLoop()
    {
        GameObject hookObj = new GameObject("StaticUpdateHook");
        hookObj.AddComponent<Hook>();
        Object.DontDestroyOnLoad(hookObj);
        hookObj.hideFlags = HideFlags.HideAndDontSave;
    }
}
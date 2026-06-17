using UnityEngine;
using UnityEngine.SceneManagement;
using System.Threading.Tasks;
using Unity.Services.Core;
using Unity.Services.Authentication;
using FishNet;
using FishNet.Transporting.UTP;
using FishNet.Managing.Scened;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

public class MainMenuController
{
    private readonly MainMenuView _view;
    private readonly IAudioService _audioService;
    private string _joinCode;

    public MainMenuController(MainMenuView view, IAudioService audioService)
    {
        _view = view;
        _audioService = audioService;

        InitializeUnityServices();

        _view.playButton.onClick.AddListener(OpenConnectionMenu);
        _view.settingsButton.onClick.AddListener(OpenSettings);
        _view.backButton.onClick.AddListener(CloseSettings);
        _view.volumeSlider.onValueChanged.AddListener(SetVolume);

        _view.hostButton.onClick.AddListener(async () => await StartHostWithRelay());
        _view.clientButton.onClick.AddListener(async () => await JoinClientWithRelay());

        if (_view.startMatchButton != null)
            _view.startMatchButton.onClick.AddListener(StartGameNetworked);

        InstanceFinder.SceneManager.OnLoadEnd += (args) => {
            if (args.LoadedScenes != null && args.LoadedScenes.Length > 0)
            {
                foreach (var scene in args.LoadedScenes)
                {
                    if (scene.name == "SampleScene")
                    {
                        UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
                        Debug.Log("[SCENE] Active scene set to SampleScene");
                        Debug.Log("[CLEANUP] Игровая сцена загружена. Чистим старые камеры...");

                        // 1. Ищем ВСЕ камеры на сцене
                        Camera[] allCameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);

                        foreach (var cam in allCameras)
                        {
                            // 2. Если камера НЕ принадлежит новой сцене SampleScene — отключаем её
                            if (cam.gameObject.scene.name != "SampleScene")
                            {
                                Debug.Log($"[CLEANUP] Отключаю камеру меню: {cam.name}");
                                cam.tag = "Untagged"; // Убираем тег MainCamera
                                cam.enabled = false;   // Выключаем саму камеру

                                // Отключаем AudioListener, чтобы не было ошибок "2 listeners in scene"
                                if (cam.TryGetComponent(out AudioListener listener))
                                    listener.enabled = false;
                            }
                        }

                        HideMenus();
                        break;
                    }
                }
            }
        };

        StaticUpdateLoop.OnUpdate += UpdateLobbyUI;
    }

    private async void InitializeUnityServices()
    {
        try
        {
            var options = new InitializationOptions();
            await UnityServices.InitializeAsync(options);
            if (!AuthenticationService.Instance.IsSignedIn)
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("[RELAY] Сервисы Unity инициализированы.");
        }
        catch (System.Exception e) { Debug.LogError($"[RELAY] Ошибка инициализации: {e.Message}"); }
    }

    private void OpenConnectionMenu()
    {
        _view.mainButtonsPanel.SetActive(false);
        _view.connectionPanel.SetActive(true);
    }

    private async Task StartHostWithRelay()
    {
        try
        {
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(4);
            _joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            if (_view.hostCodeDisplay != null) _view.hostCodeDisplay.text = $"КОД: {_joinCode}";
            Debug.LogError("МОЙ КОД ТУТ: " + _joinCode);

            if (InstanceFinder.TransportManager.Transport is UnityTransport transport)
            {
                var serverData = allocation.ToRelayServerData("udp");
                transport.SetRelayServerData(serverData);
            }

            InstanceFinder.ServerManager.StartConnection();
            InstanceFinder.ClientManager.StartConnection();

            _view.connectionPanel.SetActive(false);
            _view.lobbyPanel.SetActive(true);
            if (_view.startMatchButton != null) _view.startMatchButton.gameObject.SetActive(true);
        }
        catch (System.Exception e) { Debug.LogError($"[RELAY] Ошибка Хоста: {e.Message}"); }
    }

    private async Task JoinClientWithRelay()
    {
        // Очищаем введенный текст от пробелов и переводим в верхний регистр
        string inputCode = _view.joinCodeInputField.text.Trim().ToUpper();

        // Код Relay всегда состоит из 6 символов. Проверяем это.
        if (string.IsNullOrEmpty(inputCode) || inputCode.Length != 6)
        {
            Debug.LogError($"[RELAY] Неверный формат кода: '{inputCode}'. Код должен состоять из 6 символов.");
            return;
        }

        try
        {
            Debug.Log($"[RELAY] Попытка подключения с кодом: {inputCode}");

            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(inputCode);

            if (InstanceFinder.TransportManager.Transport is UnityTransport transport)
            {
                var clientData = joinAllocation.ToRelayServerData("udp");
                transport.SetRelayServerData(clientData);
            }

            InstanceFinder.ClientManager.StartConnection();

            _view.connectionPanel.SetActive(false);
            _view.lobbyPanel.SetActive(true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[RELAY] Ошибка Клиента (код {inputCode}): {e.Message}");
        }
    }

    private void UpdateLobbyUI()
    {
        if (_view.lobbyPanel != null && _view.lobbyPanel.activeSelf)
        {
            int count = 0;
            if (InstanceFinder.IsServerStarted) count = InstanceFinder.ServerManager.Clients.Count;

            if (_view.playerCountText != null)
            {
                if (InstanceFinder.IsServerStarted)
                    _view.playerCountText.text = $"Игроков: {count}";
                else
                    _view.playerCountText.text = "Подключено к лобби";
            }
        }
    }

    private void StartGameNetworked()
    {
        if (!InstanceFinder.IsServerStarted) return;
        SceneLoadData sld = new SceneLoadData("SampleScene");
        InstanceFinder.SceneManager.LoadGlobalScenes(sld);
        // HideMenus(); // УДАЛИ ОТСЮДА. Оно вызовется само через OnClientLoadedStartScenes
    }

    private void HideMenus()
    {
        _view.lobbyPanel.SetActive(false);
        _view.mainButtonsPanel.SetActive(false);
        _view.connectionPanel.SetActive(false);

        // Разблокируем камеру
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void OpenSettings() { _view.settingsWindow.SetActive(true); _view.mainButtonsPanel.SetActive(false); }
    private void CloseSettings() { _view.settingsWindow.SetActive(false); _view.mainButtonsPanel.SetActive(true); _view.connectionPanel.SetActive(false); _view.lobbyPanel.SetActive(false); }
    private void SetVolume(float vol) => _audioService.SetVolume(vol);
}

public static class StaticUpdateLoop
{
    public static System.Action OnUpdate;
    private class Hook : MonoBehaviour { private void Update() => OnUpdate?.Invoke(); }
    static StaticUpdateLoop()
    {
        GameObject hookObj = new GameObject("StaticUpdateHook");
        hookObj.AddComponent<Hook>();
        Object.DontDestroyOnLoad(hookObj);
        hookObj.hideFlags = HideFlags.HideAndDontSave;
    }
}
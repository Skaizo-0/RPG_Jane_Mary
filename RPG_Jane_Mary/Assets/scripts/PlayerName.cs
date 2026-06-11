using UnityEngine;
using FishNet.Object;
using FishNet.Object.Synchronizing;
using TMPro;
using System.Collections;

public class PlayerName : NetworkBehaviour
{
    [SerializeField]
    private TextMeshProUGUI nameText;

    // Синхронизируемая переменная
    private readonly SyncVar<string> _nickname = new SyncVar<string>();

    private bool _subscribed;
    private bool _isInitialized;

    #region NETWORK

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();
        Subscribe();

        Debug.Log($"[PlayerName] OnStartNetwork | Obj:{ObjectId} | Owner:{OwnerId}");

        if (Owner.IsLocalClient)
        {
            // Если мы сами зашли - генерируем себе имя
            string randomName = $"Игрок #{Random.Range(100, 999)}";
            Debug.Log($"[PlayerName] Отправляем ник на сервер: {randomName}");

            SetNicknameServerRpc(randomName);
            UpdateNameUI(randomName);
        }
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        Debug.Log($"[PlayerName] OnStartClient | Obj:{ObjectId} | CurrentNick:'{_nickname.Value}'");

        // Попытка обновить сразу при появлении
        RefreshUI();
    }

    private void Update()
    {
        // КРИТИЧЕСКИЙ ФИКС: 
        // Если текст над головой всё еще пустой или стандартный, 
        // но в сетевой переменной уже есть имя — принудительно обновляем!
        if (!_isInitialized || (nameText != null && (nameText.text == "" || nameText.text == "New Text")))
        {
            if (!string.IsNullOrEmpty(_nickname.Value))
            {
                UpdateNameUI(_nickname.Value);
                _isInitialized = true;
            }
        }
    }

    public override void OnStartServer()
    {
        base.OnStartServer();
        Debug.Log($"[PlayerName] OnStartServer | Obj:{ObjectId}");
    }

    public override void OnStopNetwork()
    {
        base.OnStopNetwork();
        if (_subscribed)
        {
            _nickname.OnChange -= OnNicknameChanged;
            _subscribed = false;
        }
    }

    #endregion

    #region RPC

    [ServerRpc]
    private void SetNicknameServerRpc(string nickname)
    {
        Debug.Log($"[SERVER] Получен ник '{nickname}' для объекта {ObjectId}");
        _nickname.Value = nickname;

        // На всякий случай дублируем через RPC с буфером
        SetNicknameObserversRpc(nickname);
    }

    [ObserversRpc(BufferLast = true)]
    private void SetNicknameObserversRpc(string nickname)
    {
        Debug.Log($"[PlayerName] Получен буферизированный RPC ник: {nickname} для объекта {ObjectId}");
        UpdateNameUI(nickname);
    }

    #endregion

    #region SYNCHRONIZATION

    private void Subscribe()
    {
        if (_subscribed) return;
        _nickname.OnChange += OnNicknameChanged;
        _subscribed = true;
    }

    private void OnNicknameChanged(string prev, string next, bool asServer)
    {
        Debug.Log($"[PlayerName] Nick changed | Obj:{ObjectId} | '{prev}' -> '{next}' | asServer:{asServer}");
        UpdateNameUI(next);
    }

    #endregion

    #region UI

    private void RefreshUI()
    {
        if (!string.IsNullOrEmpty(_nickname.Value))
        {
            UpdateNameUI(_nickname.Value);
        }
        else
        {
            Debug.Log($"[PlayerName] RefreshUI: ник ещё не пришёл для объекта {ObjectId}");
        }
    }

    private void UpdateNameUI(string nickname)
    {
        if (nameText == null)
        {
            Debug.LogError($"[PlayerName] nameText НЕ назначен на объекте {gameObject.name}");
            return;
        }

        if (string.IsNullOrEmpty(nickname)) return;

        nameText.text = nickname;
        nameText.gameObject.SetActive(true);

        Debug.Log($"[PlayerName] UI обновлён | Obj:{ObjectId} | Nick:'{nickname}'");
    }

    #endregion
}
using UnityEngine;
using FishNet; // ДОБАВЛЕНО: Теперь InstanceFinder будет работать
using FishNet.Object;
using FishNet.Object.Synchronizing;

public class EnemySpawner : NetworkBehaviour
{
    [Header("Точки спавна")]
    [Tooltip("Перетащите сюда пустышки из иерархии")]
    public Transform[] spawnPoints;

    [Header("Настройки префабов")]
    public GameObject[] enemyPrefabs;

    [Header("Редкие мобы")]
    [Range(0f, 100f)]
    public float rareMobChance = 10f;
    public float rareMobStatMultiplier = 2f;

    // Флаг, чтобы не заспавнить дважды
    private bool _hasSpawned = false;

    public override void OnStartServer()
    {
        base.OnStartServer();

        // Запускаем спавн только один раз при старте сервера
        if (!_hasSpawned)
        {
            SpawnAllEnemies();
            _hasSpawned = true;
        }
    }

    private void SpawnAllEnemies()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("[SERVER] EnemySpawner: Точки спавна не назначены!");
            return;
        }

        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            Debug.LogWarning("[SERVER] EnemySpawner: Список префабов пуст!");
            return;
        }

        Debug.Log($"[SERVER] Начинаю спавн {spawnPoints.Length} врагов...");

        // Проходим по каждой точке из массива
        foreach (Transform point in spawnPoints)
        {
            if (point == null) continue;

            SpawnSingleEnemy(point.position, point.rotation);
        }
    }

    private void SpawnSingleEnemy(Vector3 position, Quaternion rotation)
    {
        // Выбираем случайный префаб
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject prefab = enemyPrefabs[randomIndex];

        if (prefab == null) return;

        // Создаем объект
        GameObject enemy = Instantiate(prefab, position, rotation);

        // Проверяем наличие NetworkObject
        NetworkObject nob = enemy.GetComponent<NetworkObject>();
        if (nob == null)
        {
            Debug.LogError($"[SERVER] На префабе {prefab.name} нет NetworkObject!");
            Destroy(enemy);
            return;
        }

        // Твои отладочные логи теперь будут работать
        Debug.Log($"SPAWN {prefab.name}");
        Debug.Log($"Prefab ID: {nob.PrefabId}");

        // Проверка: зарегистрирован ли префаб в NetworkManager
        bool isRegistered = InstanceFinder.NetworkManager.SpawnablePrefabs.GetObject(true, nob.PrefabId) != null;
        Debug.Log($"Is Prefab Registered: {isRegistered}");

        // Регистрируем в сети FishNet
        ServerManager.Spawn(enemy);

        // Логика редкости
        bool makeRare = Random.Range(0f, 100f) <= rareMobChance;
        if (makeRare)
        {
            MakeRare(enemy);
        }
    }

    private void MakeRare(GameObject enemy)
    {
        if (enemy == null) return;

        Health hp = enemy.GetComponent<Health>();
        if (hp != null)
        {
            float newHp = hp.maxHp * rareMobStatMultiplier;
            hp.SetHealth(newHp);
        }

        SetRareVisualsObserversRpc(enemy);
    }

    [ObserversRpc(BufferLast = true)]
    private void SetRareVisualsObserversRpc(GameObject enemy)
    {
        if (enemy == null) return;

        if (!enemy.name.Contains("(RARE)"))
            enemy.name += " (RARE)";

        enemy.transform.localScale *= 1.5f;

        Renderer rend = enemy.GetComponentInChildren<Renderer>();
        if (rend != null)
        {
            rend.material.color = Color.yellow;
        }
    }

    private void OnDrawGizmos()
    {
        if (spawnPoints == null) return;

        Gizmos.color = Color.red;
        foreach (var point in spawnPoints)
        {
            if (point != null)
            {
                Gizmos.DrawSphere(point.position, 0.5f);
                Gizmos.DrawRay(point.position, point.forward * 1.5f);
            }
        }
    }
}
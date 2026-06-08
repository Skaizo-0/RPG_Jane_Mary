using UnityEngine;
using FishNet.Object; // Добавили

public class EnemySpawner : NetworkBehaviour // Изменили на NetworkBehaviour
{
    [Header("Настройки спавна")]
    public GameObject[] enemyPrefabs;
    public float spawnInterval = 5f;
    public float spawnRadius = 10f;

    [Header("Доп. балл: Редкие мобы")]
    [Range(0, 100)]
    public float rareMobChance = 10f;
    public float rareMobStatMultiplier = 2f;

    private float _nextSpawnTime;

    void Update()
    {
        // КЛЮЧЕВОЕ: Только сервер решает, когда и где спавнить врага
        if (!IsServer) return;

        if (Time.time >= _nextSpawnTime)
        {
            SpawnEnemy();
            _nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs.Length == 0) return;

        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject newEnemy = Instantiate(enemyPrefabs[randomIndex], spawnPos, Quaternion.identity);

        // КЛЮЧЕВОЕ: Сначала спавним в сети
        ServerManager.Spawn(newEnemy);

        // Логика редкого моба
        if (Random.Range(0f, 100f) <= rareMobChance)
        {
            MakeRare(newEnemy);
        }
    }

    void MakeRare(GameObject enemy)
    {
        // На сервере меняем статы
        Health hp = enemy.GetComponent<Health>();
        if (hp != null)
        {
            float boostedHp = hp.maxHp * rareMobStatMultiplier;
            hp.SetHealth(boostedHp);
        }

        // Рассылаем всем визуальные изменения (цвет и масштаб)
        SetRareVisualsObserversRpc(enemy);
    }

    [ObserversRpc]
    private void SetRareVisualsObserversRpc(GameObject enemy)
    {
        if (enemy == null) return;

        enemy.name += " (RARE)";
        enemy.transform.localScale *= 1.5f;

        Renderer rend = enemy.GetComponentInChildren<Renderer>();
        if (rend != null) rend.material.color = Color.yellow;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
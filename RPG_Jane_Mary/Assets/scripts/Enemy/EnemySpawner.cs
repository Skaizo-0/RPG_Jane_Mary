using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Настройки спавна")]
    public GameObject[] enemyPrefabs; // Массив префабов (скелеты с мечом, топором, магов с разными посохами)
    public float spawnInterval = 5f;  // Раз во сколько секунд спавнить
    public float spawnRadius = 10f;   // Радиус случайного появления вокруг спавнера

    [Header("Доп. балл: Редкие мобы")]
    [Range(0, 100)]
    public float rareMobChance = 10f; // Шанс (в %) появления усиленного моба
    public float rareMobStatMultiplier = 2f; // Во сколько раз усилить ХП и размер

    private Transform _playerTransform;
    private float _nextSpawnTime;

    void Start()
    {
        // Находим игрока один раз, чтобы передавать его созданным мобам
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null) _playerTransform = player.transform;
    }

    void Update()
    {
        if (Time.time >= _nextSpawnTime)
        {
            SpawnEnemy();
            _nextSpawnTime = Time.time + spawnInterval;
        }
    }

    void SpawnEnemy()
    {
        if (enemyPrefabs.Length == 0) return;

        // 1. Случайная точка (доп. балл)
        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);

        // 2. Выбор случайного префаба (вариативность оружия)
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject newEnemy = Instantiate(enemyPrefabs[randomIndex], spawnPos, Quaternion.identity);

        // 3. Настройка моба (передаем игрока в EnemyAI)
        EnemyAI ai = newEnemy.GetComponent<EnemyAI>();
        if (ai != null) ai.player = _playerTransform;

        // 4. Проверка на редкого моба (доп. балл)
        if (Random.Range(0f, 100f) <= rareMobChance)
        {
            MakeRare(newEnemy);
        }
    }

    void MakeRare(GameObject enemy)
    {
        enemy.name += " (RARE)";

        // Увеличиваем визуально
        enemy.transform.localScale *= 1.5f;

        // Увеличиваем HP (через наш скрипт Health)
        Health hp = enemy.GetComponent<Health>();
        if (hp != null)
        {
            float boostedHp = hp.maxHp * rareMobStatMultiplier;
            hp.SetHealth(boostedHp); // Метод SetHealth уже есть в твоем коде
        }

        // Можно подсветить его цветом (если есть Renderer)
        Renderer rend = enemy.GetComponentInChildren<Renderer>();
        if (rend != null) rend.material.color = Color.yellow;
    }

    // Рисуем радиус спавна в редакторе
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
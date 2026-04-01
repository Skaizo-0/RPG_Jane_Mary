using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Настройки спавна")]
    public GameObject[] enemyPrefabs; 
    public float spawnInterval = 5f;  
    public float spawnRadius = 10f;   

    [Header("Доп. балл: Редкие мобы")]
    [Range(0, 100)]
    public float rareMobChance = 10f; 
    public float rareMobStatMultiplier = 2f; 

    private Transform _playerTransform;
    private float _nextSpawnTime;

    void Start()
    {
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


        Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);


        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        GameObject newEnemy = Instantiate(enemyPrefabs[randomIndex], spawnPos, Quaternion.identity);


        EnemyAI ai = newEnemy.GetComponent<EnemyAI>();
        if (ai != null) ai.player = _playerTransform;


        if (Random.Range(0f, 100f) <= rareMobChance)
        {
            MakeRare(newEnemy);
        }
    }

    void MakeRare(GameObject enemy)
    {
        enemy.name += " (RARE)";

       
        enemy.transform.localScale *= 1.5f;

       
        Health hp = enemy.GetComponent<Health>();
        if (hp != null)
        {
            float boostedHp = hp.maxHp * rareMobStatMultiplier;
            hp.SetHealth(boostedHp); 
        }

       
        Renderer rend = enemy.GetComponentInChildren<Renderer>();
        if (rend != null) rend.material.color = Color.yellow;
    }

   
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
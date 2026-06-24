using UnityEngine;

public class ScoreController : MonoBehaviour
{
    [Header("Ссылки на UI")]
    public UI_HUD hudView;
    public UI_Victory victoryView;

    [Header("Параметры босса")]
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;

    [Header("Аудио клипы")]
    public AudioClip bossSpawnSfx;    // Звук появления (рык/взрыв)
    public AudioClip bossBattleMusic; // Фоновая музыка боя с боссом
    public AudioClip victoryMusic;    // Звук победы

    private int _killCount = 0;
    public int KillCount => _killCount;

    private void Start()
    {
        Health.OnEnemyDeath += HandleKill;
    }

    private void OnDestroy() => Health.OnEnemyDeath -= HandleKill;

    public void SetScore(int value)
    {
        _killCount = value;
        hudView.scoreText.text = $"Убито: {_killCount}";

        // Если при загрузке очков меньше 2, а музыка босса играет - 
        // тут можно было бы вернуть обычную музыку, но это по желанию
    }

    private void HandleKill(GameObject victim)
    {
        if (victim.CompareTag("Boss"))
        {
            ExecuteVictory();
            return;
        }

        _killCount++;
        hudView.scoreText.text = $"Убито: {_killCount}";

        // Когда убито 2 моба — призываем босса
        if (_killCount == 2)
        {
            SpawnBoss();
        }
    }

    private void SpawnBoss()
    {
        if (bossPrefab == null || bossSpawnPoint == null) return;

        // 1. Создаем босса
        GameObject spawnedBoss = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
        BossAI bossScript = spawnedBoss.GetComponent<BossAI>();
        if (bossScript != null)
        {
            bossScript.player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        // --- ЛОГИКА ЗВУКА (Лекция 3: Service Locator) ---
        var audioService = ServiceLocator.Get<IAudioService>();
        if (audioService != null)
        {
            // 2. Играем разовый звук появления (рык)
            if (bossSpawnSfx != null)
                audioService.PlaySfx(bossSpawnSfx);

            // 3. Меняем фоновую музыку на боевую (зацикленную)
            if (bossBattleMusic != null)
                audioService.PlayMusic(bossBattleMusic);
        }
    }

    private void ExecuteVictory()
    {
        var audioService = ServiceLocator.Get<IAudioService>();
        if (audioService != null && victoryMusic != null)
        {
            audioService.PlaySfx(victoryMusic);
        }

        if (victoryView != null) victoryView.Show();
    }
}
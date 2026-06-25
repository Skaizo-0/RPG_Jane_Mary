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
    public AudioClip bossSpawnSfx;    
    public AudioClip bossBattleMusic; 
    public AudioClip victoryMusic;    

    private int _killCount = 0;
    public int KillCount => _killCount;

    private void Start()
    {
        //подписка на событие
        Health.OnEnemyDeath += HandleKill;
    }
    //отписка от события
    private void OnDestroy() => Health.OnEnemyDeath -= HandleKill;
    // метод для загрузки сохранения 
    public void SetScore(int value)
    {
        _killCount = value;
        hudView.scoreText.text = $"Убито: {_killCount}";

    }

    private void HandleKill(GameObject victim)
    {
        _killCount++;
        hudView.scoreText.text = $"Убито: {_killCount}";

        if (_killCount == 3)
        {
            SpawnBoss();
        }

        if (victim.CompareTag("Boss") && _killCount >= 5)
        {
            ExecuteVictory();
        }

        else if (_killCount >= 5)
        {
            ExecuteVictory();
        }
    }

    private void SpawnBoss()
    {
        if (bossPrefab == null || bossSpawnPoint == null) return;

        GameObject spawnedBoss = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
        BossAI bossScript = spawnedBoss.GetComponent<BossAI>();
        if (bossScript != null)
        {
            // передаем боссу ссылку на игрока
            bossScript.player = GameObject.FindGameObjectWithTag("Player").transform;
        }

        var audioService = ServiceLocator.Get<IAudioService>();
        if (audioService != null)
        {
            if (bossSpawnSfx != null)
                audioService.PlaySfx(bossSpawnSfx);

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
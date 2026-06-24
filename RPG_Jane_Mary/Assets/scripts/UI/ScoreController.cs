using UnityEngine;

public class ScoreController : MonoBehaviour
{
    [Header("Ссылки на UI")]
    public UI_HUD hudView;
    public UI_Victory victoryUI; // <-- НОВАЯ ССЫЛКА

    [Header("Параметры босса")]
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;
    public AudioClip victoryMusic;

    private int _killCount = 0;
    private IAudioService _audioService;

    private void Start()
    {
        _audioService = ServiceLocator.Get<IAudioService>();
        Health.OnEnemyDeath += HandleKill;
        UpdateScoreUI();
    }

    private void OnDestroy()
    {
        Health.OnEnemyDeath -= HandleKill;
    }

    private void HandleKill(GameObject victim)
    {
        // ЕСЛИ УМЕР БОСС — ЭТО ПОБЕДА!
        if (victim.CompareTag("Boss"))
        {
            PlayVictory();
            return;
        }

        _killCount++;
        UpdateScoreUI();

        // Спавним босса на 2-м убийстве
        if (_killCount == 2)
        {
            SpawnBoss();
        }

        // (Опционально) Если вы хотите победу просто по количеству убийств:
        // if (_killCount == 10) PlayVictory(); 
    }

    private void UpdateScoreUI()
    {
        if (hudView.scoreText != null)
            hudView.scoreText.text = $"Убито: {_killCount}";
    }

    private void SpawnBoss()
    {
        if (bossPrefab == null || bossSpawnPoint == null) return;

        GameObject spawnedBoss = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
        BossAI bossScript = spawnedBoss.GetComponent<BossAI>();

        if (bossScript != null)
        {
            bossScript.player = GameObject.FindGameObjectWithTag("Player").transform;
        }
    }

    private void PlayVictory()
    {
        Debug.Log("Победа!");

        // 1. Показываем экран победы
        if (victoryUI != null)
        {
            victoryUI.ShowVictoryScreen();
        }

        // 2. Играем музыку
        if (victoryMusic != null && _audioService != null)
        {
            _audioService.PlayMusic(victoryMusic);
        }
    }
}
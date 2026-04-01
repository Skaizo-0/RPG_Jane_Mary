using UnityEngine;

public class ScoreController : MonoBehaviour
{
    [Header("Ссылки на UI")]
    public UI_HUD hudView;

    [Header("Настройки событий (ТЗ)")]
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

        if (victim.CompareTag("Boss")) return;

        _killCount++;
        UpdateScoreUI();

 
        if (_killCount == 2)
        {
            SpawnBoss();
        }


        if (_killCount == 3)
        {
            PlayVictory();
        }
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
        Debug.Log("СОБЫТИЕ: Победная музыка!");
        if (victoryMusic != null)
        {
            _audioService.PlayMusic(victoryMusic);
        }
    }
}
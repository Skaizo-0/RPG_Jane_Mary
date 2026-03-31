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
        // Получаем аудио сервис из локатора (как в Лабе 3)
        _audioService = ServiceLocator.Get<IAudioService>();

        // Подписываемся на статическое событие из твоего скрипта Health
        Health.OnEnemyDeath += HandleKill;

        UpdateScoreUI();
    }

    private void OnDestroy()
    {
        // Обязательно отписываемся (Лекция 3, раздел 4.1)
        Health.OnEnemyDeath -= HandleKill;
    }

    private void HandleKill(GameObject victim)
    {
        // Не считаем за очки смерть самого босса
        if (victim.CompareTag("Boss")) return;

        _killCount++;
        UpdateScoreUI();

        // 1. Событие: Босс после 3-х мобов
        if (_killCount == 3)
        {
            SpawnBoss();
        }

        // 2. Событие: Победная мелодия после 5 мобов
        if (_killCount == 5)
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

        // 1. Создаем босса
        GameObject spawnedBoss = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);

        // 2. Находим на нем скрипт
        BossAI bossScript = spawnedBoss.GetComponent<BossAI>();

        if (bossScript != null)
        {
            // 3. Передаем ему игрока (ищем его на сцене один раз)
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
using UnityEngine;

public class ScoreController : MonoBehaviour
{
    public UI_HUD hudView;
    public UI_Victory victoryView; // Ссылка на экран победы
    public GameObject bossPrefab;
    public Transform bossSpawnPoint;
    public AudioClip victoryMusic;

    private int _killCount = 0;

    private void Start()
    {
        Health.OnEnemyDeath += HandleKill;
    }

    private void OnDestroy() => Health.OnEnemyDeath -= HandleKill;

    private void HandleKill(GameObject victim)
    {
        // Факт: Убит босс -> Победа
        if (victim.CompareTag("Boss"))
        {
            ExecuteVictory();
            return;
        }

        _killCount++;
        hudView.scoreText.text = $"Убито: {_killCount}";

        if (_killCount == 2) SpawnBoss();
    }

    private void ExecuteVictory()
    {
        // Звук через сервис
        ServiceLocator.Get<IAudioService>().PlaySfx(victoryMusic);

        // Показ UI
        if (victoryView != null) victoryView.Show();
    }

    private void SpawnBoss()
    {
        GameObject boss = Instantiate(bossPrefab, bossSpawnPoint.position, bossSpawnPoint.rotation);
        boss.GetComponent<BossAI>().player = GameObject.FindGameObjectWithTag("Player").transform;
    }
}
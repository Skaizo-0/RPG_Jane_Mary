using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Ссылки")]
    public Animator animator;
    public LayerMask enemyLayers; // Слой Enemy

    [Header("Физическая атака (ЛКМ)")]
    public float attackRange = 2f;
    public float physDamage = 20f;

    [Header("Магическая атака (ПКМ)")]
    public GameObject magicPrefab; // Сюда тянем префаб FireBall
    public Transform firePoint;   // Сюда тянем объект FirePoint из иерархии
    public float magDamage = 30f;  // Урон магией (передается снаряду или сразу)

    void Update()
    {
        // ЛКМ - Физическая атака
        if (Input.GetMouseButtonDown(0))
        {
            PhysicalAttack();
        }

        // ПКМ - Магическая атака
        if (Input.GetMouseButtonDown(1))
        {
            MagicAttack();
        }
    }

    void PhysicalAttack()
    {
        // Запускаем триггер в аниматоре
        animator.SetTrigger("AttackPhys");

        // Поиск врагов в радиусе (создаем невидимую сферу перед игроком)
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward, attackRange, enemyLayers);

        foreach (Collider enemy in hitEnemies)
        {
            Health enemyHealth = enemy.GetComponent<Health>();
            if (enemyHealth != null)
            {
                // Наносим только физический урон
                enemyHealth.TakeDamage(physDamage, 0);
            }
        }
    }

    void MagicAttack()
    {
        // ТЕПЕРЬ ТУТ ТОЛЬКО АНИМАЦИЯ
        animator.SetTrigger("AttackMag");
    }

    // А ЭТОТ МЕТОД ВЫЗОВЕТ САМА АНИМАЦИЯ В НУЖНЫЙ МОМЕНТ
    public void ShootMagic()
    {
        if (magicPrefab != null && firePoint != null)
        {
            Instantiate(magicPrefab, firePoint.position, firePoint.rotation);
        }
    }

    // Визуализация радиуса атаки в окне Scene
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward, attackRange);
    }
}
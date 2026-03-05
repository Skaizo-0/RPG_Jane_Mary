using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;
    public Transform firePoint;
    public GameObject magicPrefab;

    [Header("Настройки урона")]
    public float physDamage = 20f;
    public float physRange = 2.5f; // Увеличил радиус, чтобы точно попадало
    public LayerMask enemyLayer;

    private IInputService _input;

    public void Construct(IInputService input) => _input = input;

    void Update()
    {
        if (_input == null) return;

        // ЛКМ - Физическая атака
        if (_input.AttackPhys)
        {
            animator.SetTrigger("AttackPhys");
            DealPhysDamage(); // ВЫЗЫВАЕМ СРАЗУ
        }

        // ПКМ - Магия
        if (_input.AttackMag)
        {
            animator.SetTrigger("AttackMag");
            ShootMagic(); // ВЫЗЫВАЕМ СРАЗУ
        }
    }

    void DealPhysDamage()
    {
        // Создаем зону поражения ПЕРЕД игроком
        Vector3 pos = transform.position + transform.forward * 1.5f + Vector3.up;
        Collider[] enemies = Physics.OverlapSphere(pos, physRange, enemyLayer);

        foreach (var enemy in enemies)
        {
            if (enemy.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(physDamage, 0);
            }
        }
    }

    public void ShootMagic()
    {
        if (magicPrefab != null && firePoint != null)
        {
            // Чтобы летело ровно, берем поворот Игрока (transform.rotation)
            Instantiate(magicPrefab, firePoint.position, transform.rotation);
        }
    }

    // Это просто рисует красный шар в редакторе, чтобы ты видела зону удара
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 pos = transform.position + transform.forward * 1.5f + Vector3.up;
        Gizmos.DrawWireSphere(pos, physRange);
    }
}
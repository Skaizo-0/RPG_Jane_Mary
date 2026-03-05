using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [Header("Ссылки")]
    public Animator animator;
    public LayerMask enemyLayers;

    [Header("Физическая атака (ЛКМ)")]
    public float attackRange = 2f;
    public float physDamage = 20f;
    [Tooltip("Поворот при ударе мечом (попробуйте 90 или -90, если бьет боком)")]
    public float physRotationOffset = 0f;

    [Header("Магическая атака (ПКМ)")]
    public GameObject magicPrefab;
    public Transform firePoint;
    public float magDamage = 30f;
    public float magicRotationOffset = 0f;

    void Update()
    {
        // ЛКМ - Ближний бой
        if (Input.GetMouseButtonDown(0))
        {
            PhysicalAttack();
        }

        // ПКМ - Магия
        if (Input.GetMouseButtonDown(1))
        {
            MagicAttack();
        }
    }

    void PhysicalAttack()
    {
        // 1. РАЗВОРОТ К КАМЕРЕ
        RotateToCamera(physRotationOffset);

        // 2. ЗАПУСК АНИМАЦИИ
        animator.SetTrigger("AttackPhys");

        // 3. УРОН (лучше вызывать через Animation Event, но можно и так)
        ApplyPhysicalDamage();
    }

    void MagicAttack()
    {
        // 1. РАЗВОРОТ К КАМЕРЕ
        RotateToCamera(magicRotationOffset);

        // 2. ЗАПУСК АНИМАЦИИ
        animator.SetTrigger("AttackMag");
    }

    // Общий метод для разворота персонажа к камере со смещением
    void RotateToCamera(float offset)
    {
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0; // Чтобы не наклонялся

        if (camForward != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(camForward);
            lookRotation *= Quaternion.Euler(0, offset, 0);
            transform.rotation = lookRotation;
        }
    }

    public void ShootMagic()
    {
        if (magicPrefab != null && firePoint != null)
        {
            Instantiate(magicPrefab, firePoint.position, firePoint.rotation);
        }
    }

    void ApplyPhysicalDamage()
    {
        // Теперь transform.forward всегда смотрит туда, куда мы довернули персонажа
        Collider[] hitEnemies = Physics.OverlapSphere(transform.position + transform.forward, attackRange, enemyLayers);
        foreach (Collider enemy in hitEnemies)
        {
            Health h = enemy.GetComponent<Health>();
            if (h != null) h.TakeDamage(physDamage, 0);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position + transform.forward, attackRange);
    }
}
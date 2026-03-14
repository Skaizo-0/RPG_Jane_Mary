using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;
    public Transform firePoint;
    public GameObject magicPrefab;

    [Header("Настройки урона")]
    public float physDamage = 20f;
    public float physRange = 2.5f;
    public LayerMask enemyLayer;

    [Header("Исправление поворота Mixamo")]
    [Tooltip("Если при ударе мечом он бьет левее/правее - подправь это число (90 или -90)")]
    public float physRotationOffset = 0f;
    [Tooltip("Если при магии рука уходит вбок - подправь это число")]
    public float magicRotationOffset = 0f;

    private IInputService _input;
    private Transform _cam;

    // Метод из лекции для внедрения зависимостей
    public void Construct(IInputService input)
    {
        _input = input;
        _cam = Camera.main.transform;
    }

    void Update()
    {
        if (_input == null) return;

        // ЛКМ - Физическая атака
        if (_input.AttackPhys)
        {
            RotateToCamera(physRotationOffset); // Разворачиваем игрока
            animator.SetTrigger("AttackPhys");

            // ВНИМАНИЕ: DealPhysDamage() отсюда УДАЛЕН. 
            // Теперь его вызовет Animation Event в момент взмаха!
        }

        // ПКМ - Магия
        if (_input.AttackMag)
        {
            RotateToCamera(magicRotationOffset); // Разворачиваем игрока
            animator.SetTrigger("AttackMag");

            // ShootMagic() вызывается через Animation Event
        }
    }

    // Метод для выравнивания персонажа по камере (чтобы не бил в бок)
    private void RotateToCamera(float offset)
    {
        if (_cam == null) _cam = Camera.main.transform;

        Vector3 camForward = _cam.forward;
        camForward.y = 0;
        if (camForward != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(camForward);
            lookRotation *= Quaternion.Euler(0, offset, 0);
            transform.rotation = lookRotation;
        }
    }

    // ВЫЗЫВАЕТСЯ ЧЕРЕЗ ANIMATION EVENT (на кадре броска)
    public void ShootMagic()
    {
        if (magicPrefab != null && firePoint != null)
        {
            Instantiate(magicPrefab, firePoint.position, firePoint.rotation);
        }
    }

    // ВЫЗЫВАЕТСЯ ЧЕРЕЗ ANIMATION EVENT (на кадре удара мечом)
    public void DealPhysDamage()
    {
        // Создаем зону поражения ПЕРЕД игроком
        // 1.5f - это смещение центра сферы вперед, чтобы бить перед собой
        Vector3 pos = transform.position + transform.forward * 1.5f + Vector3.up;
        Collider[] enemies = Physics.OverlapSphere(pos, physRange, enemyLayer);

        foreach (var enemy in enemies)
        {
            // Используем интерфейс IDamageable из лекций
            if (enemy.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(physDamage, 0);
            }
        }
    }

    // Рисует красную сферу в редакторе (только когда игрок выбран)
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 pos = transform.position + transform.forward * 1.5f + Vector3.up;
        Gizmos.DrawWireSphere(pos, physRange);
    }
}
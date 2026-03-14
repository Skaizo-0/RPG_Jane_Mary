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
    public float physRotationOffset = 0f;
    public float magicRotationOffset = 0f;

    [Header("Кулдаун магии (ТЗ)")]
    public float magicCooldown = 3f; // Время перезарядки в секундах
    private float _lastMagicTime = -10f; // Время последнего выстрела

    private IInputService _input;
    private Transform _cam;

    // Свойство для UI: возвращает значение от 0 до 1 (насколько готова магия)
    // 0 - только что выстрелили (иконка темная), 1 - готова (иконка светлая)
    public float MagicReadyProgress => Mathf.Clamp01((Time.time - _lastMagicTime) / magicCooldown);

    // Метод из лекции (раздел 3.1.3) для внедрения зависимостей
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
            RotateToCamera(physRotationOffset);
            animator.SetTrigger("AttackPhys");
            // Урон наносится через Animation Event (DealPhysDamage)
        }

        // ПКМ - Магическая атака (с проверкой кулдауна по ТЗ)
        if (_input.AttackMag && Time.time >= _lastMagicTime + magicCooldown)
        {
            _lastMagicTime = Time.time; // Запоминаем время атаки
            RotateToCamera(magicRotationOffset);
            animator.SetTrigger("AttackMag");
            // Выстрел происходит через Animation Event (ShootMagic)
        }
    }

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

    // ВЫЗЫВАЕТСЯ ЧЕРЕЗ ANIMATION EVENT
    public void ShootMagic()
    {
        if (magicPrefab != null && firePoint != null)
        {
            Instantiate(magicPrefab, firePoint.position, firePoint.rotation);
        }
    }

    // ВЫЗЫВАЕТСЯ ЧЕРЕЗ ANIMATION EVENT
    public void DealPhysDamage()
    {
        // Смещение сферы урона на 1.5 метра вперед
        Vector3 pos = transform.position + transform.forward * 1.5f + Vector3.up;
        Collider[] enemies = Physics.OverlapSphere(pos, physRange, enemyLayer);

        foreach (var enemy in enemies)
        {
            // Инверсия зависимости (раздел 3.1.3 лекции): 
            // зависим от интерфейса IDamageable, а не от конкретных классов врагов.
            if (enemy.TryGetComponent<IDamageable>(out var target))
            {
                target.TakeDamage(physDamage, 0);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Vector3 pos = transform.position + transform.forward * 1.5f + Vector3.up;
        Gizmos.DrawWireSphere(pos, physRange);
    }
}
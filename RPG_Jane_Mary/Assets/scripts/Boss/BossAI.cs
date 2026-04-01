using UnityEngine;

// Определяем стихии для Босса (согласно ТЗ Лабы 6)
public enum ElementType { Fire, Ice, Earth, Ether }

public class BossAI : EnemyAI
{
    [Header("Настройки Босса (Лаба 6)")]
    public ElementType currentElement; // Выбранная стихия
    public bool isMeleeWeapon;        // Выбранный тип оружия (галочка = ближний бой)

    [Header("Ссылки на оружие")]
    public GameObject rangedStaff;    // Staff01 (для магии)
    public GameObject meleeStaff;     // Staff02 (для ударов)

    [Header("Уникальные эффекты стихий")]
    public AudioSource audioSource;
    public AudioClip[] elementSounds;       // 4 звука (Fire, Ice, Earth, Ether)
    public GameObject[] elementProjectiles; // 4 разных префаба шаров (разные цвета частиц)

    protected override void Start()
    {
        // 1. Инициализируем базовую логику (поиск игрока и т.д.)
        base.Start();

        // 2. Настраиваем оружие и дистанцию
        SetupBoss();
    }

    void SetupBoss()
    {
        // Включаем нужную модель посоха
        if (rangedStaff != null) rangedStaff.SetActive(!isMeleeWeapon);
        if (meleeStaff != null) meleeStaff.SetActive(isMeleeWeapon);

        // Устанавливаем дистанцию атаки в зависимости от оружия
        attackDist = isMeleeWeapon ? 2.5f : 8f;

        Debug.Log($"Босс появился! Оружие: {(isMeleeWeapon ? "Ближнее" : "Дальнее")}, Стихия: {currentElement}");
    }

    // Этот метод вызывается через Animation Event в окне Animation на кадре удара/выстрела
    public void BossPerformAction()
    {
        int index = (int)currentElement; // Индекс в массиве (0=Огонь, 1=Лед и т.д.)

        if (isMeleeWeapon)
        {
            // УСЛОВИЕ ТЗ: Для ближнего боя меняем ЗВУК
            if (elementSounds.Length > index && elementSounds[index] != null)
            {
                audioSource.PlayOneShot(elementSounds[index]);
            }

            // Наносим урон игроку (метод из родительского EnemyAI)
            ApplyMeleeDamage();
            Debug.Log($"Босс ударил мечом со звуком стихии {currentElement}");
        }
        else
        {
            // УСЛОВИЕ ТЗ: Для дальнего боя меняем ВИД (ЦВЕТ) СНАРЯДА
            if (elementProjectiles.Length > index && elementProjectiles[index] != null)
            {
                // Спавним шар нужной стихии
                Instantiate(elementProjectiles[index], firePoint.position, transform.rotation);
            }
            Debug.Log($"Босс выпустил магию стихии {currentElement}");
        }
    }

    // Переопределяем логику атаки, чтобы она подходила под аниматор босса
    public override void TryAttackLogic()
    {
        // Здесь мы просто запускаем нужный триггер в зависимости от галочки
        string trigger = isMeleeWeapon ? "AttackPh" : "AttackMa";
        animator.SetTrigger(trigger);

        // Кулдаун атаки обрабатывается в самом EnemyAI или AttackState
    }
}
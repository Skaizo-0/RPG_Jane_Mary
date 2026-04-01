using UnityEngine;


public enum ElementType { Fire, Ice, Earth, Ether }

public class BossAI : EnemyAI
{
    [Header("Настройки Босса (Лаба 6)")]
    public ElementType currentElement;
    public bool isMeleeWeapon;
    [Header("Ссылки на оружие")]
    public GameObject rangedStaff;
    public GameObject meleeStaff;

    [Header("Уникальные эффекты стихий")]
    public AudioSource audioSource;
    public AudioClip[] elementSounds;
    public GameObject[] elementProjectiles;

    public StrongAttackState StrongAttackState { get; private set; }
    public DefensiveState DefensiveState { get; private set; }
    public EnragedState EnragedState { get; private set; }

    private int _attackCounter = 0;
    private bool _isEnraged = false;

    protected override void Awake()
    {
        
        base.Awake();

        StrongAttackState = new StrongAttackState(this, StateMachine);
        DefensiveState = new DefensiveState(this, StateMachine);
        EnragedState = new EnragedState(this, StateMachine);
    }

    protected override void Start()
    {
        base.Start();
        SetupBoss();
        isPeaceful = true;
    }

    protected override void Update()
    {
        base.Update();

        if (!_isEnraged && Health.CurrentHealth < (Health.MaxHealth * 0.25f))
        {
            _isEnraged = true;
            StateMachine.ChangeState(EnragedState);
        }
    }

    void SetupBoss()
    {
        if (rangedStaff != null) rangedStaff.SetActive(!isMeleeWeapon);
        if (meleeStaff != null) meleeStaff.SetActive(isMeleeWeapon);
        attackDist = isMeleeWeapon ? 2.5f : 8f;
    }

    
    public override void BossPerformAction()
    {
        int index = (int)currentElement;

        if (isMeleeWeapon)
        {
            if (elementSounds.Length > index && elementSounds[index] != null)
            {
                audioSource.PlayOneShot(elementSounds[index]);
            }
            ApplyMeleeDamage();
        }
        else
        {
            if (elementProjectiles.Length > index && elementProjectiles[index] != null)
            {
                Instantiate(elementProjectiles[index], firePoint.position, transform.rotation);
            }
        }
    }

    public override void TryAttackLogic()
    {
        _attackCounter++;

        if (_attackCounter >= 3)
        {
            _attackCounter = 0;
            StateMachine.ChangeState(StrongAttackState);
        }
        else
        {
            string trigger = isMeleeWeapon ? "AttackPh" : "AttackMa";
            animator.SetTrigger(trigger);

            if (Random.value > 0.8f) StateMachine.ChangeState(DefensiveState);
        }
    }
}
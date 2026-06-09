using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using System.Collections.Generic;

public enum EnemyType { Melee, Ranged }

public class EnemyAI : NetworkBehaviour
{
    public EnemyType enemyType;
    public Transform player;
    public Animator animator;
    public CharacterController controller;

    [Header("Настройки ИИ")]
    public float chaseDistance = 15f;
    public float attackDist = 2.5f;
    public float speed = 2f;
    public bool isPeaceful;

    [Header("Для Мага")]
    public GameObject magicPrefab;
    public Transform firePoint;

    public EnemyStateMachine StateMachine { get; private set; }
    public IdleState IdleState { get; private set; }
    public AggroState AggroState { get; private set; }
    public AttackState AttackState { get; private set; }
    public FleeState FleeState { get; private set; }

    public Health Health { get; private set; }
    protected float _attackCooldown = 2f;
    protected float _lastAttackTime;

    private float _verticalVelocity;

    protected virtual void Awake()
    {
        Health = GetComponent<Health>();
        controller = GetComponent<CharacterController>();

        StateMachine = new EnemyStateMachine();
        IdleState = new IdleState(this, StateMachine);
        AggroState = new AggroState(this, StateMachine);
        AttackState = new AttackState(this, StateMachine);
        FleeState = new FleeState(this, StateMachine);
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (enemyType == EnemyType.Ranged) attackDist = 8f;

        Health.OnHealthChanged += (cur, max) =>
        {
            if (gameObject.CompareTag("Boss"))
            {
                if (cur < max) isPeaceful = false;
            }
        };

        StateMachine.Initialize(IdleState);
    }

    protected virtual void Update()
    {
        if (!IsServer) return;

        if (Health.CurrentHealth <= 0)
        {
            ApplyGravity();
            StopMoving();
            return;
        }

        FindNearestPlayer();
        ApplyGravity();
        StateMachine.CurrentState.Update();
    }

    private void FindNearestPlayer()
    {
        float minDistance = float.MaxValue;
        Transform closest = null;

        foreach (NetworkConnection conn in ServerManager.Clients.Values)
        {
            if (conn.FirstObject != null)
            {
                float dist = Vector3.Distance(transform.position, conn.FirstObject.transform.position);
                if (dist < chaseDistance && dist < minDistance)
                {
                    minDistance = dist;
                    closest = conn.FirstObject.transform;
                }
            }
        }
        player = closest;
    }

    private void ApplyGravity()
    {
        if (controller.isGrounded && _verticalVelocity < 0)
        {
            _verticalVelocity = -2f;
        }
        else
        {
            _verticalVelocity += Physics.gravity.y * Time.deltaTime;
        }

        controller.Move(new Vector3(0, _verticalVelocity, 0) * Time.deltaTime);
    }

    public void MoveToPlayer()
    {
        if (player == null) return;
        Vector3 dir = (player.position - transform.position).normalized;
        MoveInDirection(dir);
    }

    public void MoveInDirection(Vector3 dir)
    {
        dir.y = 0;
        if (dir.magnitude > 0.1f)
        {
            controller.Move(dir * speed * Time.deltaTime);
            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            animator.SetFloat("Speed", 0.5f);
        }
        else
        {
            StopMoving();
        }
    }

    public void StopMoving()
    {
        animator.SetFloat("Speed", 0);
    }

    public void SmoothRotateToPlayer()
    {
        if (player == null) return;
        Vector3 lookDir = player.position - transform.position;
        lookDir.y = 0;
        if (lookDir.magnitude > 0.1f)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), 5f * Time.deltaTime);
    }

    public virtual void TryAttackLogic()
    {
        if (Time.time > _lastAttackTime + _attackCooldown)
        {
            _lastAttackTime = Time.time;
            string trigger = (enemyType == EnemyType.Melee) ? "AttackPh" : "AttackMa";
            PlayAttackAnimationObserversRpc(trigger);

            if (enemyType == EnemyType.Melee)
                Invoke(nameof(ApplyMeleeDamage), 0.6f);
            else
                Invoke(nameof(LaunchMagic), 0.6f);
        }
    }

    [ObserversRpc]
    private void PlayAttackAnimationObserversRpc(string triggerName)
    {
        animator.SetTrigger(triggerName);
    }

    public virtual void ApplyMeleeDamage()
    {
        if (!IsServer) return;

        // ЗАЩИТА: Если игрок исчез за время замаха, ничего не делаем
        if (player == null) return;

        if (Vector3.Distance(transform.position, player.position) <= attackDist + 1.5f)
        {
            if (player.TryGetComponent<IDamageable>(out var target))
                target.TakeDamage(10, 0);
        }
    }

    protected void LaunchMagic()
    {
        if (!IsServer) return;

        // ЗАЩИТА: Ошибка падала здесь, потому что player мог стать null
        if (player == null) return;

        if (firePoint && magicPrefab)
        {
            Vector3 targetDir = (player.position + Vector3.up - firePoint.position).normalized;
            GameObject ball = Instantiate(magicPrefab, firePoint.position, Quaternion.LookRotation(targetDir));

            // Чтобы пуля знала, что её выпустил враг, а не игрок
            ServerManager.Spawn(ball);
        }
    }

    // --- ДОБАВЛЕНО ДЛЯ БОССА ---
    public virtual void BossPerformAction() { }
}
using UnityEngine;
using FishNet.Object;
using FishNet.Connection;
using System.Collections;
using System.Collections.Generic;

// Перечисление типов врагов (сохранено полностью)
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

    // Ссылки на машину состояний (сохранено полностью)
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
        Debug.Log(
        $"ENEMY SERVER START {gameObject.name}"
    );
        if (enemyType == EnemyType.Ranged) attackDist = 8f;

        Health.OnHealthChanged += (cur, max) =>
        {
            if (gameObject.CompareTag("Boss"))
            {
                if (cur < max) isPeaceful = false;
            }
        };

        StateMachine.Initialize(IdleState);

        Debug.Log($"[SERVER] Моб {gameObject.name} инициализирован.");
    }

    public override void OnStartClient()
    {
        base.OnStartClient();
        if (!IsServer)
        {
            Debug.Log($"[CLIENT] Моб {gameObject.name} (ID: {ObjectId}) ПОЯВИЛСЯ на экране клиента!");
        }
    }

    protected virtual void Update()
    {
        // Только сервер управляет логикой
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

        // Ищем всех игроков со скриптом PlayerMovement (самый надежный способ для FishNet)
        PlayerMovement[] allPlayers = Object.FindObjectsByType<PlayerMovement>(FindObjectsSortMode.None);

        foreach (PlayerMovement p in allPlayers)
        {
            Health h = p.GetComponent<Health>();
            if (h != null && h.CurrentHealth <= 0) continue;

            float dist = Vector3.Distance(transform.position, p.transform.position);

            if (dist < chaseDistance && dist < minDistance)
            {
                minDistance = dist;
                closest = p.transform;
            }
        }
        player = closest;
    }

    private void ApplyGravity()
    {
        if (controller == null) return;

        if (controller.isGrounded && _verticalVelocity < 0) _verticalVelocity = -2f;
        else _verticalVelocity += Physics.gravity.y * Time.deltaTime;

        if (controller.enabled)
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
            if (controller.enabled)
                controller.Move(dir * speed * Time.deltaTime);

            Quaternion targetRotation = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 10f * Time.deltaTime);
            animator.SetFloat("Speed", 0.5f);
        }
        else StopMoving();
    }

    public void StopMoving()
    {
        if (animator != null) animator.SetFloat("Speed", 0);
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
        if (player == null) return;
        if (Time.time > _lastAttackTime + _attackCooldown)
        {
            _lastAttackTime = Time.time;
            string trigger = (enemyType == EnemyType.Melee) ? "AttackPh" : "AttackMa";

            PlayAttackAnimationObserversRpc(trigger);

            if (enemyType == EnemyType.Melee) StartCoroutine(DelayedMeleeDamage(0.6f));
            else StartCoroutine(DelayedMagicShot(0.6f));
        }
    }

    [ObserversRpc]
    private void PlayAttackAnimationObserversRpc(string triggerName)
    {
        if (animator != null) animator.SetTrigger(triggerName);
    }

    private IEnumerator DelayedMeleeDamage(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!IsServer || player == null) yield break;

        if (Vector3.Distance(transform.position, player.position) <= attackDist + 1.5f)
        {
            if (player.TryGetComponent<IDamageable>(out var target))
                target.TakeDamage(10, 0);
        }
    }

    private IEnumerator DelayedMagicShot(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (!IsServer || player == null || firePoint == null) yield break;

        Vector3 targetDir = (player.position + Vector3.up - firePoint.position).normalized;
        GameObject ball = Instantiate(magicPrefab, firePoint.position, Quaternion.LookRotation(targetDir));

        ServerManager.Spawn(ball);
    }

    public virtual void BossPerformAction() { }
}
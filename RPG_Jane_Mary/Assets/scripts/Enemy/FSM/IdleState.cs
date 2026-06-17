using UnityEngine;

public class IdleState : EnemyState
{
    public IdleState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Update()
    {
        // ПРОВЕРКА: Если игрока нет, враг просто стоит и ничего не делает
        if (enemy.player == null)
        {
            enemy.StopMoving();
            return;
        }

        if (enemy.Health.CurrentHealth < (enemy.Health.MaxHealth * 0.3f))
        {
            stateMachine.ChangeState(enemy.FleeState);
            return;
        }

        if (enemy.isPeaceful) return;

        if (Vector3.Distance(enemy.transform.position, enemy.player.position) < enemy.chaseDistance)
        {
            stateMachine.ChangeState(enemy.AggroState);
        }
    }
}
using UnityEngine;

public class AggroState : EnemyState
{
    public AggroState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Update()
    {

        if (enemy.Health.CurrentHealth < (enemy.Health.MaxHealth * 0.3f))
        {
            stateMachine.ChangeState(enemy.FleeState);
            return;
        }

        float dist = Vector3.Distance(enemy.transform.position, enemy.player.position);

        if (dist <= enemy.attackDist)
        {
            stateMachine.ChangeState(enemy.AttackState);
        }
        else if (dist > enemy.chaseDistance + 2f)
        {
            stateMachine.ChangeState(enemy.IdleState);
        }
        else
        {
            enemy.MoveToPlayer();
            enemy.SmoothRotateToPlayer();
        }
    }
}
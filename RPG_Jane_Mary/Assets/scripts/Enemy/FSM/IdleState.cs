using UnityEngine;

public class IdleState : EnemyState
{
    public IdleState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Update()
    {
        
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
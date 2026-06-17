using UnityEngine;

public class FleeState : EnemyState
{
    public FleeState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Update()
    {
        if (enemy.player == null) return;


        Vector3 fleeDir = (enemy.transform.position - enemy.player.position).normalized;

  
        enemy.MoveInDirection(fleeDir);


        if (Vector3.Distance(enemy.transform.position, enemy.player.position) > 25f)
        {
            stateMachine.ChangeState(enemy.IdleState);
        }
    }
}
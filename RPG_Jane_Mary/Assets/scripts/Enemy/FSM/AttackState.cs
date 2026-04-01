using UnityEngine;

public class AttackState : EnemyState
{
    public AttackState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Update()
    {
        
        enemy.SmoothRotateToPlayer();

        float dist = Vector3.Distance(enemy.transform.position, enemy.player.position);

        
        if (dist > enemy.attackDist + 1f)
        {
            stateMachine.ChangeState(enemy.AggroState);
            return;
        }

        
        enemy.TryAttackLogic();
    }
}
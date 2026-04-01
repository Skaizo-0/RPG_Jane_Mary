using UnityEngine;

public class FleeState : EnemyState
{
    public FleeState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Update()
    {
        // Направление ОТ игрока
        Vector3 fleeDir = (enemy.transform.position - enemy.player.position).normalized;
        enemy.MoveInDirection(fleeDir);

        // Если убежали очень далеко — переходим в покой
        if (Vector3.Distance(enemy.transform.position, enemy.player.position) > 25f)
        {
            stateMachine.ChangeState(enemy.IdleState);
        }
    }
}
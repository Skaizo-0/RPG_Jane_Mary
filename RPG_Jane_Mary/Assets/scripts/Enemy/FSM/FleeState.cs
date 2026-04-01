using UnityEngine;

public class FleeState : EnemyState
{
    public FleeState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Update()
    {
        if (enemy.player == null) return;

        // Направление ОТ игрока
        Vector3 fleeDir = (enemy.transform.position - enemy.player.position).normalized;

        // Используем твой новый метод передвижения через CharacterController
        enemy.MoveInDirection(fleeDir);

        // Если убежали далеко — возвращаемся в покой
        if (Vector3.Distance(enemy.transform.position, enemy.player.position) > 25f)
        {
            stateMachine.ChangeState(enemy.IdleState);
        }
    }
}
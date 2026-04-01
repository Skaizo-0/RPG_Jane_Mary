using UnityEngine;

public class IdleState : EnemyState
{
    public IdleState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Update()
    {
        // Условие бегства (ТЗ: Если мало хп - убегают)
        if (enemy.Health.CurrentHealth < (enemy.Health.MaxHealth * 0.3f))
        {
            stateMachine.ChangeState(enemy.FleeState);
            return;
        }

        // Если моб в мирном режиме, он НЕ переходит в агрессию по дистанции
        if (enemy.isPeaceful) return;

        // Если не мирный — ищем игрока
        if (Vector3.Distance(enemy.transform.position, enemy.player.position) < enemy.chaseDistance)
        {
            stateMachine.ChangeState(enemy.AggroState);
        }
    }
}
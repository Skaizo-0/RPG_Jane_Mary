using UnityEngine;

public class AttackState : EnemyState
{
    public AttackState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Update()
    {
        // 1. Всегда поворачиваемся к игроку во время атаки
        enemy.SmoothRotateToPlayer();

        float dist = Vector3.Distance(enemy.transform.position, enemy.player.position);

        // 2. Если игрок вышел за радиус атаки (с запасом +1 метр)
        if (dist > enemy.attackDist + 1f)
        {
            stateMachine.ChangeState(enemy.AggroState);
            return;
        }

        // 3. Вызываем логику атаки. 
        // Благодаря полиморфизму, если это Босс, сработает его уникальная атака.
        enemy.TryAttackLogic();
    }
}
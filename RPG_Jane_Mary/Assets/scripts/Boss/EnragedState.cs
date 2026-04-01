using UnityEngine;

public class EnragedState : EnemyState
{
    private float _roarDuration = 2.5f; // Длительность крика
    private float _timer;

    public EnragedState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Enter()
    {
        _timer = _roarDuration;
        enemy.StopMoving();
        enemy.animator.SetTrigger("Roar");

        // Усиливаем босса визуально и физически
        enemy.speed *= 1.4f;
        Debug.Log("БОСС В ЯРОСТИ! СКОРОСТЬ УВЕЛИЧЕНА!");
    }

    public override void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0)
        {
            // После того как проорался, возвращаемся к преследованию
            stateMachine.ChangeState(enemy.AggroState);
        }
    }
}
using UnityEngine;

public class DefensiveState : EnemyState
{
    private float _duration = 2.0f;
    private float _timer;

    public DefensiveState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Enter()
    {
        _timer = _duration;
        enemy.animator.SetBool("IsDefending", true);
    }

    public override void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0) stateMachine.ChangeState(enemy.AggroState);
    }

    public override void Exit() => enemy.animator.SetBool("IsDefending", false);
}
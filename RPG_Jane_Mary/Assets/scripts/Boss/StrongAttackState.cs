using UnityEngine;

public class StrongAttackState : EnemyState
{
    private float _duration = 2.5f; 
    private float _timer;

    public StrongAttackState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Enter()
    {
        _timer = _duration;
        enemy.StopMoving();
        enemy.animator.SetTrigger("StrongAt");
        Debug.Log("Босс применяет СУПЕРУДАР!");
    }

    public override void Update()
    {
        _timer -= Time.deltaTime;
        if (_timer <= 0) stateMachine.ChangeState(enemy.AggroState);
    }
}
using UnityEngine;

public class EnragedState : EnemyState
{
    private float _roarDuration = 2.5f; 
    private float _timer;

    public EnragedState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Enter()
    {
        _timer = _roarDuration;
        enemy.StopMoving();
        enemy.animator.SetTrigger("Roar");


        enemy.speed *= 1.4f;
        Debug.Log("аняя б ъпнярх! яйнпнярэ сбекхвемю!");
    }

    public override void Update()
    {
        _timer -= Time.deltaTime;

        if (_timer <= 0)
        {
       
            stateMachine.ChangeState(enemy.AggroState);
        }
    }
}
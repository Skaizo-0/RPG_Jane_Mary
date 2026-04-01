using UnityEngine;

public class EnragedState : EnemyState
{
    public EnragedState(EnemyAI enemy, EnemyStateMachine stateMachine) : base(enemy, stateMachine) { }

    public override void Enter()
    {
        enemy.StopMoving();
        enemy.animator.SetTrigger("Roar"); 
        enemy.speed *= 1.5f; 
        Debug.Log("БОСС В ЯРОСТИ!");
    }

    public override void Update()
    {
       
        
        if (Time.timeSinceLevelLoad % 2 == 0) 
            stateMachine.ChangeState(enemy.AggroState);
    }
}
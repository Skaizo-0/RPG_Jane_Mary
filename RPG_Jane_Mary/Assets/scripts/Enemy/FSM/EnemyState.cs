using UnityEngine;

public abstract class EnemyState
{
    protected EnemyAI enemy;
    protected EnemyStateMachine stateMachine;

    protected EnemyState(EnemyAI enemy, EnemyStateMachine stateMachine)
    {
        this.enemy = enemy;
        this.stateMachine = stateMachine;
    }

    public virtual void Enter() { }
    public virtual void Update() { }
    public virtual void Exit() { }
}
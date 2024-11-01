using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EWait : IState
{
    private EnemyFSM _fsm;
    private EnemyParamenter _enemyParamenter;

    private float _waitTime;
    float currentTime = 0f;
    
    public EWait(EnemyFSM fsm,float waitTime)
    {
        _fsm = fsm;
        _enemyParamenter = _fsm.enemyParamenter;

        _waitTime = waitTime;
    }
    
    public void OnEnter()
    {
        currentTime = 0;//进入等待状态后重置计时器
        
        Debug.Log("进入等待状态");
    }

    public void OnUpdate()
    {
        StartWait();
    }

    public void OnExit()
    {
        currentTime = 0;//退出等待状态后重置计时器
        
        Debug.Log("退出等待状态");
    }


    private void StartWait()
    {
        currentTime += Time.deltaTime;

        if (currentTime >= _waitTime) 
        {
            _fsm.TransitionState(EnemyFSM.EnemyStateType.Walk);
        }
    }
}

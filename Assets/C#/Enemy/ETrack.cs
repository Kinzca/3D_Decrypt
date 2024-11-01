using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ETrack : IState
{
    private EnemyFSM _fsm;
    private EnemyParamenter _enemyParamenter;
    
    public ETrack(EnemyFSM fsm)
    {
        _fsm = fsm;
        _enemyParamenter = _fsm.enemyParamenter;
    }
    
    public void OnEnter()
    {
        throw new System.NotImplementedException();
    }

    public void OnUpdate()
    {
        throw new System.NotImplementedException();
    }

    public void OnExit()
    {
        throw new System.NotImplementedException();
    }
}

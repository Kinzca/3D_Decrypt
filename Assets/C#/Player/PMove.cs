using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PMove : IState
{
    private PlayerFSM _fsm;
    private PlayerParamenter _playerParamenter;
    
    public PMove(PlayerFSM fsm)
    {
        _fsm = fsm;
        _playerParamenter = _fsm.playerParamenter;
    }

    public void OnEnter()
    {
        Debug.Log("玩家进入移动状态");
    }

    public void OnUpdate()
    {
        //对基站发送命令请求
        EventCenter.Broadcast(EventType.PlayerMove);
    }

    public void OnExit()
    {
        //Debug.Log("玩家离开移动状态");
    }
}

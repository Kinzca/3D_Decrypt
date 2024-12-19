using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class PWait : IState
{
    private PlayerFSM _fsm; 
    private PlayerParamenter _playerParamenter;

    private LayerMask _layerMask;
    public PWait(PlayerFSM fsm)
    {
        _fsm = fsm;
        _playerParamenter = _fsm.playerParamenter;
    }
    
    public void OnEnter()
    {
        Debug.Log("玩家进入等待状态");
    }

    public void OnUpdate()  
    {  
        var obj = Utility.GetMouseObject(_playerParamenter.objMask, "Object");  
        var floor = Utility.GetMouseObject(_playerParamenter.floorMask, "WalkableFloor");  

        if (Input.GetMouseButtonDown(0))  
        {  
            //重新读取 UI 点击状态  
            if (EventSystem.current.IsPointerOverGameObject() || floor == null)  
            {  
                _playerParamenter.isClickUI = true;  
            }  
            else  
            {  
                _playerParamenter.isClickUI = false;  
            }  

            if (obj != null)  
            {  
                _fsm.TransitionState(PlayerFSM.PlayerStateType.Pick);  
            }  
            else if (floor != null && !_playerParamenter.isClickUI)  
            {  
                _fsm.TransitionState(PlayerFSM.PlayerStateType.Move);  
            }  
            else if (_playerParamenter.isClickUI)  
            {  
                Debug.Log("点击到UI进行UI更新操作");  
            }  
        }  
    }

    public void OnExit()
    {
        //Debug.Log("玩家退出等待状态");
    }
}

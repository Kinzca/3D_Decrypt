using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EWalk : IState
{
    private EnemyFSM _fsm;
    private EnemyParamenter _enemyParamenter;

    private List<FloorCenter> _floorCenters;
    private static List<Vector3> _posList;//敌人需要移动的列表,行走的路径为固定的所以设置为静态

    private int currentTargetIndex = 1; // 从索引 1 开始移动
    private bool revertList=false;
    
    public EWalk(EnemyFSM fsm,List<FloorCenter> floorCenters)
    {
        _fsm = fsm;
        _enemyParamenter = _fsm.enemyParamenter;

        _floorCenters = floorCenters;
        _posList = new List<Vector3>();
    }
    
    public void OnEnter()
    {
        //Debug.Log("敌人进入移动状态");
    }

    public void OnUpdate()
    {
        GetPosLis();
        EnemyMoveToNext();
        
    }

    public void OnExit()
    {
        //Debug.Log("敌人退出移动状态");
    }

    /// <summary>
    /// 获取敌人移动列表
    /// </summary>
    private void GetPosLis()
    {
        //如果敌人行走路径列表为空，则直接返回；否则获取列表信息
        if (_posList == null) return;
        //如果列表长度等于_floorCenter长度，则复制完成，直接返回
        if (_posList.Count == _floorCenters.Count) return;
        
        //获取PosList列表
        for (int i = 0; i <_floorCenters.Count ; i++)
        {
            _posList.Add(_floorCenters[i].floorItem.center);
            //Debug.Log(_floorCenters[i].name);
        }
    }

    private void EnemyMoveToNext()
    {
        if (!revertList)
        {
            // 确保当前目标索引不超过位置列表的范围
            if (currentTargetIndex < _posList.Count)
            {
                //Debug.Log("当前结点索引：" + currentTargetIndex);
                var targetPos = _posList[currentTargetIndex];
                targetPos.y += 0.4f;

                // 移动敌人
                _fsm.transform.position = Vector3.MoveTowards(_fsm.transform.position, targetPos,
                    _enemyParamenter.moveSpeed * Time.deltaTime);

                // 检查是否到达目标位置
                if (Vector3.Distance(_fsm.transform.position, targetPos) < 0.1f)
                {
                    // 到达目标，更新目标索引
                    currentTargetIndex++;
                    
                    //到达目标位置后等待后移动
                    _fsm.TransitionState(EnemyFSM.EnemyStateType.Wait);

                }
                
                if (_posList.Count == currentTargetIndex)
                {
                    //将当前索引值减一放置索引值溢出
                    currentTargetIndex = _posList.Count-2;
                    
                    revertList = true;
                    Debug.Log("revertList:" + revertList);
                }
            }
        }
        else
        {
            // 处理 revertList 的逻辑
            //Debug.Log("反转移动");
            
            if (currentTargetIndex >= 0) 
            {
                //Debug.Log("当前结点索引：" + currentTargetIndex);
                var targetPos = _posList[currentTargetIndex];
                targetPos.y += 0.4f;

                // 移动敌人
                _fsm.transform.position = Vector3.MoveTowards(_fsm.transform.position, targetPos,
                    _enemyParamenter.moveSpeed * Time.deltaTime);

                // 检查是否到达目标位置
                if (Vector3.Distance(_fsm.transform.position, targetPos) < 0.1f)
                {
                    // 到达目标，更新目标索引
                    currentTargetIndex--;
                    
                    //到达目标位置后等待后移动
                    _fsm.TransitionState(EnemyFSM.EnemyStateType.Wait);
                }

                if (currentTargetIndex == 0)
                {
                    revertList = false;
                    Debug.Log("revertList:" + revertList);
                }
            }
        }
    }

}

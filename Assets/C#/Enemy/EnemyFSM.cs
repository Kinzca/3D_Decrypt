using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class EnemyParamenter
{
    public float moveSpeed;
    public PRoute pRoute;

    public float waitTime;
    public List<FloorCenter> floorCenters;

    public BoxCollider boxCollider;
    public Rigidbody rigidbody;
}

public class EnemyFSM : MonoBehaviour
{
    public enum EnemyStateType
    {
        Walk,Wait,Track
    }

    public EnemyParamenter enemyParamenter; 

    private IState _currentState;

    private Dictionary<EnemyStateType, IState> _states = new Dictionary<EnemyStateType, IState>();

    private void Start()
    {
        _states.Add(EnemyStateType.Walk,new EWalk(this,enemyParamenter.floorCenters));
        _states.Add(EnemyStateType.Wait,new EWait(this,enemyParamenter.waitTime));
        _states.Add(EnemyStateType.Track,new ETrack(this));

        //获取必要组件
        enemyParamenter.boxCollider = GetComponent<BoxCollider>();
        enemyParamenter.rigidbody = GetComponent<Rigidbody>();
        
        StartCoroutine(InitPos());

        TransitionState(EnemyStateType.Wait);
    }

    private void Update()
    {
        _currentState.OnUpdate();
    }

    public void TransitionState(EnemyStateType stateType)
    {
        if (_currentState != null) 
        {
            _currentState.OnExit();
        }

        _currentState = _states[stateType];
        _currentState.OnEnter();
    }
    
    /// <summary>
    /// 利用协程当PRoute中的地板中心列表不为null时进行初始化
    /// </summary>
    /// <returns></returns>
    private IEnumerator InitPos()
    {
        var pRoute = enemyParamenter.pRoute;
        
        while (pRoute == null || pRoute.startPos == null || pRoute.startPos.floorItem == null)
        {
            yield return null;
        }

        InitializePlayPos();//初始化玩家位置
    }

    /// <summary>
    /// 初始化最近的点作为敌人的起始点
    /// </summary>
    private void InitializePlayPos()
    {
        var pRoute = enemyParamenter.pRoute;
        
        if (pRoute == null || pRoute.centerPoint == null || pRoute.centerPoint.Count == 0)
        {
            Debug.LogError("PRoute or centerPoint is not initialized or contains no points.");
        }

        float startDis = 0;
        int index = 0;

        var pointList = pRoute.centerPoint; // 获取中心点列表

        for (int i = 0; i < pointList.Count; i++)
        {
            if (i == 0)
            {
                startDis = Vector3.Distance(pointList[i], transform.position);
            }
            else
            {
                var middleDis = Vector3.Distance(pointList[i], transform.position);
                if (middleDis < startDis)
                {
                    index = i;
                    startDis = middleDis;
                }
            }
        }

        //y轴偏移
        var newPos = pointList[index];
        newPos.y += 0.4f;
        transform.position = newPos;
    }

}

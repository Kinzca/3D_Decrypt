using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PMove : MonoBehaviour
{
    public PRoute route;
    public float moveSpeed;

    private int _currentPoint = 0;
    private Coroutine _currentCoroutine;
    private void Start()
    {
        StartCoroutine(InitPos());
        
        //事件订阅
        route.PlayerMoveEvent += OnPlayerMove;
    }

    private IEnumerator InitPos()
    {
        while (route == null || route.startPos == null || route.startPos.floorItem == null)
        {
            yield return null;
        }

        transform.position = InitializePlayPos();//初始化玩家位置
    }

    //初始化玩家位置
    private Vector3 InitializePlayPos()
    {
        var pos = route.startPos.floorItem.center;
        Vector3 playPos = new Vector3(pos.x, pos.y + 0.4f, pos.z);

        return playPos;
    }
    
    //进行玩家移动
    private void OnPlayerMove()
    {
        if (_currentCoroutine != null) 
        {
            StopCoroutine(_currentCoroutine);
        }

        _currentCoroutine = StartCoroutine(MoveAlongPoint());
    }

    /// <summary>
    /// 选择路径最近的结点
    /// </summary>
    /// <returns></returns>
    private int GetClosetPointOnPath(List<FloorCenter> path)
    {
        int closestIndex = -1;
        float minDistance = float.MaxValue;
    
        Vector3 playerPosition = transform.position;

        // 先找到距离玩家最近的结点
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 nodePosition = new Vector3(path[i].floorItem.center.x, 
                path[i].floorItem.center.y + 0.4f, 
                path[i].floorItem.center.z);
            float distance = Vector3.Distance(playerPosition, nodePosition);
        
            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }
        
        Vector3 directionToClosest = (path[closestIndex].floorItem.center - playerPosition).normalized;//计算玩家与最短结点间的方向
        Vector3 directionFromPrevious = (path[closestIndex].floorItem.center - path[closestIndex + 1].floorItem.center).normalized;//计算结点与它前一个结点的方向

        //计算点积，判断方向
        float dotProduct = Vector3.Dot(directionToClosest, directionFromPrevious);

        //如果点积小于0，说明方向相反，返回前一个结点
        if (dotProduct > 0)
        {
            closestIndex += 1; //选择前一个结点
        }

        Debug.Log("朝向索引为" + closestIndex + "移动");
        
        return closestIndex;
    }

    
    private IEnumerator MoveAlongPoint()
    {
        //初始化操作
        var path = route.pathList;

        _currentPoint = GetClosetPointOnPath(path);
        
        while (_currentPoint < path.Count)
        {
            Vector3 newCenter = new Vector3(path[_currentPoint].floorItem.center.x,
                path[_currentPoint].floorItem.center.y + 0.4f, path[_currentPoint].floorItem.center.z);
            
            var targetPosition = newCenter;
            while (transform.position != targetPosition)
            {
                transform.position =
                    Vector3.MoveTowards(transform.position, targetPosition, Time.deltaTime * moveSpeed);
                
                yield return null;
            }

            _currentPoint++;
        }
    }
    
    private void OnDisable()
    {
        route.PlayerMoveEvent -= OnPlayerMove;
    }
}

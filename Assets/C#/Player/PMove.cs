using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class PMove : MonoBehaviour
{
    public PRoute route;
    public float moveSpeed;

    private int _currentPoint = 0;
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
        StartCoroutine(MoveAlongPoint());
    }

    private IEnumerator MoveAlongPoint()
    {
        //初始化操作
        var path = route.pathList;
        _currentPoint = 0;

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

        route.isMove = false;
    }
    
    private void OnDisable()
    {
        route.PlayerMoveEvent -= OnPlayerMove;
    }
}

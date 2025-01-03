using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

public class PRoute : MonoBehaviour
{
    private FloorCenter _targetPos; //鼠标停留所在的物体
    public FloorCenter startPos;    //玩家开始时的位置
    
    public LayerMask floorMask;     //指定碰撞的遮罩
    
    public List<GameObject> centerGameObjects = new List<GameObject>();
    public List<Vector3> centerPoint = new List<Vector3>();
    
    private List<FloorCenter> _openList = new List<FloorCenter>();  //开放列表，存储未经历的结点
    private List<FloorCenter> _closeList = new List<FloorCenter>(); //关闭列表，存储经历的结点

    public List<FloorCenter> pathList = new List<FloorCenter>();
    
    private LineRenderer _lineRenderer; //路线绘制
    private float animationDuration = 1;

    private Coroutine _animatePathCoroutine; //用于跟踪当前的动画协程

    public event Action PlayerMoveEvent;//Player移动事件
    
    public float clickCooldown = 0.5f;  //点击的冷却时间
    private float lastClickTime = 0f; //上一次点击的时间
    private IEnumerator Start()
    {
        //等待一帧，确保所有 FloorCenter 的 Start 方法都已被调用
        yield return null;
        _lineRenderer = gameObject.GetComponent<LineRenderer>();
        GetAllFloors(gameObject);
        
        GetPlayerFloor(); //初始化玩家起始位置
    }
    
    private void LateUpdate()
    {
        if (Time.time - lastClickTime >= clickCooldown)
        {
            if (Input.GetMouseButtonDown(0)) //鼠标左键点击
            {
                lastClickTime = Time.time; //记录这次点击的时间
                GetShortestPath();  
            }
        }
    }

    // 循环选择父物体下的所有子物体
    private void GetAllFloors(GameObject parent)
    {
        for (int i = 0; i < parent.transform.childCount; i++)
        {
            if (parent.transform.GetChild(i).childCount > 0) 
            {
                GetAllFloors(parent.transform.GetChild(i).gameObject);
            }
            
            centerGameObjects.Add(parent.transform.GetChild(i).gameObject);
            centerPoint.Add(parent.transform.GetChild(i).gameObject.GetComponent<FloorCenter>().floorItem.center); // 获取所有子物体下FloorCenter脚本的Vector3值
        }
    }
    
    // 获取当前指针指向的地板对象
    private void GetMouseFloor()
    {
        var gameObject= Utility.GetMouseObject(floorMask, "WalkableFloor");
        
        if (gameObject != null) 
        {
            _targetPos = gameObject.GetComponent<FloorCenter>();
        }
        
        Debug.Log("当前格子不可移动");
    }


    /// <summary>
    /// 计算距离玩家最近的格子，并将这个格子作为玩家的初始格子，将这个格子的坐标赋值给玩家作为初始格子
    /// </summary>
    private void GetPlayerFloor()   
    {
        float startDistance = 0f;
        float middleDistance;
        int index = 0;
        
        var gameObject = GameObject.FindWithTag("Player");
        if (gameObject == null) return; 
     
        var playerPos = gameObject.transform.position; // 获取玩家位置
        
        //清空起始点
        startPos = null;
        
        // 将玩家位置与所有地板进行比较，选取距离最短的点作为起始点
        for (int i = 0; i < centerPoint.Count; i++)
        {
            if (i == 0) // 将第一个值设置为初始值
            {
                startDistance = Vector3.Distance(playerPos, centerPoint[i]);
            }
            else // 其他值与第一个值比较
            {
                middleDistance = Vector3.Distance(playerPos, centerPoint[i]);
                if (middleDistance < startDistance) // 中间值比起始值小，将其赋值给startDistance
                {
                    index = i;
                    startDistance = middleDistance;
                }
            }

            startPos = centerGameObjects[index].GetComponent<FloorCenter>();
        }
    }

    /// <summary>
    /// 每次计算时清空父节点和其他信息
    /// </summary>
    private void ClearParentAndOther()
    {
        for (int i = 0; i < centerPoint.Count; i++)
        {
            var floorItem=centerGameObjects[i].GetComponent<FloorCenter>().floorItem;

            floorItem.parentPoint = null;
            floorItem.neighbourDis = 0;
            floorItem.cost = 0;
        }
    }
    
    // 计算最短路径，采用曼哈顿距离计算
    private float Manhattan(Vector3 startPos, Vector3 goalPos)
    {
        return Mathf.Abs(startPos.x - goalPos.x) + Mathf.Abs(startPos.y - goalPos.y);
    }

    // 获取相邻结点列表，将列表中的物体存储到开放列表当中
    private void GetNeighbourPoint(FloorCenter point)
    {
        for (int i = 0; i < point.floorItem.neighbours.Count; i++)
        {
            FloorCenter neighbour = point.floorItem.neighbours[i];
        }
    }

    private void GetShortestPath()
    {
        //清空路径结点,和链表中所有的父节点
        _closeList.Clear();
        _openList.Clear();
        pathList.Clear();
        ClearParentAndOther();
        
        // 获取起点和终点
        GetPlayerFloor();
        GetMouseFloor();

        // 判断起点和终点是否为空
        if (_targetPos == null || startPos == null)
        {
            Debug.Log("起点或目标为空");
            return;
        }
        
        //起始点==终点
        if (_targetPos == startPos) 
        {
            Debug.Log("起始点==终止点");
            return;            
        }
        
        Debug.Log("起点值为：" + startPos.name + "终点值为：" + _targetPos);

        // 将起点加入开启列表
        _openList.Add(startPos);
        
        // 开始搜索路径
        while (_openList.Count > 0) 
        {
            // 从开放结点中获取cost最小的结点
            FloorCenter currentPoint = GetLowestPoint();
            
            // 如果是目标结点则停止搜索
            if (currentPoint == _targetPos)
            {
                ReactPath(startPos, _targetPos); // 回溯
                OnPlayerMoveEvent();//玩家移动
                return;
            }
            
            // 否则从开放列表中移除，加入关闭列表
            _openList.Remove(currentPoint);
            _closeList.Add(currentPoint);
            
            // 获取当前结点的临近结点
            GetNeighbourPoint(currentPoint);

            foreach (var neighbour in currentPoint.floorItem.neighbours)
            {
                if (_closeList.Contains(neighbour) || !neighbour.floorItem.isCanWalkable)
                    continue;

                float newCostToNei = Manhattan(startPos.floorItem.center, neighbour.floorItem.center);
                
                //如果该邻居节点不在开放列表中，或者新的距离值小于原来的距离值
                if (!_openList.Contains(neighbour) || newCostToNei <= neighbour.floorItem.neighbourDis)
                {
                    neighbour.floorItem.neighbourDis = newCostToNei;
                    neighbour.floorItem.neighbourToTarget = Manhattan(neighbour.floorItem.center, _targetPos.floorItem.center);
                    neighbour.floorItem.cost = neighbour.floorItem.neighbourDis + neighbour.floorItem.neighbourToTarget;
                    
                    neighbour.floorItem.parentPoint = currentPoint;

                    if (!_openList.Contains(neighbour))
                        _openList.Add(neighbour);
                    
                    //Debug.Log("设置父节点: " + neighbour.name + " 的父节点为: " + currentPoint.name);
                }
            }

        }
    }

    // 选择列表中cost最小的点
    private FloorCenter GetLowestPoint()
    {
        _openList.Sort((a, b) => a.floorItem.cost.CompareTo(b.floorItem.cost));
        return _openList[0];
    }

    // 路径回溯
    private void ReactPath(FloorCenter start, FloorCenter end)
    {
        List<FloorCenter> path = new List<FloorCenter>();
        FloorCenter currentPoint = end;

        if (currentPoint == null) 
        {
            Debug.Log("当前的end结点为null");
        }
        else if (currentPoint == start) 
        {
            Debug.Log("当前的end结点等于start结点");
        }
        
        //当前结点不为空且当前结点不等于开始结点，把除起始结点以外的结点添加到链表中
        while (currentPoint != null && currentPoint != start) 
        {
            //Debug.Log("当前节点: " + currentPoint.name + ", 父节点: " + (currentPoint.floorItem.parentPoint != null ? currentPoint.floorItem.parentPoint.name : "null"));
            path.Add(currentPoint);
            currentPoint = currentPoint.floorItem.parentPoint;
        }

        //添加起始结点
        path.Add(currentPoint);
        
        path.Reverse(); // 反转路径，使其从起点到终点

        // for (int i = 0; i < path.Count; i++)
        // {
        //     Debug.Log("链表索引：" + i + "对应物体" + path[i].name);
        // }
        
        pathList = path;
        //Debug.Log("路径节点数量: " + path.Count);
    }
    
    private void OnPlayerMoveEvent()
    {
        PlayerMoveEvent?.Invoke();
    }
}

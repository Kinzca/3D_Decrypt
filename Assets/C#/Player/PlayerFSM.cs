using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
public class PlayerParamenter
{
    public PackageUI package;
    public PRoute route;
    public float moveSpeed;

    public List<Item> itemsList;
    //public Dictionary<int,(string,string,string,int)> ItemDictionary; 
    public List<FloorCenter> floorCenters;
    
    public LayerMask objMask;
    public LayerMask floorMask;

    public bool isClickUI;
    
    public int currentPoint = 0;
    public Coroutine currentCoroutine;
}

public class PlayerFSM : MonoBehaviour
{
    public enum PlayerStateType
    {
        Move,Wait,Pick
    }

    public PlayerParamenter playerParamenter;

    private IState _currentState;
    
    private Dictionary<PlayerStateType, IState> _states = new Dictionary<PlayerStateType, IState>();

    private void Awake()
    {
        EventCenter.AddListener(EventType.PlayerMove,OnPlayerMove);
        EventCenter.AddListener<bool>(EventType.ClickUI,ClickUI);
    }

    private void Start()
    {
         _states.Add(PlayerStateType.Move,new PMove(this));
         _states.Add(PlayerStateType.Wait,new PWait(this));
         _states.Add(PlayerStateType.Pick,new PPick(this));
         //初始化玩家存储物品的字典——编号、（名称、描述、图标存取的路径、数量）
         playerParamenter.itemsList = new List<Item>();
         
         //初始化玩家位置
         StartCoroutine(InitPos());  
         
         //默认进入等待状态
         TransitionState(PlayerStateType.Wait);
    }
    
    private void Update()
    {
        _currentState.OnUpdate();
    }
    
    public void TransitionState(PlayerStateType stateType)
    {
        if (_currentState != null) 
        {
            _currentState.OnExit();
        }

        _currentState = _states[stateType];
        _currentState.OnEnter();
    }

    #region 初始化玩家坐标操作

    //初始化其实坐标
    private IEnumerator InitPos()
    {
        var route = playerParamenter.route;
        
        while (route == null || route.startPos == null || route.startPos.floorItem == null)
        {
            yield return null;
        }

        transform.position = InitializePlayPos();//初始化玩家位置
    }
    
    //初始化玩家位置
    private Vector3 InitializePlayPos()
    {
        var route = playerParamenter.route;
        
        var pos = route.startPos.floorItem.center;
        Vector3 playPos = new Vector3(pos.x, pos.y + 0.4f, pos.z);

        return playPos;
    }

    #endregion

    #region 进行玩家移动操作

    //进行玩家移动,等待一帧进行移动 
    private void OnPlayerMove()
    {
        if (playerParamenter.currentCoroutine != null) 
        {
            StopCoroutine(playerParamenter.currentCoroutine);
        }

        playerParamenter.currentCoroutine = StartCoroutine(MoveAlongPoint());
    }

    /// <summary>
    /// 选择路径最近的结点
    /// </summary>
    /// <returns></returns>
    private int GetClosetPointOnPath(List<FloorCenter> path)
    {
        if (path == null || path.Count == 0)
        {
            Debug.LogError("Path is null or empty.");
            return 0;
        }

        int closestIndex = -1;
        float minDistance = float.MaxValue;
        Vector3 playerPosition = transform.position;

        // 找到距离玩家最近的结点
        for (int i = 0; i < path.Count; i++)
        {
            Vector3 nodePosition = new Vector3(
                path[i].floorItem.center.x, 
                path[i].floorItem.center.y + 0.4f, 
                path[i].floorItem.center.z
            );
            float distance = Vector3.Distance(playerPosition, nodePosition);

            if (distance < minDistance)
            {
                minDistance = distance;
                closestIndex = i;
            }
        }

        // 检查 closestIndex 是否是最后一个元素，避免越界
        if (closestIndex == path.Count - 1)
        {
            // 如果已经是最后一个元素，则无法获取 `closestIndex + 1`，直接返回
            return closestIndex;
        }

        Vector3 directionToClosest = (path[closestIndex].floorItem.center - playerPosition).normalized;
        Vector3 directionFromPrevious = (path[closestIndex].floorItem.center - path[closestIndex + 1].floorItem.center).normalized;

        // 计算点积，判断方向
        float dotProduct = Vector3.Dot(directionToClosest, directionFromPrevious);

        // 如果点积小于0，说明方向相反，返回前一个结点
        if (dotProduct > 0 && closestIndex < path.Count - 1)
        {
            closestIndex += 1; //选择下一个结点
        }

        return closestIndex;
    }
    
    private IEnumerator MoveAlongPoint()
    {
        var path = playerParamenter.route.pathList;

        if (path == null || path.Count == 0)
        {
            Debug.Log("不可行走地块！.");
            TransitionState(PlayerStateType.Wait);
            yield break;
        }

        playerParamenter.currentPoint = GetClosetPointOnPath(path);
        //Debug.Log($"Starting movement from index: {playerParamenter.currentPoint}");

        while (playerParamenter.currentPoint < path.Count)
        {
            if (playerParamenter.currentPoint >= path.Count)
            {
                Debug.LogError("currentPoint is out of range during movement.");
                break;
            }

            Vector3 newCenter = new Vector3(
                path[playerParamenter.currentPoint].floorItem.center.x,
                path[playerParamenter.currentPoint].floorItem.center.y + 0.4f,
                path[playerParamenter.currentPoint].floorItem.center.z
            );

            var targetPosition = newCenter;
            while (transform.position != targetPosition)
            {
                transform.position = Vector3.MoveTowards(
                    transform.position, targetPosition, Time.deltaTime * playerParamenter.moveSpeed
                );
                yield return null;
            }

            playerParamenter.currentPoint++;
        }

        //完成移动后转入等待状态
        Debug.Log("Movement along path completed.");
        TransitionState(PlayerStateType.Wait);
    }

    #endregion

    #region 确定是否点击到UI组件

    private void ClickUI(bool isClickUI)
    {
        playerParamenter.isClickUI = isClickUI;
    }

    #endregion
    
    private void OnDisable()
    {
        EventCenter.RemoveListener(EventType.PlayerMove,OnPlayerMove);
        EventCenter.RemoveListener<bool>(EventType.ClickUI,ClickUI);
    }
}

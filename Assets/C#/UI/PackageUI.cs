using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Vector3 = UnityEngine.Vector3;

public class PackageUI : MonoBehaviour
{
    public PlayerFSM player;
    public PackageView packageView;
    
    public List<GameObject> childGrids;
    private Image _image;
    
    //临时拖动的物体数据
    private  Dictionary<int,(string,string,string,int)> _itemDictionary;
    private PackageGrid _dragItem;
    [Header("临时物体组件")]public GameObject gridTemporary;
    private int _originalID;
    private int _endID;
    private Sprite _dragItemSprite;
    //临时物体变量
    private int _dragID;//临时物品ID
    private string _dragPath;
    private int _dragNumber;
    private string _dragName;
    private string _dragNameDescription;
    
    private bool _isInNearFloor = false;//放置的物体是否在周围四格内
    private bool _isDrag = false;//是否正在被拖拽

    [Header("鼠标结束拖拽时的射线层级")] public LayerMask layerMask;
    
    
    private void Start()
    {
        if (player == null)  
        {  
            Debug.LogError("Player 未正确初始化");  
            return;  
        }  

        if (player.playerParamenter == null)  
        {  
            Debug.LogError("PlayerParameter 未正确初始化");  
            return;  
        }  
     
        //临时字典等于玩家字典
        _itemDictionary = player.playerParamenter.ItemDictionary; 
        
        // 获取子物体列表  
        GetGridChild();  

        foreach (var child in childGrids)  
        {  
            var slot = child.GetComponent<PackageGrid>();  
            if (slot != null)   
            {  
                EventCenter.AddListener<PackageGrid, PointerEventData>(EventType.OnDrag, OnDrag);  
                EventCenter.AddListener(EventType.EndDrag, EndDrag);  
            }  
        }  

        PackageUIInit();  
    }
    
    private void GetGridChild()
    {
        //初始化列表
        childGrids = new List<GameObject>();

        if (childGrids.Count == 0)  
        {
            Utility.GetAllChild(childGrids,this.gameObject);
        }
    }
    
    private void PackageUIInit()
    {
        PackageUIMove();
        PackageUIFade();
    }

    #region 背包UI初始化移动

    private void PackageUIMove()
    {
        //新坐标-原坐标=需设定后的坐标
        var targetPos = Utility.TransformRectPos(gameObject.GetComponent<RectTransform>(), new Vector3(0, 114.5f, 0));
        
        //局部坐标希转换
        transform.DOMove(targetPos,  1f, false).SetEase(Ease.InOutSine);
        
        //Debug.Log(targetPos);
    }

    private void PackageUIFade()
    {
        _image = gameObject.GetComponent<Image>();
        
        Utility.UIFade(_image,1f,1f);
    }

    #endregion

    #region 处理背包拖拽

    private void BeginDrag(PackageGrid grid)
    {
        _isDrag = true;
        _itemDictionary = player.playerParamenter.ItemDictionary;//重新赋值
        
        if (grid == null)  
        {  
            Debug.LogError("传入的 grid 为 null");  
            return; // 退出方法  
        }  

        int id = grid.id;  
        InitDragItem(id);  
    }
    
    private void InitDragItem(int id)  
    {  
        if (_itemDictionary == null)  
        {  
            Debug.Log("玩家字典为空");
        } 
        
        // 尝试从字典中获取键为 id 的值  
        if (_itemDictionary != null && _itemDictionary.TryGetValue(id, out var itemValue))
        {
            Debug.Log("正造拖拽id值为" + id + "的物体");
            _dragID = id;
            (_dragName, _dragNameDescription, _dragPath, _dragNumber) = itemValue;
        }  
        else  
        {  
            Debug.Log($"未找到键为 {id} 的物品");  
            _dragItem = null; // 如果未找到，设置为 null  
        }  
        
        //开启临时物体的Active,然后把原来的物体透明度调低
        gridTemporary.SetActive(true);
        GameObject grid = null;
        
        for (int i = 0; i < childGrids.Count; i++)
        {
            childGrids[i].GetComponent<PackageGrid>().id = id;

            grid = childGrids[i];
            grid.GetComponent<Image>().DOFade(0.5f,0.2f);
            
            return;
        }
    }
    
    private void OnDrag(PackageGrid grid, PointerEventData pointerEventData)
    {
        if (grid == null || pointerEventData == null)
        {
            Debug.LogError("grid 或 PointerEventData 为 null");
            return;
        }

        //初始化拖拽信息（一次性操作）
        if (_dragItem == null)
        {
            BeginDrag(grid);
        }

        //实时更新图标位置
        packageView.DragItem(grid.image.sprite, pointerEventData.position);
    }

    private void EndDrag()
    {
        if (!_isDrag)
        {
            return;
        }
        _isDrag = false;
        
        //获取结束时的地板
        GameObject endFloor = null;
        if (Utility.GetMouseObject(layerMask, "WalkableFloor"))
        {
            endFloor = Utility.GetMouseObject(layerMask, "WalkableFloor");
        }else if (Utility.GetMouseObject(layerMask, "UnWalkableFloor"))
        {
            endFloor = Utility.GetMouseObject(layerMask, "UnWalkableFloor");
        }
        
        //Debug.Log("endFloor的名字为" + endFloor);
        
        JudgeInNextFloor(endFloor);

        //如果不在周围四格直接返回，
        if (!_isInNearFloor)
        {
            Debug.Log("放置的位置超出了玩家可以放置的范围");
            
            //更新UI
            EventCenter.Broadcast(EventType.UpdateUI);
            gridTemporary.SetActive(false);
            return;
        }
        
        var floor = endFloor.GetComponent<FloorCenter>();
        var pos = floor.floorItem.center;
        Vector3 position = new Vector3(pos.x, pos.y + 0.4f, pos.z);
        
        EventCenter.Broadcast(EventType.SetActiveTrue,_dragID,position);
        
        //更新玩家物品字典,移除键为_dragID的物品
        player.playerParamenter.ItemDictionary.Remove(_dragID);
        _itemDictionary = player.playerParamenter.ItemDictionary; 
        
        EventCenter.Broadcast(EventType.UpdateUI);
        gridTemporary.SetActive(false);
    }

    #endregion
    
    #region 判断结束时的格子是否在玩家周围四格

    private void JudgeInNextFloor(GameObject gameObject)
    {
        var floor = gameObject.GetComponent<FloorCenter>();

        if (floor == null) 
        {
            Debug.LogWarning("Floor为空");
            return;
        }
        
        var playerFloor = GetPlayerFloor();
        
        //玩家所在格子的节点的邻居节点包含鼠标结束时所放置的节点位置
        if (playerFloor.floorItem.neighbours.Contains(floor))
        {
            _isInNearFloor = true;
        }
        else
        {
            _isInNearFloor = false;
        }

    }
    
    private FloorCenter GetPlayerFloor()
    {
        float distance = 2.5f;

        if (Physics.Raycast(player.transform.position,-player.transform.up,out RaycastHit hit,distance))
        {
            return hit.collider.gameObject.GetComponent<FloorCenter>();
        }

        return null;
    }

    #endregion
    
    private void OnDestroy()
    {
        foreach (var child in childGrids)
        {
            var slot = child.GetComponent<PackageGrid>();
            if (slot != null) 
            {
                EventCenter.RemoveListener<PackageGrid,PointerEventData>(EventType.OnDrag,OnDrag);
                EventCenter.RemoveListener(EventType.EndDrag,EndDrag);
            }
        }
    }
}

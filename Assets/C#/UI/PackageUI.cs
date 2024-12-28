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
    
    public List<GameObject> childGrids;//格子组件
    private Image _image;
    
    //临时拖动的物体数据
    private  List<Item> _itemsList;
    private PackageGrid _dragItem;
    [Header("临时物体组件")]public GameObject gridTemporary;
    private int _originalID;
    private int _endID;
    private Sprite _dragItemSprite;
    //原有物体数据
    private PackageGrid _packageGrid;
    //临时物体变量
    private int _dragID;//临时物品ID
    private string _dragPath;
    private int _dragWeight;
    private string _dragName;
    private string _dragNameDescription;
    [Header("可被合成的物体")] 
    public GameObject boxAndBottle;
    [Space]
    public GameObject boxAndBox;
    
    private bool _isInNearFloor = false;//放置的物体是否在周围四格内
    private bool _isDrag = false;//是否正在被拖拽
    private bool _isInUI = false;//是否放置在Ui上
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
     
        //临时列表=玩家列表
        _itemsList = player.playerParamenter.itemsList; 
        
        // 获取子物体列表  
        GetGridChild();  

        foreach (var child in childGrids)  
        {  
            var slot = child.GetComponent<PackageGrid>();  
            if (slot != null)   
            {  
                EventCenter.AddListener<PackageGrid, PointerEventData>(EventType.OnDrag, OnDrag);  
                EventCenter.AddListener<PointerEventData>(EventType.EndDrag, EndDrag);  
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

    private void BeginDrag(PackageGrid grid)//开始的格子
    {
        _isDrag = true;
        _itemsList = player.playerParamenter.itemsList;//重新赋值
        
        if (grid == null)  
        {  
            Debug.LogError("传入的 grid 为 null");  
            return; // 退出方法  
        }

        //_packageGrid = grid;
        int id = grid.id;//将 格子 中 物品ID 赋值
        Item item = null;
        
        for (int i = 0; i < _itemsList.Count; i++)
        {
            if (_itemsList[i].id == id)//如果物品列表中的其中一个物体的id值 = 格子中所在物品的id值，那么这个链表中的该物体就是正在被拖拽的物体
            {
                item = _itemsList[i];//将其赋值给临时item
                Debug.Log("传递的item,id值：" + item.id);
            }
        }
        
        InitDragItem(item);  
    }
    
    private void InitDragItem(Item item)
    {
        if (_itemsList == null || item == null)
        {
            Debug.Log("玩家背包列表为空或者选中物体为空");
            return;
        }

        //将物体信息赋值给临时物体
        if (_itemsList.Contains(item))
        {
            _dragID = item.id;
            _dragName = item.itemName;
            _dragNameDescription = item.description;
            _dragPath = item.imagePath;
            _dragWeight = item.weight;
        }
        else
        {
            _dragItem = null; //如果未找到，设置为 null
        }

        // 开启临时物体的Active, 然后把原来的物体透明度调低
        gridTemporary.SetActive(true);

        for (int i = 0; i < childGrids.Count; i++)
        {
            GameObject grid = childGrids[i];
            PackageGrid packageGrid = grid.GetComponent<PackageGrid>();
        
            if (packageGrid != null && packageGrid.id == _dragID)
            {
                _packageGrid = packageGrid;
                Debug.Log("Grid ID为："+packageGrid.id);
                grid.GetComponent<Image>().DOFade(0.5f, 0.2f); //设置透明度
            }
        }
    }

    
    private void OnDrag(PackageGrid grid, PointerEventData pointerEventData)
    {
        if (grid == null || pointerEventData == null)
        {
            Debug.LogError("grid 或 PointerEventData 为 null");
            return;
        }

        // 初始化拖拽信息（一次性操作）
        if (_dragItem == null)
        {
            BeginDrag(grid);  // 确保初始化
        }

        Debug.Log("当前拖拽的物体id是" + grid.id);

        // 实时更新图标位置
        packageView.DragItem(grid.image.sprite, pointerEventData.position);
    }


    private void EndDrag(PointerEventData eventData)
    {
        if (!_isDrag)
        {
            return;
        }
        _isDrag = false;
        
        //首先处理是否在UI在，如果在UI上进行合成
        JudgeAndDealWeight(eventData);
        
        //获取结束时的地板
        GameObject endFloor = null;
        if (Utility.GetMouseObject(layerMask, "WalkableFloor"))
        {
            endFloor = Utility.GetMouseObject(layerMask, "WalkableFloor");
        }else if (Utility.GetMouseObject(layerMask, "UnwalkableFloor"))
        {
            endFloor = Utility.GetMouseObject(layerMask, "UnwalkableFloor");
        }
        
        //Debug.Log("endFloor的名字为" + endFloor);
        
        JudgeInNextFloor(endFloor);

        //如果被拖拽到Ui上则不进行以下计算
        //if (_isInUI)
        // {
        //     return;
        // }

        //如果不在周围四格直接返回，
        if (!_isInNearFloor)
        {
            Debug.Log("放置的位置超出了玩家可以放置的范围");
            
            //更新UI
            EventCenter.Broadcast(EventType.UpdateUI);
            gridTemporary.SetActive(false);
            return;
        }
        
        //以下处理结束拖拽到地板上的操作
        var floor = endFloor.GetComponent<FloorCenter>();
        var pos = floor.floorItem.center;
        Vector3 position = new Vector3(pos.x, pos.y + 0.4f, pos.z);
        
        EventCenter.Broadcast(EventType.SetActiveTrue,_dragID,position);
        
        //更新玩家物品列表,移除键为_dragID的物品
        var list = player.playerParamenter.itemsList;
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i].id == _dragID)
            {
                list.Remove(list[i]);
            }
        }
        _itemsList = player.playerParamenter.itemsList; 
        
        //更新操作，UI更新，关闭临时物体，将地板tag转换为canWalkable
        gridTemporary.SetActive(false);
        endFloor.tag = "WalkableFloor";
        //Debug.Log("释放图标时UI更新操作完成");
        
        EventCenter.Broadcast(EventType.UpdateUI);
        
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

    #region 判断结束时是否放置在UI上，其次在对是否在周围四个进行判断

    private void JudgeAndDealWeight(PointerEventData eventData)
    {
        // 获取鼠标指向的物体
        GameObject targetObject = eventData.pointerCurrentRaycast.gameObject;

        if (targetObject == null) 
        {
            Debug.Log("鼠标指向的物体为null");
            return;
        }
        
        Debug.Log("指向的目标物体为："+targetObject.name);
        
        // 检查物体是否有PackageGrid组件
        PackageGrid targetGrid = targetObject?.GetComponent<PackageGrid>();

        if (targetGrid != null)
        {
            //_isInUI = true;
            // 输出鼠标指向的物品ID
            Debug.Log("结束时指向的Grid为：" + targetGrid.id);
            
            ComposeObject(targetGrid,_packageGrid);   
        }
        else
        {
            //_isInUI = false;
            // 如果没有找到 PackageGrid 组件
            Debug.Log("结束时鼠标指向的物体没有 PackageGrid 组件");
        }
    }
    
    /// <summary>
    /// 计算合成后的物品及其数值
    /// </summary>
    /// <param name="targetPackageGrid">被拖拽到的目标槽位</param>
    /// <param name="originalPackageGrid">原来的槽位</param>
    private void ComposeObject(PackageGrid targetPackageGrid,PackageGrid originalPackageGrid)
    {
        //合成前的目标物体权重值
        var targetWeight = targetPackageGrid.weight;
        //合成前原有物品权重值
        var originalWeight = originalPackageGrid.weight;
        
        //新的合成值
        var newWeight = targetWeight * originalWeight;
        Debug.Log("新的合成值为："+newWeight);
        
        //合成数值表
        switch (newWeight)
        {
            //紫色品质物体
            case 6:
                Debug.Log("合成紫色品质");
                PurpleQuality(targetPackageGrid,originalPackageGrid);
                break;
            //红色品质物体
            case 9:
                Debug.Log("合成红色品质");
                RedQuality(targetPackageGrid,originalPackageGrid);
                break;
        }
    }

    //紫色品质
    private void PurpleQuality(PackageGrid obj1,PackageGrid obj2)
    {
        if ((obj1.name == "箱子" && obj2.name == "粉末") || (obj1.name == "粉末" && obj2.name == "箱子"))
        {
            SeekOppositeID(obj1, obj2, boxAndBottle);
        }
    }

    //红色品质
    private void RedQuality(PackageGrid obj1,PackageGrid obj2)
    {
        if (obj1.name == "箱子" && obj2.name == "箱子")
        {
            SeekOppositeID(obj1, obj2, boxAndBox);
        }
    }

    //查找对应得ID并传入相应得物体
    private void SeekOppositeID(PackageGrid obj1, PackageGrid obj2, GameObject newObj)
    {
        // 玩家物品列表
        var playerList = player.playerParamenter.itemsList;

        // 定义一个标志变量，判断是否移除了对应的物品
        bool itemRemoved = false;

        // 使用倒序循环避免索引问题
        for (int i = playerList.Count - 1; i >= 0; i--)
        {
            Debug.Log($"当前检查的物品ID: {playerList[i].id}, obj1 ID: {obj1.id}, obj2 ID: {obj2.id}");

            if (playerList[i].id == obj1.id || playerList[i].id == obj2.id)
            {
                Debug.Log("移除：" + playerList[i].name);
                playerList.RemoveAt(i);
                itemRemoved = true;
            }
        }


        // 如果成功移除了物品，生成新物体
        if (itemRemoved)
        {
            Debug.Log("成功合成物品，新物体添加到玩家列表：" + newObj.name);
            playerList.Add(newObj.GetComponent<Item>());
        }
        else
        {
            Debug.LogWarning("合成失败，未找到可移除的物品！");
        }
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
                EventCenter.RemoveListener<PointerEventData>(EventType.EndDrag,EndDrag);
            }
        }
    }
}

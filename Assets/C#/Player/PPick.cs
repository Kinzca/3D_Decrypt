using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PPick : IState
{
    private PlayerFSM _fsm;
    private PlayerParamenter _playerParamenter;

    private GameObject _pickupObject;
    private bool _isInNearFloor;
    public PPick(PlayerFSM fsm)
    {
        _fsm = fsm;
        _playerParamenter = _fsm.playerParamenter;
    }

    public void OnEnter()
    {
        _isInNearFloor = false;
        Debug.Log("玩家进入拾取状态");
    }

    public void OnUpdate()
    {
        //获取玩家指向的物体，添加到列表
        GetGameObject();
    }

    public void OnExit()
    {
        var itemDictionary = _playerParamenter.itemsList;  

        //在此处打印字典内容
        foreach (var item in itemDictionary)  
        {
            Debug.Log($"字典包含的物品: ID={item.id}, Name={item.name}");  
        }  

        Debug.Log("玩家退出拾取状态");
    }

    //获取地面物品
    private void GetGameObject()
    {
        _pickupObject = Utility.GetMouseObject(_playerParamenter.objMask, "Object");
        JudgeInNextFloor(_pickupObject);
        
        if (_pickupObject == null)
        {
            Debug.Log("没有检测到物品!");
            return; //如果没有物品被检测到，直接返回
        }

        //检查 package 是否为 null
        if (_playerParamenter.package == null)
        {
            Debug.LogError("背包 package 为空！");
            return;
        }

        if (!_isInNearFloor)
        {
            //返回等待状态
            Debug.Log("物品超出获取物品的范围！");
            _fsm.TransitionState(PlayerFSM.PlayerStateType.Wait);
            return;
        }
        
        //如果玩家字典数量 < 背包格子数量
        if (_playerParamenter.itemsList.Count < _playerParamenter.package.childGrids.Count)  
        {
            var item = _pickupObject.GetComponent<Item>();
            var itemID = item.id;//物品ID

            var objFloor = GetObjFloor(_pickupObject);
            objFloor.tag = "WalkableFloor";
            
            //设置被点击到的物品状态关闭，同时传入该物体item的ID值
            EventCenter.Broadcast(EventType.SetActiveFalse,itemID);
            
            if (item != null)
            {
                //向字典中添加物品信息
                _playerParamenter.itemsList.Add(_pickupObject.GetComponent<Item>());

                // 更新UI
                EventCenter.Broadcast(EventType.UpdateUI);

                Debug.Log("成功添加到背包");
            }
            else
            {
                Debug.Log("物品没有 Item 组件！");
            }
        }
        else
        {
            Debug.Log("背包已满！");
        }

        // 拾取结束转换为等待
        _fsm.TransitionState(PlayerFSM.PlayerStateType.Wait);
    }

    #region 物品数量管理(暂时不用)
    //
    // //增加物品数量
    // private int AddQuality(int id)
    // {
    //     if (GetQualityByID(id) == 1)
    //     {
    //         return 1;
    //     }
    //     else
    //     {
    //         return GetQualityByID(id) + 1;
    //     }
    // }
    // private int GetQualityByID(int itemId)
    // {
    //     var itemDictionary = _playerParamenter.itemsList;
    //
    //     //检查字典中是否包含指定的ID
    //     if (itemDictionary.TryGetValue(itemId, out var itemData))
    //     {
    //         //解构获取数量
    //         int quantity = itemData.Item4; //itemData的第四个元素是数量
    //         return quantity;
    //     }
    //     else
    //     {
    //         Debug.LogWarning($"未找到ID为 {itemId} 的物品,向列表的该物品数量设为1");
    //         return 1; // 如果未找到，返回1（或其他默认值）
    //     }
    // }

    #endregion

    #region 判断所拾取的物体是否在周围4格

    private void JudgeInNextFloor(GameObject gameObject)
    {
        float distance = 2.5f;

        // 确保方向和物品位置判断准确
        if (Physics.Raycast(gameObject.transform.position, -gameObject.transform.up, out RaycastHit hit, distance))
        {
            var neighbourFloor = GetNeighbourFloor(hit.collider.gameObject);
            var player = GetPlayerFloor();

            if (neighbourFloor.Contains(player))
            {
                _isInNearFloor = true;
            }
        }
    }
    
    private FloorCenter GetPlayerFloor()
    {
        float distance = 2.5f;

        if (Physics.Raycast(_fsm.transform.position,-_fsm.transform.up,out RaycastHit hit,distance))
        {
            return hit.collider.gameObject.GetComponent<FloorCenter>();
        }

        return null;
    }
    
    //获取物品所在格子的相邻结点
    private List<FloorCenter> GetNeighbourFloor(GameObject gameObject)
    {
        var floor = gameObject.GetComponent<FloorCenter>();

        return floor.floorItem.neighbours;
    }

    #endregion

    #region 获取该物体正下方的Floor，将该Floor的值改为Walkable
    private GameObject GetObjFloor(GameObject obj)
    {
        float distance = 2.5f;

        if (Physics.Raycast(obj.transform.position,-obj.transform.up,out RaycastHit hit,distance))
        {
            return hit.collider.gameObject;
        }

        return null;
    }

    #endregion
    
}

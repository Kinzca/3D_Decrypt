using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class PackageView : MonoBehaviour
{
    public PlayerFSM playerFsm;
    public Transform itemCanvas;//显示图标的画布
    
    public GameObject gridTemporary;
    private PlayerParamenter _playerParamenter;
    private void Start()
    {
        EventCenter.AddListener(EventType.UpdateUI,UpdateUI);
        _playerParamenter = playerFsm.playerParamenter;
    }

    private void UpdateUI()
    {
        // 获取物品列表和背包格子列表
        var itemsList = _playerParamenter.itemsList;
        var gridList = _playerParamenter.package.childGrids;

        if (itemsList == null || gridList == null)
        {
            Debug.LogError("物品列表或背包格子列表为空！");
            return;
        }
        
        ClearAllGrids();
        
        //首先对字典中的物体进行排序
        SortItem(itemsList);
        
        //遍历字典对字典中的元素进行提取
        foreach (var item in itemsList)
        {
            int firstGridIndex = GetFirstGrid();
            
            if (firstGridIndex == -1) 
            {
                Debug.Log("背包已满，无法装载更多物品！");
            }
            
            //获取空格子并更新视图
            var grid = gridList[firstGridIndex];
            var packGrid = grid.GetComponent<PackageGrid>();
            if (packGrid != null)
            {
                packGrid.id = item.id;
                packGrid.name = item.name;
                packGrid.Description = item.description;
                packGrid.weight = item.weight;
                
                //加载物品图标
                Sprite itemSprite = Resources.Load<Sprite>(item.imagePath);
                packGrid.image.sprite = itemSprite;
                packGrid.image.enabled = true;//显示图标
                //根据质量选择背景图标颜色
                var image = packGrid.backGround.GetComponent<Image>();
                switch (item.weight)
                { 
                    
                    case 2:
                        image.DOColor(Color.white, 0.1f);
                        break;
                    case 3:
                        image.DOColor(Color.green, 0.1f);
                        break;
                    case 6:
                        image.DOColor(Color.blue, 0.1f);
                        break;
                    case 9:
                        image.DOColor(Color.magenta, 0.1f);
                        break;
                    case 0:
                        image.DOColor(Color.white, 0.1f);
                        break;
                }
                //重设颜色
                packGrid.image.DOFade(1, 0.5f);
            }
            
            //其他逻辑。。。
        }
    }

    //获取背包列表中第一个不为0的列表
    private int GetFirstGrid()
    {
        var grids = _playerParamenter.package.childGrids;

        // 遍历背包格子，找到第一个未占用的格子
        for (int i = 0; i < grids.Count; i++)
        {
            var packageGrid = grids[i].GetComponent<PackageGrid>();
            if (packageGrid != null && packageGrid.image.sprite == null)
            {
                return i; // 返回空格子的索引
            }
        }

        // 未找到空格子
        return -1;
    }

    //清空背包中的物品避免重复显示
    private void ClearAllGrids()
    {
        var gridList = _playerParamenter.package.childGrids;
        
        // 清空背包格子中的图标，以防更新UI时出现重复
        foreach (var grid in gridList)
        {
            var packageGrid = grid.GetComponent<PackageGrid>();
            
            var image = packageGrid.backGround.GetComponent<Image>();

            //清空格子中数据，还原图标
            //Debug.Log("已清空格子"+grid.name+"还原其背景颜色");
            image.DOColor(Color.white, 0.1f);
        
            packageGrid.id = 0;
            packageGrid.name = null;
            packageGrid.weight = 0;
            packageGrid.Description = null;
            
            if (packageGrid != null)
            {
                packageGrid.image.sprite = null;
                packageGrid.image.enabled = false; // 隐藏图标
            }
        }
    }
    
    private void SortItem(List<Item> item)
    {
        //按item4排序后转换为列表
        var sortItems = item.OrderBy(item => item.weight).ToList();
        
        //清空玩家字典，重新添加
        _playerParamenter.itemsList.Clear();
        //Debug.Log("字典已清空,当前字典值"+_playerParamenter.itemsList.Count);
        
        for (int i = 0; i < sortItems.Count() ; i++)
        {
            _playerParamenter.itemsList.Add(sortItems[i]);  
        }
    }
    
    
    // public void DragItem(Sprite icon, Vector2 screenPosition)  
    // {  
    //     if (icon == null)  
    //     {  
    //         Debug.Log("传入的图标为 null。");  
    //         return;  
    //     }  
    //
    //     // 获取 gridTemporary 的 Image 组件
    //     var image = gridTemporary.GetComponent<Image>();  
    //     if (image == null)  
    //     {  
    //         Debug.LogError("gridTemporary 上没有 Image 组件。");  
    //         return;  
    //     }  
    //
    //     // 设置图标
    //     image.sprite = icon;  
    //     gridTemporary.SetActive(true);
    //
    //     // 将屏幕坐标转换为世界坐标
    //     Vector3 worldPosition;
    //     RectTransformUtility.ScreenPointToWorldPointInRectangle(
    //         itemCanvas as RectTransform, screenPosition, Camera.main, out worldPosition);
    //
    //     gridTemporary.transform.position = worldPosition;  
    //     
    //     Debug.Log($"Screen Position: {screenPosition}, World Position: {worldPosition}");
    // }

    public void DragItem(Sprite icon, Vector2 screenPosition)  
    {  
        if (icon == null)  
        {  
            Debug.Log("传入的图标为 null。");  
            return;  
        }  

        //获取 gridTemporary 的 RectTransform 组件
        RectTransform rectTransform = gridTemporary.GetComponent<RectTransform>();  
        if (rectTransform == null)  
        {  
            Debug.LogError("gridTemporary 上没有 RectTransform 组件。");  
            return;  
        }  

        // 设置图标
        var image = gridTemporary.GetComponent<Image>();
        if (image != null)  
            image.sprite = icon;

        gridTemporary.SetActive(true);

        // 将屏幕坐标转换为 UI 本地坐标
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            itemCanvas.transform as RectTransform, screenPosition, null, out localPoint);

        // 设置 anchoredPosition（本地坐标）
        rectTransform.anchoredPosition = localPoint;
    }
    
    private void OnDestroy()
    {
        EventCenter.RemoveListener(EventType.UpdateUI,UpdateUI);
    }
}

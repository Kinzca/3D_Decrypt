using System;
using System.Collections;
using System.Collections.Generic;
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
        var itemDictionary = _playerParamenter.ItemDictionary;
        var gridList = _playerParamenter.package.childGrids;

        if (itemDictionary == null || gridList == null)
        {
            Debug.LogError("物品列表或背包格子列表为空！");
            return;
        }
        
        ClearAllGrids();
        
        //遍历字典对字典中的元素进行提取
        foreach (KeyValuePair<int,(string,string,string,int)> keyValue in itemDictionary)
        {
            int id = keyValue.Key;
            (string itemName, string description, string path, int quantity) = keyValue.Value;

            int firstGridIndex = GetFirstGrid();
            if (firstGridIndex == -1) 
            {
                Debug.Log("背包已满，无法装载更多物品！");
            }
            
            Debug.Log(path);

            //获取空格子并更新视图
            var grid = gridList[firstGridIndex];
            var packGrid = grid.GetComponent<PackageGrid>();
            if (packGrid != null)
            {
                packGrid.id = id;
                packGrid.name = itemName;
                packGrid.Description = description;
                
                //加载物品图标
                Sprite itemSprite = Resources.Load<Sprite>(path);
                packGrid.image.sprite = itemSprite;
                packGrid.image.enabled = true;//显示图标
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
            if (packageGrid != null)
            {
                packageGrid.image.sprite = null;
                packageGrid.image.enabled = false; // 隐藏图标
            }
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

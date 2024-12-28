using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PackageGrid : MonoBehaviour,IDragHandler,IEndDragHandler
{
    public float delayTime;
    public int id;//物品ID
    public Image image;
    public string name;
    public string Description;
    [Header("物体权重")] public int weight;
    [Header("所对应的物体背景")]public GameObject backGround;  
    
    private void Start()
    {
        image = gameObject.GetComponent<Image>();
        
        PackageGridInit();
    }

    #region 背包格子初始化

    private void PackageGridInit()
    {
        StartCoroutine(WaitForTime(delayTime));
    }

    private void GridFade()
    {
        Utility.UIFade(image,1f,1f);
    }

    private IEnumerator WaitForTime(float time)
    {
        yield return new WaitForSeconds(time);
    }


    #endregion

    #region 鼠标拖拽放置事件

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("鼠标放置在的物体ID是："+id);
        EventCenter.Broadcast(EventType.OnDrag,this,eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //Debug.Log("鼠标松开放置");
        EventCenter.Broadcast(EventType.EndDrag,eventData);
    }

    #endregion
    
}

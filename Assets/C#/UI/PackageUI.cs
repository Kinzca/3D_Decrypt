using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class PackageUI : MonoBehaviour
{
    private List<GameObject> _childGrids;
    private Image _image;
    private void Start()
    {
        //初始化列表
        _childGrids = new List<GameObject>();

        if (_childGrids.Count == 0)  
        {
            Utility.GetAllChild(_childGrids,this.gameObject);
        }
        
        //Debug.Log("子物体数量"+_childGrids.Count);
        
        PackageUIInit();
    }

    private void PackageUIInit()
    {
        PackageUIMove();
        PackageUIFade();
    }  
    
    private void PackageUIMove()
    {
        //新坐标-原坐标=需设定后的坐标
        var targetPos = Utility.TransformRectPos(gameObject.GetComponent<RectTransform>(), new Vector3(0, 64.5f, 0));
        
        //局部坐标希转换
        transform.DOMove(targetPos,  1f, false).SetEase(Ease.InOutSine);
        
        Debug.Log(targetPos);
    }

    private void PackageUIFade()
    {
        _image = gameObject.GetComponent<Image>();
        
        Utility.UIFade(_image,1f,1f);
    }
}

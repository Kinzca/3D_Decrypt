using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GridInit : MonoBehaviour
{
    private Image _image;
    
    private void Start()
    {
        PackageUIInit();
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
}

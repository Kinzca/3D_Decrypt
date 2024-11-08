using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public static class Utility
{
    /// <summary>
    /// 获取一个物体下的所有子物体
    /// </summary>
    /// <param name="childrenList"></param>
    /// <param name="gameObject"></param>
    public static void GetAllChild(List<GameObject> childrenList,GameObject gameObject){
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            //子物体下还有子物体，则递归该物体下的子物体
            if (gameObject.transform.GetChild(i).childCount > 0) 
            {
                GetAllChild(childrenList,gameObject.transform.GetChild(i).gameObject);
            }
            childrenList.Add(gameObject.transform.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// 把目标的世界坐标转换为局部坐标
    /// </summary>
    /// <param name="rectTransform">物体的RectTranForm或Transform</param>
    /// <param name="targetPos">转换后的坐标</param>
    /// <returns>局部坐标</returns>
    public static Vector3 TransformRectPos(RectTransform rectTransform,Vector3 targetPos)
    {
        Vector3 targetWorldPosition = rectTransform.TransformPoint(targetPos);

        return targetWorldPosition;
    }

    /// <summary>
    /// UI组件透明度变化
    /// </summary>
    /// <param name="image">图像</param>
    /// <param name="endValue">结束值</param>
    /// <param name="duration">持续时间</param>
    public static void UIFade(Image image,float endValue,float duration)
    {
        //首先将透明度设为0
        Color color = image.color;
        color.a = 0;
        image.color = color;

        image.DOFade(endValue, endValue).SetEase(Ease.InOutSine);
    }
}

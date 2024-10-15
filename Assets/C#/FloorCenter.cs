using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorCenter : MonoBehaviour
{
    public Vector3 floorCenter;

    private void Start()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>();//获取该物体上的碰撞体组件

        if (boxCollider==null)
            return;//碰撞体为空，直接返回
        
        Vector3 localCenter = boxCollider.center;//获取组件的中心点位置
        floorCenter = transform.TransformPoint(localCenter);//将局部坐标转换为世界坐标
    }
}

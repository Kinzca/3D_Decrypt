using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class FloorItem
{
    public bool isCanWalkable;
    public Vector3 center;
    public List<FloorCenter> neighbours;//相邻的物体列表
    
    public float neighbourDis;//该物体到相邻距离的长度
    public float neighbourToTarget;
    public float cost;//判断函数，等于临近方格的距离到，该方格到目标点的距离
    public FloorCenter parentPoint;//父节点，回溯查找列表时使用
}

public class FloorCenter : MonoBehaviour
{
    public FloorItem floorItem;

    private void Start()
    {
        floorItem = new FloorItem();
        floorItem.neighbours = new List<FloorCenter>(); // 初始化 neighbour 列表
        
        JudgeIsWalkable();
        CalculateCenter();
        StartCoroutine(FindFloorsAround());
    }

    private IEnumerator FindFloorsAround()
    {
        yield return null; // 确保其他对象的Start方法已经执行,即等待一段时间，等待初始化完成
        GetFloorsAround();
    }
    
    private void CalculateCenter()
    {
        BoxCollider boxCollider = GetComponent<BoxCollider>(); //获取该物体上的碰撞体组件

        if (boxCollider == null)
            return; //碰撞体为空，直接返回

        Vector3 localCenter = boxCollider.center; //获取组件的中心点位置
        floorItem.center = transform.TransformPoint(localCenter); //将局部坐标转换为世界坐标
    }

    private void JudgeIsWalkable()
    {
        floorItem.isCanWalkable = gameObject.CompareTag("WalkableFloor");
    }

    private void GetFloorsAround()
    {
        CheckAndAddNeighbour(transform.forward);
        CheckAndAddNeighbour(-transform.forward);
        CheckAndAddNeighbour(-transform.right);
        CheckAndAddNeighbour(transform.right);
    }

    private void CheckAndAddNeighbour(Vector3 direction)
    {
        float distance = 2.5f;
        var posCenter = new Vector3(floorItem.center.x, floorItem.center.y - 0.1f, floorItem.center.z);//将地板中心向下移动，以免遮挡到其他物体
        
        // 在场景视图中绘制射线
        #if UNITY_EDITOR
        Debug.DrawRay(posCenter, direction * distance, Color.red, 5f);
        #endif
        
        if (Physics.Raycast(transform.position, direction, out RaycastHit hit, distance))
        {
            var neighbourCenter = hit.collider.gameObject.GetComponent<FloorCenter>();
            floorItem.neighbours.Add(neighbourCenter);
        }
    }

}

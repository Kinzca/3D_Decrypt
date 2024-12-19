using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    private GameObject[] _objectsInScene;
    private List<GameObject> _activeFalse;
    private List<GameObject> _activeTrue;
    
    // Start is called before the first frame update
    void Start()
    {
        _activeFalse = new List<GameObject>();
        _activeTrue = new List<GameObject>();
        
        FindObjectsOfTag("Object");
        
        //开始时将所有物品的加入到Active列表中
        foreach (var item in _objectsInScene)
        {
            _activeTrue.Add(item);
        }
        
        //添加事件
        EventCenter.AddListener<int,Vector3>(EventType.SetActiveTrue,SetActiveTrue);
        EventCenter.AddListener<int>(EventType.SetActiveFalse,SetActiveFalse);
    }

    //查找场景中所有物体的，并添加到列表
    private void FindObjectsOfTag(string tag)
    {
        _objectsInScene = GameObject.FindGameObjectsWithTag(tag);

        foreach (var objectInScene in _objectsInScene)
        {
            Debug.Log("场景中的所有可交互的物体名为" + objectInScene);
        }
    }

    //设置物体关闭
    private void SetActiveFalse(int id)
    {
        SeekAndSetActive(_activeTrue, id, false);
    }

    //设置物体开启
    private void SetActiveTrue(int id, Vector3 pos)
    {
        //如果 _activeFalse 列表为空，提前返回
        if (_activeFalse.Count == 0)
        {
            Debug.LogWarning("SetActiveTrue: _activeFalse 列表为空，无法查找物品！");
            return;
        }

        //遍历 _activeFalse，尝试找到对应 ID 的物体
        for (int i = 0; i < _activeFalse.Count; i++)
        {
            var item = _activeFalse[i];
            if (item.GetComponent<Item>().id == id)
            {
                Debug.Log($"SetActiveTrue: 找到关闭状态的物体 {item.name}，设置位置并激活");
                item.transform.position = pos;

                SeekAndSetActive(_activeFalse, id, true);
                return;
            }
        }

        Debug.LogWarning($"SetActiveTrue: _activeFalse 列表中未找到 ID 为 {id} 的物体");
    }

    
    /// <summary>
    /// 设置物体Active状态
    /// </summary>
    /// <param name="objects">物体列表</param>
    /// <param name="id">物体id</param>
    /// <param name="trueOrFalse">物体状态</param>
    private void SeekAndSetActive(List<GameObject> objects, int id, bool trueOrFalse)
    {
        for (int i = 0; i < objects.Count; i++)
        {
            var item = objects[i];
            if (item == null) continue; // 确保物体存在

            var itemId = item.GetComponent<Item>().id;

            if (itemId == id)
            {
                // 更新物体的 Active 状态
                item.SetActive(trueOrFalse);

                if (trueOrFalse)
                {
                    _activeFalse.Remove(item);
                    _activeTrue.Add(item);
                    Debug.Log("添加到开启列表的物品名为: " + item.name);
                }
                else
                {
                    _activeTrue.Remove(item);
                    _activeFalse.Add(item);
                    Debug.Log("添加到关闭列表的物品名为: " + item.name);
                }

                // 找到后立即返回，防止后续操作
                return;
            }
        }

        Debug.LogWarning("未找到 ID 匹配的物体，无法设置 Active 状态: " + id);
    }

    
    private void OnDestroy()
    {
        EventCenter.RemoveListener<int,Vector3>(EventType.SetActiveTrue,SetActiveTrue);
        EventCenter.RemoveListener<int>(EventType.SetActiveFalse,SetActiveFalse);
    }
}

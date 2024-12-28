using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SceneManager : MonoBehaviour
{
    private GameObject[] _objectsInScene;
    private List<GameObject> _activeFalse;
    private List<GameObject> _activeTrue;
    public List<GameObject> newComposite;//新合成物品列表

    private bool _isNeedComposite = false;//是否需要合成标记
    
    private void OnEnable()
    {
        //添加事件
        EventCenter.AddListener<int,Vector3>(EventType.SetActiveTrue,SetActiveTrue);
        EventCenter.AddListener<int>(EventType.SetActiveFalse,SetActiveFalse);
    }

    // Start is called before the first frame update
    void Start()
    {
        _activeFalse = new List<GameObject>();
        _activeTrue = new List<GameObject>();
        
        FindObjectsOfTag("Object");
        
        //开始时将所有物品的加入到Active列表中
        foreach (var item in _objectsInScene)
        {
            Debug.Log("开始时Active列表的物体为："+item.name);
            _activeTrue.Add(item);
        }
    }

    //查找场景中所有激活的物体的，并添加到Active列表
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
        Debug.Log("设置物品状态false");
        SeekAndSetActive(_activeTrue, id, false);
    }

    //设置物体开启
    private void SetActiveTrue(int id, Vector3 pos)
    {
        //默认不需要合成
        _isNeedComposite = false;
        
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
            SeekFormID(item,id,pos);

            if (i == _activeFalse.Count)
            {
                _isNeedComposite = true;
            }
        }

        //如果不需要合成则直接返回
        if (_isNeedComposite) 
        {
            return;
        }
        
        //入过未找到物体，则说明是新合成得物体那么直接实例化这个物体
        Debug.Log($"SetActiveTrue: _activeFalse 列表中未找到 ID 为 {id} 的物体");
        
        for (int i = 0; i < newComposite.Count; i++)
        {
            var item = newComposite[i];
            
            //比对合成表中得物体id值，与传入ID值相同则实例化该物体
            if (item.GetComponent<Item>().id == id)
            {
                Debug.Log($"在列表中找到ID为 {id} 的新合成物体");
                
                //生成时先添加到False列表
                StartCoroutine(LoadPrefabAsync(item.GetComponent<Item>().prefabPath, pos));
                
                //将新物体添加到场景物体数组,扩展 _objectsInScene 数组，增加一个元素
                Array.Resize(ref _objectsInScene, _objectsInScene.Length + 1);
                //将新的物体添加到数组中
                _objectsInScene[^1] = item;

            }else if (i == newComposite.Count) //如果到结尾都没有倒找合成列表中的物体
            {
                Debug.Log("在合成列表中没有找到对应ID的物体");
            }
        }
    }

    /// <summary>
    /// 从关闭列表中设置物体状态和位置
    /// </summary>
    /// <param name="item">需要设置得物品</param>
    /// <param name="id">需要比较得ID值</param>
    /// <param name="pos">设置得目标位置</param>
    private void SeekFormID(GameObject item,int id,Vector3 pos)
    {
        if (item.GetComponent<Item>().id == id)
        {
            Debug.Log($"SetActiveTrue: 找到关闭状态的物体 {item.name}，设置位置并激活");
            item.transform.position = pos;

            SeekAndSetActive(_activeFalse, id, true);
        }
    }

    private IEnumerator LoadPrefabAsync(string prefabPath, Vector3 pos)
    {
        ResourceRequest loadRequest = Resources.LoadAsync<GameObject>(prefabPath);
    
        yield return loadRequest; // 等待加载完成
    
        GameObject prefab = loadRequest.asset as GameObject;
    
        if (prefab != null)
        {
            Debug.Log("预制体加载完成，实例化中...");
            GameObject instance = Instantiate(prefab, pos, Quaternion.identity);
            instance.SetActive(true); // 确保生成时是关闭状态
            _activeTrue.Add(instance); // 添加到关闭列表
            Debug.Log("预制体实例化并加入开启列表：" + instance.name);
        }
        else
        {
            Debug.LogError("无法加载预制体: " + prefabPath);
        }
    }

    
    /// <summary>
    /// 设置物体Active状态
    /// </summary>
    /// <param name="objects">物体列表</param>
    /// <param name="id">物体id</param>
    /// <param name="trueOrFalse">物体状态</param>
    private void SeekAndSetActive(List<GameObject> objects, int id, bool trueOrFalse)
    {
        Debug.Log("进入设置玩家位置方法");
        
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

    
    private void OnDisable()
    {
        EventCenter.RemoveListener<int,Vector3>(EventType.SetActiveTrue,SetActiveTrue);
        EventCenter.RemoveListener<int>(EventType.SetActiveFalse,SetActiveFalse);
    }
}

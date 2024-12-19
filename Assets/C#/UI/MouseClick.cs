using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class MouseClick : MonoBehaviour
{
    private void Update()
    {
        //如果按下左键
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("玩家按下左键");
            Click();
        }
    }

    private void Click()  
    {  
        if (EventSystem.current.IsPointerOverGameObject())  
        {  
            Debug.Log("点击到了UI，不进行移动");  
            EventCenter.Broadcast(EventType.ClickUI, true);  
            return; // 如果是UI，不再进行射线检测  
        }  

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);  
        RaycastHit hitInfo;  

        if (Physics.Raycast(ray, out hitInfo))  
        {  
            if (hitInfo.collider.CompareTag("UIPlane"))  
            {  
                Debug.Log("点击到了UI，不进行移动");  
                EventCenter.Broadcast(EventType.ClickUI, true);  
            }  
            else  
            {  
                EventCenter.Broadcast(EventType.ClickUI, false);  
            }  
        }  
    }
}

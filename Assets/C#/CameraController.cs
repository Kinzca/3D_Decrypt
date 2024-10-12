using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject player;

    [Header("摄像机初始值")]
    public float offsetZ;
    public float offsetY;
    public float rotationOffsetX;

    [Header("摄像机旋转")] 
    public float rotationSpeed;  // 旋转速度
    public float rotationAngle; // 单次旋转角度
    private int _rotationToward = 0;
    private float _currentRotation = 0f;  // 当前累计旋转的角度
    private float _targetRotation = 0f;  //目标旋转角度

    private void LateUpdate()
    {
        CameraRotation();
        CameraFollow();
    }

    //摄像机跟随
    private void CameraFollow()
    {
        if (player != null)
        {
            Vector3 offset = new Vector3(0, offsetY, offsetZ);//原偏移量
            offset = Quaternion.Euler(0, _currentRotation, 0) * offset; // 根据旋转后的角度计算偏移

            Vector3 targetPosition = player.transform.position + offset;//计算摄像机的偏移量
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 5f);//应用移动！！！

            Vector3 directionToPlayer = player.transform.position - transform.position;//计算摄像机和玩家的矢量方向
            Quaternion quaternion = Quaternion.LookRotation(directionToPlayer);//计算摄像机旋转的矢量方向
            Quaternion targetRotation = quaternion * Quaternion.Euler(rotationOffsetX, 0, 0);//应用x轴上的偏移
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * 5f);//应用旋转
        }
    }

    //摄像机旋转
    private void CameraRotation()
    {
        if (player != null) 
        {
            // 处理输入，只在按下按键时设定旋转方向
            if (Input.GetButtonDown("LeftRotation"))
            {
                _rotationToward = -1;
                _targetRotation += rotationAngle;
            }
            else if (Input.GetButtonDown("RightRotation"))
            {
                _rotationToward = 1;
                _targetRotation -= rotationAngle;
            }

            // 计算旋转，防止旋转超过指定的角度
            if (!Mathf.Approximately(_currentRotation, _targetRotation)) // 当当前角度还未达到目标角度时继续旋转
            {
                float step = rotationSpeed * Time.deltaTime * 100f;
                _currentRotation = Mathf.MoveTowards(_currentRotation, _targetRotation, step); // 平滑旋转，直到到达目标角度
            }
        }
    }
}

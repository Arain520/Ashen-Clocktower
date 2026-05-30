using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParallaxBackground : MonoBehaviour
{
    //创建相机对象
    private Transform cam;

    //相机移动的效果
    [SerializeField] private float parallaxEffect;

    private float xPosition;

    void Start()
    {
        //赋值相机对象
        if (Camera.main != null)
        {
            cam = Camera.main.transform;
        }
        else
        {
            GameObject foundCamera = GameObject.Find("MainCamera");

            if (foundCamera == null)
                foundCamera = GameObject.Find("Main Camera");

            if (foundCamera != null)
                cam = foundCamera.transform;
        }

        //初始化为该脚本对应目标的x坐标
        xPosition = transform.position.x;
    }

    void Update()
    {
        if (cam == null)
            return;
        float distanceToMove = cam.position.x * parallaxEffect;

        transform.position = new Vector3(xPosition + distanceToMove, transform.position.y);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PackageGrid : MonoBehaviour
{
    public float delayTime;
    private Image _image;

    private void Start()
    {
        _image = gameObject.GetComponent<Image>();
        
        PackageGridInit();
    }

    private void PackageGridInit()
    {
        StartCoroutine(WaitForTime(delayTime));
    }

    private void GridFade()
    {
        Utility.UIFade(_image,1f,1f);
    }

    private IEnumerator WaitForTime(float time)
    {
        yield return new WaitForSeconds(time);
    }
}

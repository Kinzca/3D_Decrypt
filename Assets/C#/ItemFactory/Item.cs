using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Item : MonoBehaviour
{
    public int id;
    public string itemName;
    public string imagePath;//图标路径
    public string prefabPath;//预制体路径
    public string description;
    public int weight;
}


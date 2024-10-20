#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif
using System.Collections.Generic;
using UnityEngine;

public class GridLayoutGroup3D : MonoBehaviour
{
#if UNITY_EDITOR
    public Vector3 matrixInterval = new Vector3(0.25f, 0.25f, 0.25f);
    public Vector3Int matrixSize = new Vector3Int(3, 3, 3);
    public Color matrixColor = new Color(255 / 225f, 225 / 225f, 0, 100 / 255f);
    public float radius = 0.1f;
    private int ChildIndex = 0;
    public List<Location> locations = new List<Location>();
    //子物体间隔
    public Vector3 MatrixInterval
    {
        get => matrixInterval; set
        {
            matrixInterval = value;
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        }
    }
    //矩阵大小
    public Vector3Int MatrixSize
    {
        get => matrixSize; set
        {
            matrixSize = value;
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        }
    }

    public float Radius
    {
        get => radius; set
        {
            radius = value;
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        }
    }
    public Color MatrixColor
    {
        get => matrixColor; set
        {
            matrixColor = value;
            UnityEditor.EditorApplication.QueuePlayerLoopUpdate();
        }
    }
    private void OnDrawGizmosSelected()
    {
        DrawAndSetLocation();
    }

    public void DrawAndSetLocation()
    {
        ChildIndex = 0;
        int MatrixSizeX = Mathf.Abs(MatrixSize.x);
        int MatrixSizeY = Mathf.Abs(MatrixSize.y);
        int MatrixSizeZ = Mathf.Abs(MatrixSize.z);

        //刷新子物体的位置
        for (int j = 0; j < MatrixSizeY; j++)
        {
            for (int z = 0; z < MatrixSizeZ; z++)
            {
                for (int i = 0; i < MatrixSizeX; i++)
                {
                    Vector3 CurLoc = new Vector3(
                        MatrixSize.x > 0 ? i : -i,
                        MatrixSize.y > 0 ? j : -j,
                        MatrixSize.z > 0 ? -z : z
                        );
                    Vector3 Location = new Vector3(
                        CurLoc.x * MatrixInterval.x,
                        CurLoc.y * MatrixInterval.y,
                        CurLoc.z * MatrixInterval.z
                        );
                    if (ChildIndex < transform.childCount)
                    {
                        if (transform.GetChild(ChildIndex).localPosition != Location)
                        {
                            transform.GetChild(ChildIndex).localPosition = Location;
                            EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                        }
                    }
                    Gizmos.color = MatrixColor;
                    Gizmos.matrix = transform.localToWorldMatrix;
                    Gizmos.DrawCube(Location, Radius * Vector3.one);
                    ChildIndex++;
                }
            }
        }
    }
    public void CopyInfo()
    {
        if (locations.Count == 0)
        {
            foreach (var item in GetComponentsInChildren<Transform>())
            {
                locations.Add(new Location(item.name, item.position, item.rotation));
            }
        }
    }
    public string PasteInfo()
    {
        Transform[] transforms = GetComponentsInChildren<Transform>();
        if (transforms.Length != locations.Count)
        {
            return "子物体数量不对等！停止赋值！";
        }
        for (int i = 0; i < transforms.Length; i++)
        {
            if (transforms[i].name == locations[i].name)
            {
                transforms[i].position = locations[i].position;
                transforms[i].rotation = locations[i].rotation;
            }
            else
            {
                return "子物体" + locations[i].name + "有变动！停止赋值！";
            }
        }
        return "赋值成功！";
    }
    public class Location
    {
        public string name;
        public Vector3 position;
        public Quaternion rotation;

        public Location(string name, Vector3 position, Quaternion rotation)
        {
            this.name = name;
            this.position = position;
            this.rotation = rotation;
        }
    }
#endif
}
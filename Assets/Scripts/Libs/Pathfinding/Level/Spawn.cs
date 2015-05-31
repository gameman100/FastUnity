using UnityEngine;
using System.Collections;

/// <summary>
/// 主角出生点
/// </summary>
public class Spawn : IDObject
{
    [HideInInspector]
    public int zone_id = 1;
    public float radius = 2.0f;
    public Transform getTransform = null;

    void Awake()
    {
        getTransform = this.transform;
        Vector3 pos = getTransform.position;
        //pos.y = 0;
        getTransform.position = pos;
    }

}

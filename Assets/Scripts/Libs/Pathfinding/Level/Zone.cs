using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 小关
/// </summary>
public class Zone : IDObject
{
    /// <summary>
    /// 该值当前没什么用?
    /// </summary>
    public int transporter_id = 0;
    /// <summary>
    /// 默认摄像机角度
    /// </summary>
    public Vector3 cameraAngle = new Vector3(37,-50,0);

    public static GameObject Create()
    {
        GameObject go = new GameObject();
        go.name = "zone";
        go.AddComponent<Zone>();

        return go;
    }
}

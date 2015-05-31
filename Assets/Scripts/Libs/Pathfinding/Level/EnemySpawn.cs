using UnityEngine;
using System.Collections;

public class EnemySpawn : Spawn {

    public int group = -1;

    private bool isInit = false;
    /// <summary>
    /// 创建EnemySpawn
    /// </summary>
    public static GameObject Create()
    {
        GameObject go = new GameObject();
        go.name = "EnemySpawn";
        go.AddComponent<EnemySpawn>();

        return go;
    }

    public void Init()
    {
        if (isInit)
            return;
        isInit = true;
    }


    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "enemy.PNG", true);

        Gizmos.color = new Color(1, 0, 0);

        Gizmos.DrawRay(transform.position, transform.TransformDirection(0, 0, 1));
    }
}

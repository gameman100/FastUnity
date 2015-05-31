using UnityEngine;
using System.Collections;

public class PlayerSpawn : Spawn {

    /// <summary>
    /// 创建Spawn
    /// </summary>
    public static GameObject Create()
    {
        GameObject go = new GameObject();
        go.name = "PlayerSpawn";
        go.AddComponent<PlayerSpawn>();

        return go;
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "player.PNG", true);

        Gizmos.color = new Color(1, 0, 0);

        Gizmos.DrawRay(transform.position, transform.TransformDirection(0, 0, 1));
        //Gizmos.DrawWireSphere(this.transform.position, radius);

    }

}
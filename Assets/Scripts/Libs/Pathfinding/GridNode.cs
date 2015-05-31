using UnityEngine;
using System.Collections;

/// <summary>
/// [deprecated]
/// </summary>
[AddComponentMenu("Game/Pathfinding/GridNode")]
public class GridNode : MonoBehaviour
{
    /// <summary>
    /// 节点大小
    /// </summary>
    public int m_NodeSize = 0;
    /// <summary>
    /// 是否堵塞
    /// </summary>
    public bool m_IsBlocked = true;


	// Use this for initialization
	void Start () {
	
	}

    void OnDrawGizmos()
    {
        Gizmos.DrawIcon(transform.position, "guanbi.png");
    }
}

using UnityEngine;
using System.Collections;

/// <summary>
/// [deprecated]
/// </summary>
[AddComponentMenu("Game/Pathfinding/GridNode")]
public class GridNode : MonoBehaviour
{
    /// <summary>
    /// �ڵ��С
    /// </summary>
    public int m_NodeSize = 0;
    /// <summary>
    /// �Ƿ����
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

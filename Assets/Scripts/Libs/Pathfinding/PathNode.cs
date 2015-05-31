using UnityEngine;
using System.Collections;

public class PathNode {

    protected PathMap m_map;
    public PathMap map { get { return m_map; }}
    // 父节点
    protected PathNode m_parent = null;
    public PathNode parent { get { return m_parent; } }
    protected int m_parentCount = 0;
    public int parentCount { get { return m_parentCount; } }

    // 节点的坐标位置
    public PathVector3 position = new PathVector3();

    // 节点所在的Tile位置
    protected int m_tx = 0, m_tz = 0;
    public int tx { get { return m_tx; } }
    public int tz { get { return m_tz; } }

    // 花费
    public float cost = 0;

    /// <summary>
    /// 从当前节点到起始点的花费 
    /// </summary>
    public float fromStart = 0;

    /// <summary>
    /// 从当前节点到目标点的花费
    /// </summary>
    public float heuristic = 0;

    public enum stateID
    {
        NULL = 0,
        OPEN,
        CLOSE,
    }
    // 当前节点的状态
    public stateID state = stateID.NULL;

    public PathNode( PathMap map_ )
    {
        m_map = map_;
    }

    public void SetParent(PathNode parentnode)
    {
        m_parent = parentnode;
        m_parentCount = parentnode.parentCount + 1;
    }

    /// <summary>
    /// 设置当前节点的坐标位置
    /// </summary>
    /// <param name="ix">0 base 地图节点X</param>
    /// <param name="iz">0 base 地图节点Z</param>
    public void SetPosition(int ix, int iz )
    {
        m_tx = ix;
        m_tz = iz;
        position.x = m_map.startx + ix * m_map.QUAD_SIZE + m_map.QUAD_SIZE * 0.5f;
        position.y = 0;
        position.z = m_map.startz + iz * m_map.QUAD_SIZE + m_map.QUAD_SIZE * 0.5f;
    }

    public void SetPosition( PathVector3 v )
    {
        m_tx = v.tx(m_map);
        m_tz = v.tz(m_map);
        position.x = v.x;
        position.y = v.y;
        position.z = v.z;
       
    }

    public void CenteredPosition()
    {
        position.x = m_map.startx + m_tx * m_map.QUAD_SIZE + m_map.QUAD_SIZE * 0.5f;
        position.y = 0;
        position.z = m_map.startz + m_tz * m_map.QUAD_SIZE + m_map.QUAD_SIZE * 0.5f;

    }

    /// <summary>
    /// 计算当前节点的花费
    /// </summary>
    /// <param name="origin">一个长度为3的数组保存起始点的X Y Z三个坐标位置</param>
    /// <param name="target">一个长度为3的数组保存目标点的X Y Z三个坐标位置</param>
    /// <returns>返回花费,值越大表示这个节点的代价越高</returns>
    public float SetCost(PathVector3 origin, PathVector3 destination, float extraCost )
    {
        int ox = origin.tx(map);
        int oz = origin.tz(map);

        int dx = destination.tx(map);
        int dz = destination.tz(map);

        //fromStart = (float)(System.Math.Abs(tx - orx) + System.Math.Abs(tz - orz)) + extraCost;
        fromStart = parentCount + extraCost;
        heuristic = (float)(System.Math.Abs(tx - dx) + System.Math.Abs(tz - dz));
        cost = fromStart + heuristic;

        return cost;
    }

    public bool CompareTo( PathNode node )
    {
        if (tx == node.tx && tz == node.tz)
            return true;

        return false;
    }

}

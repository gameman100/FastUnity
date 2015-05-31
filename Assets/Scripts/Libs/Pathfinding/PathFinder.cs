using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 寻路器
/// </summary>
public class PathFinder
{
    /// <summary>
    /// 地图信息
    /// </summary>
    protected PathMap m_map=null;
    public PathMap map { get { return m_map; } }

    /// <summary>
    /// 存储路径节点
    /// </summary>
    protected List<PathNode> m_PathNodes = new List<PathNode>();
    public int NodesCount{ get{return m_PathNodes.Count;}}
    public PathVector3 DestNodePosition { get { return m_PathNodes.Count==0? null: m_PathNodes[0].position; } }

    /// <summary>
    /// 当前位置
    /// </summary>
    private PathVector3 m_currentPos = new PathVector3();
    public PathVector3 currentPos { get { return m_currentPos; } }

    /// <summary>
    /// 目标位置
    /// </summary>
    private PathVector3 m_destination = new PathVector3();
    public PathVector3 destination{ get{return m_destination;}}

    /// <summary>
    /// 上一个路点的位置
    /// </summary>
    private PathVector3 m_lastNodePos = new PathVector3();
 
    /// <summary>
    /// 是否可以阻塞格子
    /// </summary>
    private bool m_isBlock = true;
    public bool isBlock { get { return m_isBlock; } }

    /// <summary>
    /// 寻路实体的压力大小，当这个值为0时，对周围不形成任何压力，这个值最大为4，格子的压力与自身的大小的和超过或达到5，则不能进入
    /// </summary>
    protected int m_size = 0;
    public int entitySize{get{return m_size;}}

    /// <summary>
    /// Group ID
    /// </summary>
    private int m_DynamicID = 0;
    public int DynamicID
    {
        set
        {
            m_DynamicID = value;
        }

        get
        {
            return m_DynamicID;
        }

    }

    /// <summary>
    /// 移动速度
    /// </summary>
    protected float m_moveSpeed = 1.0f;

    /// <summary>
    /// 当前位置到下一个路点的距离
    /// </summary>
    private float m_distCache=0;


    /// <summary>
    /// 在随机确定一个方向时,避免使用这个方向(前一次走来的路线)
    /// </summary>
    private int m_InvalidDir = -1;
    public int InvalidDir
    {
        get
        {
            return m_InvalidDir;
        }
    }

    /// <summary>
    /// 内部随机数
    /// </summary>
    private int m_InternalCounter = 1;
    private int m_dirCounter = 0;

    /// <summary>
    /// 容差
    /// </summary>
    private float m_Tolerance = 0.001f;

    private bool m_searchCenter = false;
    /// <summary>
    /// 是否处于查找中心点状态
    /// </summary>
    public bool searchCenter {  get { return m_searchCenter; }}
    public void SetSearchCenter(bool search) { m_searchCenter = search; }

    public delegate void VoidDelegate();
    public event VoidDelegate onPathNodeUpdate;

    /// <summary>
    /// 初始化构造函数
    /// </summary>
    /// <param name="pox">位置X</param>
    /// <param name="poz">位置Z</param>
    /// <param name="isblocker">是否是一个占有体</param>
    /// <param name="dynamicid">动态id</param>
    /// <param name="entitySize">大小</param>
    public PathFinder( PathMap map, float pox ,float poz ,bool isblock ,int entitySize ,int dynamicid ,float movespeed )
    {
        m_map = map;

        // 当前位置
        m_currentPos.Set( pox, poz);
        // 目标位置
        m_destination.Set( pox, poz);
        m_lastNodePos.Set( pox, poz );

        m_isBlock = isblock;
        m_size = entitySize;
        m_DynamicID = dynamicid;

        m_moveSpeed = movespeed;
        m_distCache = 0;

        if (m_isBlock)
        {
            m_map.SetBlock( m_currentPos, m_size, dynamicid ,m_moveSpeed);
        }
    }


    /// <summary>
    /// 清除在地图上留下的信息
    /// </summary>
    public void ClearAll()
    {
        if (m_isBlock)
        {
            m_map.RemoveBlock(m_currentPos.x, m_currentPos.z, m_size, m_DynamicID, m_moveSpeed);
        }

        if (m_PathNodes!=null)
            m_PathNodes.Clear();
    }

    /// <summary>
    /// 清空路点
    /// </summary>
    public void ClearNodes()
    {
        m_PathNodes.Clear();
    }

    /// <summary>
    /// 创建到达目标点的路线
    /// </summary>
    /// <param name="origin">当前的位置</param>
    /// <param name="target">目标位置</param>
    /// <param name="maxnodes">最大节点数</param>
    /// <param name="Optimize">是否优化路径</param>
    /// <returns>返回真找到目标点</returns>
    public bool BuildPath(PathVector3 origin, PathVector3 target, int maxnodes, bool Optimize, bool followgroup, bool centeredQuad = true  )
    {
        bool findtarget = false;

        target = CheckDenstination(origin, target);


        // 初始化Open count
        int OpenCount = 0;

        // 当前要查找的节点
        PathNode FNode = new PathNode(m_map);
        // 目标节点
        PathNode targetNode = new PathNode(m_map);
        targetNode.SetPosition(target);
        if (centeredQuad)
        {
            targetNode.CenteredPosition();
            target.center(m_map);
        }

        // 保存当前位置
        m_currentPos.Set(origin);
         
        // 保存目标点位置
        m_destination.Set(targetNode.position);

        // 取得起始和结束Tile位置
        int sx = origin.tx(m_map);
        int sz = origin.tz(m_map);

        // 初始化 NodeList
        m_PathNodes.Clear();

        // 如果起始和结束位置一样
        if (sx == targetNode.tx && sz == targetNode.tz )
        {
            if (PathHelper.Distance3(origin, targetNode.position) > m_Tolerance )
            {
                PathNode node = new PathNode(m_map);
                node.SetPosition(targetNode.position);

                m_PathNodes.Add(node);

                CacheDistance(origin, targetNode.position);

                return true;
            }
            else
                return false;
        }

        // 创建第一个路点(原点)
        PathNode root = new PathNode(m_map); // 这个节点的位置和origin不同
        root.SetPosition(sx, sz);
        root.state = PathNode.stateID.OPEN;
        root.SetCost(origin, targetNode.position, 0);

        // 将起始点加入检查表
        m_PathNodes.Add(root);
        OpenCount++;

        // 暂时关掉block
        //if (m_isBlock)
        //{
        //    m_map.RemoveBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);
        //}

        // 1.检测所有节点,创建路点表
        while (OpenCount > 0 && m_PathNodes.Count < maxnodes)
        {
            // 检查效率最高的路点
            PathNode temp = null;
            foreach (PathNode pn in m_PathNodes)
            {
                if (pn.state == PathNode.stateID.CLOSE)
                    continue;

                if (temp == null)
                {
                    temp = pn;
                    continue;
                }

                if (pn.cost < temp.cost)
                {
                    temp = pn;
                }
                else if (pn.cost == temp.cost)
                {
                    if (m_map.GetDynamic(pn.tx, pn.tz) > 0 && this.m_DynamicID == m_map.GetDynamic(pn.tx, pn.tz) && followgroup)
                        temp = pn;
                }
            }

            if (temp != null)
            {
                FNode = temp;
                FNode.state = PathNode.stateID.CLOSE;
                OpenCount--;
            }
            else
            {
                break;
            }

            // 检查周边路点(4)
            int nx = FNode.tx;
            int nz = FNode.tz;
            for (int cx = nx - 1; cx < nx + 2; cx++)
            {
                for (int cz = nz - 1; cz < nz + 2; cz++)
                {
                    // 不计算当前节点
                    if (nx == cx && nz == cz)
                        continue;

                    // 不计算越界节点
                    if (cx < 0 || cz < 0 || cx >= m_map.SizeX || cz >= m_map.SizeZ)
                        continue;

                    float extraCost = 0;//如果格子是被移动体占有,虽然可以看作可以通过,但会产生额外的花费

                    // 斜角的四个格
                    if (cx != nx && cz != nz)
                    {
                        extraCost = 0.4f;

                        int tempx = 0, tempz = 0, tempx1 = 0, tempz1 = 0;

                        if (cx > nx && cz > nz) //右上角
                        {
                            tempx = nx + 1;
                            tempz = nz;
                            tempx1 = nx;
                            tempz1 = nz + 1;

                        }
                        else if (cx > nx && cz < nz) // 右下角
                        {
                            tempx = nx + 1;
                            tempz = nz;
                            tempx1 = nx;
                            tempz1 = nz - 1;
                        }
                        else if (cx < nx && cz < nz) // 左下角
                        {
                            tempx = nx - 1;
                            tempz = nz;
                            tempx1 = nx;
                            tempz1 = nz - 1;
                        }
                        else if (cx < nx && cz > nz) // 左上角
                        {
                            tempx = nx;
                            tempz = nz + 1;
                            tempx1 = nx - 1;
                            tempz1 = nz;
                        }

                        // o o o
                        // o x !   // p表示当前角色, 如果x不可通过, !也不可通过
                        // o p x
                        if (m_map.IsBlocked(tempx, tempz, m_size) || m_map.IsBlocked(tempx1, tempz1, m_size))
                        {
                            continue;
                        }
                    }

                    // 如果这个节点是死节点,并不是移动体,退出这个节点,进行下一个节点的检查
                    if (m_map.IsBlocked(cx, cz, m_size))
                    {
                        // 如果是目标点
                        if (cx == targetNode.tx && cz == targetNode.tz)
                        {
                            findtarget = true;
                            goto _FinishChecking;

                        }

                        int dynamicid = m_map.GetDynamic(cx, cz);

                        // 如果节点是静态的，退出这个节点的检查
                        if (dynamicid == 0)
                            continue;
                        else if (System.Math.Abs(this.DynamicID) != System.Math.Abs(dynamicid)) // 如果id不同，退出检查
                        {
                            continue;
                        }

                        // 如果跟随
                        if (followgroup)
                        {
                            if (dynamicid < 0) // 如果是暂时不移动的，退出检查
                            {
                                continue;
                            }
                            else
                            {
                                // 如果当前速度大于这个格子的速度，增加额外开销
                                if (this.m_moveSpeed > m_map.GetSpeed(cx, cz))
                                {
                                    extraCost = 5.0f;
                                }
                                else
                                    extraCost = 2.0f;
                            }
                        }
                        else
                            continue;
                       
                    }

                    //检查是否是已存在路点
                    bool isExist = false;
                    foreach (PathNode pn in m_PathNodes)
                    {
                        if (pn.tx == cx && pn.tz == cz)
                        {
                            isExist = true;

                            if (pn.state == PathNode.stateID.OPEN)
                            {
                                if (pn.fromStart > FNode.fromStart + 1)
                                {
                                    pn.SetParent(FNode);
                                    pn.SetCost(origin, targetNode.position, extraCost);
                                }
                            }
                            break;
                        }
                    }
                    if (isExist)
                    {
                        continue;
                    }

                    //创建新OPEN节点
                    PathNode checknode = new PathNode(m_map);
                    checknode.SetPosition(cx, cz);
                    checknode.SetParent(FNode);
                    checknode.state = PathNode.stateID.OPEN;
                    checknode.SetCost(origin, targetNode.position, extraCost);

                    //加入OPEN表
                    m_PathNodes.Add(checknode);
                    OpenCount++;

                    //如果找到目标路点
                    if (cx == targetNode.tx && cz == targetNode.tz)
                    {
                        FNode = checknode;
                        checknode.SetPosition(targetNode.position);
                        m_destination.Set(checknode.position);
                        findtarget = true;
                        goto _FinishChecking;

                    }

                } //for (int z = nz - 1; z < nz+2; z++)
            }//for (int x = nx - 1; x < nx+2; x++)
        }// while

        _FinishChecking:

        //如果找不到终点,找出效率最高的节点作为目标节点
        
        if ( !findtarget )
        {
            PathNode temp = null;
            bool valid = m_map.CheckValid(target);
            foreach (PathNode pn in m_PathNodes)
            {
                if (!m_map.CheckValid(pn.tx, pn.tz))
                    continue;
                if (temp == null)
                {
                    temp = pn;
                    continue;
                }
                if (pn.heuristic < temp.heuristic)
                {
                    temp = pn;
                }
                /*

                if (!valid)
                {
                    if (pn.heuristic < temp.heuristic)
                    {
                        temp = pn;
                    }
                }
                else
                {
                    if (pn.cost < temp.cost)
                    {
                        temp = pn;
                    }
                }
                */
            } // foreach (PathNode pn in CheckedNode)

            if (temp != null)
            {
                FNode = temp;
            }
        }
        
        // 初始化 (!这里可能需要优化)
        m_PathNodes.Clear();

        // 如果只有一个节点,且这个节点是自己,退出查找
        if (FNode.tx == sx && FNode.tz == sz )
        {
            if ( FNode.tx == targetNode.tx && FNode.tz == targetNode.tz ){
                if (!centeredQuad)
                {
                    m_PathNodes.Add(targetNode);
                    m_destination.Set(targetNode.position);
                }
            }
            
            //if (m_isBlock)
            //{
            //    m_map.SetBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);
            //}
            return findtarget;
        }

        // 如果不对路径进行优化，直接计算出最后路径
        if (!Optimize)
        {
            // 建立网格点表
            while (true)
            {
                if (FNode == root || FNode.parent == null)
                {
                    break;
                }

                m_PathNodes.Add(FNode);
                FNode = FNode.parent;
            }

            if (m_PathNodes.Count > 0)
            {
                PathNode node = m_PathNodes[m_PathNodes.Count - 1];
                //if (node.CompareTo(targetNode))
                //    CacheDistance(origin, targetNode.position);
                //else
                    CacheDistance(origin, node.position);
            }
            else
                findtarget = false;

            //if (m_isBlock)
            //{
            //    m_map.SetBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);
            //}
            return findtarget;
        }
        else
        {
            // 优化路径
            m_PathNodes.Add(FNode);
            int ox = FNode.tx;
            int oz = FNode.tz;

            while (true)
            {
                if (FNode.parent == null || FNode.parent.parent == null)
                {
                    break;
                }

                int nx = FNode.parent.parent.tx;
                int nz = FNode.parent.parent.tz;

                bool isconnect = true;

                int row = nx - ox;
                int col = nz - oz;

                int it = row > 0 ? 1 : -1;
                int kt = col > 0 ? 1 : -1;

                for (int i = nx; i != nx - it * 2; i -= it)
                {
                    for (int k = nz; k != nz - kt * 2; k -= kt)
                    {
                        if ((i == ox && k == oz) || (i == nx && k == nz))
                            continue;

                        if (m_map.IsBlocked(i, k, m_size) == true)
                        {
                            if ( !followgroup || m_map.GetDynamic(i, k) == 0 )
                            {
                                isconnect = false;
                                goto _DisconnectPoint;
                            }
                        }
                    }
                }

            _DisconnectPoint:

                if (isconnect == true)
                {
                    FNode = FNode.parent;
                }
                else
                {
                    if (FNode.parent != null)
                    {
                        ox = FNode.parent.tx;
                        oz = FNode.parent.tz;

                        m_PathNodes.Add(FNode.parent);

                        FNode = FNode.parent;
                    }
                    else
                    {
                        break;
                    }
                }
            }//while


            if (m_PathNodes.Count > 0)
            {
                PathNode node = (PathNode)m_PathNodes[m_PathNodes.Count - 1];
                //if (node.CompareTo(targetNode))
                //{
                //    CacheDistance(origin, targetNode.position);
                //    m_destination.Set(targetNode.position);
                //}
                //else
                //{
                    CacheDistance(origin, node.position);
                    m_destination.Set(node.position);
                //}
            }
            else
                findtarget = false;

            //if (m_isBlock)
            //{
            //    m_map.SetBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);
            //}
        }

        return findtarget;

    }

    /// <summary>
    /// 获得目标节点位置
    /// </summary>
    public PathVector3 GetDestination()
    {
        int count = m_PathNodes.Count;
        if (count <= 0)
        {
            return null;
        }
        else
            return m_PathNodes[count - 1].position;
    }

    /// <summary>
    /// 是否有路径
    /// </summary>
    public bool HasPath()
    {
        int count = m_PathNodes.Count;
        if (count > 0)
            return true;

        return false;
    }

    /// <summary>
    /// 前进到目标节点
    /// </summary>
    /// <param name="entryPos">当前位置</param>
    /// <param name="entryAngleY">当前角度</param>
    /// <param name="movespeed">移动速度</param>
    /// <param name="rotspeed">旋转速度</param>
    /// <returns>可以向下一个节点移动返回真</returns>
    public bool MoveToTargetNode(ref PathVector3 entitypos, ref float entityangley, float movespeed, float rotspeed)
    {
        if (!this.HasPath()) // if there is no any node
            return false;

        // 获得当前目标节点的位置
        PathVector3 nodepos = this.GetDestination();
        if (nodepos == null)
            return false;

        // 复制输入位置
        PathVector3 copypos = new PathVector3();
        copypos.Set(entitypos);
        // 复制输入角度
        float copyangle = entityangley;

        // 旋转实体面向目标
        copyangle = PathHelper.PolarAngle2(copypos, nodepos);
        // 移动
        copypos.x += movespeed * (float)System.Math.Sin(PathHelper.DegToRad * copyangle);
        copypos.z += movespeed * (float)System.Math.Cos(PathHelper.DegToRad * copyangle);

        // 取得当前位置并更新地图
        bool path = this.UpdateMapData(ref copypos);
        //Debug.Log("copypos:" + copypos.x + "," + copypos.z);
        if (!path)
        {
            return false;
        }
        else
        {
            this.UpdatePathNodes(ref copypos);
            //Debug.Log("copypos:" + copypos.x + "," + copypos.z);
            if ( this.NodesCount > 0  )
            {
                entityangley = RotateToTarget(entitypos, copypos, entityangley, rotspeed);
                
            }
            entitypos.Set(copypos);
            //Debug.Log("entitypos:" + entitypos.x + "," + entitypos.z);
           
            return true;
        }

    }

    /// <summary>
    /// 向目标点旋转
    /// </summary>
    /// <param name="entryPos">当前位置</param>
    /// <param name="destination">目标位置</param>
    /// <param name="entryAngleY">当前角度</param>
    /// <param name="speed">旋转速度</param>
    /// <returns>返回旋转角度</returns>
    public float RotateToTarget(PathVector3 entryPos, PathVector3 des, float entryAngleY, float speed)
    {

        float newAngle = entryAngleY;

        // 获得目标角度
        float targetAngle = PathHelper.PolarAngle2(entryPos, des);

        float subangle = PathHelper.DeltaAngle(entryAngleY, targetAngle);

        float varspeed = speed;
        if (subangle < 0)
            varspeed = -varspeed;

        newAngle += varspeed;


        if (speed >= System.Math.Abs(subangle))
        {
            newAngle = targetAngle;
        }

        return newAngle;
    }

    /// <summary>
    /// 实体位置改变更新地图信息
    /// </summary>
    /// <param name="pos">实体的当前坐标位置</param>
    /// <param name="entrySize">实体的体积,这里设为零</param>
    /// <returns>如果路径被断开,返回false</returns>
    public bool UpdateMapData(ref PathVector3 pos  )
    {
        bool isPathContinue = true;

        // 格子不能进入
        if (!m_map.CheckValid(pos))
        {
            Debug.Log("not valid tile");
            pos.Set(m_currentPos);
            return isPathContinue = false;
        }

        int ix = pos.tx(m_map);
        int iz = pos.tz(m_map);

        int cx = m_currentPos.tx(m_map);
        int cz = m_currentPos.tz(m_map);

        // 在当前格子中
        if (ix == cx && iz == cz)
        {
            //Debug.Log("the same tile:" + pos.x +"," + pos.z + ",tile:" + ix +"," +iz +":current tile:" + cx +","+cz );
            m_currentPos.Set(pos);
            return isPathContinue = true;
        }

        if ( m_map.IsFree2(pos.x, pos.z, m_size)  )
        {
            if (m_isBlock)
                m_map.RemoveBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);

            m_currentPos.Set(pos);
            isPathContinue = true;

            if (m_isBlock)
                m_map.SetBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);
        }
        else
        {
            pos.Set(m_currentPos);
            isPathContinue = false;
        }

        return isPathContinue;
    }


    /// <summary>
    /// 查询是否到达目标节点位置,这个操作需要在每个循环中执行
    /// </summary>
    /// <param name="entryPos">一个长度为3的数组保存当前实体所在的坐标位置</param>
    /// <returns>如果实体已经到达目标节点的位置返回true</returns>
    private bool UpdatePathNodes(ref PathVector3 entryPos)
    {
        int count = m_PathNodes.Count;
        if (count <= 0)
            return false;


        float dist = PathHelper.Distance3(entryPos, m_lastNodePos);
        //如果到达目标节点,将这个节点去掉
        if (dist >= m_distCache - m_Tolerance)
        {
            PathVector3 targetpos = m_PathNodes[count - 1].position;
            m_PathNodes.RemoveAt(count - 1);

            if (onPathNodeUpdate != null)
                onPathNodeUpdate();

            count = m_PathNodes.Count;
            if (count >= 1)
            {
                CacheDistance(entryPos, m_PathNodes[count - 1].position);

            }
            else
            {
                if (m_isBlock)
                    m_map.RemoveBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);

                entryPos.Set(targetpos);
                m_currentPos.Set(entryPos);

                if (m_isBlock)
                    m_map.SetBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);
                // 如果到达 [这步会造成与update map data的值不一致，产生严重错误]
                // entryPos.Set(targetpos);
            }

            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// 设置位置
    /// </summary>
    /// <param name="pos">目标位置</param>
    public void WrapPosition(PathVector3 pos, PathMap map_)
    {
        //ClearAll();
        if (m_isBlock)
            m_map.RemoveBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);
        m_map = map_;
        m_currentPos.Set(pos);
        if (m_isBlock)
        {
            m_map.SetBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);
        }
    }

    /// <summary>
    /// 记录从当前节点到下一个节点的距离
    /// </summary>
    private void CacheDistance(PathVector3 current, PathVector3 next)
    {
        m_lastNodePos.Set(current);
        m_distCache = PathHelper.Distance3(current, next);
    }

    // temp
    public PathVector3 CheckDenstination(PathVector3 start, PathVector3 target)
    {
        // 暂时关掉block
        //if (m_isBlock)
        //{
        //    m_map.RemoveBlock(start, m_size, m_DynamicID, m_moveSpeed);
        //}
        PathVector3 des = target;
        int tx = target.tx(m_map);
        int tz = target.tz(m_map);
        // 超出边界
        if (!m_map.CheckValid(target))
        {
            /*
            List<PathNode> nodelist = new List<PathNode>();
            PathNode current = new PathNode(m_map);
            current.SetPosition(tx, tz);
            while (nodelist.Count > 0)
            {
                // 检查当前节点是否可靠
                bool b = m_map.IsBlocked(tx, tz, m_size);
                if (!b)
                {
                    target[0] = tx;
                    target[2] = tz;
                    return target;
                }
                else // 如果不可靠，检查下一个周边8个节点是否可靠
                {
                    nodelist.Remove(current);
                    for (int i = current.tx - 1; i < current.tx + 1; i++)
                        for (int k = current.tz - 1; k < current.tz + 1; k++)
                        {
                            if (i == current.tx && k == current.tz)
                                continue;


                        }
                }
            */
        }
        else if (m_map.IsBlocked2(target.x, target.z, m_size))
        {
            /*
           PathVector3 selecttarget = null;
           float cost = -1;
           int checktime = 2;
           while (checktime > 0)
           {

               for (int i = tx - 1; i < tx + 1; i++)
               {
                   for (int k = tz - 1; k < tz + 1; k++)
                   {
                       if (i == tx && k == tz)
                           continue;
                       if (checktime == 2 && m_map.IsBlocked(i, k, m_size))
                           continue;
                       PathVector3 temp = new PathVector3();
                       temp.Set(i, k, m_map);
                       float nextcost = PathHelper.Distance3Fast(start, temp);

                       if (cost < 0 || nextcost < cost)
                       {
                           if (selecttarget == null)
                               selecttarget = new PathVector3();
                           selecttarget.Set(i, k, m_map);
                           cost = nextcost;
                       }
                   }
               }

               if (selecttarget != null)
               {
                   des.Set(selecttarget);
                   break;
               }
               else
                   checktime--;
           }*/
        }
        // 暂时关掉block
        //if (m_isBlock)
        //{
        //    m_map.SetBlock(start, m_size, m_DynamicID, m_moveSpeed);
        //}
        return des;
    }

    /// <summary>
    /// 查找一个free的格
    /// </summary>
    public static PathVector3 FindFreeBlock(PathVector3 center, int maxareasize, int bodysize, PathMap map_)
    {
        int tx = center.tx(map_);
        int tz = center.tz(map_);
        PathVector3 selecttarget = null;
        if (!map_.IsBlocked(tx, tz, bodysize))
        {
            return center;
        }

        int size = 0;
        while (size < maxareasize)
        {
            for (int i = tx - 1 - size; i <= tx + 1 + size; i += ( 1 + size * 2 ) )
            {
                for (int k = tz - 1 - size; k <= tz + 1 + size; k += (1 + size * 2) )
                {
                    if (map_.IsBlocked(i, k, bodysize))
                        continue;
                    selecttarget = new PathVector3();
                    selecttarget.Set(i, k, map_);
                    break;
                }
                if (selecttarget != null)
                    break;
            }
            if (selecttarget != null)
                break;
            size++;
        }
        return selecttarget;
    }

    // temp
    public PathVector3 FindFreeBlock(PathVector3 center, int maxareasize)
    {
        return FindFreeBlock(center, maxareasize, m_size, m_map);
    }

    #region 未使用，不确定，不稳定的代码

    /// <summary>
    /// 
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="target"></param>
    /// <param name="maxnodes"></param>
    /// <returns></returns>
    public float SearchPath(PathVector3 origin, PathVector3 target, int maxnodes)
    {
        float findcount = -1;
        bool findtarget = false;

        // 初始化 NodeList
        ArrayList m_SearchNodes = new ArrayList();

        // 初始化Open count
        int OpenCount = 0;

        // 当前要查找的节点
        PathNode FNode = new PathNode(m_map);

        // 取得起始和结束Tile位置
        int sx = origin.tx(m_map);
        int sz = origin.tz(m_map);

        int tx = target.tx(m_map);
        int tz = target.tz(m_map);


        // 如果起始和结束位置一样
        if (sx == tx && sz == tz)
        {
            findcount = 0;

            return findcount;
        }


        // 创建第一个路点(原点)
        PathNode root = new PathNode(m_map);
        root.SetPosition(sx, sz);
        root.state = PathNode.stateID.OPEN;
        root.SetCost(origin, target, 0);

        // 将起始点加入检查表
        m_SearchNodes.Add(root);
        OpenCount++;

        // 暂时关掉block
        //if (m_isBlock)
        //{
        //    m_map.RemoveBlock(origin, m_size, m_DynamicID, m_moveSpeed);
        //}

        // 1.检测所有节点,创建路点表
        while (OpenCount > 0 && m_SearchNodes.Count < maxnodes)
        {

            // 检查效率最高的路点
            PathNode temp = null;

            foreach (PathNode pn in m_SearchNodes)
            {
                if (pn.state == PathNode.stateID.CLOSE)
                    continue;

                if (temp == null)
                {
                    temp = pn;
                    continue;
                }

                if (pn.cost < temp.cost)
                {
                    temp = pn;
                }

            }

            if (temp != null)
            {
                FNode = temp;
                FNode.state = PathNode.stateID.CLOSE;
                OpenCount--;

            }
            else
            {
                break;
            }


            // 检查周边路点(4)
            int nx = FNode.tx;
            int nz = FNode.tz;
            for (int cx = nx - 1; cx < nx + 2; cx++)
            {
                for (int cz = nz - 1; cz < nz + 2; cz++)
                {
                    // 不计算当前节点
                    if (nx == cx && nz == cz)
                        continue;

                    // 不计算越界节点
                    if (cx < 0 || cz < 0 || cx >= m_map.SizeX || cz >= m_map.SizeZ)
                        continue;

                    float extraCost = 0;//如果格子是被移动体占有,虽然可以看作可以通过,但会产生额外的花费


                    // 斜角的四个格
                    if (cx != nx && cz != nz)
                    {
                        int tempx = 0, tempz = 0, tempx1 = 0, tempz1 = 0;

                        extraCost = 0.4f;

                        if (cx > nx && cz > nz) //右上角
                        {

                            tempx = nx + 1;
                            tempz = nz;

                            tempx1 = nx;
                            tempz1 = nz + 1;

                        }
                        else if (cx > nx && cz < nz) // 右下角
                        {
                            tempx = nx + 1;
                            tempz = nz;

                            tempx1 = nx;
                            tempz1 = nz - 1;
                        }
                        else if (cx < nx && cz < nz) // 左下角
                        {
                            tempx = nx - 1;
                            tempz = nz;

                            tempx1 = nx;
                            tempz1 = nz - 1;
                        }
                        else if (cx < nx && cz > nz) // 左上角
                        {
                            tempx = nx;
                            tempz = nz + 1;

                            tempx1 = nx - 1;
                            tempz1 = nz;
                        }

                        if (m_map.IsBlocked(tempx, tempz, m_size) || m_map.IsBlocked(tempx1, tempz1, m_size))
                        {
                            continue;
                        }
                    }


                    // 如果找到目标
                    if (cx == tx && cz == tz)
                    {
                        findcount = FNode.fromStart + 1;

                        findtarget = true;

                        goto _FinishChecking;
                    }

                    if (m_map.IsBlocked(cx, cz, m_size) && (m_map.GetDynamic(cx, cz) <= 0 || this.DynamicID != m_map.GetDynamic(cx, cz)))
                    {
                        continue;
                    }


                    //检查是否是已存在路点
                    bool isExist = false;
                    foreach (PathNode pn in m_SearchNodes)
                    {
                        if (pn.tx == cx && pn.tz == cz)
                        {
                            isExist = true;

                            if (pn.state == PathNode.stateID.OPEN)
                            {

                                if (pn.fromStart > FNode.fromStart + 1)
                                {
                                    pn.SetParent(FNode);
                                    pn.SetCost(origin, target, extraCost);

                                }

                            }

                            break;
                        }
                    }
                    if (isExist)
                    {
                        continue;
                    }



                    //创建新节点
                    PathNode checknode = new PathNode(m_map);
                    checknode.SetPosition(cx, cz);
                    checknode.SetParent(FNode);
                    checknode.state = PathNode.stateID.OPEN;
                    checknode.SetCost(origin, target, extraCost);


                    //加入OPEN表
                    m_SearchNodes.Add(checknode);
                    OpenCount++;


                } //for (int z = nz - 1; z < nz+2; z++)
            }//for (int x = nx - 1; x < nx+2; x++)


        }// while

    _FinishChecking:

        //if (m_isBlock)
        //{
        //    m_map.SetBlock(origin, m_size, m_DynamicID, m_moveSpeed);
        //}

        if (!findtarget)
            return -1;


        return findcount;
    }



    /// <summary>
    /// 更新随机值
    /// </summary>
    public void Update()
    {
        this.RandomInternal(); 
    }

    /// <summary>
    /// 内部的随机数1-100000
    /// </summary>
    private void RandomInternal()
    {
        m_InternalCounter++;
        if (m_InternalCounter > 100000)
            m_InternalCounter = 1;
    }

    /// <summary>
    /// 创建路径
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="target"></param>
    /// <param name="maxnodes"></param>
    /// <param name="Optimize"></param>
    /// <returns></returns>
    public bool BuildPathEx(PathVector3 origin, PathVector3 target, int maxnodes, bool Optimize)
    {
        // 正常查找,计算所有动态障碍,组队模式
        bool b = BuildPath(origin, target, maxnodes, Optimize, true);
        if (b)
            return true;


        // 使用优化路径,不计算动态障碍,组队模式
        b = BuildPath(origin, target, maxnodes, false, false);
        if (m_PathNodes.Count < 1)
            return false;



        // 获得最近节点
        //PathNode dirnode = (PathNode)m_PathNodes[m_PathNodes.Count-1];
        PathNode dirnode = null;

        for (int i = m_PathNodes.Count-1; i >=0; i--)
        {
            PathNode pn =(PathNode)m_PathNodes[i];

            if (m_map.GetDynamic(pn.tx, pn.tz) == 0)
                continue;

            if (System.Math.Abs(this.m_DynamicID) == System.Math.Abs(m_map.GetDynamic(pn.tx, pn.tz)))
            {
                dirnode = pn;
                continue;
            }


            dirnode = pn;
            break;
        }
 
        if (dirnode == null)
            return false;

        PathVector3 finaltarget=this.LocateTarget(origin, dirnode.position, 5);

        if (finaltarget == null)
            return false;

        PathVector3 des = new PathVector3(finaltarget.x, finaltarget.y, finaltarget.z);

        // 计算所有动态障碍,计算动态障碍,非组队模式
        b = BuildPath(origin, des, maxnodes, Optimize, true);

        if (b)
        {
            return true;
        }
        else
            return false;

    }


    /// <summary>
    /// 查找合法目标点
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="target"></param>
    /// <param name="offset"></param>
    /// <returns>返回长度为3的实数数组</returns>
    public PathVector3 LocateTarget(PathVector3 origin, PathVector3 target, int offset)
    {
        int tx = origin.tx(m_map);
        int tz = origin.tz(m_map);

        if (!m_map.IsBlocked(tx, tz, m_size) || m_map.GetDynamic(tx,tz)>0)
            return target;

        if (offset == 0)
        {
            offset = 1;
        }

        int MAX_AREA = offset;      // 查找的范围

        PathVector3 newtarget = new PathVector3();   //新的目标位置
        newtarget.Set(target);
        // 暂时去掉阻塞

        //if (m_isBlock)
        //{
        //    m_map.RemoveBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);
        //}


        // 检索范围内所有Tile
        for (int i = 1; i < MAX_AREA+1; i++ )
        {
            float dist = 0;
            for (int dir = -1; dir < 2; dir++)  //不同方向
            {
                if (dir == 0)
                    continue;

                this.TargetDistance(origin, ref newtarget, ref dist,0, tx + i * dir, tz);
                this.TargetDistance(origin, ref newtarget, ref dist,0, tx, tz + i * dir);
                   

            }

            float extra = 0.2f;

            // 检索边角
            for (int k = 1; k < i + 1; k++)
            {
                for (int dir = -1; dir < 2; dir++) //不同方向
                {
                    if (dir == 0)
                        continue;

                    if (k != i)
                    {

                        this.TargetDistance(origin, ref newtarget, ref dist, extra, tx + i * dir, tz + k * dir); // 右上 左下
                        this.TargetDistance(origin, ref newtarget, ref dist, extra, tx + k * dir, tz + i * dir); // 右上 左下

                        this.TargetDistance(origin, ref newtarget, ref dist, extra, tx - i * dir, tz + k * dir); // 左上 右下
                        this.TargetDistance(origin, ref newtarget, ref dist, extra, tx - k * dir, tz + i * dir); // 左上 右下


                    }
                    else
                    {
                        this.TargetDistance(origin, ref newtarget, ref dist, extra, tx + k * dir, tz + k * dir); // 最右上边 最左下边

                        this.TargetDistance(origin, ref newtarget, ref dist, extra, tx - k * dir, tz + k * dir); // 最左上边 最右下边
                    }
                }


            }

            if (newtarget != null)
            {
                goto _findtarget;
            }

        }

        _findtarget:

        //if (m_isBlock)
        //{
        //    m_map.SetBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);
        //}

        return newtarget;
    }

    /// <summary>
    /// 检查与目标之间的测试距离
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="tx"></param>
    /// <param name="tz"></param>
    /// <returns></returns>
    private float TargetDistance(PathVector3 origin, ref PathVector3 newtarget, ref float olddist, float extra, int tx, int tz)
    {
        bool b = m_map.IsBlocked(tx, tz, m_size);
        if (b)
            return -1;

        PathVector3 temppos = new PathVector3(tx, tz);

        float newdist = PathHelper.Distance3(origin, temppos);

        if (olddist == 0 || newdist + extra < olddist)
        {
            olddist = newdist + extra;
            newtarget = new PathVector3();
            newtarget.Set(temppos);

            return olddist;
        }

        return -1;
    }



    /// <summary>
    /// 查找预估目标点
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="target"></param>
    /// <param name="target"></param>
    /// <returns>返回长度为3的实数数组</returns>
    public PathVector3 FindBoundTarget( PathVector3 origin ,int maxlength  )
    {

        int ox = origin.tx(m_map);
        int oz = origin.tz(m_map);

        PathVector3 target=null;
        float[,] temptarget = new float[4,3];
        for (int i = 0; i < 4; i++)
        {
            temptarget[i, 0] = -1;
            temptarget[i, 1] = -1;
            temptarget[i, 2] = -1;
        }


        //查看周边情况
        int dir = -1;

        //if (m_isBlock)
        //{
        //    m_map.RemoveBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);
        //}

        for (int dx = ox - 1; dx < ox + 2; dx++)
        {
            for (int dz = oz - 1; dz < oz + 2; dz++)
            {
                if (dx == ox && dz == oz)
                    continue;
                if (dx != ox && dz != oz)
                    continue;

                dir++; //dir 0-4

                // 加入一些随机性
                // 0==left ,1==down, 2==up ,3==right
                if (dir ==m_InvalidDir)
                    continue;


                if (m_map.IsBlocked(dx, dz, m_size))
                    continue;

                // 探测
                int counter = 0;
                int x = dx, z = dz;
                int lastx = x, lastz = z;

                while (counter < maxlength)
                {
                    counter++;

                    switch (dir)
                    {
                        case 0:
                            x--; // left
                            break;
                        case 1:
                            z--; // down
                            break;
                        case 2:
                            z++; // up
                            break;
                        case 3:
                            x++; // right
                            break;
                    }

                    bool b = m_map.IsBlocked(x, z, m_size);


                    if (b || counter == maxlength )
                    {
                        if (m_map.IsDynamic(x, z))
                        {
                            break;
                        }

                        if (target == null)
                        {
                            target = new PathVector3(lastx, lastz);
                        }


                        temptarget[dir, 0] = target.x;
                        temptarget[dir, 1] = target.y;
                        temptarget[dir, 2] = target.z;

                        break;
                    }

                    lastx = x;
                    lastz = z;
                }
            }
        }

        int lastid=0;
        int tempdir = -1;
        for (int i = 0; i < 4; i++)
        {
            if (i == m_InvalidDir || temptarget[i, 0] < 0)
                continue;

            int newid = (i + 1) % (m_dirCounter + 1);
            if (lastid == 0 || lastid < newid)
            {
                lastid = newid;
                target.x = temptarget[i, 0];
                target.y = temptarget[i, 1];
                target.z = temptarget[i, 2];


                switch (i)
                {
                    case 0:
                        tempdir = 3;
                        break;
                    case 1:
                        tempdir = 2;
                        break;
                    case 2:
                        tempdir = 1;
                        break;
                    case 3:
                        tempdir = 0;
                        break;
                }
            }
        }

        m_InvalidDir = tempdir;


        // 更新随机值
        int ranvar = m_InternalCounter % 2 == 0 ? 1 : 2;
        m_dirCounter = m_InvalidDir + ranvar;
        if (m_dirCounter >= 4)
            m_dirCounter = 0;


        //if (m_isBlock)
        //{
        //    m_map.SetBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);
        //}

        return target;
    }

    /// <summary>
    /// 查找路口
    /// </summary>
    /// <param name="origin">原点</param>
    /// <returns>true 是路口</returns>
    public bool IsCrossRoad(float[] origin )
    {
        int ox = (int)origin[0];
        int oz = (int)origin[2];

        //if (m_isBlock)
        //{
        //    m_map.RemoveBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);
        //}

        //查看周边情况
        int block = 0;
        for (int dx = ox - 1; dx < ox + 2; dx++)
        {
            for (int dz = oz - 1; dz < oz + 2; dz++)
            {
                if (dx == ox && dz == oz)
                    continue;
                if (dx != ox && dz != oz)
                    continue;


                if (dx < 0 || dx >= m_map.SizeX || dz < 0 || dz >= m_map.SizeZ)
                {
                    block++;
                    continue;
                }


                if (m_map.IsBlocked(dx, dz, m_size))
                {
                    block++;
                    continue;
                }
             
            }
        }

        //if (m_isBlock)
        //{
        //    m_map.SetBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);
        //}

            

        if (block < 2)
            return true;
        else
            return false;
    }

    /// <summary>
    /// 寻找逃跑路线
    /// </summary>
    /// <param name="origin">原始位置</param>
    /// <param name="enemy">躲避的对象</param>
    /// <param name="maxlength">探索的格子数量</param>
    /// <returns>返回目标位置,当前位置为null</returns>
    public PathVector3 FindRunawayTarget(PathVector3 origin, PathVector3 enemy, int maxlength)
    {
        // 初始化 NodeList
        ArrayList templist = new ArrayList();

        // 初始化Open count
        int OpenCount = 0;

        // 当前要查找的节点
        PathNode FNode = new PathNode(m_map);

        // 取得起始和结束位置
        int sx = origin.tx(m_map); ;
        int sz = origin.tz(m_map);

        int tx = enemy.tx(m_map);
        int tz = enemy.tz(m_map);


        // 创建第一个路点(原点)
        PathNode root = new PathNode(m_map);
        root.SetPosition(sx, sz);
        root.state = PathNode.stateID.OPEN;
        root.SetCost(origin, enemy, 0);

        // 将起始点加入检查表
        templist.Add(root);
        OpenCount++;

        // 暂时关掉block
        //if (m_isBlock)
        //{
        //    m_map.RemoveBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);
        //}

        // 1.检测所有节点,创建路点表
        while (OpenCount > 0 && templist.Count < maxlength)
        {

            // 检查效率最高的路点
            PathNode temp = null;

            foreach (PathNode pn in templist)
            {
                if (pn.state == PathNode.stateID.CLOSE)
                    continue;

                if (temp == null)
                {
                    temp = pn;
                    continue;
                }

                // 越远越好
                if (pn.heuristic > temp.heuristic)
                {
                    temp = pn;
                }
            }

            if (temp != null)
            {
                FNode = temp;
                FNode.state = PathNode.stateID.CLOSE;
                OpenCount--;

            }
            else
            {
                break;
            }


            // 检查周边路点(4)
            int nx = FNode.tx;
            int nz = FNode.tz;
            for (int cx = nx - 1; cx < nx + 2; cx++)
            {
                for (int cz = nz - 1; cz < nz + 2; cz++)
                {


                    //去掉当前节点
                    if (nx == cx && nz == cz)
                        continue;

                    // 去掉斜角的四个格
                    if (cx != nx && cz != nz)
                        continue;

                    //如果找到目标路点
                    if (cx == tx && cz == tz)
                    {
                        continue;

                    }

                    if (cx < 0 || cz < 0 || cx >= m_map.SizeX || cz >= m_map.SizeZ)
                        continue;

                    float extraCost = 0;//如果格子是被移动体占有,虽然可以看作可以通过,但会产生额外的花费

                    // 如果这个节点是死节点,并不是移动体,退出这个节点,进行下一个节点的检查
                    if (m_map.IsBlocked(cx, cz, m_size))
                    {
                        if (m_map.m_Data[cx, cz].dynamicID == 0 || m_map.m_Data[cx, cz].dynamicID != m_DynamicID)
                            continue;
                        else
                        {
                            extraCost = 8;
                        }
                    }


                    //检查是否是已存在路点
                    bool isExist = false;
                    foreach (PathNode pn in templist)
                    {
                        if (pn.tx == cx && pn.tz == cz)
                        {
                            isExist = true;

                            break;
                        }
                    }
                    if (isExist)
                    {
                        continue;
                    }

                    //创建新节点
                    PathNode checknode = new PathNode(m_map);
                    checknode.SetPosition(cx, cz);
                    checknode.SetParent(FNode);
                    checknode.state = PathNode.stateID.OPEN;
                    checknode.SetCost(origin, enemy, extraCost);


                    //加入OPEN表
                    templist.Add(checknode);
                    OpenCount++;

                      
                } //for (int z = nz - 1; z < nz+2; z++)
            }//for (int x = nx - 1; x < nx+2; x++)

        }// while



        FNode = null;
        foreach (PathNode pn in templist)
        {

            if (FNode == null)
            {
                FNode = pn;
                continue;
            }

            if (pn.heuristic > FNode.heuristic)
            {
                FNode = pn;
            }
        } // foreach (PathNode pn in CheckedNode)


          

        //if (m_isBlock)
        //{
        //    m_map.SetBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);
        //}


        return FNode!=root?FNode.position:null;
    }

    /// <summary>
    /// [deprecated]路径节点表是否为空
    /// </summary>
    /// <returns>如果为空返回true</returns>
    private bool IsEmpty()
    {
        int count = m_PathNodes.Count;
        if (count <= 0)
            return true;
        return false;
    }

    /// <summary>
    /// [deprecated]获得目标节点位置
    /// </summary>
    public PathVector3 GetCurrentNodePosition()
    {
        int count = m_PathNodes.Count;
        if (count <= 0)
        {
            return null;
        }
        else
            return m_PathNodes[count - 1].position;
    }

   

    #endregion
}

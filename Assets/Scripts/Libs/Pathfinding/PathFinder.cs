using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Ѱ·��
/// </summary>
public class PathFinder
{
    /// <summary>
    /// ��ͼ��Ϣ
    /// </summary>
    protected PathMap m_map=null;
    public PathMap map { get { return m_map; } }

    /// <summary>
    /// �洢·���ڵ�
    /// </summary>
    protected List<PathNode> m_PathNodes = new List<PathNode>();
    public int NodesCount{ get{return m_PathNodes.Count;}}
    public PathVector3 DestNodePosition { get { return m_PathNodes.Count==0? null: m_PathNodes[0].position; } }

    /// <summary>
    /// ��ǰλ��
    /// </summary>
    private PathVector3 m_currentPos = new PathVector3();
    public PathVector3 currentPos { get { return m_currentPos; } }

    /// <summary>
    /// Ŀ��λ��
    /// </summary>
    private PathVector3 m_destination = new PathVector3();
    public PathVector3 destination{ get{return m_destination;}}

    /// <summary>
    /// ��һ��·���λ��
    /// </summary>
    private PathVector3 m_lastNodePos = new PathVector3();
 
    /// <summary>
    /// �Ƿ������������
    /// </summary>
    private bool m_isBlock = true;
    public bool isBlock { get { return m_isBlock; } }

    /// <summary>
    /// Ѱ·ʵ���ѹ����С�������ֵΪ0ʱ������Χ���γ��κ�ѹ�������ֵ���Ϊ4�����ӵ�ѹ��������Ĵ�С�ĺͳ�����ﵽ5�����ܽ���
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
    /// �ƶ��ٶ�
    /// </summary>
    protected float m_moveSpeed = 1.0f;

    /// <summary>
    /// ��ǰλ�õ���һ��·��ľ���
    /// </summary>
    private float m_distCache=0;


    /// <summary>
    /// �����ȷ��һ������ʱ,����ʹ���������(ǰһ��������·��)
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
    /// �ڲ������
    /// </summary>
    private int m_InternalCounter = 1;
    private int m_dirCounter = 0;

    /// <summary>
    /// �ݲ�
    /// </summary>
    private float m_Tolerance = 0.001f;

    private bool m_searchCenter = false;
    /// <summary>
    /// �Ƿ��ڲ������ĵ�״̬
    /// </summary>
    public bool searchCenter {  get { return m_searchCenter; }}
    public void SetSearchCenter(bool search) { m_searchCenter = search; }

    public delegate void VoidDelegate();
    public event VoidDelegate onPathNodeUpdate;

    /// <summary>
    /// ��ʼ�����캯��
    /// </summary>
    /// <param name="pox">λ��X</param>
    /// <param name="poz">λ��Z</param>
    /// <param name="isblocker">�Ƿ���һ��ռ����</param>
    /// <param name="dynamicid">��̬id</param>
    /// <param name="entitySize">��С</param>
    public PathFinder( PathMap map, float pox ,float poz ,bool isblock ,int entitySize ,int dynamicid ,float movespeed )
    {
        m_map = map;

        // ��ǰλ��
        m_currentPos.Set( pox, poz);
        // Ŀ��λ��
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
    /// ����ڵ�ͼ�����µ���Ϣ
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
    /// ���·��
    /// </summary>
    public void ClearNodes()
    {
        m_PathNodes.Clear();
    }

    /// <summary>
    /// ��������Ŀ����·��
    /// </summary>
    /// <param name="origin">��ǰ��λ��</param>
    /// <param name="target">Ŀ��λ��</param>
    /// <param name="maxnodes">���ڵ���</param>
    /// <param name="Optimize">�Ƿ��Ż�·��</param>
    /// <returns>�������ҵ�Ŀ���</returns>
    public bool BuildPath(PathVector3 origin, PathVector3 target, int maxnodes, bool Optimize, bool followgroup, bool centeredQuad = true  )
    {
        bool findtarget = false;

        target = CheckDenstination(origin, target);


        // ��ʼ��Open count
        int OpenCount = 0;

        // ��ǰҪ���ҵĽڵ�
        PathNode FNode = new PathNode(m_map);
        // Ŀ��ڵ�
        PathNode targetNode = new PathNode(m_map);
        targetNode.SetPosition(target);
        if (centeredQuad)
        {
            targetNode.CenteredPosition();
            target.center(m_map);
        }

        // ���浱ǰλ��
        m_currentPos.Set(origin);
         
        // ����Ŀ���λ��
        m_destination.Set(targetNode.position);

        // ȡ����ʼ�ͽ���Tileλ��
        int sx = origin.tx(m_map);
        int sz = origin.tz(m_map);

        // ��ʼ�� NodeList
        m_PathNodes.Clear();

        // �����ʼ�ͽ���λ��һ��
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

        // ������һ��·��(ԭ��)
        PathNode root = new PathNode(m_map); // ����ڵ��λ�ú�origin��ͬ
        root.SetPosition(sx, sz);
        root.state = PathNode.stateID.OPEN;
        root.SetCost(origin, targetNode.position, 0);

        // ����ʼ��������
        m_PathNodes.Add(root);
        OpenCount++;

        // ��ʱ�ص�block
        //if (m_isBlock)
        //{
        //    m_map.RemoveBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);
        //}

        // 1.������нڵ�,����·���
        while (OpenCount > 0 && m_PathNodes.Count < maxnodes)
        {
            // ���Ч����ߵ�·��
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

            // ����ܱ�·��(4)
            int nx = FNode.tx;
            int nz = FNode.tz;
            for (int cx = nx - 1; cx < nx + 2; cx++)
            {
                for (int cz = nz - 1; cz < nz + 2; cz++)
                {
                    // �����㵱ǰ�ڵ�
                    if (nx == cx && nz == cz)
                        continue;

                    // ������Խ��ڵ�
                    if (cx < 0 || cz < 0 || cx >= m_map.SizeX || cz >= m_map.SizeZ)
                        continue;

                    float extraCost = 0;//��������Ǳ��ƶ���ռ��,��Ȼ���Կ�������ͨ��,�����������Ļ���

                    // б�ǵ��ĸ���
                    if (cx != nx && cz != nz)
                    {
                        extraCost = 0.4f;

                        int tempx = 0, tempz = 0, tempx1 = 0, tempz1 = 0;

                        if (cx > nx && cz > nz) //���Ͻ�
                        {
                            tempx = nx + 1;
                            tempz = nz;
                            tempx1 = nx;
                            tempz1 = nz + 1;

                        }
                        else if (cx > nx && cz < nz) // ���½�
                        {
                            tempx = nx + 1;
                            tempz = nz;
                            tempx1 = nx;
                            tempz1 = nz - 1;
                        }
                        else if (cx < nx && cz < nz) // ���½�
                        {
                            tempx = nx - 1;
                            tempz = nz;
                            tempx1 = nx;
                            tempz1 = nz - 1;
                        }
                        else if (cx < nx && cz > nz) // ���Ͻ�
                        {
                            tempx = nx;
                            tempz = nz + 1;
                            tempx1 = nx - 1;
                            tempz1 = nz;
                        }

                        // o o o
                        // o x !   // p��ʾ��ǰ��ɫ, ���x����ͨ��, !Ҳ����ͨ��
                        // o p x
                        if (m_map.IsBlocked(tempx, tempz, m_size) || m_map.IsBlocked(tempx1, tempz1, m_size))
                        {
                            continue;
                        }
                    }

                    // �������ڵ������ڵ�,�������ƶ���,�˳�����ڵ�,������һ���ڵ�ļ��
                    if (m_map.IsBlocked(cx, cz, m_size))
                    {
                        // �����Ŀ���
                        if (cx == targetNode.tx && cz == targetNode.tz)
                        {
                            findtarget = true;
                            goto _FinishChecking;

                        }

                        int dynamicid = m_map.GetDynamic(cx, cz);

                        // ����ڵ��Ǿ�̬�ģ��˳�����ڵ�ļ��
                        if (dynamicid == 0)
                            continue;
                        else if (System.Math.Abs(this.DynamicID) != System.Math.Abs(dynamicid)) // ���id��ͬ���˳����
                        {
                            continue;
                        }

                        // �������
                        if (followgroup)
                        {
                            if (dynamicid < 0) // �������ʱ���ƶ��ģ��˳����
                            {
                                continue;
                            }
                            else
                            {
                                // �����ǰ�ٶȴ���������ӵ��ٶȣ����Ӷ��⿪��
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

                    //����Ƿ����Ѵ���·��
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

                    //������OPEN�ڵ�
                    PathNode checknode = new PathNode(m_map);
                    checknode.SetPosition(cx, cz);
                    checknode.SetParent(FNode);
                    checknode.state = PathNode.stateID.OPEN;
                    checknode.SetCost(origin, targetNode.position, extraCost);

                    //����OPEN��
                    m_PathNodes.Add(checknode);
                    OpenCount++;

                    //����ҵ�Ŀ��·��
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

        //����Ҳ����յ�,�ҳ�Ч����ߵĽڵ���ΪĿ��ڵ�
        
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
        
        // ��ʼ�� (!���������Ҫ�Ż�)
        m_PathNodes.Clear();

        // ���ֻ��һ���ڵ�,������ڵ����Լ�,�˳�����
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

        // �������·�������Ż���ֱ�Ӽ�������·��
        if (!Optimize)
        {
            // ����������
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
            // �Ż�·��
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
    /// ���Ŀ��ڵ�λ��
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
    /// �Ƿ���·��
    /// </summary>
    public bool HasPath()
    {
        int count = m_PathNodes.Count;
        if (count > 0)
            return true;

        return false;
    }

    /// <summary>
    /// ǰ����Ŀ��ڵ�
    /// </summary>
    /// <param name="entryPos">��ǰλ��</param>
    /// <param name="entryAngleY">��ǰ�Ƕ�</param>
    /// <param name="movespeed">�ƶ��ٶ�</param>
    /// <param name="rotspeed">��ת�ٶ�</param>
    /// <returns>��������һ���ڵ��ƶ�������</returns>
    public bool MoveToTargetNode(ref PathVector3 entitypos, ref float entityangley, float movespeed, float rotspeed)
    {
        if (!this.HasPath()) // if there is no any node
            return false;

        // ��õ�ǰĿ��ڵ��λ��
        PathVector3 nodepos = this.GetDestination();
        if (nodepos == null)
            return false;

        // ��������λ��
        PathVector3 copypos = new PathVector3();
        copypos.Set(entitypos);
        // ��������Ƕ�
        float copyangle = entityangley;

        // ��תʵ������Ŀ��
        copyangle = PathHelper.PolarAngle2(copypos, nodepos);
        // �ƶ�
        copypos.x += movespeed * (float)System.Math.Sin(PathHelper.DegToRad * copyangle);
        copypos.z += movespeed * (float)System.Math.Cos(PathHelper.DegToRad * copyangle);

        // ȡ�õ�ǰλ�ò����µ�ͼ
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
    /// ��Ŀ�����ת
    /// </summary>
    /// <param name="entryPos">��ǰλ��</param>
    /// <param name="destination">Ŀ��λ��</param>
    /// <param name="entryAngleY">��ǰ�Ƕ�</param>
    /// <param name="speed">��ת�ٶ�</param>
    /// <returns>������ת�Ƕ�</returns>
    public float RotateToTarget(PathVector3 entryPos, PathVector3 des, float entryAngleY, float speed)
    {

        float newAngle = entryAngleY;

        // ���Ŀ��Ƕ�
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
    /// ʵ��λ�øı���µ�ͼ��Ϣ
    /// </summary>
    /// <param name="pos">ʵ��ĵ�ǰ����λ��</param>
    /// <param name="entrySize">ʵ������,������Ϊ��</param>
    /// <returns>���·�����Ͽ�,����false</returns>
    public bool UpdateMapData(ref PathVector3 pos  )
    {
        bool isPathContinue = true;

        // ���Ӳ��ܽ���
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

        // �ڵ�ǰ������
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
    /// ��ѯ�Ƿ񵽴�Ŀ��ڵ�λ��,���������Ҫ��ÿ��ѭ����ִ��
    /// </summary>
    /// <param name="entryPos">һ������Ϊ3�����鱣�浱ǰʵ�����ڵ�����λ��</param>
    /// <returns>���ʵ���Ѿ�����Ŀ��ڵ��λ�÷���true</returns>
    private bool UpdatePathNodes(ref PathVector3 entryPos)
    {
        int count = m_PathNodes.Count;
        if (count <= 0)
            return false;


        float dist = PathHelper.Distance3(entryPos, m_lastNodePos);
        //�������Ŀ��ڵ�,������ڵ�ȥ��
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
                // ������� [�ⲽ�������update map data��ֵ��һ�£��������ش���]
                // entryPos.Set(targetpos);
            }

            return true;
        }
        else
            return false;
    }

    /// <summary>
    /// ����λ��
    /// </summary>
    /// <param name="pos">Ŀ��λ��</param>
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
    /// ��¼�ӵ�ǰ�ڵ㵽��һ���ڵ�ľ���
    /// </summary>
    private void CacheDistance(PathVector3 current, PathVector3 next)
    {
        m_lastNodePos.Set(current);
        m_distCache = PathHelper.Distance3(current, next);
    }

    // temp
    public PathVector3 CheckDenstination(PathVector3 start, PathVector3 target)
    {
        // ��ʱ�ص�block
        //if (m_isBlock)
        //{
        //    m_map.RemoveBlock(start, m_size, m_DynamicID, m_moveSpeed);
        //}
        PathVector3 des = target;
        int tx = target.tx(m_map);
        int tz = target.tz(m_map);
        // �����߽�
        if (!m_map.CheckValid(target))
        {
            /*
            List<PathNode> nodelist = new List<PathNode>();
            PathNode current = new PathNode(m_map);
            current.SetPosition(tx, tz);
            while (nodelist.Count > 0)
            {
                // ��鵱ǰ�ڵ��Ƿ�ɿ�
                bool b = m_map.IsBlocked(tx, tz, m_size);
                if (!b)
                {
                    target[0] = tx;
                    target[2] = tz;
                    return target;
                }
                else // ������ɿ��������һ���ܱ�8���ڵ��Ƿ�ɿ�
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
        // ��ʱ�ص�block
        //if (m_isBlock)
        //{
        //    m_map.SetBlock(start, m_size, m_DynamicID, m_moveSpeed);
        //}
        return des;
    }

    /// <summary>
    /// ����һ��free�ĸ�
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

    #region δʹ�ã���ȷ�������ȶ��Ĵ���

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

        // ��ʼ�� NodeList
        ArrayList m_SearchNodes = new ArrayList();

        // ��ʼ��Open count
        int OpenCount = 0;

        // ��ǰҪ���ҵĽڵ�
        PathNode FNode = new PathNode(m_map);

        // ȡ����ʼ�ͽ���Tileλ��
        int sx = origin.tx(m_map);
        int sz = origin.tz(m_map);

        int tx = target.tx(m_map);
        int tz = target.tz(m_map);


        // �����ʼ�ͽ���λ��һ��
        if (sx == tx && sz == tz)
        {
            findcount = 0;

            return findcount;
        }


        // ������һ��·��(ԭ��)
        PathNode root = new PathNode(m_map);
        root.SetPosition(sx, sz);
        root.state = PathNode.stateID.OPEN;
        root.SetCost(origin, target, 0);

        // ����ʼ��������
        m_SearchNodes.Add(root);
        OpenCount++;

        // ��ʱ�ص�block
        //if (m_isBlock)
        //{
        //    m_map.RemoveBlock(origin, m_size, m_DynamicID, m_moveSpeed);
        //}

        // 1.������нڵ�,����·���
        while (OpenCount > 0 && m_SearchNodes.Count < maxnodes)
        {

            // ���Ч����ߵ�·��
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


            // ����ܱ�·��(4)
            int nx = FNode.tx;
            int nz = FNode.tz;
            for (int cx = nx - 1; cx < nx + 2; cx++)
            {
                for (int cz = nz - 1; cz < nz + 2; cz++)
                {
                    // �����㵱ǰ�ڵ�
                    if (nx == cx && nz == cz)
                        continue;

                    // ������Խ��ڵ�
                    if (cx < 0 || cz < 0 || cx >= m_map.SizeX || cz >= m_map.SizeZ)
                        continue;

                    float extraCost = 0;//��������Ǳ��ƶ���ռ��,��Ȼ���Կ�������ͨ��,�����������Ļ���


                    // б�ǵ��ĸ���
                    if (cx != nx && cz != nz)
                    {
                        int tempx = 0, tempz = 0, tempx1 = 0, tempz1 = 0;

                        extraCost = 0.4f;

                        if (cx > nx && cz > nz) //���Ͻ�
                        {

                            tempx = nx + 1;
                            tempz = nz;

                            tempx1 = nx;
                            tempz1 = nz + 1;

                        }
                        else if (cx > nx && cz < nz) // ���½�
                        {
                            tempx = nx + 1;
                            tempz = nz;

                            tempx1 = nx;
                            tempz1 = nz - 1;
                        }
                        else if (cx < nx && cz < nz) // ���½�
                        {
                            tempx = nx - 1;
                            tempz = nz;

                            tempx1 = nx;
                            tempz1 = nz - 1;
                        }
                        else if (cx < nx && cz > nz) // ���Ͻ�
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


                    // ����ҵ�Ŀ��
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


                    //����Ƿ����Ѵ���·��
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



                    //�����½ڵ�
                    PathNode checknode = new PathNode(m_map);
                    checknode.SetPosition(cx, cz);
                    checknode.SetParent(FNode);
                    checknode.state = PathNode.stateID.OPEN;
                    checknode.SetCost(origin, target, extraCost);


                    //����OPEN��
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
    /// �������ֵ
    /// </summary>
    public void Update()
    {
        this.RandomInternal(); 
    }

    /// <summary>
    /// �ڲ��������1-100000
    /// </summary>
    private void RandomInternal()
    {
        m_InternalCounter++;
        if (m_InternalCounter > 100000)
            m_InternalCounter = 1;
    }

    /// <summary>
    /// ����·��
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="target"></param>
    /// <param name="maxnodes"></param>
    /// <param name="Optimize"></param>
    /// <returns></returns>
    public bool BuildPathEx(PathVector3 origin, PathVector3 target, int maxnodes, bool Optimize)
    {
        // ��������,�������ж�̬�ϰ�,���ģʽ
        bool b = BuildPath(origin, target, maxnodes, Optimize, true);
        if (b)
            return true;


        // ʹ���Ż�·��,�����㶯̬�ϰ�,���ģʽ
        b = BuildPath(origin, target, maxnodes, false, false);
        if (m_PathNodes.Count < 1)
            return false;



        // �������ڵ�
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

        // �������ж�̬�ϰ�,���㶯̬�ϰ�,�����ģʽ
        b = BuildPath(origin, des, maxnodes, Optimize, true);

        if (b)
        {
            return true;
        }
        else
            return false;

    }


    /// <summary>
    /// ���ҺϷ�Ŀ���
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="target"></param>
    /// <param name="offset"></param>
    /// <returns>���س���Ϊ3��ʵ������</returns>
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

        int MAX_AREA = offset;      // ���ҵķ�Χ

        PathVector3 newtarget = new PathVector3();   //�µ�Ŀ��λ��
        newtarget.Set(target);
        // ��ʱȥ������

        //if (m_isBlock)
        //{
        //    m_map.RemoveBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);
        //}


        // ������Χ������Tile
        for (int i = 1; i < MAX_AREA+1; i++ )
        {
            float dist = 0;
            for (int dir = -1; dir < 2; dir++)  //��ͬ����
            {
                if (dir == 0)
                    continue;

                this.TargetDistance(origin, ref newtarget, ref dist,0, tx + i * dir, tz);
                this.TargetDistance(origin, ref newtarget, ref dist,0, tx, tz + i * dir);
                   

            }

            float extra = 0.2f;

            // �����߽�
            for (int k = 1; k < i + 1; k++)
            {
                for (int dir = -1; dir < 2; dir++) //��ͬ����
                {
                    if (dir == 0)
                        continue;

                    if (k != i)
                    {

                        this.TargetDistance(origin, ref newtarget, ref dist, extra, tx + i * dir, tz + k * dir); // ���� ����
                        this.TargetDistance(origin, ref newtarget, ref dist, extra, tx + k * dir, tz + i * dir); // ���� ����

                        this.TargetDistance(origin, ref newtarget, ref dist, extra, tx - i * dir, tz + k * dir); // ���� ����
                        this.TargetDistance(origin, ref newtarget, ref dist, extra, tx - k * dir, tz + i * dir); // ���� ����


                    }
                    else
                    {
                        this.TargetDistance(origin, ref newtarget, ref dist, extra, tx + k * dir, tz + k * dir); // �����ϱ� �����±�

                        this.TargetDistance(origin, ref newtarget, ref dist, extra, tx - k * dir, tz + k * dir); // �����ϱ� �����±�
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
    /// �����Ŀ��֮��Ĳ��Ծ���
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
    /// ����Ԥ��Ŀ���
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="target"></param>
    /// <param name="target"></param>
    /// <returns>���س���Ϊ3��ʵ������</returns>
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


        //�鿴�ܱ����
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

                // ����һЩ�����
                // 0==left ,1==down, 2==up ,3==right
                if (dir ==m_InvalidDir)
                    continue;


                if (m_map.IsBlocked(dx, dz, m_size))
                    continue;

                // ̽��
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


        // �������ֵ
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
    /// ����·��
    /// </summary>
    /// <param name="origin">ԭ��</param>
    /// <returns>true ��·��</returns>
    public bool IsCrossRoad(float[] origin )
    {
        int ox = (int)origin[0];
        int oz = (int)origin[2];

        //if (m_isBlock)
        //{
        //    m_map.RemoveBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);
        //}

        //�鿴�ܱ����
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
    /// Ѱ������·��
    /// </summary>
    /// <param name="origin">ԭʼλ��</param>
    /// <param name="enemy">��ܵĶ���</param>
    /// <param name="maxlength">̽���ĸ�������</param>
    /// <returns>����Ŀ��λ��,��ǰλ��Ϊnull</returns>
    public PathVector3 FindRunawayTarget(PathVector3 origin, PathVector3 enemy, int maxlength)
    {
        // ��ʼ�� NodeList
        ArrayList templist = new ArrayList();

        // ��ʼ��Open count
        int OpenCount = 0;

        // ��ǰҪ���ҵĽڵ�
        PathNode FNode = new PathNode(m_map);

        // ȡ����ʼ�ͽ���λ��
        int sx = origin.tx(m_map); ;
        int sz = origin.tz(m_map);

        int tx = enemy.tx(m_map);
        int tz = enemy.tz(m_map);


        // ������һ��·��(ԭ��)
        PathNode root = new PathNode(m_map);
        root.SetPosition(sx, sz);
        root.state = PathNode.stateID.OPEN;
        root.SetCost(origin, enemy, 0);

        // ����ʼ��������
        templist.Add(root);
        OpenCount++;

        // ��ʱ�ص�block
        //if (m_isBlock)
        //{
        //    m_map.RemoveBlock(m_currentPos, m_size, m_DynamicID, m_moveSpeed);
        //}

        // 1.������нڵ�,����·���
        while (OpenCount > 0 && templist.Count < maxlength)
        {

            // ���Ч����ߵ�·��
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

                // ԽԶԽ��
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


            // ����ܱ�·��(4)
            int nx = FNode.tx;
            int nz = FNode.tz;
            for (int cx = nx - 1; cx < nx + 2; cx++)
            {
                for (int cz = nz - 1; cz < nz + 2; cz++)
                {


                    //ȥ����ǰ�ڵ�
                    if (nx == cx && nz == cz)
                        continue;

                    // ȥ��б�ǵ��ĸ���
                    if (cx != nx && cz != nz)
                        continue;

                    //����ҵ�Ŀ��·��
                    if (cx == tx && cz == tz)
                    {
                        continue;

                    }

                    if (cx < 0 || cz < 0 || cx >= m_map.SizeX || cz >= m_map.SizeZ)
                        continue;

                    float extraCost = 0;//��������Ǳ��ƶ���ռ��,��Ȼ���Կ�������ͨ��,�����������Ļ���

                    // �������ڵ������ڵ�,�������ƶ���,�˳�����ڵ�,������һ���ڵ�ļ��
                    if (m_map.IsBlocked(cx, cz, m_size))
                    {
                        if (m_map.m_Data[cx, cz].dynamicID == 0 || m_map.m_Data[cx, cz].dynamicID != m_DynamicID)
                            continue;
                        else
                        {
                            extraCost = 8;
                        }
                    }


                    //����Ƿ����Ѵ���·��
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

                    //�����½ڵ�
                    PathNode checknode = new PathNode(m_map);
                    checknode.SetPosition(cx, cz);
                    checknode.SetParent(FNode);
                    checknode.state = PathNode.stateID.OPEN;
                    checknode.SetCost(origin, enemy, extraCost);


                    //����OPEN��
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
    /// [deprecated]·���ڵ���Ƿ�Ϊ��
    /// </summary>
    /// <returns>���Ϊ�շ���true</returns>
    private bool IsEmpty()
    {
        int count = m_PathNodes.Count;
        if (count <= 0)
            return true;
        return false;
    }

    /// <summary>
    /// [deprecated]���Ŀ��ڵ�λ��
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

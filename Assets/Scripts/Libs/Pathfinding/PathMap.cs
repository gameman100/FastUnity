using UnityEngine;
using System.Collections;

[System.Serializable]
public class PathMap
{
    /// <summary>
    /// 每个格子内可承受的最大大小,实体的大小0,1,2,3,4
    /// </summary>
    public static int MAX_SIZE =1005;
    /// <summary>
    /// 每个格子的大小
    /// </summary>
    public float QUAD_SIZE { get { return m_quad_size; } }
    protected float m_quad_size = 1;

    private float m_startx = 0;
    public float startx { get { return m_startx; } }
    private float m_startz = 0;
    public float startz { get { return m_startz; } }

    private int m_sizeX = 128;
    public int SizeX{get{return m_sizeX;}}
    private int m_sizeZ = 128;
    public int SizeZ{get{return m_sizeZ;}}

    public float boundx { get { return startx + m_sizeX; } }
    public float boundz { get { return startz + m_sizeZ; } }
    /// <summary>
    /// 地图信息
    /// </summary>
    public QuadData[,] m_Data;

        
    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="sizex"> 地图X方向大小 </param>
    /// <param name="sizez"> 地图Z方向大小 </param>
    public PathMap( int sizex ,int sizez, float start_x=0, float start_z=0, float quadsize =1 )
    {
        m_sizeX = sizex;
        m_sizeZ = sizez;
        m_startx = start_x;
        m_startz = start_z;
        m_quad_size = quadsize;

        m_Data = new QuadData[sizex, sizez];

        for (int i = 0; i < m_sizeX; i++)
        {
            for (int k = 0; k < m_sizeZ; k++)
            {
                m_Data[i, k] = new QuadData();
                m_Data[i, k].data = null;
                m_Data[i, k].size = 0;
                m_Data[i, k].block = 0;
                m_Data[i, k].dynamicID = 0;
                m_Data[i, k].movespeed = 0;  
            }
        }
    }

        /// <summary>
        /// 清除地图信息
        /// </summary>
        public void Clear()
        {
            m_Data = null;
        }

        public void SetBlock( PathVector3 v, int entrySize, int dynamic, float movespeed)
        {
            SetBlock(v.x, v.z, entrySize, dynamic, movespeed);
        }

        /// <summary>
        /// 设置地图节点状态
        /// </summary>
        /// <param name="x">坐标位置X</param>
        /// <param name="z">坐标位置Z</param>
        /// <param name="entrySize">体积对周围形成的压力</param>
        /// <param name="dynamic">活动体id</param>
        public void SetBlock( float x ,float z ,int entitysize ,int dynamic ,float movespeed)
        {
            int ix = Mathf.FloorToInt( (x - m_startx) / m_quad_size );
            int iz = Mathf.FloorToInt( (z - m_startz) / m_quad_size );

            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ )
            {
                m_Data[(int)ix, (int)iz].block ++;
                m_Data[(int)ix, (int)iz].dynamicID += dynamic;
                m_Data[(int)ix, (int)iz].size += entitysize;
                m_Data[(int)ix, (int)iz].movespeed += movespeed;

                if (entitysize == 0)
                {
                    return;
                }

                for (int i = ix - 1; i < ix + 2; i++)
                {
                    for (int k = iz - 1; k < iz + 2; k++)
                    {
                        if (i >= 0 && k >= 0 && i < SizeX && k < SizeZ)
                        {
                            if (i == ix && k == iz)
                                continue;
                            m_Data[i, k].size += entitysize;

                        }
                    }
                }
            }
        }

        public void RemoveBlock(PathVector3 v, int entitySize, int dy, float mv)
        {
            RemoveBlock(v.x, v.z, entitySize, dy, mv);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <param name="entrySize"></param>
        public void RemoveBlock(float x, float z , int entitysize, int dy, float mv)
        {
            int ix = Mathf.FloorToInt((x - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((z - m_startz) / m_quad_size);

            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ)
            {
                // 解开当前格阻塞
                m_Data[ix, iz].block --;
                m_Data[ix, iz].dynamicID -= dy;
                m_Data[ix, iz].movespeed -= mv;
                // 当前格大小设为零
                m_Data[ix, iz].size -= entitysize;

                if (m_Data[ix, iz].block < 0)
                    Debug.LogError("<0");

                if (entitysize == 0)
                    return;

                // 查找当前格的周边8个格 [i,k]
                for (int i = ix - 1; i < ix + 2; i++)
                {
                    for (int k = iz - 1; k < iz + 2; k++)
                    {
                        if (i >= 0 && k >= 0 && i < SizeX && k < SizeZ)
                        {
                            // 排除当前格
                            if (i == ix && k == iz)
                                continue;

                            m_Data[i, k].size -= entitysize;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// [deprecated]是否block
        /// </summary>
        /// <param name="tilex">0 base tile x</param>
        /// <param name="tilez">0 base tile z</param>
        public bool IsBlocked(int tilex, int tilez, int entrySize)
        {
            int ix = tilex;
            int iz = tilez;
            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ) // 检查边界
            {
                if (m_Data[ix, iz].block>0 || m_Data[ix, iz].size + entrySize >= MAX_SIZE)
                {
                    return true;
                }
            }
            else
                return true;
            return false;
        }

        /// <summary>
        /// [deprecated]判断地图节点是否堵塞
        /// </summary>
        /// <param name="worldx">坐标位置X(world position, may not 0 based)</param>
        /// <param name="worldz">坐标位置Z</param>
        /// <param name="entitysize">实体的体积</param>
        /// <returns>true表示堵塞</returns>
        public bool IsBlocked2(float worldx, float worldz, int entitysize )
        {
            int ix = Mathf.FloorToInt((worldx - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((worldz - m_startz) / m_quad_size);

            return IsBlocked(ix, iz, entitysize);
        }

        /// <summary>
        /// 是否可以通过
        /// </summary>
        /// <param name="tilex">0 base tile x</param>
        /// <param name="tilez">0 base tile z</param>
        public bool IsFree(int tilex, int tilez, int entrySize)
        {
            int ix = tilex;
            int iz = tilez;
            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ) // 检查边界
            {
                if (m_Data[ix, iz].block > 0 || m_Data[ix, iz].size + entrySize >= MAX_SIZE)
                {
                    return false;
                }
            }
            else
                return false;
            return true;
        }

        /// <summary>
        /// 判断地图节点是否可以通过
        /// </summary>
        /// <param name="worldx">坐标位置X(world position, may not 0 based)</param>
        /// <param name="worldz">坐标位置Z</param>
        /// <param name="entitysize">实体的体积</param>
        /// <returns>false表示堵塞</returns>
        public bool IsFree2(float worldx, float worldz, int entitysize)
        {
            int ix = Mathf.FloorToInt((worldx - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((worldz - m_startz) / m_quad_size);

            return !IsBlocked(ix, iz, entitysize);
        }

        /// <summary>
        /// 设置地图节点数据
        /// </summary>
        /// <param name="x">坐标位置X</param>
        /// <param name="z">坐标位置Z</param>
        /// <param name="data">数据</param>
        public void SetData(float x, float z, System.Object data)
        {
            int ix = Mathf.FloorToInt((x - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((z - m_startz) / m_quad_size);
            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ)
            {
                m_Data[(int)ix, (int)iz].data = data;
            }
        }

        /// <summary>
        /// 取得地图节点的数据
        /// </summary>
        /// <param name="x">坐标位置X</param>
        /// <param name="z">坐标位置Z</param>
        /// <returns>返回null表示坐标无效</returns>
        public System.Object GetData(float x, float z)
        {
            int ix = Mathf.FloorToInt((x - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((z - m_startz) / m_quad_size);
            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ)
            {
                return m_Data[(int)ix, (int)iz].data;
            }
            else
                return null;
        }

        /// <summary>
        /// 设置包括一个移动体
        /// </summary>
        /// <param name="x">坐标位置X</param>
        /// <param name="z">坐标位置Z</param>
        /// <param name="targetx">坐标位置X</param>
        /// <param name="targetz">坐标位置Z</param>
        internal void SetDynamic(float x, float z, int dynamic)
        {
            int ix = Mathf.FloorToInt((x - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((z - m_startz) / m_quad_size);
            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ)
            {
                m_Data[(int)ix, (int)iz].dynamicID = dynamic;

             
            }
        }

        /// <summary>
        /// 去掉移动体的信息
        /// </summary>
        /// <param name="x">坐标X</param>
        /// <param name="z">坐标Z</param>
        internal void RemoveDynamic(float x, float z)
        {
            int ix = Mathf.FloorToInt((x - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((z - m_startz) / m_quad_size);
            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ)
            {
                m_Data[(int)ix, (int)iz].dynamicID = 0;
            }
        }

        /// <summary>
        /// 是否包括一个移动体
        /// </summary>
        /// <param name="x">坐标位置X</param>
        /// <param name="z">坐标位置Z</param>
        /// <returns>是否包括</returns>
        public bool IsDynamic(float x, float z)
        {
            int ix = Mathf.FloorToInt((x - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((z - m_startz) / m_quad_size);
            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ)
            {
                if (m_Data[(int)ix, (int)iz].dynamicID > 0)
                    return true;
                else
                    return false;
            }

            return false;
        }

        /// <summary>
        /// 是否包括一个移动体
        /// </summary>
        /// <param name="x">坐标位置X</param>
        /// <param name="z">坐标位置Z</param>
        /// <returns>是否包括</returns>
        public int GetDynamic(float x, float z)
        {
            int ix = Mathf.FloorToInt((x - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((z - m_startz) / m_quad_size);
            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ)
            {
                return m_Data[(int)ix, (int)iz].dynamicID;
            }

            return 0;
        }


        public float GetSpeed(float x, float z)
        {
            int ix = Mathf.FloorToInt((x - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((z - m_startz) / m_quad_size);
            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ)
            {
                return m_Data[(int)ix, (int)iz].movespeed;
            }

            return 0;
        }

        /// <summary>
        /// 取得地图节点的大小
        /// </summary>
        /// <param name="x">坐标位置X</param>
        /// <param name="z">坐标位置Z</param>
        /// <returns>返回-1表示坐标无效</returns>
        public int GetSize(float x, float z)
        {
            int ix = Mathf.FloorToInt((x - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((z - m_startz) / m_quad_size);
            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ)
            {
                return m_Data[(int)ix, (int)iz].size;
            }
            else
                return -1;
        }



        /// <summary>
        /// 取得地图节点的中心点坐标位置
        /// </summary>
        /// <param name="x">位置X</param>
        /// <param name="z">位置Z</param>
        /// <param name="vec">输出长度为3的数组表示X Y Z坐标,如果输入坐标不合法,输出null</param>
        public void GetTileCenter(float x, float z, ref float[] vec)
        {
            int ix = Mathf.FloorToInt((x - m_startx) / m_quad_size);
            int iz = Mathf.FloorToInt((z - m_startz) / m_quad_size);

            if (ix >= 0 && iz >= 0 && ix < SizeX && iz < SizeZ)
            {
                vec = new float[3];
                vec[0] = ix + QUAD_SIZE * 0.5f;
                vec[1] = 0;
                vec[2] = iz + QUAD_SIZE * 0.5f;
            }
            else
            {
                vec = null;
            }

        }

        public bool CheckValid(int tilex, int tilez)
        {
            if (tilex < 0 || tilez < 0 || tilex >= SizeX || tilez >= SizeZ)
                return false;
            else
                return true;

        }

        public bool CheckValid(PathVector3 v)
        {
            if (v.x < startx || v.z < startz || v.x >= boundx || v.z >= boundz)
                return false;
            else
                return true;

        }
  

  

    /// <summary>
    /// 地图信息
    /// </summary>
    public struct QuadData
    {
        /// <summary>
        /// 数据
        /// </summary>
        public System.Object data;
        /// <summary>
        /// 大小
        /// </summary>
        public int size;
        /// <summary>
        /// >0被占用
        /// </summary>
        public int block;
        /// <summary>
        /// 当前格子内包括一个可移动的智能体 ,0表示不移动,大于0表示移动，小于0表示暂时停止
        /// </summary>
        public int dynamicID;
        /// <summary>
        /// 当前格子内智能体的移动速度
        /// </summary>
        public float movespeed;

 
    }

}
